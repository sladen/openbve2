using System;
using Tao.Sdl;

namespace OpenBve {
	/// <summary>Provides methods to create and destroy the SDL window.</summary>
	internal static class Window {
		
		// window options
		/// <summary>Stores options required to initialize the window.</summary>
		internal struct WindowOptions {
			// members
			/// <summary>The width of the window in pixels.</summary>
			internal int Width;
			/// <summary>The height of the window in pixels.</summary>
			internal int Height;
			/// <summary>The bit depth. Can be either 16 or 32.</summary>
			internal int BitsPerPixel;
			/// <summary>Whether to enable fullscreen mode.</summary>
			internal bool Fullscreen;
			/// <summary>Whether to enable vertical synchronization.</summary>
			internal bool VerticalSynchronization;
			// constructors
			internal WindowOptions(int Width, int Height, int BitsPerPixel, bool Fullscreen, bool VerticalSynchronization) {
				this.Width = Width;
				this.Height = Height;
				this.BitsPerPixel = BitsPerPixel;
				this.Fullscreen = Fullscreen;
				this.VerticalSynchronization = VerticalSynchronization;
			}
		}
		
		// window properties
		/// <summary>Stores information about the current window.</summary>
		internal struct WindowProperties {
			// members
			/// <summary>The width of the window in pixels.</summary>
			internal int Width;
			/// <summary>The height of the window in pixels.</summary>
			internal int Height;
			/// <summary>The width to height ratio.</summary>
			internal double AspectRatio;
			// constructors
			internal WindowProperties(int Width, int Height) {
				this.Width = Width;
				this.Height = Height;
				this.AspectRatio = Height == 0 ? 1.0 : (double)Width / (double)Height;
			}
			internal WindowProperties(WindowOptions Options) {
				this.Width = Options.Width;
				this.Height = Options.Height;
				this.AspectRatio = (double)Options.Width / (double)Options.Height;
			}
		}
		
		// members
		/// <summary>Whether the window has been correctly initialized.</summary>
		private static bool Initialized = false;
		/// <summary>The current properties of the display window.</summary>
		internal static WindowProperties Properties;
		
		// initialize
		/// <summary>Initializes the SDL window.</summary>
		/// <param name="Options">The options to set up the window.</param>
		/// <returns>A boolean indicating the success of the operation.</returns>
		internal static bool Initialize(WindowOptions Options) {
			if (Sdl.SDL_InitSubSystem(Sdl.SDL_INIT_VIDEO) != 0) {
				return false;
			} else {
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DOUBLEBUFFER, 1);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DEPTH_SIZE, 16);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, 8);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, 8);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, 8);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_ALPHA_SIZE, 8);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_SWAP_CONTROL, Options.VerticalSynchronization ? 1 : 0);
				Sdl.SDL_ShowCursor(Sdl.SDL_DISABLE);
				int flags = Sdl.SDL_OPENGL | Sdl.SDL_HWSURFACE | Sdl.SDL_ANYFORMAT | Sdl.SDL_DOUBLEBUF;
				if (Options.Fullscreen) {
					flags |= Sdl.SDL_FULLSCREEN;
				} else {
					flags |= Sdl.SDL_RESIZABLE;
				}
				IntPtr video = Sdl.SDL_SetVideoMode(Options.Width, Options.Height, Options.BitsPerPixel, flags);
				if (video == IntPtr.Zero) {
					Sdl.SDL_QuitSubSystem(Sdl.SDL_INIT_VIDEO);
					return false;
				} else {
					Sdl.SDL_WM_SetCaption("openBVE", null);
					Properties = new WindowProperties(Options);
					Initialized = true;
					return true;
				}
			}
		}
		
		// resize
		/// <summary>Informs about a change of the window size.</summary>
		internal static void ResizeNotify(int Width, int Height) {
			Properties = new WindowProperties(Width, Height);
		}
		
		// deinitialize
		/// <summary>Deinitializes the SDL window.</summary>
		/// <remarks>This function can be called regardless of whether the initialization of the window was successful or not.</remarks>
		internal static void Deinitialize() {
			if (Initialized) {
				Sdl.SDL_QuitSubSystem(Sdl.SDL_INIT_VIDEO);
				Initialized = false;
			}
		}
		
	}
	
}