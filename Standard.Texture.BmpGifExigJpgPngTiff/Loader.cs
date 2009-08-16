using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenBveApi;

namespace Plugin {
	internal static class Loader {
		
		internal static OpenBveApi.General.Result LoadTexture(string fileName, out OpenBveApi.Texture.TextureData texture) {
			try {
				System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(fileName);
				// convert to 32-bit RGBA
				if (bitmap.PixelFormat != PixelFormat.Format32bppArgb) {
					Bitmap compatibleBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
					Graphics graphics = Graphics.FromImage(compatibleBitmap);
					Rectangle dest = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
					Rectangle source = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
					graphics.DrawImage(bitmap, dest, source, GraphicsUnit.Pixel);
					graphics.Dispose();
					bitmap.Dispose();
					bitmap = compatibleBitmap;
				}
				// extract raw data
				BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
				byte[] raw = new byte[data.Stride * data.Height];
				System.Runtime.InteropServices.Marshal.Copy(data.Scan0, raw, 0, data.Stride * data.Height);
				bitmap.UnlockBits(data);
				// finish
				OpenBveApi.Texture.TextureFormat format = new OpenBveApi.Texture.TextureFormat(bitmap.Width, bitmap.Height, 8);
				texture = new OpenBveApi.Texture.TextureData(format, raw);
				return General.Result.Successful;
			} catch {
				texture = null;
				return General.Result.InvalidData;
			}
		}
		
	}
}