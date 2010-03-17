using System;
using System.Globalization;

namespace Plugin {
	internal static partial class Parser {
		
		// --- classes ---
		
		/// <summary>Represents the type of an expression.</summary>
		internal enum ExpressionType {
			/// <summary>The type of the expression has not been determined yet.</summary>
			Unknown = 0,
			/// <summary>The expression is a track position.</summary>
			Position = 1,
			/// <summary>The expression is from a namespace other than Track.</summary>
			General = 2,
			/// <summary>The expression is from the Track namespace.</summary>
			Track = 3
		}
		
		/// <summary>Represents an expression consisting of a command, and optionally, indices, a suffix or arguments.</summary>
		internal class Expression {
			// members
			/// <summary>The file in which the expression occured.</summary>
			internal string File;
			/// <summary>The zero-based row at which the expression occured.</summary>
			internal int Row;
			/// <summary>The zero-based column at which the expression occured.</summary>
			internal int Column;
			/// <summary>The name of the command as it appears in the file.</summary>
			internal string OriginalCommand;
			/// <summary>The name of the command in CSV-equivalent form.</summary>
			internal string CsvEquivalentCommand;
			/// <summary>An array of indices. This field may be a null reference.</summary>
			internal string[] Indices;
			/// <summary>The suffix to the command. This field may be a null reference.</summary>
			internal string Suffix;
			/// <summary>An array of arguments. This field may be a null reference.</summary>
			internal string[] Arguments;
			/// <summary>The type of expression.</summary>
			internal ExpressionType Type;
			/// <summary>For commands from the Track namespace, this stores the associated track position in text form.</summary>
			internal string PositionString;
			/// <summary>For commands from the Track namespace, this stores the associated track position as a number.</summary>
			internal double Position;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="file">The file in which the expression occured.</param>
			/// <param name="row">The zero-based row at which the expression occured.</param>
			/// <param name="column">The zero-based column at which the expression occured.</param>
			/// <param name="command">The name of the command.</param>
			/// <param name="indices">An array of indices. This field may be a null reference.</param>
			/// <param name="suffix">The suffix to the command. This field may be a null reference.</param>
			/// <param name="arguments">An array of arguments. This field may be a null reference.</param>
			/// <remarks>This constructor may only be used for CSV routes.</remarks>
			internal Expression(string file, int row, int column, string command, string[] indices, string suffix, string[] arguments) {
				this.File = file;
				this.Row = row;
				this.Column = column;
				this.OriginalCommand = command;
				this.CsvEquivalentCommand = command;
				this.Indices = indices;
				this.Suffix = suffix;
				this.Arguments = arguments;
				this.PositionString = null;
				this.Position = 0.0;
			}
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="file">The file in which the expression occured.</param>
			/// <param name="row">The zero-based row at which the expression occured.</param>
			/// <param name="column">The zero-based column at which the expression occured.</param>
			/// <param name="originalCommand">The name of the command as it appears in the file.</param>
			/// <param name="csvEquivalentCommand">The name of the command in CSV-equivalent form.</param>
			/// <param name="indices">An array of indices. This field may be a null reference.</param>
			/// <param name="suffix">The suffix to the command. This field may be a null reference.</param>
			/// <param name="arguments">An array of arguments. This field may be a null reference.</param>
			internal Expression(string file, int row, int column, string originalCommand, string csvEquivalentCommand, string[] indices, string suffix, string[] arguments) {
				this.File = file;
				this.Row = row;
				this.Column = column;
				this.OriginalCommand = originalCommand;
				this.CsvEquivalentCommand = csvEquivalentCommand;
				this.Indices = indices;
				this.Suffix = suffix;
				this.Arguments = arguments;
				this.PositionString = null;
				this.Position = 0.0;
			}
		}
		
		
		// --- functions ---
		
