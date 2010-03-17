using System;
using System.Globalization;
using Tao.Sdl;

namespace OpenBve {
	internal static class Loop {
		
		
		// TODO: This is a stub. Expand as necessary.
		
		
		
		// members
		private static bool ExitLoop = false;
		
		// enter
		internal static void Enter() {
			
			int totalFrames = 0;
			double totalTime = 0.0;
			int frames = 0;
			double time = 0.0;
			
			Timing.GetElapsedTime();
			
			while (!ExitLoop) {
				
				double elapsedTime = Timing.GetElapsedTime();
				totalTime += elapsedTime;
				totalFrames++;
				time += elapsedTime;
				frames++;
				if (time >= 1.0 & frames >= 1) {
					double fps = (double)frames / time;
					Windows.UpdateDebugText(fps.ToString("0.0") + " fps");
					time = 0.0;
					frames = 0;
				}
				Timing.SecondsSinceMidnight += 3600.0 * elapsedTime;
				
				Renderer.Render(elapsedTime);
				HandleEvents(elapsedTime);
				
				double factor = (QuickKeys.Shift ? 4.0 : 1.0) * (QuickKeys.Ctrl ? 16.0 : 1.0) * (QuickKeys.Alt ? 64.0 : 1.0);
				double moveFactor = 20.0 * factor * elapsedTime;
				double rotateFactor = 1.0 * elapsedTime;
				if (QuickKeys.A) {
					Camera.Position -= Camera.Orientation.X * moveFactor;
				}
				if (QuickKeys.D) {
					Camera.Position += Camera.Orientation.X * moveFactor;
				}
				if (QuickKeys.S) {
					Camera.Position -= Camera.Orientation.Z * moveFactor;
				}
				if (QuickKeys.W) {
					Camera.Position += Camera.Orientation.Z * moveFactor;
				}
				if (QuickKeys.PageUp) {
					Camera.Position += Camera.Orientation.Y * moveFactor;
				}
				if (QuickKeys.PageDown) {
					Camera.Position -= Camera.Orientation.Y * moveFactor;
				}
				if (QuickKeys.YawMinus) {
					Camera.Orientation.Rotate(OpenBveApi.Math.Vector3.Up, Math.Cos(-rotateFactor), Math.Sin(-rotateFactor));
				}
				if (QuickKeys.YawPlus) {
					Camera.Orientation.Rotate(OpenBveApi.Math.Vector3.Up, Math.Cos(rotateFactor), Math.Sin(rotateFactor));
				}
				if (QuickKeys.PitchPlus) {
					Camera.Orientation.RotateAroundXAxis(Math.Cos(-rotateFactor), Math.Sin(-rotateFactor));
				}
				if (QuickKeys.PitchMinus) {
					Camera.Orientation.RotateAroundXAxis(Math.Cos(rotateFactor), Math.Sin(rotateFactor));
				}
				if (QuickKeys.RollMinus) {
					Camera.Orientation.RotateAroundZAxis(Math.Cos(-rotateFactor), Math.Sin(-rotateFactor));
				}
				if (QuickKeys.RollPlus) {
					Camera.Orientation.RotateAroundZAxis(Math.Cos(rotateFactor), Math.Sin(rotateFactor));
				}
				
			}
			double totalFps = (double)totalFrames / totalTime;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("average fps: " + totalFps.ToString("0.00"));
			Console.ForegroundColor = ConsoleColor.Gray;
		}
		
		// leave
		internal static void Leave() {
			ExitLoop = true;
		}
		
		
		
		internal struct Keys {
			internal bool A;
			internal bool D;
			internal bool W;
			internal bool S;
			internal bool PageUp;
			internal bool PageDown;
			internal bool YawMinus;
			internal bool YawPlus;
			internal bool PitchMinus;
			internal bool PitchPlus;
			internal bool RollMinus;
			internal bool RollPlus;
			internal bool Shift;
			internal bool Ctrl;
			internal bool Alt;
		}
		internal static Keys QuickKeys;
		
		
		
