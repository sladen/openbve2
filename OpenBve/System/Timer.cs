using System;
using Tao.Sdl;

namespace OpenBve {
	public static class Timer {
		
		// members
		private static double SdlTime;
		
		// initialize
		internal static void Initialize() {
			SdlTime = 0.001 * (double)Sdl.SDL_GetTicks();
		}
		
		// get elapsed time
		internal static double GetElapsedTime() {
			double time = 0.001 * (double)Sdl.SDL_GetTicks();
			double delta = time - SdlTime;
			SdlTime = time;
			return delta;
		}
		
	}
}