		/// <summary>Takes a cell from a CSV file and splits it into command, indices, suffix and arguments.</summary>
		/// <param name="file">The path to the route file.</param>
		/// <param name="row">The zero-based row at which the cell is stored.</param>
		/// <param name="column">The zero-based column at which the cell is stored.</param>
		/// <param name="cell">The content of the trimmed cell.</param>
		/// <param name="with">The last argument to the With command.</param>
		/// <param name="expression">Receives the expression on success.</param>
		/// <returns>Whether an expression could be extracted.</returns>
		private static bool GetExpressionFromCsvCell(string file, int row, int column, string cell, ref string with, out Expression expression) {
			/*
			 * The following valid syntax variations are detected by this algorithm:
			 * 
			 * command (indexSequence) .suffix (argumentSequence)     command, indices, suffix, arguments
			 * command (indexSequence) .suffix argumentSequence       command, indices, suffix, arguments
			 * command (indexSequence) .suffix                        command, indices, suffix
			 * command (indexSequence) (argumentSequence)             command, indices, arguments
			 * command (indexSequence) argumentSequence               command, indices, arguments
			 * command (argumentSequence)                             command, arguments
			 * command argumentSequence                               command, arguments
			 * command                                                command only
			 * 
			 * Non-valid syntax is reported but still processed for the best of it.
			 * */
			if (cell.Length == 0) {
				/*
				 * The cell is empty.
				 * */
				expression = null;
				return false;
			} else if (cell[0] == '.') {
				/*
				 * The cell starts with a period. Append it to the
				 * argument to the last With command. If no With
				 * command was used before, this is invalid.
				 * */
				if (with == null) {
					IO.ReportInvalidData(file, row, column, "A With statement is required before commands starting with a period can be used.");
					expression = null;
					return false;
				} else {
					cell = with + cell;
				}
			}
			/*
			 * Find the first character that is not part of
			 * the command and not part of whitespaces
			 * following the command.
			 * */
			int firstNonCommandNonWhitespace;
			for (firstNonCommandNonWhitespace = 0; firstNonCommandNonWhitespace < cell.Length; firstNonCommandNonWhitespace++) {
				if (cell[firstNonCommandNonWhitespace] == '(') {
					break;
				} else if (char.IsWhiteSpace(cell[firstNonCommandNonWhitespace])) {
					firstNonCommandNonWhitespace++;
					while (true) {
						if (!char.IsWhiteSpace(cell[firstNonCommandNonWhitespace])) {
							break;
						} else {
							firstNonCommandNonWhitespace++;
						}
					}
					break;
				}
			}
			if (firstNonCommandNonWhitespace == cell.Length) {
				/*
				 * Neither a whitespace or an opening paranthesis was found.
				 * The entire cell is just the command.
				 * 
				 * command
				 * */
				expression = new Expression(file, row, column, cell, null, null, null);
			} else if (cell[firstNonCommandNonWhitespace] == '(') {
				/*
				 * An opening paranthesis was found. This could mark
				 * the start of indices or of arguments. Let's find
				 * the matching closing paranthesis first.
				 * */
				string command = cell.Substring(0, firstNonCommandNonWhitespace).TrimEnd();
				int closingParanthesis = cell.IndexOf(')', firstNonCommandNonWhitespace + 1);
				if (closingParanthesis != -1) {
					/*
					 * The closing paranthesis was found. The text in-between
					 * the parantheses could be indices or arguments. If the
					 * closing paranthesis is at the end of the cell, the
					 * text is the arguments, otherwise, the indices.
					 * 
					 * command (indexSequence) .suffix (argumentSequence)
					 * command (indexSequence) .suffix argumentSequence
					 * command (indexSequence) .suffix
					 * command (indexSequence) (argumentSequence)
					 * command (indexSequence) argumentSequence
					 * command (argumentSequence)
					 * */
					if (closingParanthesis == cell.Length - 1) {
						/*
						 * The closing paranthesis is at the end of the cell.
						 * The text in-between the parantheses is the
						 * arguments. Indices or suffixes do not occur.
						 * If the argument sequence contains further
						 * opening parantheses, this is invalid.
						 * 
						 * command (argumentSequence)
						 * */
						string argumentSequence = cell.Substring(firstNonCommandNonWhitespace + 1, closingParanthesis - firstNonCommandNonWhitespace - 1);
						if (argumentSequence.IndexOf('(') != -1) {
							IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid opening paranthesis.");
						}
						string[] arguments = argumentSequence.Split(';');
						for (int k = 0; k < arguments.Length; k++) {
							arguments[k] = arguments[k].Trim();
						}
						expression = new Expression(file, row, column, command, null, null, arguments);
					} else {
						/*
						 * The closing paranthesis is followed by something.
						 * The text in-between the parantheses is the indices.
						 * If the index sequence contains further opening
						 * parantheses, this is invalid.
						 * 
						 * command (indexSequence) .suffix (argumentSequence)
						 * command (indexSequence) .suffix argumentSequence
						 * command (indexSequence) .suffix
						 * command (indexSequence) (argumentSequence)
						 * command (indexSequence) argumentSequence
						 * */
						string indexSequence = cell.Substring(firstNonCommandNonWhitespace + 1, closingParanthesis - firstNonCommandNonWhitespace - 1);
						if (indexSequence.IndexOf('(') != -1) {
							IO.ReportInvalidData(file, row, column, "The index sequence contains an invalid opening paranthesis.");
						}
						string[] indices = indexSequence.Split(';');
						for (int k = 0; k < indices.Length; k++) {
							indices[k] = indices[k].Trim();
						}
						/*
						 * If the first non-whitespace character after the
						 * closing paranthesis is a period, this marks the
						 * start of a suffix, otherwise, of arguments.
						 * */
						int firstNonWhitespace;
						for (firstNonWhitespace = closingParanthesis + 1; firstNonWhitespace < cell.Length; firstNonWhitespace++) {
							if (!char.IsWhiteSpace(cell[firstNonWhitespace])) {
								break;
							}
						}
						if (cell[firstNonWhitespace] == '.') {
							/*
							 * The first non-whitespace character is a period.
							 * This marks the start of a suffix. The suffix
							 * terminates at the first whitespace or opening
							 * paranthesis. If not present, the rest of the
							 * cell is just the suffix.
							 * 
							 * command (indexSequence) .suffix (argumentSequence)
							 * command (indexSequence) .suffix argumentSequence
							 * command (indexSequence) .suffix
							 * */
							int firstNonSuffix;
							for (firstNonSuffix = firstNonWhitespace + 1; firstNonSuffix < cell.Length; firstNonSuffix++) {
								if (cell[firstNonSuffix] == '(' || char.IsWhiteSpace(cell[firstNonSuffix])) {
									break;
								}
							}
							if (firstNonSuffix == cell.Length) {
								/*
								 * No whitespace or opening parantheses was found.
								 * The rest of the cell is just the suffix. If
								 * the suffix contains closing parantheses, this
								 * is invalid. If the suffix ends in a period,
								 * this is invalid as well.
								 * 
								 * command (indexSequence) .suffix
								 * */
								string suffix = cell.Substring(firstNonWhitespace);
								if (suffix.IndexOf(')') != -1) {
									IO.ReportInvalidData(file, row, column, "The suffix contains an invalid closing paranthesis.");
								} else if (suffix[suffix.Length - 1] == '.') {
									IO.ReportInvalidData(file, row, column, "The suffix must not end in a period.");
								}
								expression = new Expression(file, row, column, command, indices, suffix, null);
							} else {
								/*
								 * A whitespace or opening paranthesis was found.
								 * This marks the end of the suffix and the start
								 * of arguments. The arguments may be enclosed
								 * by parantheses. Other occurences of parantheses
								 * are invalid.
								 * 
								 * command (indexSequence) .suffix (argumentSequence)
								 * command (indexSequence) .suffix argumentSequence
								 * */
								string suffix = cell.Substring(firstNonWhitespace, firstNonSuffix - firstNonWhitespace);
								string argumentSequence = cell.Substring(firstNonSuffix).TrimStart();
								if (argumentSequence[0] == '(' & argumentSequence[argumentSequence.Length - 1] == ')') {
									argumentSequence = argumentSequence.Substring(1, argumentSequence.Length - 2);
									if (argumentSequence.IndexOf('(') != -1) {
										IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid opening paranthesis.");
									} else if (argumentSequence.IndexOf(')') != -1) {
										IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid closing paranthesis.");
									}
								} else if (argumentSequence[0] == '(') {
									IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid opening paranthesis.");
									argumentSequence = argumentSequence.Substring(1);
								} else if (argumentSequence[argumentSequence.Length - 1] == ')') {
									IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid closing paranthesis.");
									argumentSequence = argumentSequence.Substring(0, argumentSequence.Length - 1);
								}
								string[] arguments = argumentSequence.Split(';');
								for (int k = 0; k < arguments.Length; k++) {
									arguments[k] = arguments[k].Trim();
								}
								expression = new Expression(file, row, column, command, indices, suffix, arguments);
							}
						} else {
							/*
							 * The first non-whitespace character is not a period.
							 * This marks the start of arguments. The arguments
							 * may be enclosed by parantheses. Other occurences
							 * of parantheses are invalid.
							 * 
							 * command (indexSequence) (argumentSequence)
							 * command (indexSequence) argumentSequence
							 * */
							string argumentSequence = cell.Substring(closingParanthesis + 1).TrimStart();
							if (argumentSequence[0] == '(' & argumentSequence[argumentSequence.Length - 1] == ')') {
								argumentSequence = argumentSequence.Substring(1, argumentSequence.Length - 2);
								if (argumentSequence.IndexOf('(') != -1) {
									IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid opening paranthesis.");
								} else if (argumentSequence.IndexOf(')') != -1) {
									IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid closing paranthesis.");
								}
							} else if (argumentSequence[0] == '(') {
								IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid opening paranthesis.");
								argumentSequence = argumentSequence.Substring(1);
							} else if (argumentSequence[argumentSequence.Length - 1] == ')') {
								IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid closing paranthesis.");
								argumentSequence = argumentSequence.Substring(0, argumentSequence.Length - 1);
							}
							string[] arguments = argumentSequence.Split(';');
							for (int k = 0; k < arguments.Length; k++) {
								arguments[k] = arguments[k].Trim();
							}
							expression = new Expression(file, row, column, command, indices, null, arguments);
						}
					}
				} else {
					/*
					 * A closing paranthesis was not found. This
					 * is invalid. Let's treat the text after the
					 * opening paranthesis as arguments.
					 * */
					IO.ReportInvalidData(file, row, column, "Missing closing paranthesis.");
					string argumentSequence = cell.Substring(firstNonCommandNonWhitespace + 1);
					string[] arguments = argumentSequence.Split(';');
					for (int k = 0; k < arguments.Length; k++) {
						arguments[k] = arguments[k].Trim();
					}
					expression = new Expression(file, row, column, command, null, null, arguments);
				}
			} else {
				/*
				 * Whitespace was found before any opening paranthesis.
				 * The text following the whitespace is the arguments.
				 * 
				 * command argumentSequence
				 * */
				string command = cell.Substring(0, firstNonCommandNonWhitespace).TrimEnd();
				string argumentSequence = cell.Substring(firstNonCommandNonWhitespace);
				string[] arguments = argumentSequence.Split(';');
				for (int k = 0; k < arguments.Length; k++) {
					arguments[k] = arguments[k].Trim();
				}
				expression = new Expression(file, row, column, command, null, null, arguments);
			}
			/*
			 * Now that we have the expression, check if it is
			 * a With command, and extract the argument to it
			 * if applicable.
			 * */
			if (expression.CsvEquivalentCommand.Equals("With", StringComparison.OrdinalIgnoreCase)) {
				if (expression.Arguments == null) {
					IO.ReportInvalidData(file, row, column, "The With statement must have exactly one argument.");
				} else {
					if (expression.Indices != null | expression.Suffix != null | expression.Arguments.Length != 1) {
						IO.ReportInvalidData(file, row, column, "The With statement must not contain indices or a suffix, and must have exactly one argument.");
					}
					if (expression.Arguments[0][0] == '.' | expression.Arguments[0][expression.Arguments[0].Length - 1] == '.') {
						IO.ReportInvalidData(file, row, column, "The argument to the With statement must not start or end with a period.");
					} else {
						with = expression.Arguments[0];
					}
				}
				return false;
			} else {
				return true;
			}
		}
		