		// handle events
		private static void HandleEvents(double elapsedTime) {
			Sdl.SDL_Event e;
			while (Sdl.SDL_PollEvent(out e) != 0) {
				switch(e.type) {
					case Sdl.SDL_QUIT:
						// quit
						ExitLoop = true;
						return;
					case Sdl.SDL_VIDEORESIZE:
						// video resize
						Screen.ResizeNotify(e.resize.w, e.resize.h);
						break;
					case Sdl.SDL_KEYDOWN:
						// key down
						switch ((int)e.key.keysym.sym) {
							case Sdl.SDLK_a:
								QuickKeys.A = true;
								break;
							case Sdl.SDLK_d:
								QuickKeys.D = true;
								break;
							case Sdl.SDLK_w:
								QuickKeys.W = true;
								break;
							case Sdl.SDLK_s:
								QuickKeys.S = true;
								break;
							case Sdl.SDLK_PAGEUP:
							case Sdl.SDLK_KP8:
								QuickKeys.PageUp = true;
								break;
							case Sdl.SDLK_PAGEDOWN:
							case Sdl.SDLK_KP2:
								QuickKeys.PageDown = true;
								break;
							case Sdl.SDLK_LEFT:
								QuickKeys.YawMinus = true;
								break;
							case Sdl.SDLK_RIGHT:
								QuickKeys.YawPlus = true;
								break;
							case Sdl.SDLK_UP:
								QuickKeys.PitchPlus = true;
								break;
							case Sdl.SDLK_DOWN:
								QuickKeys.PitchMinus = true;
								break;
							case Sdl.SDLK_KP_DIVIDE:
								QuickKeys.RollPlus = true;
								break;
							case Sdl.SDLK_KP_MULTIPLY:
								QuickKeys.RollMinus = true;
								break;
							case Sdl.SDLK_LSHIFT:
							case Sdl.SDLK_RSHIFT:
								QuickKeys.Shift = true;
								break;
							case Sdl.SDLK_LCTRL:
							case Sdl.SDLK_RCTRL:
								QuickKeys.Ctrl = true;
								break;
							case Sdl.SDLK_LALT:
							case Sdl.SDLK_RALT:
								QuickKeys.Alt = true;
								break;
							case Sdl.SDLK_ESCAPE:
								ExitLoop = true;
								break;
						}
						break;
					case Sdl.SDL_KEYUP:
						// key up
						switch ((int)e.key.keysym.sym) {
							case Sdl.SDLK_a:
								QuickKeys.A = false;
								break;
							case Sdl.SDLK_d:
								QuickKeys.D = false;
								break;
							case Sdl.SDLK_w:
								QuickKeys.W = false;
								break;
							case Sdl.SDLK_s:
								QuickKeys.S = false;
								break;
							case Sdl.SDLK_PAGEUP:
							case Sdl.SDLK_KP8:
								QuickKeys.PageUp = false;
								break;
							case Sdl.SDLK_PAGEDOWN:
							case Sdl.SDLK_KP2:
								QuickKeys.PageDown = false;
								break;
							case Sdl.SDLK_LEFT:
								QuickKeys.YawMinus = false;
								break;
							case Sdl.SDLK_RIGHT:
								QuickKeys.YawPlus = false;
								break;
							case Sdl.SDLK_UP:
								QuickKeys.PitchPlus = false;
								break;
							case Sdl.SDLK_DOWN:
								QuickKeys.PitchMinus = false;
								break;
							case Sdl.SDLK_KP_DIVIDE:
								QuickKeys.RollPlus = false;
								break;
							case Sdl.SDLK_KP_MULTIPLY:
								QuickKeys.RollMinus = false;
								break;
							case Sdl.SDLK_LSHIFT:
							case Sdl.SDLK_RSHIFT:
								QuickKeys.Shift = false;
								break;
							case Sdl.SDLK_LCTRL:
							case Sdl.SDLK_RCTRL:
								QuickKeys.Ctrl = false;
								break;
							case Sdl.SDLK_LALT:
							case Sdl.SDLK_RALT:
								QuickKeys.Alt = false;
								break;
						}
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