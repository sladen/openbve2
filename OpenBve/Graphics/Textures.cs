using System;
using System.Drawing;
using System.Drawing.Imaging;
using Tao.OpenGl;
using Tao.Sdl;

namespace OpenBve {
	internal static partial class Textures {
		
		/*
		 * Overview
		 * --------
		 * 
		 * The RegisteredTextured field stores information about all textures that have been registered.
		 * A registered texture is not automatically loaded. Instead, the Status member of a texture
		 * tells whether the texture has not been loaded yet, whether the background worker thread is
		 * supposed to load the texture in the background, whether the raw data of the texture is
		 * already available to be submitted to OpenGL, or whether the texture has already been loaded
		 * and submitted to OpenGL.
		 * 
		 * Registered textures are identified by their file or folder names, the plugin that is supposed
		 * to load the texture, and via additional properties. If a texture with the same properties
		 * is registered multiple times, it will in fact only reuse the already registered texture. This
		 * means that this texture manager only stores and managed each unique texture once.
		 * 
		 * Generally, plugins will indirectly call RegisterTexture to register a texture and acquire a
		 * handle to refer to this texture for later use. The renderer, or more precisely, the object
		 * management system, will eventually call UseTexture to obtain the OpenGL texture name (index)
		 * that can be directly used with OpenGL. This process might infer loading the texture for the
		 * first time, unless the background worker thread has already done so.
		 * 
		 * The general code flow is stored in Textures.cs, while additional helper functions are stored
		 * in Textures_Helper.cs for easier maintenance.
		 * 
		 * TODO: Not all functions in this file have been completely implemented yet.
		 * 
		 * */
		
		// texture type
		internal enum TextureType {
			Unknown = 0,
			Opaque = 1,
			TransparentColor = 2,
			Alpha = 3
		}
		
		// texture status
		internal enum TextureStatus {
			/// <summary>Indicates that the texture has not been loaded yet.</summary>
			NotLoaded = 0,
			/// <summary>Indicates that the raw data of the texture is scheduled to be loaded by the background worker, but the background worker has not yet finished doing so.</summary>
			Scheduled = 1,
			/// <summary>Indicates that the raw data has been loaded, but the texture has not yet been submitted to OpenGL.</summary>
			Finalizing = 2,
			/// <summary>Indicates that the texture has been submitted to OpenGL and that the OpenGlTextureIndex is valid.</summary>
			Loaded = 3
		}
		
		// texture
		internal class Texture {
			/// <summary>The origin of the texture. If a path is provided, it must be an absolute path.</summary>
			internal OpenBveApi.General.Origin Origin;
			/// <summary>The parameters for the texture.</summary>
			internal OpenBveApi.Texture.TextureParameters Parameters;
			/// <summary>The RGBA raw data of the texture.</summary>
			/// <remarks>If a valid texture origin is supplied, this field is only temporarily populated during the loading stage. Otherwise, the raw data is permanent.</remarks>
			internal byte[] RawData;
			/// <summary>The width of the raw data, i.e. the extracted portion of the texture, converted to a power of two.</summary>
			internal int RawWidth;
			/// <summary>The height of the raw data, i.e. the extracted portion of the texture, converted to a power of two.</summary>
			internal int RawHeight;
			/// <summary>The texture type of the extracted portion of the texture.</summary>
			internal TextureType Type;
			/// <summary>The current texture status.</summary>
			internal TextureStatus Status;
			/// <summary>The OpenGL texture index when the texture is loaded.</summary>
			internal int OpenGlTextureIndex;
		}
		
		// members
		internal static Texture[] RegisteredTextures = new Texture[16];
		internal static int RegisteredTextureCount = 0;
		
