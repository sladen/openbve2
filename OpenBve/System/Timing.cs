using System;
using Tao.Sdl;

namespace OpenBve {
	/// <summary>Provides functions related to time.</summary>
	public static class Timing {
		
		
		// --- members ---
		
		/// <summary>The seconds since midnight in game time.</summary>
		internal static double SecondsSinceMidnight = 0.0;
		
		/// <summary>The last time obtained from SDL.</summary>
		private static double LastSdlTime = 0.0;

		
		// --- functions ---
		
		/// <summary>Initializes the timer.</summary>
		internal static void Initialize() {
			LastSdlTime = 0.001 * (double)Sdl.SDL_GetTicks();
		}
		
		/// <summary>Gets the time that elapsed since the last call to this function or to the Initialize function.</summary>
		/// <returns>The time that elapsed since the last call in seconds.</returns>
		internal static double GetElapsedTime() {
			double time = 0.001 * (double)Sdl.SDL_GetTicks();
			double delta = time - LastSdlTime;
			LastSdlTime = time;
			return delta >= 0.0 ? delta : 0.0;
		}
		
	}
}