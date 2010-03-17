using System;
using Tao.OpenGl;

namespace OpenBve {
	internal static class Windows {
		
		// TODO: This file is a stub. Expand as necessary.
		
		private static string DebugText = null;
		private static bool ContentHasChanged = true;
		private static DisplayList List = new DisplayList();
		
		
		// --- structures and classes ---
		
		internal struct Point {
			// members
			internal int Left;
			internal int Top;
			// constructors
			internal Point(int left, int top) {
				this.Left = left;
				this.Top = top;
			}
		}
		
		internal struct Size {
			// members
			internal int Width;
			internal int Height;
			// constructors
			internal Size(int width, int height) {
				this.Width = width;
				this.Height = height;
			}
		}
		
		internal struct Rectangle {
			// members
			internal Point Location;
			internal Size Size;
			// constructors
			internal Rectangle(Point location, Size size) {
				this.Location = location;
				this.Size = size;
			}
			internal Rectangle(int left, int top, int width, int height) {
				this.Location.Left = left;
				this.Location.Top = top;
				this.Size.Width = width;
				this.Size.Height = height;
			}
		}
		
		internal abstract class Control {
			// members
			/// <summary>The dimensions relative to the parent control.</summary>
			internal Rectangle Bounds;
		}
		
		internal class Label : Control {
			// members
			internal string Text;
		}
		
		internal class Button : Control {
			// members
			internal string Text;
		}
		
		internal class Window {
			// members
			internal Control[] Controls;
			// constructors
			internal Window(Control[] controls, bool autoLayout) {
				this.Controls = controls;
			}
		}
		
		
		// --- static functions ---
		
		internal static void UpdateDebugText(string text) {
			if (text != DebugText) {
				DebugText = text;
				ContentHasChanged = true;
			}
		}
		
		internal static void Render() {
			if (List.IsUnavailable()) {
				ContentHasChanged = true;
			};
			if (ContentHasChanged) {
				Renderer.OpenGlState state;
				List.Begin(out state);
				if (DebugText != null) {
					//int hours = (int)Math.Floor((Timing.SecondsSinceMidnight % 86400.0) / 3600.0);
					//int minutes = (int)Math.Floor((Timing.SecondsSinceMidnight % 3600.0) / 60.0);
					//string text = "Time: " + hours.ToString("00") + ":" + minutes.ToString("00");
					string text = DebugText;
					RenderString(text, new Point(10, 10), new Rectangle(0, 0, Screen.Properties.Width, Screen.Properties.Height), Text.DefaultFont, OpenBveApi.Color.ColorRGB.White, ref state);
				}
				List.End(ref state);
				ContentHasChanged = false;
			}
			List.Call();
		}
		
		/// <summary>Renders a string at a specified location.</summary>
		/// <param name="text">The string.</param>
		/// <param name="location">The top-left corner in screen coordinates.</param>
		/// <param name="clip">The clip rectangle. The string will only be rendered inside the bounds of this clip rectangle.</param>
		/// <param name="font">The font.</param>
		/// <param name="color">The font color.</param>
		/// <param name="state">The current OpenGL state.</param>
		private static void RenderString(string text, Point location, Rectangle clip, Text.Font font, OpenBveApi.Color.ColorRGB color, ref Renderer.OpenGlState state) {
			for (int i = 0; i < text.Length; i++) {
				string value;
				if (char.IsSurrogatePair(text, i)) {
					value = text.Substring(i, 2);
					i++;
				} else {
					value = text.Substring(i, 1);
				}
				Text.Character character = font.GetCharacter(value);
				RenderCharacter(character, location, clip, color, ref state);
				const int spacing = -4;
				location.Left += character.CharacterWidth + spacing;
			}
		}
		
		/// <summary>Renders a single character at a specified location.</summary>
		/// <param name="character">The character.</param>
		/// <param name="location">The top-left corner in screen coordinates.</param>
		/// <param name="clip">The clip rectangle. The character will only be rendered inside the bounds of this clip rectangle.</param>
		/// <param name="color">The font color.</param>
		/// <param name="state">The current OpenGL state.</param>
		private static void RenderCharacter(Text.Character character, Point location, Rectangle clip, OpenBveApi.Color.ColorRGB color, ref Renderer.OpenGlState state) {
			if (location.Left < clip.Location.Left + clip.Size.Width && location.Top < clip.Location.Top + clip.Size.Height) {
				if (location.Left + character.CharacterWidth > clip.Location.Left && location.Top + character.CharacterHeight > clip.Location.Top) {
					OpenBveApi.Math.Vector2[] spatialCoordinates = new OpenBveApi.Math.Vector2[] {
						new OpenBveApi.Math.Vector2(location.Left, location.Top),
						new OpenBveApi.Math.Vector2(location.Left + character.CharacterWidth, location.Top),
						new OpenBveApi.Math.Vector2(location.Left + character.CharacterWidth, location.Top + character.CharacterHeight),
						new OpenBveApi.Math.Vector2(location.Left, location.Top + character.CharacterHeight)
					};
					OpenBveApi.Math.Vector2[] textureCoordinates = new OpenBveApi.Math.Vector2[4];
					for (int i = 0; i < 4; i++) {
						if (spatialCoordinates[i].X < clip.Location.Left) {
							spatialCoordinates[i].X = clip.Location.Left;
						}
						if (spatialCoordinates[i].X > clip.Location.Left + clip.Size.Width) {
							spatialCoordinates[i].X = clip.Location.Left + clip.Size.Width;
						}
						if (spatialCoordinates[i].Y < clip.Location.Top) {
							spatialCoordinates[i].Y = clip.Location.Top;
						}
						if (spatialCoordinates[i].Y > clip.Location.Top + clip.Size.Height) {
							spatialCoordinates[i].Y = clip.Location.Top + clip.Size.Height;
						}
						textureCoordinates[i].X = (spatialCoordinates[i].X - location.Left) / character.BitmapWidth;
						textureCoordinates[i].Y = (spatialCoordinates[i].Y - location.Top) / character.BitmapHeight;
					}
					state.BindTexture(character.OpenGlTextureIndex);
					if (Program.CurrentOptions.SmoothTextRendering) {
						state.SetBlend(Gl.GL_ZERO, Gl.GL_ONE_MINUS_SRC_COLOR);
						Gl.glBegin(Gl.GL_POLYGON);
						for (int i = 0; i < 4; i++) {
							Gl.glColor3f(1.0f, 1.0f, 1.0f);
							Gl.glTexCoord2d(textureCoordinates[i].X, textureCoordinates[i].Y);
							Gl.glVertex2d(spatialCoordinates[i].X, spatialCoordinates[i].Y);
						}
						Gl.glEnd();
						state.SetBlend(Gl.GL_SRC_ALPHA, Gl.GL_ONE);
					} else {
						state.SetBlend(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
					}
					Gl.glBegin(Gl.GL_POLYGON);
					for (int i = 0; i < 4; i++) {
						Gl.glColor3f(color.R, color.G, color.B);
						Gl.glTexCoord2d(textureCoordinates[i].X, textureCoordinates[i].Y);
						Gl.glVertex2d(spatialCoordinates[i].X, spatialCoordinates[i].Y);
					}
					Gl.glEnd();
				}
			}
		}
		
	}
}