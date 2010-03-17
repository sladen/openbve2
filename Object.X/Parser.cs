using System;
using System.Text;
using System.IO;
using System.IO.Compression;

/*
 * TODO: Check normals with a variety of X objects
 * TODO: Revisit texture wrapping mode at a later date
 */

namespace Plugin {
	internal static partial class Parser {
		// load from file
		/// <summary>Loads a .X object from a file and returns the success of the operation.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="fallback">The fallback encoding.</param>
		/// <param name="obj">Receives a compatible mesh from the object data, or null if the file could not be processed.</param>
		/// <returns>The success of the operation.</returns>
		internal static OpenBveApi.General.Result LoadFromFile(string fileName, System.Text.Encoding fallback, out OpenBveApi.Geometry.GenericObject obj) {
			/*
			 * initialize
			 */
			OpenBveApi.Geometry.GenericObject mesh = null;
			obj = null;
			/*
			 * prepare
			 */
			byte[] data = System.IO.File.ReadAllBytes(fileName);
			if (data.Length < 16 || data[0] != 120 | data[1] != 111 | data[2] != 102 | data[3] != 32) {
				/* 
				 * not an x object
				 */
				IO.ReportError(fileName, "Invalid X object file encountered");
				return OpenBveApi.General.Result.InvalidData;
			}
			if (data[4] != 48 | data[5] != 51 | data[6] != 48 | data[7] != 50 & data[7] != 51) {
				/* 
				 * unrecognized version
				 */
				System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
				string s = new string(ascii.GetChars(data, 4, 4));
				IO.ReportError(fileName, "Unsupported X object file version " + s + " encountered");
			}
			/*
			 * floating-point format
			 */
			int floatingPointSize;
			if (data[12] == 48 & data[13] == 48 & data[14] == 51 & data[15] == 50) {
				floatingPointSize = 32;
			} else if (data[12] == 48 & data[13] == 48 & data[14] == 54 & data[15] == 52) {
				floatingPointSize = 64;
			} else {
				IO.ReportError(fileName, "Unsupported floating point format encountered in X object");
				return OpenBveApi.General.Result.InvalidData;
			}
			/* 
			 * supported floating point format
			 */
			if (data[8] == 116 & data[9] == 120 & data[10] == 116 & data[11] == 32) {
				/*
				 * textual flavor
				 */
				mesh = LoadTextualX(fileName, System.IO.File.ReadAllText(fileName), fallback);
			} else if (data[8] == 98 & data[9] == 105 & data[10] == 110 & data[11] == 32) {
				/*
				 * binary flavor
				 */
				mesh = LoadBinaryX(fileName, data, 16, fallback, floatingPointSize);
			} else if (data[8] == 116 & data[9] == 122 & data[10] == 105 & data[11] == 112) {
				/* 
				 * compressed textual flavor
				 */
				#if !DEBUG
				try {
					#endif
					byte[] uncompressed = Decompress(data);
					string text = fallback.GetString(uncompressed);
					mesh = LoadTextualX(fileName, text, fallback);
					#if !DEBUG
				} catch (Exception ex) {
					IO.ReportError(fileName, true, "An unexpected error occured (" + ex.Message + ")  while attempting to decompress data");
					return OpenBveApi.General.Result.InvalidData;
				}
				#endif
			} else if (data[8] == 98 & data[9] == 122 & data[10] == 105 & data[11] == 112) {
				/*
				 * compressed binary flavor
				 */
				#if !DEBUG
				try {
					#endif
					byte[] uncompressed = Decompress(data);
					mesh = LoadBinaryX(fileName, uncompressed, 0, fallback, floatingPointSize);
					#if !DEBUG
				} catch (Exception ex) {
					IO.ReportError(fileName, true, "An unexpected error occured (" + ex.Message + ")  while attempting to decompress data");
					return OpenBveApi.General.Result.InvalidData;
				}
				#endif
			} else {
				/*
				 * unsupported flavor
				 */
				IO.ReportError(fileName, "Unsupported X object file encountered");
				return OpenBveApi.General.Result.InvalidData;
			}
			/*
			 * finish
			 */
			if (mesh != null) {
				obj = (OpenBveApi.Geometry.GenericObject)mesh;
				return OpenBveApi.General.Result.Successful;
			} else {
				return OpenBveApi.General.Result.InvalidData;
			}
		}
		
		// decompress
		/// <summary>Decompresses data within a compressed X object file and returns the uncompressed data.</summary>
		/// <param name="data">The compressed data stream.</param>
		/// <returns>The decompressed data stream.</returns>
		private static byte[] Decompress(byte[] data) {
			byte[] target;
			using (MemoryStream inputStream = new MemoryStream(data)) {
				inputStream.Position = 26;
				using (DeflateStream deflate = new DeflateStream(inputStream, CompressionMode.Decompress, true)) {
					using (MemoryStream outputStream = new MemoryStream()) {
						byte[] buffer = new byte[4096];
						while (true) {
							int count = deflate.Read(buffer, 0, buffer.Length);
							if (count != 0) {
								outputStream.Write(buffer, 0, count);
							}
							if (count != buffer.Length) {
								break;
							}
						}
						target = new byte[outputStream.Length];
						outputStream.Position = 0;
						outputStream.Read(target, 0, target.Length);
					}
				}
			}
			return target;
		}
		
		// get template
		/// <summary>Gets a .X object template.</summary>
		/// <param name="name">The template name.</param>
		/// <returns>A .X object template.</returns>
		private static Template GetTemplate(string name) {
			for (int i = 0; i < Templates.Length; i++) {
				if (Templates[i].Name == name) {
					return Templates[i];
				}
			}
			return new Template(name, new string[] { "[???]" });
		}
	}
}








