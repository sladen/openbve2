using System;

namespace OpenBve {
	internal static partial class Textures {
		
		/*
		 * Helper
		 * ------
		 * 
		 * This file contains helper functions that are called from Textures.cs.
		 * 
		 * TODO: Not all functions in this file have been completely implemented yet.
		 * 
		 * */
		
		// convert to 8 bits per channel
		private static void ConvertTo8BitsPerChannel(OpenBveApi.Texture.TextureData texture, out byte[] raw) {
			switch(texture.Format.BitsPerChannel) {
				case 8:
					raw = new byte[texture.Bytes.Length];
					for (int i = 0; i < raw.Length; i++) {
						raw[i] = texture.Bytes[i];
					}
					break;
				case 16:
					raw = new byte[texture.Bytes.Length >> 1];
					for (int i = 0; i < raw.Length; i++) {
						raw[i] = texture.Bytes[i << 1];
					}
					break;
				default:
					throw new InvalidOperationException();
			}
		}
		
		// extract clip region
		private static void ExtractClipRegion(ref int width, ref int height, ref byte[] raw, OpenBveApi.Texture.TextureClipRegion region) {
			if (region != null) {
				int clipLeft = region.Left;
				int clipTop = region.Top;
				int clipWidth = region.Width;
				int clipHeight = region.Height;
				if (clipLeft < 0) {
					clipLeft = 0;
				}
				if (clipTop < 0) {
					clipTop = 0;
				}
				if (clipWidth < 0) {
					clipWidth = 0;
				} else if (clipWidth > width - clipLeft) {
					clipWidth = width - clipLeft;
				}
				if (clipHeight < 0) {
					clipHeight = 0;
				} else if (clipHeight > height - clipTop) {
					clipHeight = height - clipTop;
				}
				if (
					region.Left != 0 | region.Top != 0 |
					region.Width != width | region.Height != height
				) {
					byte[] clip = new byte[4 * clipWidth * clipHeight];
					int i = 0;
					int j = 4 * clipTop * width + 4 * clipLeft;
					int jSkip = 4 * (width - clipWidth);
					for (int y = 0; y < clipHeight; y++) {
						for (int x = 0; x < clipWidth; x++) {
							clip[i + 0] = raw[j + 0];
							clip[i + 1] = raw[j + 1];
							clip[i + 2] = raw[j + 2];
							clip[i + 3] = raw[j + 3];
							i += 4;
							j += 4;
						}
						j += jSkip;
					}
					raw = clip;
					width = clipWidth;
					height = clipHeight;
				}
			}
		}
		
		// eliminate transparent color
		private static void EliminateTransparentColor(int width, int height, ref byte[] raw, OpenBveApi.Color.TransparentColor color) {
			if (color.Assigned) {
				// TODO: Replace this algorithm by a higher quality one.
				for (int i = 0; i < raw.Length; i += 4) {
					if (
						raw[i + 0] == color.R &
						raw[i + 1] == color.G &
						raw[i + 2] == color.B
					) {
						raw[i + 0] = 128;
						raw[i + 1] = 128;
						raw[i + 2] = 128;
						raw[i + 3] = 0;
					}
				}
			}
		}
		
		// convert to power of two size
		private static void ConvertToPowerOfTwoSize(ref int width, ref int height, ref byte[] raw) {
			int powerOfTwoWidth = GetPowerOfTwo(width);
			int powerOfTwoHeight = GetPowerOfTwo(height);
			if (width != powerOfTwoWidth | height != powerOfTwoHeight) {
				// TODO: Implement this.
			}
		}
		
		// get power of two
		private static int GetPowerOfTwo(int Value) {
			Value -= 1;
			for (int i = 1; i < sizeof(int) << 3; i <<= 1) {
				Value = Value | Value >> i;
			}
			return Value + 1;
		}
		
	}
}