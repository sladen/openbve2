using System;
using System.Globalization;
using System.Text;

namespace Plugin {
	internal static partial class Parser {
		
		/// <summary>Preprocesses a cell for the $-group of commands and adds the resulting expressions to a specified list of expressions.</summary>
		/// <param name="folder">The platform-specific absolute path to the folder where the main route file is stored.</param>
		/// <param name="file">The platform-specific absolute path to the file that contains the cell.</param>
		/// <param name="isRw">Whether the route is of RW format.</param>
		/// <param name="fallback">The fallback encoding.</param>
		/// <param name="row">The zero-based row at which the cell is stored.</param>
		/// <param name="column">The zero-based column at which the cell is stored.</param>
		/// <param name="cell">The non-empty content of the trimmed CSV cell or the trimmed RW line.</param>
		/// <param name="expressions">The list of expressions.</param>
		/// <param name="expressionCount">The number of expressions.</param>
		/// <param name="withOrSection">The last argument to the With command for CSV files, or the last opened section for RW files.</param>
		/// <remarks>For RW files, the line is further split at at sign boundaries after preprocessing but before expressions are extracted.</remarks>
		private static void PreprocessAndAddExpressions(string folder, string file, bool isRw, Encoding fallback, int row, int column, string cell, ref Expression[] expressions, ref int expressionCount, ref Sub[] subs, ref int subCount, ref string withOrSection) {
			/*
			 * Find $Chr, $Rnd and $Sub directives from right to left
			 * and substitute their results back into the cell.
			 * */
			for (int i = cell.Length - 1; i >= 0; i--) {
				if (cell[i] == '$' ) {
					for (int j = i + 1; j <= cell.Length; j++) {
						if (cell[j] == '(') {
							for (int k = j + 1; k < cell.Length; k++) {
								if (cell[k] == '(') {
									IO.ReportInvalidData(file, row, column, "Invalid opening paranthesis.");
									return;
								} else if (cell[k] == ')') {
									string argumentSequence = cell.Substring(j + 1, k - j - 1);
									string[] arguments = argumentSequence.Split(';');
									for (int h = 0; h < arguments.Length; h++) {
										arguments[h] = arguments[h].Trim();
									}
									string command = cell.Substring(i, j - i);
									switch (command.ToLowerInvariant()) {
										case "$chr":
											{
												if (IO.CheckArgumentCount(file, row, column, command, arguments.Length, 1, 1, false)) {
													int value;
													if (IO.ParseInt(file, row, column, command, arguments, 0, "codepoint", 0, 1114111, 0, out value)) {
														if (value < 0xD800 | value > 0xDFFF) {
															string substitute = char.ConvertFromUtf32(value);
															cell = cell.Substring(0, i) + substitute + cell.Substring(k + 1);
														} else {
															IO.ReportInvalidData(file, row, column, command, "The codepoint is in the range of surrogates. For characters outside the basic multilingual plane, use the corresponding codepoint directly.");
															return;
														}
													} else {
														return;
													}
												} else {
													return;
												}
											}
											break;
										case "$rnd":
											{
												if (IO.CheckArgumentCount(file, row, column, command, arguments.Length, 2, 2, false)) {
													int minimum;
													if (IO.ParseInt(file, row, column, command, arguments, 0, "minimum", int.MinValue, int.MaxValue, 0, out minimum)) {
														int maximum;
														if (IO.ParseInt(file, row, column, command, arguments, 1, "maximum", int.MinValue, int.MaxValue, 0, out maximum)) {
															if (minimum <= maximum) {
																int number = RandomNumberGenerator.Next(minimum, maximum + 1);
																string substitute = number.ToString(System.Globalization.CultureInfo.InvariantCulture);
																cell = cell.Substring(0, i) + substitute + cell.Substring(k + 1);
															} else {
																IO.ReportInvalidData(file, row, column, command, "Minimum must be less than or equal to Maximum.");
																return;
															}
														} else {
															return;
														}
													} else {
														return;
													}
												} else {
													return;
												}
											}
											break;
										case "$sub":
											{
												bool assignment = false;
												for (int h = k + 1; h < cell.Length; h++) {
													if (cell[h] == '=') {
														assignment = true;
														if (IO.CheckArgumentCount(file, row, column, command, arguments.Length, 1, 1, false)) {
															int index;
															if (IO.ParseInt(file, row, column, command, arguments, 0, "index", 0, int.MaxValue, 0, out index)) {
																string value = cell.Substring(h + 1).TrimStart();
																Sub.Set(ref subs, ref subCount, index, value);
																cell = cell.Substring(0, i);
															} else {
																return;
															}
														} else {
															return;
														}
													} else if (!char.IsWhiteSpace(cell[h])) {
														break;
													}
												}
												if (!assignment) {
													if (IO.CheckArgumentCount(file, row, column, command, arguments.Length, 1, 1, true)) {
														int index;
														if (IO.ParseInt(file, row, column, command, arguments, 0, "index", 0, int.MaxValue, 0, out index)) {
															string value;
															if (Sub.Get(subs, subCount, index, out value)) {
																cell = cell.Substring(0, i) + value + cell.Substring(k + 1);
															} else {
																IO.ReportInvalidData(file, row, column, command, "A $Sub variable is queried which has not been defined.");
																return;
															}
														} else {
															return;
														}
													} else {
														return;
													}
												}
											}
											break;
										case "$include":
											if (IO.CheckArgumentCount(file, row, column, command, arguments.Length, 1, int.MaxValue, true)) {
												int count = (arguments.Length + 1) / 2;
												string[] files = new string[count];
												double[] weights = new double[count];
												double weightsTotal = 0.0;
												for (int h = 0; h < count; h++) {
													files[h] = OpenBveApi.Path.CombineFile(folder, arguments[2 * h]);
													if (h == count - 1 && (arguments.Length & 1) == 1) {
														weights[h] = 1.0;
													} else {
														if (!IO.ParseDouble(file, row, column, command, arguments, 2 * h + 1, "weight" + h.ToString(CultureInfo.InvariantCulture), 0.0, double.MaxValue, 1.0, out weights[h])) {
															weights[h] = 1.0;
														}
													}
													weightsTotal += weights[h];
												}
												double number = RandomNumberGenerator.NextDouble() * weightsTotal;
												int index = count - 1;
												for (int h = 0; h < count; h++) {
													number -= weights[h];
													if (number < 0.0) {
														index = h;
														break;
													}
												}
												if (System.IO.File.Exists(files[index])) {
													if (isRw) {
														GetExpressionsFromRwFile(folder, files[index], fallback, ref expressions, ref expressionCount, ref subs, ref subCount, ref withOrSection);
													} else {
														GetExpressionsFromCsvFile(folder, files[index], fallback, ref expressions, ref expressionCount, ref subs, ref subCount, ref withOrSection);
													}
												} else {
													IO.ReportMissingFile(file, row, column, command, files[index]);
												}
												cell = cell.Substring(0, i) + cell.Substring(k + 1);
											} else {
												return;
											}
											break;
										default:
											IO.ReportInvalidData(file, row, column, command, "The directive is not recognized.");
											return;
									}
									break;
								}
							}
							break;
						} else if (cell[j] == ')') {
							IO.ReportInvalidData(file, row, column, "Missing closing paranthesis.");
							return;
						}
					}
				}
			}
			/*
			 * If the cell is not empty, create
			 * an expression from it and add it
			 * to the list of expressions.
			 * */
			if (cell.Length != 0) {
				cell = cell.Trim();
				if (cell.Length != 0) {
					if (isRw) {
						/*
						 * RW file.
						 * */
						if (withOrSection.Equals("railway", StringComparison.OrdinalIgnoreCase)) {
							/*
							 * Inside the [Railway] section, multiple expressions
							 * may be chained together. Split them apart at @-sign
							 * boundaries, where the @-sign is considered to be
							 * part of the following cell.
							 * */
							column = 0;
							while (true) {
								int atSign = cell.IndexOf('@', 1);
								if (atSign >= 1) {
									string text = cell.Substring(0, atSign).TrimEnd();
									if (text.Length != 0) {
										if (expressionCount == expressions.Length) {
											Array.Resize<Expression>(ref expressions, expressions.Length << 1);
										}
										Expression expression;
										if (GetExpressionFromRwCell(file, row, column, text, withOrSection, out expression)) {
											expressions[expressionCount] = expression;
											expressionCount++;
										}
									}
									cell = cell.Substring(atSign);
									column++;
								} else {
									break;
								}
							}
							if (cell.Length != 0) {
								if (expressionCount == expressions.Length) {
									Array.Resize<Expression>(ref expressions, expressions.Length << 1);
								}
								Expression expression;
								if (GetExpressionFromRwCell(file, row, column, cell, withOrSection, out expression)) {
									expressions[expressionCount] = expression;
									expressionCount++;
								}
							}
						} else {
							/*
							 * Outside the [Railway] section, only one
							 * expression per line is allowed.
							 * */
							if (expressionCount == expressions.Length) {
								Array.Resize<Expression>(ref expressions, expressions.Length << 1);
							}
							Expression expression;
							if (GetExpressionFromRwCell(file, row, column, cell, withOrSection, out expression)) {
								expressions[expressionCount] = expression;
								expressionCount++;
							}
						}
					} else {
						/*
						 * CSV file.
						 * */
						if (cell[0] != ';') {
							if (expressionCount == expressions.Length) {
								Array.Resize<Expression>(ref expressions, expressions.Length << 1);
							}
							Expression expression;
							if (GetExpressionFromCsvCell(file, row, column, cell, ref withOrSection, out expression)) {
								expressions[expressionCount] = expression;
								expressionCount++;
							}
						}
					}
				}
			}
		}

	}
}