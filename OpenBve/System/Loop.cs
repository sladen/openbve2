using System;
using Tao.Sdl;

namespace OpenBve {
	internal static class Loop {
		
		// members
		private static bool ExitLoop = false;
		
		// enter
		internal static void Enter() {
			int frames = 0;
			Timer.GetElapsedTime();
			while (!ExitLoop) {
				Renderer.Render();
				HandleEvents();
				frames++;
			}
			double fps = (double)frames / Timer.GetElapsedTime();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("fps: " + fps.ToString("0.00"));
			Console.ForegroundColor = ConsoleColor.Gray;
		}
		
		// leave
		internal static void Leave() {
			ExitLoop = true;
		}
		
		// handle events
		private static void HandleEvents() {
			Sdl.SDL_Event e;
			while (Sdl.SDL_PollEvent(out e) != 0) {
				switch(e.type) {
					case Sdl.SDL_QUIT:
						// quit
						ExitLoop = true;
						return;
					case Sdl.SDL_VIDEORESIZE:
						// video resize
						Window.ResizeNotify(e.resize.w, e.resize.h);
						break;
					case Sdl.SDL_KEYDOWN:
						// key down
						ExitLoop = true;
						break;
					case Sdl.SDL_KEYUP:
						// key up
						break;
					case Sdl.SDL_JOYBUTTONDOWN:
						// joystick button down
						break;
					case Sdl.SDL_JOYBUTTONUP:
						// joystick button up
						break;
					case Sdl.SDL_JOYHATMOTION:
						// joystick hat motion
						break;
					case Sdl.SDL_JOYAXISMOTION:
						// joystick axis motion
						break;
					case Sdl.SDL_MOUSEBUTTONDOWN:
						// mouse button down
						break;
					case Sdl.SDL_MOUSEBUTTONUP:
						// mouse button up
						break;
				}
			}
			
		}
	}
}