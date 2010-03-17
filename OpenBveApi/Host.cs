using System;
using System.Text;

namespace OpenBveApi {
	
	
	/// <summary>Exposes the interface to be implemented by the host application.</summary>
	public interface IHost { }
	
	
	/// <summary>Exposes version 1.0 of the interface to be implemented by the host application.</summary>
	public interface IHost10 : IHost {

		
		// --- general ---
		
		/// <summary>Reports a problem to the host application.</summary>
		/// <param name="type">The type of problem to be reported.</param>
		/// <param name="keyValuePairs">A list of key-value pairs containing information about the problem.</param>
		[Obsolete]
		void Report(General.ReportType type, params General.ReportKeyValuePair[] keyValuePairs);
		
		/// <summary>Reports a problem to the host application.</summary>
		/// <param name="data">Information about the problem, e.g. the location where it occured, and a description.</param>
		void Report(params General.ReportData[] data);

		
		// --- textures (loading) ---
		
		/// <summary>Loads a texture suitable for post-processing.</summary>
		/// <param name="origin">The origin of the texture which includes a valid path.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>The success of the operation.</returns>
		General.Result LoadTexture(General.Origin origin, out Texture.TextureData texture);
		
		/// <summary>Registers a texture with the host application.</summary>
		/// <param name="origin">The origin of the texture which includes a valid path.</param>
		/// <param name="parameters">The parameters for the texture.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>The success of the operation.</returns>
		General.Result RegisterTexture(General.Origin origin, Texture.TextureParameters parameters, out Texture.TextureHandle handle);
		
		/// <summary>Registers a texture with the host application.</summary>
		/// <param name="texture">The texture to register.</param>
		/// <param name="parameters">The parameters for the texture.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>The success of the operation.</returns>
		General.Result RegisterTexture(Texture.TextureData texture, Texture.TextureParameters parameters, out Texture.TextureHandle handle);
		
		
		// --- objects (loading) ---
		
		/// <summary>Loads an object from a file, suitable for post-processing.</summary>
		/// <param name="origin">The origin of the object which includes a valid path.</param>
		/// <param name="obj">Receives the object.</param>
		/// <returns>The success of the operation.</returns>
		General.Result LoadObject(General.Origin origin, out Geometry.GenericObject obj);
		
		/// <summary>Registers an object with the host application.</summary>
		/// <param name="obj">The object to register.</param>
		/// <param name="handle">Receives a handle to the object.</param>
		/// <returns>The success of the operation.</returns>
		General.Result RegisterObject(Geometry.GenericObject obj, out Geometry.ObjectHandle handle);
		
		/// <summary>Creates a new instance of an object at a specified position with a specified orietation.</summary>
		/// <param name="handle">The handle to the object.</param>
		/// <param name="position">The position of the object.</param>
		/// <param name="orientation">The orientation of the object.</param>
		/// <returns>The success of the operation.</returns>
		General.Result CreateObject(Geometry.ObjectHandle handle, Math.Vector3 position, Math.Orientation3 orientation);

		
		// --- sound (loading) ---
		
		/// <summary>Loads a sound from a file, suitable for post-processing.</summary>
		/// <param name="origin">The origin of the sound which includes a valid path.</param>
		/// <param name="sound">Receives the sound data.</param>
		/// <returns>The success of the operation.</returns>
		General.Result LoadSound(General.Origin origin, out Sound.SoundData sound);
		
		/// <summary>Registers a sound with the host application.</summary>
		/// <param name="origin">The origin of the sound which includes a valid path.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>The success of the operation.</returns>
		General.Result RegisterSound(General.Origin origin, out Sound.SoundBufferHandle handle);
		
		/// <summary>Registers a sound with the host application.</summary>
		/// <param name="sound">The sound data to register.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>The success of the operation.</returns>
		General.Result RegisterSound(Sound.SoundData sound, out Sound.SoundBufferHandle handle);
		
		
		// --- sound (runtime) ---
		
		/// <summary>Plays the specified sound buffer and returns a handle by which the generated sound source can be identified.</summary>
		/// <param name="bufferHandle">A handle to the sound buffer.</param>
		/// <param name="position">The absolute position at which to play the sound.</param>
		/// <param name="velocity">The velocity vector at which the sound travels.</param>
		/// <param name="pitch">The pitch of the sound, where 1 represents nominal pitch.</param>
		/// <param name="volume">The volume of the sound, where 1 represents nominal volume.</param>
		/// <param name="looped">A boolean indicating whether to play the sound in an endless loop.</param>
		/// <param name="sourceHandle">Receives a handle to the sound source.</param>
		/// <returns>The success of the operation.</returns>
		General.Result PlaySound(Sound.SoundBufferHandle bufferHandle, Math.Vector3 position, Math.Vector3 velocity, double pitch, double volume, bool looped, out Sound.SoundSourceHandle sourceHandle);
		
		/// <summary>Updates an already playing sound.</summary>
		/// <param name="handle">A handle to the sound source.</param>
		/// <param name="position">The absolute position at which to play the sound.</param>
		/// <param name="velocity">The velocity vector at which the sound travels.</param>
		/// <param name="pitch">The pitch of the sound, where 1 represents nominal pitch.</param>
		/// <param name="volume">The volume of the sound, where 1 represents nominal volume.</param>
		/// <returns>The success of the operation.</returns>
		General.Result UpdateSound(Sound.SoundSourceHandle handle, Math.Vector3 position, Math.Vector3 velocity, double pitch, double volume);
		
		/// <summary>Stops playing the specified sound source.</summary>
		/// <param name="handle">The sound source to stop playing.</param>
		General.Result StopSound(ref Sound.SoundSourceHandle handle);
		
		
		// --- plugins (runtime) ---

		/// <summary>Queries data from another runtime plugin.</summary>
		/// <param name="pluginType">The plugin to query data from.</param>
		/// <param name="contentType">The type of content to query.</param>
		/// <param name="contentData">Receives the queried content.</param>
		/// <returns>The success of the operation.</returns>
		General.Result QueryPluginData(int pluginType, int contentType, out object contentData);
		
		/// <summary>Submits data to another runtime plugin.</summary>
		/// <param name="pluginType">The plugin to submit data to.</param>
		/// <param name="contentType">The type of content to submit.</param>
		/// <param name="contentData">The data to submit.</param>
		/// <returns>The success of the operation.</returns>
		General.Result SubmitPluginData(int pluginType, int contentType, object contentData);
		
		
	}
}