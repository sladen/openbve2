using System;
using System.Drawing;
using System.Text;

namespace OpenBveApi {
	
	
	/// <summary>Exposes the interface to be implemented by the host application.</summary>
	public interface IHost {
	
		// --- general ---
		
		/// <summary>Reports a problem to the host application.</summary>
		/// <param name="type">The type of problem to be reported.</param>
		/// <param name="data">A list of key-value pairs containing information about the problem.</param>
		/// <remarks>Commonly used keys include "Source", "File", "Row", "Column", "Position" or "Text".</remarks>
		void Report(General.ReportType type, params General.KeyValuePair[] data);
		
		
		// --- path ---
		
		/// <summary>Resolves a file or folder reference into a platform-specific absolute path.</summary>
		/// <param name="reference">The file or folder reference to resolve.</param>
		/// <returns>The platform-specific absolute path.</returns>
		string Resolve(Path.PathReference reference);
	
	}
	
	
	/// <summary>Exposes version 1.0 of the interface to be implemented by the host application.</summary>
	public interface IHost10 : IHost {

		// --- textures ---
		
		/// <summary>Loads a texture suitable for post-processing.</summary>
		/// <param name="origin">The origin of the texture which includes a valid path.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>The success of the operation.</returns>
		General.Result LoadTexture(General.Origin origin, out Bitmap texture);
		
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
		General.Result RegisterTexture(Bitmap texture, Texture.TextureParameters parameters, out Texture.TextureHandle handle);
		
		
		// --- objects ---
		
		/// <summary>Loads an object from a file, suitable for post-processing.</summary>
		/// <param name="origin">The origin of the object which includes a valid path.</param>
		/// <param name="obj">Receives the object.</param>
		/// <returns>The success of the operation.</returns>
		General.Result LoadObject(General.Origin origin, out Geometry.GenericObject obj);
		
		
		// --- sound ---
		
		/// <summary>Loads a sound from a file, suitable for post-processing.</summary>
		/// <param name="origin">The origin of the sound which includes a valid path.</param>
		/// <param name="sound">Receives the sound data.</param>
		/// <returns>The success of the operation.</returns>
		General.Result LoadSound(General.Origin origin, out Sound.SoundData sound);
		
		/// <summary>Registers a sound with the host application.</summary>
		/// <param name="origin">The origin of the sound which includes a valid path.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>The success of the operation.</returns>
		General.Result RegisterSound(General.Origin origin, out Sound.SoundHandle handle);
		
		/// <summary>Registers a sound with the host application.</summary>
		/// <param name="sound">The sound data to register.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>The success of the operation.</returns>
		General.Result RegisterSound(Sound.SoundData sound, out Sound.SoundHandle handle);
		
	}
}