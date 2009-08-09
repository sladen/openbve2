using System;
using System.Drawing;
using System.Drawing.Imaging;
using Tao.OpenGl;
using Tao.Sdl;

namespace OpenBve {
	
	internal static class Textures {
		
		// texture type
		internal enum TextureType {
			Unknown = 0,
			Opaque = 1,
			TransparentColor = 2,
			Alpha = 3
		}
		
		// texture status
		internal enum TextureStatus {
			/// <summary>Indicates that the texture has never been loaded and the final dimensions and type are unknown.</summary>
			NeverLoaded = 0,
			/// <summary>Indicates that the texture has been queried for loading, but the background worker has not yet finished loading the raw data.</summary>
			Queried = 1,
			/// <summary>Indicates that the background worker has finished loading the raw data, but the texture has not yet been submitted to OpenGL.</summary>
			Finalizing = 2,
			/// <summary>Indicates that the texture has been submitted to OpenGL and that the OpenGlTextureIndex is valid.</summary>
			Loaded = 3,
			/// <summary>Indicates that the texture has been unloaded. The difference to NeverLoaded is that the dimensions and the type of the texture are known.</summary>
			Unloaded = 4
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
			/// <summary>The final width of the extracted portion of the texture after converting to a power of two.</summary>
			internal int FinalWidth;
			/// <summary>The final height of the extracted portion of the texture after converting to a power of two.</summary>
			internal int FinalHeight;
			/// <summary>The texture type of the extracted portion of the texture.</summary>
			internal TextureType Type;
			/// <summary>The current texture status.</summary>
			internal TextureStatus Status;
			/// <summary>The positive OpenGL texture index when the texture is loaded, or 0 if invalid.</summary>
			internal int? OpenGlTextureIndex;
		}
		
		// members
		internal static Texture[] RegisteredTextures = new Texture[16];
		internal static int RegisteredTextureCount = 0;
		
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
				RegisteredTextures[RegisteredTextureCount].FinalWidth = 0;
				RegisteredTextures[RegisteredTextureCount].FinalHeight = 0;
				RegisteredTextures[RegisteredTextureCount].Type = TextureType.Unknown;
				RegisteredTextures[RegisteredTextureCount].Status = TextureStatus.NeverLoaded;
				RegisteredTextures[RegisteredTextureCount].OpenGlTextureIndex = null;
				RegisteredTextureCount++;
			}
		}
		
		// unregister texture
		internal static void UnregisterTexture(int TextureIndex) {
			if (RegisteredTextures[TextureIndex] != null) {
				if (RegisteredTextures[TextureIndex].Status == TextureStatus.Loaded) {
					if (RegisteredTextures[TextureIndex].OpenGlTextureIndex != null) {
						int index = (int)RegisteredTextures[TextureIndex].OpenGlTextureIndex;
						Gl.glDeleteTextures(1, new int[] { index });
					}
				}
				RegisteredTextures[TextureIndex] = null;
			}
		}
		
		// deinitializes
		internal static void Deinitialize() {
			for (int i = 0; i < RegisteredTextures.Length; i++) {
				UnregisterTexture(i);
			}
		}

	}
}