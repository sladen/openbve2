using System;
using System.Globalization;
using Tao.OpenGl;
using Tao.Sdl;

namespace OpenBve {
	/// <summary>Provides methods to create and destroy the SDL window.</summary>
	internal static class Screen {
		
		
		// --- structures ---
		
		/// <summary>Stores options required to initialize the window.</summary>
		internal struct ScreenOptions {
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
			internal ScreenOptions(int width, int height, int bitsPerPixel, bool fullscreen, bool verticalSynchronization) {
				this.Width = width;
				this.Height = height;
				this.BitsPerPixel = bitsPerPixel;
				this.Fullscreen = fullscreen;
				this.VerticalSynchronization = verticalSynchronization;
			}
		}
		
		/// <summary>Stores information about the current window.</summary>
		internal struct ScreenProperties {
			// members
			/// <summary>The width of the window in pixels.</summary>
			internal int Width;
			/// <summary>The height of the window in pixels.</summary>
			internal int Height;
			/// <summary>The width to height ratio.</summary>
			internal double AspectRatio;
			// constructors
			internal ScreenProperties(int width, int height) {
				this.Width = width;
				this.Height = height;
				this.AspectRatio = height != 0 ? (double)width / (double)height : 1.0;
			}
			internal ScreenProperties(ScreenOptions options) {
				this.Width = options.Width;
				this.Height = options.Height;
				this.AspectRatio = (double)options.Width / (double)options.Height;
			}
		}
		
		
		// --- members ---
		
		/// <summary>Whether the window has been correctly initialized.</summary>
		private static bool Initialized = false;
		
		/// <summary>The current properties of the display window.</summary>
		internal static ScreenProperties Properties;
		
		
		// --- static functions
		
		/// <summary>Initializes the SDL window.</summary>
		/// <param name="Options">The options to set up the window.</param>
		/// <returns>A boolean indicating the success of the operation.</returns>
		internal static bool Initialize(ScreenOptions options) {
			if (Sdl.SDL_InitSubSystem(Sdl.SDL_INIT_VIDEO) != 0) {
				return false;
			} else {
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DOUBLEBUFFER, 1);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, Program.CurrentOptions.RedSize);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, Program.CurrentOptions.GreenSize);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, Program.CurrentOptions.BlueSize);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_ALPHA_SIZE, Program.CurrentOptions.AlphaSize);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DEPTH_SIZE, Program.CurrentOptions.DepthSize);
				Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_SWAP_CONTROL, options.VerticalSynchronization ? 1 : 0);
				Sdl.SDL_ShowCursor(Sdl.SDL_DISABLE);
				int flags = Sdl.SDL_OPENGL | Sdl.SDL_HWSURFACE | Sdl.SDL_ANYFORMAT | Sdl.SDL_DOUBLEBUF;
				if (options.Fullscreen) {
					flags |= Sdl.SDL_FULLSCREEN;
				} else {
					flags |= Sdl.SDL_RESIZABLE;
				}
				IntPtr video = Sdl.SDL_SetVideoMode(options.Width, options.Height, options.BitsPerPixel, flags);
				if (video == IntPtr.Zero) {
					Sdl.SDL_QuitSubSystem(Sdl.SDL_INIT_VIDEO);
					return false;
				} else {
					{
						int value;
						Sdl.SDL_GL_GetAttribute(Sdl.SDL_GL_RED_SIZE, out value);
						Program.Log.Append("SDL_GL_RED_SIZE = ").Append(value.ToString(CultureInfo.InvariantCulture)).Append(" (").Append(Program.CurrentOptions.RedSize).AppendLine(" requested)");
						Sdl.SDL_GL_GetAttribute(Sdl.SDL_GL_GREEN_SIZE, out value);
						Program.Log.Append("SDL_GL_GREEN_SIZE = ").Append(value.ToString(CultureInfo.InvariantCulture)).Append(" (").Append(Program.CurrentOptions.GreenSize).AppendLine(" requested)");
						Sdl.SDL_GL_GetAttribute(Sdl.SDL_GL_BLUE_SIZE, out value);
						Program.Log.Append("SDL_GL_BLUE_SIZE = ").Append(value.ToString(CultureInfo.InvariantCulture)).Append(" (").Append(Program.CurrentOptions.BlueSize).AppendLine(" requested)");
						Sdl.SDL_GL_GetAttribute(Sdl.SDL_GL_ALPHA_SIZE, out value);
						Program.Log.Append("SDL_GL_ALPHA_SIZE = ").Append(value.ToString(CultureInfo.InvariantCulture)).Append(" (").Append(Program.CurrentOptions.AlphaSize).AppendLine(" requested)");
						Sdl.SDL_GL_GetAttribute(Sdl.SDL_GL_DEPTH_SIZE, out value);
						Program.Log.Append("SDL_GL_DEPTH_SIZE = ").Append(value.ToString(CultureInfo.InvariantCulture)).Append(" (").Append(Program.CurrentOptions.DepthSize).AppendLine(" requested)");
						Program.Log.AppendLine();
					}
					Sdl.SDL_WM_SetCaption("openBVE", null);
					string[] extensions = Gl.glGetString(Gl.GL_EXTENSIONS).Split(new char[] { ' ' });
					for (int i = 0; i < extensions.Length; i++) {
						if (string.Equals(extensions[i], "GL_EXT_texture_filter_anisotropic", StringComparison.OrdinalIgnoreCase)) {
							Gl.glGetFloatv(Gl.GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, out Program.AnisotropicMaximum);
							break;
						}
					}
					Properties = new ScreenProperties(options);
					Initialized = true;
					return true;
				}
			}
		}
		
		/// <summary>Informs about a change of the window size.</summary>
		internal static void ResizeNotify(int width, int height) {
			Properties = new ScreenProperties(width, height);
		}
		
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