		// register texture
		/// <summary>Registers a texture and returns the associated texture index.</summary>
		/// <param name="origin">The origin of the texture. If a path is provided, it must be an absolute path.</param>
		/// <param name="parameters">The parameters for the texture.</param>
		/// <param name="textureIndex">Receives the managed texture index on success.</param>
		internal static void RegisterTexture(OpenBveApi.General.Origin origin, OpenBveApi.Texture.TextureParameters parameters, out int textureIndex) {
			int index;
			if (FindTexture(origin, parameters, out index)) {
				// texture already exists
				textureIndex = index;
			} else {
				// register new texture
				if (RegisteredTextures.Length == RegisteredTextureCount) {
					Array.Resize<Texture>(ref RegisteredTextures, RegisteredTextures.Length << 1);
				}
				textureIndex = RegisteredTextureCount;
				RegisteredTextures[RegisteredTextureCount] = new Texture();
				RegisteredTextures[RegisteredTextureCount].Origin = origin;
				RegisteredTextures[RegisteredTextureCount].Parameters = parameters;
				RegisteredTextures[RegisteredTextureCount].RawData = null;
				RegisteredTextures[RegisteredTextureCount].RawWidth = 0;
				RegisteredTextures[RegisteredTextureCount].RawHeight = 0;
				RegisteredTextures[RegisteredTextureCount].Type = TextureType.Unknown;
				RegisteredTextures[RegisteredTextureCount].Status = TextureStatus.NotLoaded;
				RegisteredTextures[RegisteredTextureCount].OpenGlTextureIndex = 0;
				RegisteredTextureCount++;
			}
		}
		
		// find texture
		/// <summary>Finds a texture of a given file name and load options, receives the texture index in an output parameter, and returns the success of the operation.</summary>
		/// <param name="origin">The texture origin to search for.</param>
		/// <param name="parameters">The texture parameters to search for.</param>
		/// <param name="textureIndex">Receives the texture index if the texture was found.</param>
		/// <returns>A boolean indicating the success of the operation.</returns>
		private static bool FindTexture(OpenBveApi.General.Origin origin, OpenBveApi.Texture.TextureParameters parameters, out int textureIndex) {
			if (origin.Path != null) {
				for (int i = 0; i < RegisteredTextureCount; i++) {
					if (RegisteredTextures[i] != null && RegisteredTextures[i].Origin.Path != null) {
						if (OpenBveApi.General.Origin.Equals(Interfaces.Host10, RegisteredTextures[i].Origin, origin)) {
							if (RegisteredTextures[i].Parameters == parameters) {
								textureIndex = i;
								return true;
							}
						}
					}
				}
			}
			textureIndex = -1;
			return false;
		}

		// use texture
		internal static int UseTexture(int TextureIndex) {
			switch(RegisteredTextures[TextureIndex].Status) {
				case TextureStatus.NotLoaded:
					// not loaded
					LoadTextureRawData(TextureIndex);
					SubmitRawDataToOpenGl(TextureIndex);
					return RegisteredTextures[TextureIndex].OpenGlTextureIndex;
				case TextureStatus.Scheduled:
					// scheduled - wait for the background worker to complete loading
					while (RegisteredTextures[TextureIndex].Status == TextureStatus.Scheduled) {
						System.Threading.Thread.Sleep(0);
					}
					SubmitRawDataToOpenGl(TextureIndex);
					return RegisteredTextures[TextureIndex].OpenGlTextureIndex;
				case TextureStatus.Finalizing:
					// finalizing
					SubmitRawDataToOpenGl(TextureIndex);
					return RegisteredTextures[TextureIndex].OpenGlTextureIndex;
				case TextureStatus.Loaded:
					// loaded
					return RegisteredTextures[TextureIndex].OpenGlTextureIndex;
					break;
				default:
					// invalid texture status
					throw new InvalidOperationException();
			}
		}
		
