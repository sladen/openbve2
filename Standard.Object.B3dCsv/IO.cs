using System;
using System.Globalization;

namespace Plugin {
	internal static class IO {
		
		// members
		private static CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
		
		// parse
		internal static bool Parse(string Expression, out int Value) {
			if (int.TryParse(Expression, NumberStyles.Integer, InvariantCulture, out Value)) {
				return true;
			} else {
				Expression = TrimInside(Expression);
				for (int n = Expression.Length; n >= 1; n--) {
					if (int.TryParse(Expression.Substring(0, n), NumberStyles.Integer, InvariantCulture, out Value)) {
						return true;
					}
				}
				return false;
			}
		}
		internal static bool Parse(string Expression, out double Value) {
			if (double.TryParse(Expression, NumberStyles.Float, InvariantCulture, out Value)) {
				return true;
			} else {
				Expression = TrimInside(Expression);
				for (int n = Expression.Length; n >= 1; n--) {
					if (double.TryParse(Expression.Substring(0, n), NumberStyles.Float, InvariantCulture, out Value)) {
						return true;
					}
				}
				return false;
			}
		}
		
		// trim inside
		private static string TrimInside(string Expression) {
			System.Text.StringBuilder builder = new System.Text.StringBuilder(Expression.Length);
			for (int i = 0; i < Expression.Length; i++) {
				char c = Expression[i];
				if (!char.IsWhiteSpace(c)) {
					builder.Append(c);
				}
			} 
			return builder.ToString();
		}
		
	}
}