using System;

namespace OpenBveApi {
	/// <summary>Provides structures to store color data.</summary>
	public static class Color {
		
		// color rgb
		/// <summary>Represents a floating-point RGB color.</summary>
		/// <remarks>In each color channel, a value of zero represents no contribution to that channel, while a value of one represents full contribution.</remarks>
		public struct ColorRGB {
			// members
			/// <summary>The red component.</summary>
			public float R;
			/// <summary>The green component.</summary>
			public float G;
			/// <summary>The blue component.</summary>
			public float B;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="r">The red component.</param>
			/// <param name="g">The green component.</param>
			/// <param name="b">The blue component.</param>
			public ColorRGB(float r, float g, float b) {
				this.R = r;
				this.G = g;
				this.B = b;
			}
			// read-only fields
			/// <summary>A black color.</summary>
			public static readonly ColorRGB Black = new ColorRGB(0.0f, 0.0f, 0.0f);
			/// <summary>A red color.</summary>
			public static readonly ColorRGB Red = new ColorRGB(1.0f, 0.0f, 0.0f);
			/// <summary>A green color.</summary>
			public static readonly ColorRGB Green = new ColorRGB(0.0f, 1.0f, 0.0f);
			/// <summary>A blue color.</summary>
			public static readonly ColorRGB Blue = new ColorRGB(0.0f, 0.0f, 1.0f);
			/// <summary>A cyan color.</summary>
			public static readonly ColorRGB Cyan = new ColorRGB(0.0f, 1.0f, 1.0f);
			/// <summary>A magenta color.</summary>
			public static readonly ColorRGB Magenta = new ColorRGB(1.0f, 0.0f, 1.0f);
			/// <summary>A yellow color.</summary>
			public static readonly ColorRGB Yellow = new ColorRGB(1.0f, 1.0f, 0.0f);
			/// <summary>A white color.</summary>
			public static readonly ColorRGB White = new ColorRGB(1.0f, 1.0f, 1.0f);
			// arithmetic operators
			/// <summary>Adds two colors.</summary>
			/// <param name="a">The first color.</param>
			/// <param name="b">The second color.</param>
			/// <returns>The sum of the two color.</returns>
			public static ColorRGB operator +(ColorRGB a, ColorRGB b) {
				return new ColorRGB(a.R + b.R, a.G + b.G, a.B + b.B);
			}
			/// <summary>Adds a color and a constant.</summary>
			/// <param name="a">The color.</param>
			/// <param name="b">The constant.</param>
			/// <returns>The sum of the color and the constant.</returns>
			public static ColorRGB operator +(ColorRGB a, float b) {
				return new ColorRGB(a.R + b, a.G + b, a.B + b);
			}
			/// <summary>Adds a constant and a color.</summary>
			/// <param name="a">The constant.</param>
			/// <param name="b">The color.</param>
			/// <returns>The sum of the constant and the color.</returns>
			public static ColorRGB operator +(float a, ColorRGB b) {
				return new ColorRGB(a + b.R, a + b.G, a + b.B);
			}
			/// <summary>Subtracts two colors.</summary>
			/// <param name="a">The first color.</param>
			/// <param name="b">The second color.</param>
			/// <returns>The difference of the two colors.</returns>
			public static ColorRGB operator -(ColorRGB a, ColorRGB b) {
				return new ColorRGB(a.R - b.R, a.G - b.G, a.B - b.B);
			}
			/// <summary>Subtracts a constant from a color.</summary>
			/// <param name="a">The color.</param>
			/// <param name="b">The constant.</param>
			/// <returns>The difference of the color and the constant.</returns>
			public static ColorRGB operator -(ColorRGB a, float b) {
				return new ColorRGB(a.R - b, a.G - b, a.B - b);
			}
			/// <summary>Subtracts a color from a constant.</summary>
			/// <param name="a">The constant.</param>
			/// <param name="b">The color.</param>
			/// <returns>The difference of the constant and the color.</returns>
			public static ColorRGB operator -(float a, ColorRGB b) {
				return new ColorRGB(a - b.R, a - b.G, a - b.B);
			}
			/// <summary>Negates a color.</summary>
			/// <param name="a">The color.</param>
			/// <returns>The negation of the color.</returns>
			public static ColorRGB operator -(ColorRGB a) {
				return new ColorRGB(-a.R, -a.G, -a.B);
			}
			/// <summary>Multiplies two colors.</summary>
			/// <param name="a">The first color.</param>
			/// <param name="b">The second color.</param>
			/// <returns>The product of the two colors.</returns>
			public static ColorRGB operator *(ColorRGB a, ColorRGB b) {
				return new ColorRGB(a.R * b.R, a.G * b.G, a.B * b.B);
			}
			/// <summary>Multiplies a color and a constant.</summary>
			/// <param name="a">The color.</param>
			/// <param name="b">The constant.</param>
			/// <returns>The product of the color and the constant.</returns>
			public static ColorRGB operator *(ColorRGB a, float b) {
				return new ColorRGB(a.R * b, a.G * b, a.B * b);
			}
			/// <summary>Multiplies a constant and a color.</summary>
			/// <param name="a">The constant.</param>
			/// <param name="b">The color.</param>
			/// <returns>The product of the constant and the color.</returns>
			public static ColorRGB operator *(float a, ColorRGB b) {
				return new ColorRGB(a * b.R, a * b.G, a * b.B);
			}
			/// <summary>Divides two colors.</summary>
			/// <param name="a">The first color.</param>
			/// <param name="b">The second color.</param>
			/// <returns>The quotient of the two colors.</returns>
			/// <exception cref="DivideByZeroException">Raised when any member of the second color is zero.</exception>
			public static ColorRGB operator /(ColorRGB a, ColorRGB b) {
				if (b.R == 0.0 | b.G == 0.0 | b.B == 0.0) {
					throw new DivideByZeroException();
				} else {
					return new ColorRGB(a.R / b.R, a.G / b.G, a.B / b.B);
				}
			}
			/// <summary>Divides a color by a constant.</summary>
			/// <param name="a">The color.</param>
			/// <param name="b">The constant.</param>
			/// <returns>The quotient of the color and the constant.</returns>
			/// <exception cref="DivideByZeroException">Raised when any member of the color is zero.</exception>
			public static ColorRGB operator /(ColorRGB a, float b) {
				if (b == 0.0) {
					throw new DivideByZeroException();
				} else {
					float factor = 1.0f / b;
					return new ColorRGB(a.R * factor, a.G * factor, a.B * factor);
				}
			}
			/// <summary>Divides a constant by a color.</summary>
			/// <param name="a">The constant.</param>
			/// <param name="b">The color.</param>
			/// <returns>The quotient of the constant and the color.</returns>
			/// <exception cref="DivideByZeroException">Raised when any member of the color is zero.</exception>
			public static ColorRGB operator /(float a, ColorRGB b) {
				if (b.R == 0.0 | b.G == 0.0 | b.B == 0.0) {
					throw new DivideByZeroException();
				} else {
					return new ColorRGB(a / b.R, a / b.G, a / b.B);
				}
			}
			// comparisons
			/// <summary>Checks two colors for equality.</summary>
			/// <param name="a">The first color.</param>
			/// <param name="b">The second color.</param>
			/// <returns>A boolean indicating whether the two colors are equal.</returns>
			public static bool operator ==(ColorRGB a, ColorRGB b) {
				return a.R == b.R & a.G == b.G & a.B == b.B;
			}
			/// <summary>Checks two colors for inequality.</summary>
			/// <param name="a">The first color.</param>
			/// <param name="b">The second color.</param>
			/// <returns>A boolean indicating whether the two colors are unequal.</returns>
			public static bool operator !=(ColorRGB a, ColorRGB b) {
				return a.R != b.R | a.G != b.G | a.B != b.B;
			}
			/// <summary>Checks this instance and a specified object for equality.</summary>
			/// <param name="obj">The object to compare.</param>
			/// <returns>A boolean indicating whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (obj is ColorRGB) {
					ColorRGB color = (ColorRGB)obj;
					return this.R == color.R & this.G == color.G & this.B == color.B;
				} else {
					return base.Equals(obj);
				}
			}
			/// <summary>Gets the hash code for this instance.</summary>
			/// <returns>The hash code.</returns>
			public override int GetHashCode() {
				return base.GetHashCode();
			}

		}
		
		// color rgba
		/// <summary>Represents a floating-point RGBA color.</summary>
		/// <remarks>In each color channel, a value of zero represents no contribution to that channel, while a value of one represents full contribution.</remarks>
		public struct ColorRGBA {
			// members
			/// <summary>The red component.</summary>
			public float R;
			/// <summary>The green component.</summary>
			public float G;
			/// <summary>The blue component.</summary>
			public float B;
			/// <summary>The alpha component.</summary>
			public float A;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="r">The red component.</param>
			/// <param name="g">The green component.</param>
			/// <param name="b">The blue component.</param>
			/// <param name="a">The alpha component.</param>
			public ColorRGBA(float r, float g, float b, float a) {
				this.R = r;
				this.G = g;
				this.B = b;
				this.A = a;
			}
			// read-only fields
			/// <summary>A black color.</summary>
			public static readonly ColorRGBA Black = new ColorRGBA(0.0f, 0.0f, 0.0f, 1.0f);
			/// <summary>A red color.</summary>
			public static readonly ColorRGBA Red = new ColorRGBA(1.0f, 0.0f, 0.0f, 1.0f);
			/// <summary>A green color.</summary>
			public static readonly ColorRGBA Green = new ColorRGBA(0.0f, 1.0f, 0.0f, 1.0f);
			/// <summary>A blue color.</summary>
			public static readonly ColorRGBA Blue = new ColorRGBA(0.0f, 0.0f, 1.0f, 1.0f);
			/// <summary>A cyan color.</summary>
			public static readonly ColorRGBA Cyan = new ColorRGBA(0.0f, 1.0f, 1.0f, 1.0f);
			/// <summary>A magenta color.</summary>
			public static readonly ColorRGBA Magenta = new ColorRGBA(1.0f, 0.0f, 1.0f, 1.0f);
			/// <summary>A yellow color.</summary>
			public static readonly ColorRGBA Yellow = new ColorRGBA(1.0f, 1.0f, 0.0f, 1.0f);
			/// <summary>A white color.</summary>
			public static readonly ColorRGBA White = new ColorRGBA(1.0f, 1.0f, 1.0f, 1.0f);
		}
		
		// transparent color
		/// <summary>Represents a color intended to become transparent.</summary>
		public struct TransparentColor {
			// member
			/// <summary>The red component.</summary>
			public byte R;
			/// <summary>The green component.</summary>
			public byte G;
			/// <summary>The blue component.</summary>
			public byte B;
			/// <summary>Indicates whether the transparent color has been assigned a value.</summary>
			public bool Assigned;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="r">The red component.</param>
			/// <param name="g">The green component.</param>
			/// <param name="b">The blue component.</param>
			/// <param name="assigned">Indicates whether the transparent color has been assigned a value.</param>
			public TransparentColor(byte r, byte g, byte b, bool assigned) {
				this.R = r;
				this.G = g;
				this.B = b;
				this.Assigned = assigned;
			}
			// operators
			/// <summary>Checks two colors for equality.</summary>
			/// <param name="a">The first color.</param>
			/// <param name="b">The second color.</param>
			/// <returns>A boolean indicating whether the two colors are equal.</returns>
			public static bool operator ==(TransparentColor a, TransparentColor b) {
				if (a.Assigned != b.Assigned) return false;
				if (!a.Assigned) return true;
				if (a.R != b.R) return false;
				if (a.G != b.G) return false;
				if (a.B != b.B) return false;
				return true;
			}
			/// <summary>Checks two colors for inequality.</summary>
			/// <param name="a">The first color.</param>
			/// <param name="b">The second color.</param>
			/// <returns>A boolean indicating whether the two colors are unequal.</returns>
			public static bool operator !=(TransparentColor a, TransparentColor b) {
				if (a.Assigned != b.Assigned) return true;
				if (!a.Assigned) return false;
				if (a.R != b.R) return true;
				if (a.G != b.G) return true;
				if (a.B != b.B) return true;
				return false;
			}
			/// <summary>Checks this instance and a specified object for equality.</summary>
			/// <param name="obj">The object to compare.</param>
			/// <returns>A boolean indicating whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (obj is TransparentColor) {
					return this == (TransparentColor)obj;
				} else {
					return base.Equals(obj);
				}
			}
			/// <summary>Gets the hash code for this instance.</summary>
			/// <returns>The hash code.</returns>
			public override int GetHashCode() {
				return base.GetHashCode();
			}
			// static fields
			/// <summary>Represents that no transparent color is to be used.</summary>
			public static readonly TransparentColor None = new TransparentColor(0, 0, 0, false);
		}
		
	}
}