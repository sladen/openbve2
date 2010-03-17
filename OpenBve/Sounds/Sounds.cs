using System;

namespace OpenBve {
	public static class Sounds {
		
		// api buffer handle
		/// <summary>Represents a handle to a sound buffer.</summary>
		/// <remarks>This class is used for interaction with the API.</remarks>
		internal class ApiBufferHandle : OpenBveApi.Sound.SoundBufferHandle {
			/// <summary>The index to the sound buffer.</summary>
			internal int SoundBufferIndex;
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="libraryIndex">The index to the sound buffer.</param>
			internal ApiBufferHandle(int soundBufferIndex) {
				this.SoundBufferIndex = soundBufferIndex;
			}
		}
		
	}
}
