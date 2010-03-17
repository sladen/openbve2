using System;

namespace OpenBve {
	internal static class Text {
		
		
		// --- classes ---
		
		/// <summary>A single unicode character.</summary>
		internal class Character {
			// members
			/// <summary>The index to the managed texture.</summary>
			internal int ManagedTextureIndex;
			/// <summary>The index to the OpenGL texture.</summary>
			internal int OpenGlTextureIndex;
			/// <summary>The width of the bitmap the character is embedded in, in pixels.</summary>
			internal int BitmapWidth;
			/// <summary>The height of the bitmap the character is embedded in, in pixels.</summary>
			internal int BitmapHeight;
			/// <summary>The width of the character in pixels.</summary>
			internal int CharacterWidth;
			/// <summary>The height of the character in pixels.</summary>
			internal int CharacterHeight;
			// constructor
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="character">The character.</param>
			/// <param name="baseFont">The font family and font size used.</param>
			internal Character(string character, System.Drawing.Font baseFont) {
				/*
				 * Create a bitmap and a graphics object,
				 * then render the character to the bitmap.
				 * */
				const int bitmapWidth = 32;
				const int bitmapHeight = 32;
				System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
				graphics.PageUnit = System.Drawing.GraphicsUnit.Pixel;
				if (Program.CurrentOptions.SmoothTextRendering) {
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				} else {
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
				}
				System.Drawing.SizeF size = graphics.MeasureString(character, baseFont);
				System.Drawing.Point point = new System.Drawing.Point(0, 0);
				if (Program.CurrentOptions.SmoothTextRendering) {
					graphics.Clear(System.Drawing.Color.Black);
				} else {
					graphics.Clear(System.Drawing.Color.Transparent);
				}
				graphics.DrawString(character, baseFont, System.Drawing.Brushes.White, point);
				/*
				 * Extract the raw bitmap data and free
				 * the resources of the bitmap and graphics.
				 * */
				System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
				byte[] raw = new byte[data.Stride * data.Height];
				if (data.Stride == 4 * data.Width) {
					/*
					 * Copy the raw data.
					 * */
					System.Runtime.InteropServices.Marshal.Copy(data.Scan0, raw, 0, data.Stride * data.Height);
					/*
					 * System.Drawing.Imaging.PixelFormat.Format32bppArgb
					 * is a misnomer. The actual byte order is BGRA, but
					 * we need RGBA, so swap the red and blue bytes.
					 * */
					for (int i = 0; i < raw.Length; i += 4) {
						byte temp = raw[i];
						raw[i] = raw[i + 2];
						raw[i + 2] = temp;
					}
				} else {
					/*
					 * The bitmap is of an invalid stride.
					 * Use a black box substitution character.
					 * */
					for (int i = 0; i < raw.Length; i += 4) {
						raw[i] = 0;
						raw[i + 1] = 0;
						raw[i + 2] = 0;
						raw[i + 3] = 255;
					}
				}
				bitmap.UnlockBits(data);
				graphics.Dispose();
				bitmap.Dispose();
				/*
				 * Register and load the texture from the raw data.
				 * */
				Textures.RegisterAndLoadTexture(bitmapWidth, bitmapHeight, raw, out this.ManagedTextureIndex, out this.OpenGlTextureIndex);
				this.BitmapWidth = bitmapWidth;
				this.BitmapHeight = bitmapHeight;
				this.CharacterWidth = (int)Math.Ceiling((double)size.Width);
				this.CharacterHeight = (int)Math.Ceiling((double)size.Height);
				if (char.IsWhiteSpace(character[0])) {
					/*
					 * System.Drawing.Graphics.MeasureString does not
					 * measure whitespaces, so we need to set up a
					 * custom character width for whitespaces.
					 * */
					this.CharacterWidth = (int)Math.Round(0.666666666666667 * baseFont.Size);
				}
			}
		}
		
		/// <summary>A block of 256 consecutive Unicode characters.</summary>
		internal class Block {
			// members
			/// <summary>An array of 256 characters. Individual array members may be null references.</summary>
			internal Character[] Characters;
			// constructors
			internal Block() {
				this.Characters = new Character[256];
			}
		}
		
		/// <summary>Represents a set of characters associated to a particular font and size.</summary>
		internal class Font {
			// members
			/// <summary>The font family and font size used.</summary>
			internal System.Drawing.Font BaseFont;
			/// <summary>An array of 4352 blocks at 256 characters each. Individual array members may be null references.</summary>
			private Block[] Blocks;
			// constructors
			internal Font(System.Drawing.FontFamily family, float emSizeInPixels) {
				this.BaseFont = new System.Drawing.Font(family, emSizeInPixels, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
				this.Blocks = new Block[4352];
			}
			// instance functions
			internal Character GetCharacter(string text) {
				int codepoint = char.ConvertToUtf32(text, 0);
				int blockIndex = codepoint >> 8;
				int charIndex = codepoint & 255;
				if (this.Blocks[blockIndex] == null) {
					this.Blocks[blockIndex] = new Block();
				}
				if (this.Blocks[blockIndex].Characters[charIndex] == null) {
					this.Blocks[blockIndex].Characters[charIndex] = new Character(text, this.BaseFont);
				}
				return this.Blocks[blockIndex].Characters[charIndex];
			}
		}
		
		
		// --- members ---
		
		/// <summary>Represents the default font.</summary>
		internal static Font DefaultFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 12.0f);
		
		
	}
}