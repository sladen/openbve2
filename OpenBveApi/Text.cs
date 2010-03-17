using System;
using System.Text;

namespace OpenBveApi {
	/// <summary>Provides functions for dealing with textual data.</summary>
	public static class Text {
		
		
		// --- public functions ---

		/// <summary>Splits a text at line boundaries and returns an array of lines.</summary>
		/// <param name="text">The text.</param>
		/// <returns>The array of lines in the text.</returns>
		public static string[] SplitLines(string text) {
			return text.Split(Newlines, StringSplitOptions.None);
		}
		
		/// <summary>Takes an array of bytes representing text, identifies the encoding of that text by the byte order mark, and returns the corresponding text.</summary>
		/// <param name="data">The array of bytes representing the text.</param>
		/// <param name="fallback">A fallback encoding in case the encoding cannot be determined.</param>
		/// <returns>The text represented by the byte data.</returns>
		/// <exception cref="System.NullReferenceException">Raised when the array of bytes is a null reference.</exception>
		public static string GetTextFromBytes(byte[] data, Encoding fallback) {
			Encoding encoding;
			if (GetEncodingFromBytes(data, out encoding)) {
				return encoding.GetString(data);
			} else {
				return fallback.GetString(data);
			}
		}
		
		/// <summary>Takes an array of bytes representing text, identifies the encoding of that text by the byte order mark, and returns an array of lines of the corresponding text.</summary>
		/// <param name="data">The array of bytes representing the text.</param>
		/// <param name="fallback">A fallback encoding in case the encoding cannot be determined.</param>
		/// <returns>The array of lines in the text represented by the byte data.</returns>
		/// <exception cref="System.NullReferenceException">Raised when the array of bytes is a null reference.</exception>
		public static string[] GetLinesFromBytes(byte[] data, Encoding fallback) {
			string text = GetTextFromBytes(data, fallback);
			return SplitLines(text);
		}
		
		/// <summary>Takes a text file, identifies its encoding, and returns the corresponding text.</summary>
		/// <param name="file">The text file.</param>
		/// <param name="fallback">A fallback encoding in case the encoding cannot be determined.</param>
		/// <returns>The text contained in the file.</returns>
		public static string GetTextFromFile(string file, Encoding fallback) {
			byte[] data = System.IO.File.ReadAllBytes(file);
			return GetTextFromBytes(data, fallback);
		}

		/// <summary>Takes a text file, identifies its encoding, and returns an array of lines of the corresponding text.</summary>
		/// <param name="file">The text file.</param>
		/// <param name="fallback">A fallback encoding in case the encoding cannot be determined.</param>
		/// <returns>The array of lines of the text contained in the file.</returns>
		public static string[] GetLinesFromFile(string file, Encoding fallback) {
			byte[] data = System.IO.File.ReadAllBytes(file);
			return GetLinesFromBytes(data, fallback);
		}

		/// <summary>Takes an array of bytes representing text, identifies the encoding of that text by the byte order mark, and returns the corresponding encoding in an output parameter.</summary>
		/// <param name="data">The byte data of the text.</param>
		/// <param name="encoding">Receives the encoding on success.</param>
		/// <returns>A boolean indicating whether a byte order mark was found and the matching encoding is available on this system.</returns>
		/// <exception cref="System.NullReferenceException">Raised when the array of bytes is a null reference.</exception>
		public static bool GetEncodingFromBytes(byte[] data, out Encoding encoding) {
			if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF) {
				return TryGetEncoding("utf-8", out encoding);
			} else if (data.Length >= 2 && data[0] == 0xFE && data[1] == 0xFF) {
				return TryGetEncoding("unicodeFFFE", out encoding);
			} else if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xFE) {
				return TryGetEncoding("utf-16", out encoding);
			} else if (data.Length >= 4 && data[0] == 0x00 && data[1] == 0x00 && data[2] == 0xFE && data[3] == 0xFF) {
				return TryGetEncoding("utf-32BE", out encoding);
			} else if (data.Length >= 4 && data[0] == 0xFF && data[1] == 0xFE && data[2] == 0x00 && data[3] == 0x00) {
				return TryGetEncoding("utf-32", out encoding);
			} else if (data.Length >= 4 && data[0] == 0x2B && data[1] == 0x2F && data[2] == 0x76 && (data[3] == 0x38 || data[3] == 0x39 || data[3] == 0x2B || data[3] == 0x2F)) {
				return TryGetEncoding("utf-7", out encoding);
			} else if (data.Length >= 4 && data[0] == 0x84 && data[1] == 0x31 && data[2] == 0x95 && data[3] == 0x33) {
				return TryGetEncoding("GB18030", out encoding);
			} else {
				encoding = null;
				return false;
			}
		}
		
		
		// --- private fields and functions ---
		
		/// <summary>Represents the set of Unicode newlines.</summary>
		private static readonly string[] Newlines = new string[] { "\x0D\x0A", "\x0A", "\x0C", "\x0D", "\x85", "\u2028", "\u2029" };

		/// <summary>Gets a specified encoding by name provided that the specified encoding is available on this system.</summary>
		/// <param name="name">The name of the encoding.</param>
		/// <param name="encoding">On success, receives the encoding.</param>
		/// <returns>The success of the operation.</returns>
		private static bool TryGetEncoding(string name, out Encoding encoding) {
			try {
				encoding = Encoding.GetEncoding(name);
				return true;
			} catch {
				encoding = null;
				return false;
			}
		}
		
	}
}