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
				if (host is IHost10) {
					Interfaces.Host = (IHost10)host;
					return true;
				}
			}
			return false;
		}
		
		/// <summary>Is called when the plugin is unloaded.</summary>
		public void Unload() { }
		
		
		// --- textures (loading) ---
		
		/// <summary>Returns whether the plugin is capable of loading textures.</summary>
		/// <returns>A boolean indicating whether the plugin is capable of loading textures.</returns>
		public bool CanLoadTextures() {
			return true;
		}
		
		/// <summary>Returns whether the plugin is capable of loading the specified texture.</summary>
		/// <param name="path">The file or folder where the texture is stored.</param>
		/// <param name="encoding">The suggested encoding in case the texture format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <returns>The priority at which the plugin supports loading the specified texture.</returns>
		public General.Priority CanLoadTexture(Path.PathReference path, Encoding encoding, object data) {
			if (path is Path.FileReference) {
				string fileName = ((Path.FileReference)path).Path;
				if (
					fileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
					fileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
					fileName.EndsWith(".exig", StringComparison.OrdinalIgnoreCase) ||
					fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
					fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
					fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
					fileName.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
					fileName.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase)
				) {
					return General.Priority.Normal;
				} else {
					return General.Priority.NotCapable;
				}
			} else {
				return General.Priority.NotCapable;
			}
		}
		
		/// <summary>Loads a texture into an output parameter and returns the success of the operation.</summary>
		/// <param name="path">The file or folder where the texture is stored.</param>
		/// <param name="encoding">The suggested encoding in case the texture format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>The success of the operation.</returns>
		public General.Result LoadTexture(Path.PathReference path, Encoding encoding, object data, out Texture.TextureData texture) {
			#if !DEBUG
			try {
				#endif
				string fileName = ((Path.FileReference)path).Path;
				if (System.IO.File.Exists(fileName)) {
					return Loader.LoadTexture(fileName, out texture);
				} else {
					texture = null;
					return OpenBveApi.General.Result.FileNotFound;
				}
				#if !DEBUG
			} catch {
				texture = null;
				return OpenBveApi.General.Result.InternalError;
			}
			#endif
		}
		
		
		// --- objects (loading) ---
		
		/// <summary>Returns whether the plugin is capable of loading objects.</summary>
		/// <returns>A boolean indicating whether the plugin is capable of loading objects.</returns>
		public bool CanLoadObjects() {
			return false;
		}
		
		/// <summary>Returns whether the plugin is capable of loading the specified object.</summary>
		/// <param name="path">The file or folder where the object is stored.</param>
		/// <param name="encoding">The suggested encoding in case the object format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <returns>The priority at which the plugin supports loading the specified object.</returns>
		public General.Priority CanLoadObject(Path.PathReference path, Encoding encoding, object data) {
			return General.Priority.NotCapable;
		}
		
		/// <summary>Loads an object into an output parameter and returns the success of the operation.</summary>
		/// <param name="path">The file or folder where the object is stored.</param>
		/// <param name="encoding">The suggested encoding in case the object format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <param name="obj">Receives the object.</param>
		/// <returns>The success of the operation.</returns>
		public General.Result LoadObject(Path.PathReference path, Encoding encoding, object data, out Geometry.GenericObject obj) {
			obj = null;
			return General.Result.NotSupported;
		}
		
		
		// --- sounds (loading) ---
		
		/// <summary>Returns whether the plugin is capable of loading sounds.</summary>
		/// <returns>A boolean indicating whether the plugin is capable of loading sounds.</returns>
		public bool CanLoadSounds() {
			return false;
		}
		
		/// <summary>Returns whether the plugin is capable of loading the specified sound.</summary>
		/// <param name="path">The file or folder where the sound is stored.</param>
		/// <param name="encoding">The suggested encoding in case the sound format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <returns>The priority at which the plugin supports loading the specified sound.</returns>
		public General.Priority CanLoadSound(Path.PathReference path, Encoding encoding, object data) {
			return General.Priority.NotCapable;
		}
		
		/// <summary>Loads a sound into an output parameter and returns the success of the operation.</summary>
		/// <param name="path">The file or folder where the sound is stored.</param>
		/// <param name="encoding">The suggested encoding in case the sound format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>The success of the operation.</returns>
		public General.Result LoadSound(Path.PathReference path, Encoding encoding, object data, out Sound.SoundData sound) {
			sound = null;
			return General.Result.NotSupported;
		}
		
		
		// --- routes (loading) ---
		
		/// <summary>Returns whether the plugin is capable of loading routes.</summary>
		/// <returns>A boolean indicating whether the plugin is capable of loading routes.</returns>
		public bool CanLoadRoutes() {
			return false;
		}
		
		/// <summary>Returns whether the plugin is capable of loading the specified route.</summary>
		/// <param name="path">The file or folder where the route is stored.</param>
		/// <param name="encoding">The suggested encoding in case the route format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <returns>The priority at which the plugin supports loading the specified route.</returns>
		public General.Priority CanLoadRoute(Path.PathReference path, Encoding encoding, object data) {
			return General.Priority.NotCapable;
		}
		
		/// <summary>Loads a route into an output parameter and returns the success of the operation.</summary>
		/// <param name="path">The file or folder where the route is stored.</param>
		/// <param name="encoding">The suggested encoding in case the route format does not mandate a specific encoding.</param>
		/// <param name="data">Optional data passed from another plugin. If you access this field, you must check the type before casting to that type.</param>
		/// <param name="route">Receives the route.</param>
		/// <returns>The success of the operation.</returns>
		public General.Result LoadRoute(Path.PathReference path, Encoding encoding, object data, out Route.RouteData route) {
			route = null;
			return General.Result.NotSupported;
		}
		
		
		// --- runtime ---

		/// <summary>Queries data from this runtime plugin.</summary>
		/// <param name="contentType">The type of content to query.</param>
		/// <param name="contentData">Receives the queried content.</param>
		/// <returns>The success of the operation.</returns>
		public General.Result QueryPluginData(int contentType, out object contentData) {
			contentData = null;
			return General.Result.NotSupported;
		}
		
		/// <summary>Submits data to this runtime plugin.</summary>
		/// <param name="contentType">The type of content to submit.</param>
		/// <param name="contentData">The data to submit.</param>
		/// <returns>The success of the operation.</returns>
		public General.Result SubmitPluginData(int contentType, object contentData) {
			return General.Result.NotSupported;
		}
		
	}
}