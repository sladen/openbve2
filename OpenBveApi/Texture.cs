using System;

namespace OpenBveApi {
	/// <summary>Provides support for textures.</summary>
	public static class Texture {
		
		// texture format
		/// <summary>Represents a texture format.</summary>
		public struct TextureFormat {
			// members
			/// <summary>The width.</summary>
			public int Width;
			/// <summary>The height.</summary>
			public int Height;
			/// <summary>The number of bits per channel. Allowed values are 8 or 16.</summary>
			public int BitsPerChannel;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="width">The width.</param>
			/// <param name="height">The height.</param>
			/// <param name="bitsPerChannel">The number of bits per channel. Allowed values are 8 or 16.</param>
			public TextureFormat(int width, int height, int bitsPerChannel) {
				this.Width = width;
				this.Height = height;
				this.BitsPerChannel = bitsPerChannel;
			}
		}
		
		// texture data
		/// <summary>Represents texture raw data.</summary>
		public class TextureData {
			// members
			/// <summary>The texture format of the raw data.</summary>
			public TextureFormat Format;
			/// <summary>The byte raw data, stored row-based from top to bottom, and within a row from left to right, with each pixel in RGBA form. With more than 8 bits per channel, each channel is stored in little endian byte order.</summary>
			/// <remarks>Each channel can take values from 0 to 255 (8 bits per channel), or 0 to 65535 (16 bits per channel), with 0 representing no contribution / transparency, and 255/65535 representing full contribution / opaqueness.</remarks>
			public byte[] Bytes;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="format">The texture format of the raw data.</param>
			/// <param name="bytes">The byte raw data, stored row-based from top to bottom, and within a row from left to right.</param>
			public TextureData(TextureFormat format, byte[] bytes) {
				this.Format = format;
				this.Bytes = bytes;
			}
		}
		
		// texture wrap mode
		/// <summary>Represents how the texture should wrap at edges.</summary>
		public enum TextureWrapMode {
			/// <summary>The texture repeats on an infinite grid.</summary>
			Repeat = 0,
			/// <summary>Pixels outside of the texture sample from the closest edge pixel.</summary>
			ClampToEdge = 1
		}
		
		// texture clip region
		/// <summary>Represents a region of a texture to be extracted by a texture load operation.</summary>
		public class TextureClipRegion {
			/// <summary>The x-coordinate of the left margin of the region to be extracted, in pixels.</summary>
			/// <remarks>The coordinate is zero-based.</remarks>
			public int Left;
			/// <summary>The y-coordinate of the top margin of the region to be extracted, in pixels.</summary>
			/// <remarks>The coordinate is zero-based.</remarks>
			public int Top;
			/// <summary>The width of the region to be extracted in pixels.</summary>
			public int Width;
			/// <summary>The height of the region to be extracted in pixels.</summary>
			public int Height;
		}

		
		// texture parameters
		/// <summary>Represents options for the texture loading process.</summary>
		public struct TextureParameters {
			// members
			/// <summary>The color in the texture that should become transparent.</summary>
			public Color.TransparentColor TransparentColor;
			/// <summary>The horizontal wrap mode for this texture.</summary>
			public TextureWrapMode HorizontalWrapMode;
			/// <summary>The vertical wrap mode for this texture.</summary>
			public TextureWrapMode VerticalWrapMode;
			/// <summary>The region of the texture to be extracted, or a null reference for the entire texture.</summary>
			public TextureClipRegion ClipRegion;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="transparentColor">The color in the texture that should become transparent.</param>
			/// <param name="horizontalWrapMode">The horizontal wrap mode for this texture.</param>
			/// <param name="verticalWrapMode">The vertical wrap mode for this texture.</param>
			/// <param name="clipRegion">The region of the texture to be extracted, or a null reference for the entire texture.</param>
			/// <remarks>If more than one transparent color is specified, you must ensure that they are in canonical order. Use the static method GetCanonicalOrder of the TextureLoadOptions structure to ensure this order.</remarks>
			public TextureParameters(Color.TransparentColor transparentColor, TextureWrapMode horizontalWrapMode, TextureWrapMode verticalWrapMode, TextureClipRegion clipRegion) {
				this.TransparentColor = transparentColor;
				this.HorizontalWrapMode = horizontalWrapMode;
				this.VerticalWrapMode = verticalWrapMode;
				this.ClipRegion = clipRegion;
			}
			// comparisons
			/// <summary>Checks two texture load options for equality.</summary>
			/// <param name="a">The first texture load option.</param>
			/// <param name="b">The second texture load option.</param>
			/// <returns>A boolean indicating whether the two texture load options are equal.</returns>
			public static bool operator ==(TextureParameters a, TextureParameters b) {
				if (a.TransparentColor != b.TransparentColor) return false;
				if (a.HorizontalWrapMode != b.HorizontalWrapMode) return false;
				if (a.VerticalWrapMode != b.VerticalWrapMode) return false;
				if (a.ClipRegion != b.ClipRegion) return false;
				return true;
			}
			/// <summary>Checks two texture load options for inequality.</summary>
			/// <param name="a">The first texture load option.</param>
			/// <param name="b">The second texture load option.</param>
			/// <returns>A boolean indicating whether the two texture load options are unequal.</returns>
			public static bool operator !=(TextureParameters a, TextureParameters b) {
				if (a.TransparentColor != b.TransparentColor) return true;
				if (a.HorizontalWrapMode != b.HorizontalWrapMode) return true;
				if (a.VerticalWrapMode != b.VerticalWrapMode) return true;
				if (a.ClipRegion != b.ClipRegion) return true;
				return false;
			}
			/// <summary>Checks this instance and a specified object for equality.</summary>
			/// <param name="obj">The object to compare.</param>
			/// <returns>A boolean indicating whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (obj is TextureParameters) {
					return this == (TextureParameters)obj;
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
		
		// texture handle
		/// <summary>Represents a handle to a texture as obtained from the host application.</summary>
		public struct TextureHandle {
			// members
			/// <summary>Data by which the host application can identify the texture this handle points to, or a null reference.</summary>
			public object TextureData;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="textureData">Data by which the host application can identify the texture this handle points to, or a null reference.</param>
			public TextureHandle(object textureData) {
				this.TextureData = textureData;
			}
			// read-only fields
			/// <summary>Represents a handle that has not been allocated by the host application.</summary>
			public static readonly TextureHandle Null = new TextureHandle(null);
		}
		
	}
}