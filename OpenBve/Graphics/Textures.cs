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
		 * means that this texture manager only stores and manages each unique texture once.
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
		 * TODO: The XML documentation in this file is still incomplete.
		 * 
		 * */
		
		// api handle
		/// <summary>Represents a handle to a texture.</summary>
		/// <remarks>This class is used for interaction with the API.</remarks>
		internal class ApiHandle : OpenBveApi.Texture.TextureHandle {
			/// <summary>The index to the texture.</summary>
			internal int TextureIndex;
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="libraryIndex">The index to the texture.</param>
			internal ApiHandle(int textureIndex) {
				this.TextureIndex = textureIndex;
			}
		}
		
		// texture type
		internal enum TextureType {
			Unknown = 0,
			Opaque = 1,
			Transparent = 2,
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
		/// <param name="textureIndex">Receives the managed texture index.</param>
		internal static void RegisterTexture(OpenBveApi.General.Origin origin, OpenBveApi.Texture.TextureParameters parameters, out int textureIndex) {
			int index;
			if (FindTexture(origin, parameters, out index)) {
				textureIndex = index;
			} else {
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
		
		// register and load texture
		/// <summary>Registers and loads a texture directly from raw data.</summary>
		/// <param name="raw">The width of the texture.</param>
		/// <param name="raw">The height of the texture.</param>
		/// <param name="raw">The raw texture data.</param>
		/// <param name="textureIndex">Receives the managed texture index.</param>
		/// <param name="openGlTextureIndex">Receives the OpenGL texture index.</param>
		internal static void RegisterAndLoadTexture(int width, int height, byte[] raw, out int textureIndex, out int openGlTextureIndex) {
			if (RegisteredTextures.Length == RegisteredTextureCount) {
				Array.Resize<Texture>(ref RegisteredTextures, RegisteredTextures.Length << 1);
			}
			textureIndex = RegisteredTextureCount;
			RegisteredTextures[RegisteredTextureCount] = new Texture();
			RegisteredTextures[RegisteredTextureCount].Origin = OpenBveApi.General.Origin.Empty;
			RegisteredTextures[RegisteredTextureCount].Parameters = OpenBveApi.Texture.TextureParameters.ClampToEdge;
			RegisteredTextures[RegisteredTextureCount].RawData = raw;
			RegisteredTextures[RegisteredTextureCount].RawWidth = width;
			RegisteredTextures[RegisteredTextureCount].RawHeight = height;
			RegisteredTextures[RegisteredTextureCount].Type = TextureType.Alpha;
			RegisteredTextures[RegisteredTextureCount].Status = TextureStatus.Finalizing;
			RegisteredTextures[RegisteredTextureCount].OpenGlTextureIndex = 0;
			SubmitRawDataToOpenGl(RegisteredTextureCount, false);
			RegisteredTextures[RegisteredTextureCount].Status = TextureStatus.Loaded;
			textureIndex = RegisteredTextureCount;
			openGlTextureIndex = RegisteredTextures[RegisteredTextureCount].OpenGlTextureIndex;
			RegisteredTextureCount++;
		}
		
		// find texture
		/// <summary>Finds a texture of a specified file name and load options, receives the texture index in an output parameter, and returns the success of the operation.</summary>
		/// <param name="origin">The texture origin to search for.</param>
		/// <param name="parameters">The texture parameters to search for.</param>
		/// <param name="textureIndex">Receives the texture index if the texture was found.</param>
		/// <returns>A boolean indicating the success of the operation.</returns>
		private static bool FindTexture(OpenBveApi.General.Origin origin, OpenBveApi.Texture.TextureParameters parameters, out int textureIndex) {
			if (origin.Path != null) {
				for (int i = 0; i < RegisteredTextureCount; i++) {
					if (RegisteredTextures[i] != null && RegisteredTextures[i].Origin.Path != null) {
						if (OpenBveApi.General.Origin.Equals(RegisteredTextures[i].Origin, origin)) {
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

		// load texture
		/// <summary>Loads a registered texture immediately or via the background worker.</summary>
		/// <param name="textureIndex">The index of the registered texture.</param>
		/// <param name="loadImmediately">Whether to load the texture immediately or via the background worker.</param>
		internal static void LoadTexture(int textureIndex, bool loadImmediately) {
			if (loadImmediately) {
				switch(RegisteredTextures[textureIndex].Status) {
					case TextureStatus.NotLoaded:
						// not loaded
						LoadTextureRawData(textureIndex);
						SubmitRawDataToOpenGl(textureIndex, true);
						RegisteredTextures[textureIndex].Status = TextureStatus.Loaded;
						break;
					case TextureStatus.Scheduled:
						// scheduled - wait for the background worker to complete loading
						while (RegisteredTextures[textureIndex].Status != TextureStatus.Finalizing) {
							System.Threading.Thread.Sleep(0);
						}
						SubmitRawDataToOpenGl(textureIndex, true);
						break;
					case TextureStatus.Finalizing:
						// finalizing
						SubmitRawDataToOpenGl(textureIndex, true);
						RegisteredTextures[textureIndex].Status = TextureStatus.Loaded;
						break;
				}
			} else {
				throw new NotImplementedException("This operation cannot be performed because the background worker is not yet implemented.");
				/*
					if (RegisteredTextures[textureIndex].Status == TextureStatus.NotLoaded) {
						// let the background worker load the texture
						RegisteredTextures[textureIndex].Status = TextureStatus.Scheduled;
					}
				 */
			}
		}
		
		// get opengl texture index
		/// <summary>Gets the OpenGL texture index for a registered texture provided that the texture has been loaded into OpenGL.</summary>
		/// <param name="textureIndex">The index of the registered texture.</param>
		/// <param name="openGlTextureIndex">Receives the OpenGL texture index.</param>
		/// <returns>A boolean indicating the success of the operation.</returns>
		internal static bool GetOpenGlTextureIndex(int textureIndex, out int openGlTextureIndex) {
			if (RegisteredTextures[textureIndex].Status == TextureStatus.Loaded) {
				openGlTextureIndex = RegisteredTextures[textureIndex].OpenGlTextureIndex;
				return true;
			} else {
				openGlTextureIndex = 0;
				return false;
			}
		}
		
		// load texture raw data
		/// <summary>Loads the raw data of a registered texture. The texture status must be NotLoaded or Scheduled.</summary>
		/// <param name="TextureIndex">The index to the registered texture.</param>
		/// <exception cref="InvalidOperationException">Raised when the status of the registered texture is neither NotLoaded nor Scheduled.</exception>
		/// <remarks>When this operation completes, the status of the registered texture is set to Finalizing.</remarks>
		internal static void LoadTextureRawData(int textureIndex) {
			if (
				RegisteredTextures[textureIndex].Status == TextureStatus.NotLoaded |
				RegisteredTextures[textureIndex].Status == TextureStatus.Scheduled
			) {
				// load texture
				OpenBveApi.General.Origin origin = RegisteredTextures[textureIndex].Origin;
				OpenBveApi.Texture.TextureParameters parameters = RegisteredTextures[textureIndex].Parameters;
				OpenBveApi.Texture.TextureData texture;
				if (Interfaces.Host10.LoadTexture(origin, out texture) == OpenBveApi.General.Result.Successful) {
					// convert to 8 bits per channel
					byte[] raw;
					ConvertTo8BitsPerChannel(texture, out raw);
					int width = texture.Format.Width;
					int height = texture.Format.Height;
					// extract clip
					if (parameters.ClipRegion != null) {
						ExtractClipRegion(ref width, ref height, ref raw, parameters.ClipRegion);
					}
					// eliminate transparent color
					EliminateTransparentColor(width, height, ref raw, parameters.TransparentColor);
					// convert to power-of-two
					ConvertToPowerOfTwoSize(ref width, ref height, ref raw);
					// determine texture type
					RegisteredTextures[textureIndex].Type = DetermineTextureType(width, height, raw);
					// store data
					RegisteredTextures[textureIndex].RawWidth = width;
					RegisteredTextures[textureIndex].RawHeight = height;
					RegisteredTextures[textureIndex].RawData = raw;
					RegisteredTextures[textureIndex].Status = TextureStatus.Finalizing;
				} else {
					// loading failed
					RegisteredTextures[textureIndex].Type = TextureType.Opaque;
					RegisteredTextures[textureIndex].RawWidth = 32;
					RegisteredTextures[textureIndex].RawHeight = 8;
					RegisteredTextures[textureIndex].RawData = new byte[] {
						/* white "error" on red background */
						0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF,
						0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF,
						0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF,
						0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF,
						0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF,
						0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF,
						0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF,
						0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF
					};
					RegisteredTextures[textureIndex].Status = TextureStatus.Finalizing;
				}
			} else {
				// invalid texture status
				throw new InvalidOperationException();
			}
		}
		
		// submit raw data to opengl
		/// <summary>Submits the raw data of a registered texture to OpenGL. The texture status must be Finalizing.</summary>
		/// <param name="textureIndex">The index to the registered texture.</param>
		/// <param name="respectQuality">Whether to respect the global texture quality setting. If set to False, bilinear filtering is used without mipmapping.</param>
		internal static void SubmitRawDataToOpenGl(int textureIndex, bool respectQuality) {
			int[] names = new int[1];
			Gl.glGenTextures(1, names);
			RegisteredTextures[textureIndex].OpenGlTextureIndex = names[0];
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, names[0]);
			if (respectQuality) {
				switch (Program.CurrentOptions.InterpolationMode) {
					case Options.TextureInterpolationMode.NearestNeighbor:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
						break;
					case Options.TextureInterpolationMode.NearestNeighborMipmapped:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_NEAREST);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
						break;
					case Options.TextureInterpolationMode.Bilinear:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
						break;
					case Options.TextureInterpolationMode.BilinearMipmapped:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
						break;
					case Options.TextureInterpolationMode.TrilinearMipmapped:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
						break;
					default:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
						break;
				}
			} else {
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
			}
			if (Program.CurrentOptions.InterpolationMode == Options.TextureInterpolationMode.AnisotropicFiltering & Program.AnisotropicMaximum != 0.0f) {
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAX_ANISOTROPY_EXT, Program.AnisotropicMaximum);
			}
			/*
			 * Set the wrap mode.
			 * */
			if (RegisteredTextures[textureIndex].Parameters.HorizontalWrapMode == OpenBveApi.Texture.TextureWrapMode.ClampToEdge) {
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
			} else {
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
			}
			if (RegisteredTextures[textureIndex].Parameters.VerticalWrapMode == OpenBveApi.Texture.TextureWrapMode.ClampToEdge) {
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
			} else {
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
			}
			/*
			 * Submit the texture to OpenGL.
			 * */
			if (respectQuality) {
				Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_GENERATE_MIPMAP, Gl.GL_TRUE);
			} else {
				Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_GENERATE_MIPMAP, Gl.GL_TRUE);
			}
			int width = RegisteredTextures[textureIndex].RawWidth;
			int height = RegisteredTextures[textureIndex].RawHeight;
			byte[] raw = RegisteredTextures[textureIndex].RawData;
			Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA8, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, raw);
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