		/// <summary>Takes a cell from an RW file and splits it into command, indices, suffix and arguments.</summary>
		/// <param name="file">The path to the route file.</param>
		/// <param name="row">The zero-based row at which the cell is stored.</param>
		/// <param name="column">The zero-based column at which the cell is stored.</param>
		/// <param name="cell">The content of the trimmed cell.</param>
		/// <param name="section">The last opened section.</param>
		/// <param name="expression">Receives the expression on success.</param>
		/// <returns>Whether an expression could be extracted.</returns>
		private static bool GetExpressionFromRwCell(string file, int row, int column, string cell, string section, out Expression expression) {
			/*
			 * The following valid syntax variations are detected by this algorithm:
			 * 
			 * command (indexSequence) .suffix = argumentSequence     command, indices, suffix, arguments
			 * command (indexSequence) = argumentSequence             command, indices, arguments
			 * command = argumentSequence                             command, arguments
			 * 
			 * @ command (indexSequence) .suffix (argumentSequence)   command, indices, suffix, arguments
			 * @ command (indexSequence) (argumentSequence)           command, indices, arguments
			 * @ command (indexSequence) .suffix                      command, indices, suffix
			 * @ command (argumentSequence)                           command, arguments
			 * @ command                                              command only
			 * 
			 * The @ character is optional. Non-valid syntax is reported.
			 * 
			 * The syntax variations with the equals sign are not possible
			 * for the [Railway] section and are thus not considered.
			 * 
			 * The syntax variations without the equals sign are invalid
			 * when used outside the [Railway] section.
			 * */
			if (cell.Length == 0) {
				/*
				 * The cell is empty.
				 * */
				expression = null;
				return false;
			} else {
				/*
				 * Check if the cell contains an equals sign that
				 * determines the possible syntax variations, but
				 * only if outside the [Railway] section.
				 * */
				int equals;
				bool isRailwaySection = section.Equals("railway", StringComparison.OrdinalIgnoreCase);
				if (isRailwaySection) {
					equals = -1;
				} else {
					equals = cell.IndexOf('=');
				}
				if (equals >= 0) {
					/*
					 * An equals sign was found. This means the argument
					 * sequence will follow the equals sign. If the text
					 * prior to the equals sign contains parantheses,
					 * this marks the presence of indices.
					 * 
					 * command (indexSequence) .suffix = argumentSequence
					 * command (indexSequence) = argumentSequence
					 * command = argumentSequence
					 * */
					string argumentSequence = cell.Substring(equals + 1).TrimStart();
					string[] arguments = argumentSequence.Split(',', ';');
					for (int k = 0; k < arguments.Length; k++) {
						arguments[k] = arguments[k].Trim();
					}
					string commandSequence = cell.Substring(0, equals).TrimEnd();
					int openingParanthesis = commandSequence.IndexOf('(');
					if (openingParanthesis >= 0) {
						/*
						 * An opening paranthesis was found.
						 * This marks the start of indices.
						 * Let's find the matching closing
						 * parantheses first.
						 * 
						 * command (indexSequence) .suffix = argumentSequence
						 * command (indexSequence) = argumentSequence
						 * */
						string originalCommand = commandSequence.Substring(0, openingParanthesis).TrimEnd();
						string csvEquivalentCommand = GetCsvEquivalentCommand(file, row, column, originalCommand, section, false);
						int closingParanthesis = commandSequence.IndexOf(')', openingParanthesis + 1);
						if (closingParanthesis >= 0) {
							/*
							 * A closing paranthesis was found.
							 * */
							string indexSequence = commandSequence.Substring(openingParanthesis + 1, closingParanthesis - openingParanthesis - 1);
							if (indexSequence.IndexOf('(') != -1) {
								IO.ReportInvalidData(file, row, column, "The index sequence contains an invalid opening paranthesis.");
							}
							string[] indices = indexSequence.Split(',', ';');
							for (int k = 0; k < indices.Length; k++) {
								indices[k] = indices[k].Trim();
							}
							if (closingParanthesis == commandSequence.Length - 1) {
								/*
								 * The closing paranthesis was found at the end
								 * of the command sequence (just before the
								 * equals sign). This means that there is
								 * no suffix.
								 * 
								 * command (indexSequence) = argumentSequence
								 * */
								expression = new Expression(file, row, column, originalCommand, csvEquivalentCommand, indices, null, arguments);
							} else {
								/*
								 * The closing paranthesis was found before
								 * the end of the command sequence. This
								 * means that the indices are followed by
								 * a suffix. The suffix must start in a
								 * period and must not end in a period.
								 * 
								 * command (indexSequence) .suffix = argumentSequence
								 * */
								string suffix = commandSequence.Substring(closingParanthesis + 1).TrimStart();
								if (suffix.IndexOf('(') != -1) {
									IO.ReportInvalidData(file, row, column, "The suffix contains an invalid opening paranthesis.");
								}
								if (suffix[0] != '.' || suffix[suffix.Length - 1] == '.') {
									IO.ReportInvalidData(file, row, column, "The suffix must start with a period but must not end in a period.");
								}
								expression = new Expression(file, row, column, originalCommand, csvEquivalentCommand, indices, suffix, arguments);
							}
						} else {
							/*
							 * A closing paranthesis was not found. This
							 * is invalid. Let's treat the text after the
							 * opening paranthesis as indices.
							 * */
							IO.ReportInvalidData(file, row, column, "Missing closing paranthesis.");
							string indexSequence = commandSequence.Substring(openingParanthesis + 1).TrimStart();
							string[] indices = indexSequence.Split(',', ';');
							for (int k = 0; k < indices.Length; k++) {
								indices[k] = indices[k].Trim();
							}
							expression = new Expression(file, row, column, originalCommand, csvEquivalentCommand, indices, null, arguments);
						}
					} else {
						/*
						 * An opening paranthesis was not found. There
						 * is only the command and the arguments.
						 * 
						 * command = argumentSequence
						 * */
						if (commandSequence.IndexOf(')') != -1) {
							IO.ReportInvalidData(file, row, column, "The command contains an invalid closing paranthesis.");
						}
						string originalCommand = commandSequence;
						string csvEquivalentCommand = GetCsvEquivalentCommand(file, row, column, originalCommand, section, false);
						expression = new Expression(file, row, column, originalCommand, csvEquivalentCommand, null, null, arguments);
					}
				} else {
					/*
					 * No equals sign was found. If this situation is
					 * encountered outside the [Railway] section, it
					 * is invalid. Otherwise, now try to find an
					 * an opening paranthesis.
					 * 
					 * @ command (indexSequence) .suffix (argumentSequence)
					 * @ command (indexSequence) (argumentSequence)
					 * @ command (indexSequence) .suffix
					 * @ command (argumentSequence)
					 * @ command
					 * */
					if (!isRailwaySection) {
						IO.ReportInvalidData(file, row, column, "This syntax is not allowed outside the [Railway] section.");
					}
					int openingParanthesis = cell.IndexOf('(');
					if (openingParanthesis >= 0) {
						/*
						 * An opening paranthesis was found. This can
						 * mark the start of indices or arguments.
						 * First, find the matching closing paranthesis.
						 * */
						string originalCommand = cell.Substring(0, openingParanthesis).TrimEnd();
						if (originalCommand.IndexOf(')') != -1) {
							IO.ReportInvalidData(file, row, column, "The command contains an invalid closing paranthesis.");
						}
						string csvEquivalentCommand = GetCsvEquivalentCommand(file, row, column, originalCommand, section, true);
						int closingParanthesis = cell.IndexOf(')', openingParanthesis + 1);
						if (closingParanthesis >= 0) {
							/*
							 * The matching closing paranthesis was found. If
							 * it is at the end of the cell, this marks the
							 * end of arguments, otherwise of indices.
							 * 
							 * @ command (indexSequence) .suffix (argumentSequence)
							 * @ command (indexSequence) (argumentSequence)
							 * @ command (indexSequence) .suffix
							 * @ command (argumentSequence)
							 * */
							if (closingParanthesis == cell.Length - 1) {
								/*
								 * The matching closing paranthesis is at
								 * the end of the cell. The text between
								 * the opening and closing parantheses
								 * is the arguments.
								 * 
								 * @ command (argumentSequence)
								 * */
								string argumentSequence = cell.Substring(openingParanthesis + 1, closingParanthesis - openingParanthesis - 1);
								if (argumentSequence.IndexOf('(') != -1) {
									IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid opening paranthesis.");
								}
								string[] arguments = argumentSequence.Split(',');
								for (int k = 0; k < arguments.Length; k++) {
									arguments[k] = arguments[k].Trim();
								}
								expression = new Expression(file, row, column, originalCommand, csvEquivalentCommand, null, null, arguments);
							} else {
								/*
								 * The matching closing paranthesis is before
								 * the end of the cell. The text between the
								 * opening and closing parantheses is the
								 * indices. If another opening paranthesis
								 * is found after the closing paranthesis,
								 * this marks the start of arguments.
								 * 
								 * @ command (indexSequence) .suffix (argumentSequence)
								 * @ command (indexSequence) (argumentSequence)
								 * @ command (indexSequence) .suffix
								 * */
								string indexSequence = cell.Substring(openingParanthesis + 1, closingParanthesis - openingParanthesis - 1);
								if (indexSequence.IndexOf('(') != -1) {
									IO.ReportInvalidData(file, row, column, "The index sequence contains an invalid opening paranthesis.");
								}
								string[] indices = indexSequence.Split(',');
								for (int k = 0; k < indices.Length; k++) {
									indices[k] = indices[k].Trim();
								}
								openingParanthesis = cell.IndexOf('(', closingParanthesis + 1);
								if (openingParanthesis >= 0) {
									/*
									 * Another opening paranthesis was found. The cell
									 * must end in a closing paranthesis and must not
									 * contain further parantheses.
									 * 
									 * @ command (indexSequence) .suffix (argumentSequence)
									 * @ command (indexSequence) (argumentSequence)
									 * */
									string suffix = cell.Substring(closingParanthesis + 1, openingParanthesis - closingParanthesis - 1).Trim();
									if (suffix.Length != 0) {
										if (suffix.IndexOf(')') != -1) {
											IO.ReportInvalidData(file, row, column, "The suffix contains an invalid closing paranthesis.");
										}
										if (suffix[0] != '.' || suffix[suffix.Length - 1] == '.') {
											IO.ReportInvalidData(file, row, column, "The suffix must start with a period but must not end in a period.");
										}
									} else {
										suffix = null;
									}
									closingParanthesis = cell.IndexOf(')', openingParanthesis + 1);
									string argumentSequence;
									if (closingParanthesis >= 0) {
										if (closingParanthesis != cell.Length - 1) {
											/*
											 * The closing paranthesis was found before
											 * the end of the cell. This is invalid.
											 * */
											IO.ReportInvalidData(file, row, column, "The cell must end in a closing paranthesis.");
										}
										argumentSequence = cell.Substring(openingParanthesis + 1, closingParanthesis - openingParanthesis - 1);
									} else {
										/*
										 * No closing paranthesis was found. This is invalid.
										 * */
										argumentSequence = cell.Substring(openingParanthesis + 1);
										IO.ReportInvalidData(file, row, column, "Missing closing paranthesis.");
									}
									if (argumentSequence.IndexOf('(') != -1) {
										IO.ReportInvalidData(file, row, column, "The argument sequence contains an invalid opening paranthesis.");
									}
									string[] arguments = argumentSequence.Split(',');
									for (int k = 0; k < arguments.Length; k++) {
										arguments[k] = arguments[k].Trim();
									}
									expression = new Expression(file, row, column, originalCommand, csvEquivalentCommand, indices, suffix, arguments);
								} else {
									/*
									 * Another opening paranthesis was not found.
									 * The rest of the cell is the suffix.
									 * 
									 * @ command (indexSequence) .suffix
									 * */
									string suffix = cell.Substring(closingParanthesis + 1).TrimStart();
									if (suffix.IndexOf(')') != -1) {
										IO.ReportInvalidData(file, row, column, "The suffix contains an invalid closing paranthesis.");
										if (suffix.Length == 1) {
											/*
											 * Special invalid case. What we thought were
											 * indices were meant to be arguments.
											 * */
											expression = new Expression(file, row, column, originalCommand, csvEquivalentCommand, null, null, indices);
											return true;
										}
									}
									if (suffix[0] != '.' || suffix[suffix.Length - 1] == '.') {
										IO.ReportInvalidData(file, row, column, "The suffix must start with a period but must not end in a period.");
									}
									expression = new Expression(file, row, column, originalCommand, csvEquivalentCommand, indices, suffix, null);
								}
							}
						} else {
							/*
							 * A closing paranthesis was not found. This
							 * is invalid. Let's treat the text after the
							 * opening paranthesis as arguments.
							 * */
							IO.ReportInvalidData(file, row, column, "Missing closing paranthesis.");
							string argumentSequence = cell.Substring(openingParanthesis + 1);
							string[] arguments = argumentSequence.Split(',');
							for (int k = 0; k < arguments.Length; k++) {
								arguments[k] = arguments[k].Trim();
							}
							expression = new Expression(file, row, column, originalCommand, csvEquivalentCommand, null, null, arguments);
						}
					} else {
						/*
						 * An opening paranthesis was not found. This
						 * means that the entire cell is just the
						 * command. Check for track positions and
						 * don't transform them to a CSV equivalent.
						 * 
						 * @ command
						 * */
						// TODO: Make sure to support colon-separated track positions here, too.
						double value;
						if (double.TryParse(cell, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
							expression = new Expression(file, row, column, cell, null, null, null);
						} else {
							string originalCommand = cell;
							if (originalCommand.IndexOf(')') != -1) {
								IO.ReportInvalidData(file, row, column, "The command contains an invalid closing paranthesis.");
							}
							string csvEquivalentCommand = GetCsvEquivalentCommand(file, row, column, originalCommand, section, true);
							expression = new Expression(file, row, column, originalCommand, csvEquivalentCommand, null, null, null);
						}
					}
				}
			}
			AdjustCsvEquivalentExpression(expression);
			return true;
		}
		
		/// <summary>Gets the CSV-equivalent command from an RW command.</summary>
		/// <param name="file">The path to the route file.</param>
		/// <param name="row">The zero-based row at which the cell is stored.</param>
		/// <param name="column">The zero-based column at which the cell is stored.</param>
		/// <param name="command">The RW command.</param>
		/// <param name="section">The last used section without the enclosing brackets.</param>
		/// <param name="allowAtSign">Whether to allow an at-sign at the beginning of the command.</param>
		/// <returns>The CSV-equivalent command.</returns>
		private static string GetCsvEquivalentCommand(string file, int row, int column, string command, string section, bool allowAtSign) {
			switch (section.ToLowerInvariant()) {
				case "object":
					section = "Structure";
					break;
				case "railway":
					section = "Track";
					break;
				case "cycle":
					section = "Cycle.Ground";
					break;
			}
			if (command.Length != 0 && command[0] == '@') {
				if (!allowAtSign) {
					IO.ReportInvalidData(file, row, column, "An @-sign is not allowed at the beginning of the command.");
				}
				command = command.Substring(1).TrimStart();
			}
			return section + "." + command;
		}
		
		/// <summary>Adjusts an expression to convert from RW-specific organization to CSV-equivalent organization.</summary>
		/// <param name="expression">The expression to convert.</param>
		private static void AdjustCsvEquivalentExpression(Expression expression) {
			if (expression.Indices == null & expression.Suffix == null) {
				double value;
				if (double.TryParse(expression.OriginalCommand, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
					if (expression.CsvEquivalentCommand.StartsWith("signal.", StringComparison.OrdinalIgnoreCase)) {
						expression.Indices = new string[] { expression.CsvEquivalentCommand.Substring(7) };
						expression.CsvEquivalentCommand = "Signal";
						expression.Suffix = ".Load";
					} else if (expression.CsvEquivalentCommand.StartsWith("cycle.ground.", StringComparison.OrdinalIgnoreCase)) {
						expression.Indices = new string[] { expression.CsvEquivalentCommand.Substring(13) };
						expression.CsvEquivalentCommand = "Cycle.Ground";
						expression.Suffix = ".Params";
					}
				}
			}
		}
		
	}
}