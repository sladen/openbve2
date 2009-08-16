using System;
using System.Text;
using OpenBveApi;

namespace Plugin {

	// host interface
	internal static class Interfaces {
		internal static IHost10 Host = null;
	}
	
	// plugin interface
	/// <summary>Exposes version 1.0 of the interface to be implemented by a plugin.</summary>
	public class Plugin : IPlugin10 {

		// --- general ---
		
		/// <summary>Is called when the plugin is loaded.</summary>
		/// <param name="hosts">A list of versions of the host interface as used for callbacks.</param>
		/// <returns>A boolean indicating whether the plugin was successfully loaded.</returns>
		/// <remarks>A plugin should make use of the smallest version the host interface provides as possible. If the plugin expects a certain version that is not supplied by the host application, this operation should return as unsuccessful.</remarks>
		public bool Load(IHost[] hosts) {
			foreach (IHost host in hosts) {
				IHost10 api = host as IHost10;
				if (api != null) {
					Interfaces.Host = api;
					return true;
				}
			}
			return false;
		}
		
		/// <summary>Is called when the plugin is unloaded.</summary>
		public void Unload() { }
		
		
		// --- textures ---
		/// <summary>Returns whether the plugin is capable of loading textures.</summary>
		/// <returns>A boolean indicating whether the plugin is capable of loading textures.</returns>
		public bool CanLoadTextures() {
			return false;
		}
		
		/// <summary>Returns whether the plugin is capable of loading the specified texture.</summary>
		/// <param name="type">The type of the path, i.e. a file or a folder.</param>
		/// <param name="path">The absolute path of the file or folder where the texture is stored.</param>
		/// <param name="encoding">The suggested encoding in case the texture format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <returns>The priority at which the plugin supports loading the specified texture.</returns>
		public General.Priority CanLoadTexture(Path.PathType type, string path, Encoding encoding, object data) {
			return General.Priority.NotCapable;
		}
		
		/// <summary>Loads a texture into an output parameter and returns the success of the operation.</summary>
		/// <param name="type">The type of the path, i.e. a file or a folder.</param>
		/// <param name="path">The absolute path of the file or folder where the texture is stored.</param>
		/// <param name="encoding">The suggested encoding in case the texture format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>The success of the operation.</returns>
		public General.Result LoadTexture(Path.PathType type, string path, Encoding encoding, object data, out Texture.TextureData texture) {
			texture = null;
			return General.Result.NotSupported;
		}
		
		
		// --- objects ---
		
		/// <summary>Returns whether the plugin is capable of loading objects.</summary>
		/// <returns>A boolean indicating whether the plugin is capable of loading objects.</returns>
		public bool CanLoadObjects() {
			return true;
		}
		
		/// <summary>Returns whether the plugin is capable of loading the specified object.</summary>
		/// <param name="type">The type of the path, i.e. a file or a folder.</param>
		/// <param name="path">The absolute path of the file or folder where the object is stored.</param>
		/// <param name="encoding">The suggested encoding in case the object format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <returns>The priority at which the plugin supports loading the specified object.</returns>
		public General.Priority CanLoadObject(Path.PathType type, string path, Encoding encoding, object data) {
			if (type != Path.PathType.File) {
				return General.Priority.NotCapable;
			} else {
				string extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
				switch (extension) {
					case ".b3d":
					case ".csv":
						return General.Priority.Normal;
					default:
						return General.Priority.NotCapable;
				}
			}
		}
		
		/// <summary>Loads an object into an output parameter and returns the success of the operation.</summary>
		/// <param name="type">The type of the path, i.e. a file or a folder.</param>
		/// <param name="path">The absolute path of the file or folder where the object is stored.</param>
		/// <param name="encoding">The suggested encoding in case the object format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <param name="obj">Receives the object.</param>
		/// <returns>The success of the operation.</returns>
		public General.Result LoadObject(Path.PathType type, string path, Encoding encoding, object data, out Geometry.GenericObject obj) {
			OpenBveApi.Geometry.FaceVertexMesh mesh;
			OpenBveApi.General.Result result = B3dCsvParser.LoadFromFile(path, encoding, out mesh);
			obj = (Geometry.GenericObject)mesh;
			return result;
		}
		
		
		// --- sounds ---
		
		/// <summary>Returns whether the plugin is capable of loading sounds.</summary>
		/// <returns>A boolean indicating whether the plugin is capable of loading sounds.</returns>
		public bool CanLoadSounds() {
			return false;
		}
		
		/// <summary>Returns whether the plugin is capable of loading the specified sound.</summary>
		/// <param name="type">The type of the path, i.e. a file or a folder.</param>
		/// <param name="path">The absolute path of the file or folder where the sound is stored.</param>
		/// <param name="encoding">The suggested encoding in case the sound format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <returns>The priority at which the plugin supports loading the specified sound.</returns>
		public General.Priority CanLoadSound(Path.PathType type, string path, Encoding encoding, object data) {
			return General.Priority.NotCapable;
		}
		
		/// <summary>Loads a sound into an output parameter and returns the success of the operation.</summary>
		/// <param name="type">The type of the path, i.e. a file or a folder.</param>
		/// <param name="path">The absolute path of the file or folder where the sound is stored.</param>
		/// <param name="encoding">The suggested encoding in case the sound format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>The success of the operation.</returns>
		public General.Result LoadSound(Path.PathType type, string path, Encoding encoding, object data, out Sound.SoundData sound) {
			sound = null;
			return General.Result.NotSupported;
		}
	}
	
}