		// load texture raw data
		/// <summary>Loads the raw data of a registered texture. The texture status must be NeverLoaded, Queried or Unloaded.</summary>
		/// <param name="TextureIndex">The index to the registered texture.</param>
		/// <exception cref="InvalidOperationException">Raised when the status of the registered texture is not NeverLoaded, Queried or Unloaded.</exception>
		/// <remarks>When this operation completes, the status of the registered texture is set to Finalizing.</remarks>
		internal static void LoadTextureRawData(int TextureIndex) {
			if (
				RegisteredTextures[TextureIndex].Status == TextureStatus.NotLoaded |
				RegisteredTextures[TextureIndex].Status == TextureStatus.Scheduled
			) {
				// load texture
				OpenBveApi.General.Origin origin = RegisteredTextures[TextureIndex].Origin;
				OpenBveApi.Texture.TextureParameters parameters = RegisteredTextures[TextureIndex].Parameters;
				OpenBveApi.Texture.TextureData texture;
				Interfaces.Host10.LoadTexture(origin, out texture);
				// convert to 8 bits per channel
				byte[] raw;
				ConvertTo8BitsPerChannel(texture, out raw);
				int width = texture.Format.Width;
				int height = texture.Format.Height;
				// extract clip
				if (parameters.ClipRegion != null) {
					ExtractClipRegion(ref width, ref height, ref raw, parameters.ClipRegion);
				}
				// remove transparent color
				if (parameters.TransparentColor.Assigned) {
					EliminateTransparentColor(width, height, ref raw, parameters.TransparentColor);
				}
				// convert to power-of-two
				ConvertToPowerOfTwoSize(ref width, ref height, ref raw);
				// store data
				RegisteredTextures[TextureIndex].RawWidth = width;
				RegisteredTextures[TextureIndex].RawHeight = height;
				RegisteredTextures[TextureIndex].RawData = raw;
				RegisteredTextures[TextureIndex].Status = TextureStatus.Finalizing;
			} else {
				// invalid texture status
				throw new InvalidOperationException();
			}
		}
		
		// submit raw data to opengl
		/// <summary>Submits the raw data of a registered texture to OpenGL. The texture status must be Finalizing.</summary>
		/// <param name="TextureIndex">The index to the registered texture.</param>
		/// <exception cref="InvalidOperationException">Raised when the status of the registered texture is not Finalizing.</exception>
		/// <remarks>When this operation completes, the status of the registered texture is set to Loaded.</remarks>
		internal static void SubmitRawDataToOpenGl(int TextureIndex) {
			int[] names = new int[1];
			Gl.glGenTextures(1, names);
			RegisteredTextures[TextureIndex].OpenGlTextureIndex = names[0];
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, RegisteredTextures[TextureIndex].OpenGlTextureIndex);
			
			// filter
			// TODO: Implement this.

//			switch (Options.CurrentOptions.TextureInterpolationMode) {
//				case Options.TextureInterpolationMode.NearestNeighbor:
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
//					break;
//				case Options.TextureInterpolationMode.NearestNeighborMipmapping:
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_NEAREST);
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
//					break;
//				case Options.TextureInterpolationMode.Bilinear:
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
//					break;
//				case Options.TextureInterpolationMode.BilinearMipmapping:
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_LINEAR);
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
//					break;
//				case Options.TextureInterpolationMode.Trilinear:
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
//					break;
//				default:
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
//					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
//					break;
//			}
			Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
			Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
			
			// wrap mode
			if (RegisteredTextures[TextureIndex].Parameters.HorizontalWrapMode == OpenBveApi.Texture.TextureWrapMode.ClampToEdge) {
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
			} else {
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
			}
			if (RegisteredTextures[TextureIndex].Parameters.VerticalWrapMode == OpenBveApi.Texture.TextureWrapMode.ClampToEdge) {
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
			} else {
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
			}
			// submit
			Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_GENERATE_MIPMAP, Gl.GL_TRUE);
			int width = RegisteredTextures[TextureIndex].RawWidth;
			int height = RegisteredTextures[TextureIndex].RawHeight;
			byte[] raw = RegisteredTextures[TextureIndex].RawData;
			Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA8, width, height, 0, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, raw);
		}
		
		// deinitializes
		internal static void Deinitialize() {
			for (int i = 0; i < RegisteredTextures.Length; i++) {
				UnregisterTexture(i);
			}
		}
		
		// unregister texture
		internal static void UnregisterTexture(int TextureIndex) {
			if (RegisteredTextures[TextureIndex] != null) {
				if (RegisteredTextures[TextureIndex].Status == TextureStatus.Loaded) {
					int index = (int)RegisteredTextures[TextureIndex].OpenGlTextureIndex;
					Gl.glDeleteTextures(1, new int[] { index });
				}
				RegisteredTextures[TextureIndex] = null;
			}
		}

	}
}