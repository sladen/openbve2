using System;
using System.Globalization;
using System.Text;

namespace Plugin {
	internal static partial class Parser {
		
		private static void WriteExpressionsToFile(string file, Expression[] expressions) {
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("; --- General ---");
			builder.AppendLine();
			for (int i = 0; i < expressions.Length; i++) {
				if (expressions[i].Type == ExpressionType.General) {
					AppendExpression(builder, expressions[i], false);
				}
			}
			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine("; --- Track ---");
			builder.AppendLine();
			for (int i = 0; i < expressions.Length; i++) {
				if (expressions[i].Type == ExpressionType.Track) {
					AppendExpression(builder, expressions[i], true);
				}
			}
			System.IO.File.WriteAllText(file, builder.ToString(), new UTF8Encoding(true));
		}
		
		private static void AppendExpression(StringBuilder builder, Expression expression, bool outputPosition) {
			if (outputPosition) {
				builder.Append(expression.Position.ToString(CultureInfo.InvariantCulture).PadLeft(12)).Append(", ");
			}
			builder.Append(Escape(expression.CsvEquivalentCommand));
			if (expression.Indices != null) {
				builder.Append('(').Append(string.Join("; ", Escape(expression.Indices))).Append(')');
			}
			if (expression.Suffix != null) {
				builder.Append(Escape(expression.Suffix));
			}
			if (expression.Arguments != null) {
				builder.Append('(').Append(string.Join("; ", Escape(expression.Arguments))).Append(')');
			}
			builder.AppendLine();
		}
		
		private static string[] Escape(string[] texts) {
			string[] temp = new string[texts.Length];
			for (int i = 0; i < texts.Length; i++) {
				temp[i] = Escape(texts[i]);
			}
			return temp;
		}
		
		private static string Escape(string text) {
			text = text.Replace("\x09", "<tab>");
			text = text.Replace("\x0A", "<LF>");
			text = text.Replace("\x0C", "<FF>");
			text = text.Replace("\x0D", "<CR>");
			text = text.Replace("\x2C", "<comma>");
			text = text.Replace("\x28", "<opening-paranthesis>");
			text = text.Replace("\x29", "<closing-paranthesis>");
			text = text.Replace("\x3B", "<semicolon>");
			text = text.Replace("\x5B", "<opening-bracket>");
			text = text.Replace("\x5D", "<closing-bracket>");
			text = text.Replace("\x85", "<NEL>");
			text = text.Replace("\u2028", "<LS>");
			text = text.Replace("\u2029", "<PS>");
			return text;
		}
		
	}
}