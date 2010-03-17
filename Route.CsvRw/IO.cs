using System;
using System.Globalization;

/*
 * TODO:
 * Add XML annotation.
 * */

namespace Plugin {
	internal static class IO {
		
		// --- members ---
		
		private static CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

		
		// --- functions ---
		
		internal static void ReportMissingRootFolder(string file) {
			Interfaces.Host.Report(
				new OpenBveApi.General.ReportSourcePath(file),
				new OpenBveApi.General.ReportDescription("The root folder that contains Railway and Train could not be found.")
			);
		}
		
		internal static void ReportInvalidData(Parser.Expression expression, string description) {
			string location;
			if (expression.Position >= 0.0) {
				location = expression.OriginalCommand + " at " + expression.Position.ToString("0.0", InvariantCulture);
			} else {
				location = expression.OriginalCommand;
			}
			Interfaces.Host.Report(
				new OpenBveApi.General.ReportSourcePath(expression.File),
				new OpenBveApi.General.ReportTextRow(expression.Row + 1),
				new OpenBveApi.General.ReportTextColumn(expression.Column + 1),
				new OpenBveApi.General.ReportAdditionalLocation(location),
				new OpenBveApi.General.ReportDescription(description)
			);
		}
		
		internal static void ReportInvalidData(string fileName, int row, int column, string description) {
			Interfaces.Host.Report(
				new OpenBveApi.General.ReportSourcePath(fileName),
				new OpenBveApi.General.ReportTextRow(row + 1),
				new OpenBveApi.General.ReportTextColumn(column + 1),
				new OpenBveApi.General.ReportDescription(description)
			);
		}
		
		internal static void ReportInvalidData(string fileName, int row, int column, string command, string description) {
			Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, column + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, command),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
			                      );
		}
		
		internal static void ReportMissingFile(Parser.Expression expression, string missingFile) {
			ReportMissingFile(expression.File, expression.Row, expression.Column, expression.OriginalCommand, missingFile);
		}
		
		internal static void ReportMissingFile(string fileName, int row, int column, string command, string missingFile) {
			Interfaces.Host.Report(OpenBveApi.General.ReportType.FileNotFound,
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.TargetPath, missingFile),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, column + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, command)
			                      );
		}
		
		internal static void ReportNotSupportedCommand(string fileName, int row, string command) {
			const string description = "This command is not supported.";
			Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, command),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
			                      );
		}
		

		
		
		
		// --- check expression ---
		
		/// <summary>Checks if sufficient indices and arguments are provided to an expression, and reports messages to the host application if the amount of indices or arguments is outside of the specified range.</summary>
		/// <param name="expression">The expression to check.</param>
		/// <param name="indicesMinimum">The minimum number of indices the expression is allowed to have.</param>
		/// <param name="indicesMaximum">The maximum number of indices the expression is allowed to have.</param>
		/// <param name="acceptSuffix">Whether the expression is allowed to have a suffix.</param>
		/// <param name="argumentsMinimum">The minimum number of arguments the expression is allowed to have.</param>
		/// <param name="argumentsMaximum">The maximum number of arguments the expression is allowed tohave.</param>
		/// <returns>Whether the expression has at least the specified minimum number of indices and arguments.</returns>
		internal static bool CheckExpression(Parser.Expression expression, int indicesMinimum, int indicesMaximum, bool acceptSuffix, int argumentsMinimum, int argumentsMaximum) {
			int indicesProvided = expression.Indices != null ? expression.Indices.Length : 0;
			int argumentsProvided = expression.Arguments != null ? expression.Arguments.Length : 0;
			if (
				indicesProvided >= indicesMinimum & indicesProvided <= indicesMaximum &&
				(expression.Suffix == null | acceptSuffix) &&
				argumentsProvided >= argumentsMinimum & argumentsProvided <= argumentsMaximum
			) {
				return true;
			} else {
				/*
				 * Check and report indices.
				 * */
				if (indicesProvided < indicesMinimum | indicesProvided > indicesMaximum) {
					string description;
					if (indicesMinimum == 0) {
						string provided = indicesProvided == 1 ? "1 index was" : indicesProvided.ToString(InvariantCulture) + " indices were";
						string expected = indicesMaximum == 1 ? "1 index is" : indicesMaximum.ToString(InvariantCulture) + " indices are";
						description = provided + " provided, but at most " + expected + " expected.";
					} else if (indicesMaximum == int.MaxValue) {
						string provided = indicesProvided == 1 ? "1 index was" : indicesProvided.ToString(InvariantCulture) + " indices were";
						string expected = indicesMinimum == 1 ? "1 index is" : indicesMinimum.ToString(InvariantCulture) + " indices are";
						description = provided + " provided, but at least " + expected + " expected.";
					} else if (indicesMinimum == indicesMaximum) {
						string provided = indicesProvided == 1 ? "1 index was" : indicesProvided.ToString(InvariantCulture) + " indices were";
						string expected = indicesMinimum == 1 ? "1 index is" : indicesMinimum.ToString(InvariantCulture) + " indices are";
						description = provided + " provided, but exactly " + expected + " expected.";
					} else {
						string provided = indicesProvided == 1 ? "1 index was" : indicesProvided.ToString(InvariantCulture) + " indices were";
						string minimum = indicesMinimum.ToString(InvariantCulture);
						string maximum = indicesMaximum.ToString(InvariantCulture);
						description = provided + " provided, but between " + minimum + " and " + maximum + " indices are expected.";
					}
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, expression.File),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, expression.Row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, expression.Column + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, expression.OriginalCommand),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
					                      );
				}
				/*
				 * Check and report the suffix.
				 * */
				if (expression.Suffix != null & !acceptSuffix) {
					string description = "A suffix was provided, but the command does not take a suffix.";
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, expression.File),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, expression.Row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, expression.Column + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, expression.OriginalCommand),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
					                      );
				}
				/*
				 * Check and report arguments.
				 * */
				if (argumentsProvided < argumentsMinimum | argumentsProvided > argumentsMaximum) {
					string description;
					if (argumentsMinimum == 0) {
						string provided = argumentsProvided == 1 ? "1 argument was" : argumentsProvided.ToString(InvariantCulture) + " arguments were";
						string expected = argumentsMaximum == 1 ? "1 argument is" : argumentsMaximum.ToString(InvariantCulture) + " arguments are";
						description = provided + " provided, but at most " + expected + " expected.";
					} else if (argumentsMaximum == int.MaxValue) {
						string provided = argumentsProvided == 1 ? "1 argument was" : argumentsProvided.ToString(InvariantCulture) + " arguments were";
						string expected = argumentsMinimum == 1 ? "1 argument is" : argumentsMinimum.ToString(InvariantCulture) + " arguments are";
						description = provided + " provided, but at least " + expected + " expected.";
					} else if (argumentsMinimum == argumentsMaximum) {
						string provided = argumentsProvided == 1 ? "1 argument was" : argumentsProvided.ToString(InvariantCulture) + " arguments were";
						string expected = argumentsMinimum == 1 ? "1 argument is" : argumentsMinimum.ToString(InvariantCulture) + " arguments are";
						description = provided + " provided, but exactly " + expected + " expected.";
					} else {
						string provided = argumentsProvided == 1 ? "1 argument was" : argumentsProvided.ToString(InvariantCulture) + " arguments were";
						string minimum = argumentsMinimum.ToString(InvariantCulture);
						string maximum = argumentsMaximum.ToString(InvariantCulture);
						description = provided + " provided, but between " + minimum + " and " + maximum + " arguments are expected.";
					}
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, expression.File),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, expression.Row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, expression.Column + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, expression.OriginalCommand),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
					                      );
				}
				/*
				 * Check for success.
				 * */
				if (indicesProvided < indicesMinimum | argumentsProvided < argumentsMinimum) {
					return false;
				} else {
					return true;
				}
			}
		}
		
		
		// --- arguments ---
		
		internal static bool CheckArgumentCount(string fileName, int row, int column, string command, int argumentsProvided, int argumentsMinimum, int argumentsMaximum, bool allowTrailingArguments) {
			if (argumentsProvided >= argumentsMinimum & argumentsProvided <= argumentsMaximum) {
				return true;
			} else {
				string description;
				if (argumentsMinimum == 0) {
					string provided = argumentsProvided == 1 ? "1 argument was" : argumentsProvided.ToString(InvariantCulture) + " arguments were";
					string expected = argumentsMaximum == 1 ? "1 argument is" : argumentsMaximum.ToString(InvariantCulture) + " arguments are";
					description = provided + " provided, but at most " + expected + " expected.";
				} else if (argumentsMaximum == int.MaxValue) {
					string provided = argumentsProvided == 1 ? "1 argument was" : argumentsProvided.ToString(InvariantCulture) + " arguments were";
					string expected = argumentsMinimum == 1 ? "1 argument is" : argumentsMinimum.ToString(InvariantCulture) + " arguments are";
					description = provided + " provided, but at least " + expected + " expected.";
				} else if (argumentsMinimum == argumentsMaximum) {
					string provided = argumentsProvided == 1 ? "1 argument was" : argumentsProvided.ToString(InvariantCulture) + " arguments were";
					string expected = argumentsMinimum == 1 ? "1 argument is" : argumentsMinimum.ToString(InvariantCulture) + " arguments are";
					description = provided + " provided, but exactly " + expected + " expected.";
				} else {
					string provided = argumentsProvided == 1 ? "1 argument was" : argumentsProvided.ToString(InvariantCulture) + " arguments were";
					string minimum = argumentsMinimum.ToString(InvariantCulture);
					string maximum = argumentsMaximum.ToString(InvariantCulture);
					description = provided + " provided, but between " + minimum + " and " + maximum + " arguments are expected.";
				}
				Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
				                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
				                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
				                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, column + 1),
				                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, command),
				                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
				                      );
				if (allowTrailingArguments & argumentsProvided >= argumentsMinimum) {
					return true;
				} else {
					return false;
				}
			}
		}
		
		internal static void ReportInvalidArgument(Parser.Expression expression, int argumentIndex, string argumentName, string description) {
			ReportInvalidArgument(expression.File, expression.Row, expression.Column, expression.OriginalCommand, argumentIndex, argumentName, description);
		}
		
		internal static void ReportInvalidArgument(string fileName, int row, int column, string command, int argumentIndex, string argumentName, string description) {
			string location = argumentName + " (argument " + (argumentIndex + 1).ToString(CultureInfo.InvariantCulture) + ") in " + command;
			Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, column + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, location),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
			                      );
		}
		
		
		// --- suffix ---
		
		internal static void ReportInvalidSuffix(Parser.Expression expression) {
			const string description = "The suffix is invalid.";
			Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, expression.File),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, expression.Row + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, expression.Column + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, expression.OriginalCommand),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
			                      );
		}
		
		
		// --- parse int ---
		
		internal static bool ParseIntFromIndex(Parser.Expression expression, int indexIndex, string indexName, int minimumValue, int maximumValue, int defaultValue, out int value) {
			return ParseInt(expression.File, expression.Row, expression.Column, expression.OriginalCommand, expression.Indices, indexIndex, indexName, minimumValue, maximumValue, defaultValue, out value);
		}
		
		internal static bool ParseIntFromArgument(Parser.Expression expression, int argumentIndex, string argumentName, int minimumValue, int maximumValue, int defaultValue, out int value) {
			return ParseInt(expression.File, expression.Row, expression.Column, expression.OriginalCommand, expression.Arguments, argumentIndex, argumentName, minimumValue, maximumValue, defaultValue, out value);
		}
		
		internal static bool ParseInt(string fileName, int row, int column, string command, string[] arguments, int argumentIndex, string argumentName, int minimumValue, int maximumValue, int defaultValue, out int value) {
			if (ParseInt(fileName, row, column, command, arguments, argumentIndex, argumentName, defaultValue, out value)) {
				if (value < minimumValue | value > maximumValue) {
					string location = "Argument " + (argumentIndex + 1).ToString(CultureInfo.InvariantCulture) + " in " + command;
					string description = "The argument is expected to be between " + minimumValue.ToString(InvariantCulture) + " and " + maximumValue.ToString(InvariantCulture) + ".";
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, column + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, location),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
					                      );
				}
				if (value < minimumValue) {
					value = minimumValue;
					return false;
				} else if (value > maximumValue) {
					value = maximumValue;
					return false;
				} else {
					return true;
				}
			} else {
				value = defaultValue;
				return false;
			}
		}
		
		internal static bool ParseInt(string fileName, int row, int column, string command, string[] arguments, int argumentIndex, string argumentName, int defaultValue, out int value) {
			if (arguments != null && argumentIndex >= 0 && argumentIndex < arguments.Length && arguments[argumentIndex] != null && arguments[argumentIndex].Length != 0) {
				if (ParseInt(arguments[argumentIndex], out value)) {
					return true;
				} else {
					string location = "Argument " + (argumentIndex + 1).ToString(CultureInfo.InvariantCulture) + " in " + command;
					string description = "The argument is not a valid integer.";
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, column + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, location),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
					                      );
					value = defaultValue;
					return false;
				}
			} else {
				value = defaultValue;
				return false;
			}
		}
		
		
		private static bool ParseInt(string text, out int value) {
			if (int.TryParse(text, NumberStyles.Integer, InvariantCulture, out value)) {
				return true;
			} else {
				text = TrimInside(text);
				for (int n = text.Length; n >= 1; n--) {
					if (int.TryParse(text.Substring(0, n), NumberStyles.Integer, InvariantCulture, out value)) {
						return true;
					}
				}
				return false;
			}
		}
		
		
		// --- parse double extended ---
		
		internal static bool ParseDoubleFromArgumentExtended(Parser.Expression expression, int argumentIndex, string argumentName, double defaultValue, double[] factors, out double value) {
			if (expression.Arguments != null && argumentIndex < expression.Arguments.Length) {
				string text = expression.Arguments[argumentIndex];
				int colon = text.IndexOf(':');
				if (colon >= 0) {
					string[] parts = text.Split(':');
					value = 0.0;
					for (int i = 0; i < parts.Length; i++) {
						double number;
						if (double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out number)) {
							int index = factors.Length - parts.Length + i;
							if (index >= 0) {
								value += number * factors[index];
							}
						} else {
							string location = "Argument " + (argumentIndex + 1).ToString(CultureInfo.InvariantCulture) + " in " + expression.OriginalCommand;
							string description = "The argument is not a valid floating-point number.";
							Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
							                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, expression.File),
							                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, expression.Row + 1),
							                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, expression.Column + 1),
							                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, location),
							                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
							                      );
							value = defaultValue;
							return false;
						}
					}
					return true;
				} else {
					if (ParseDouble(text, out value)) {
						value *= factors[factors.Length - 1];
						return true;
					} else {
						value = defaultValue;
						return false;
					}
				}
			} else {
				value = defaultValue;
				return false;
			}
		}
		
		// --- parse double ---
		
		internal static bool ParseDoubleFromIndex(Parser.Expression expression, int indexIndex, string indexName, double minimumValue, double maximumValue, double defaultValue, out double value) {
			return ParseDouble(expression.File, expression.Row, expression.Column, expression.OriginalCommand, expression.Indices, indexIndex, indexName, minimumValue, maximumValue, defaultValue, out value);
		}
		
		internal static bool ParseDoubleFromArgument(Parser.Expression expression, int argumentIndex, string argumentName, double minimumValue, double maximumValue, double defaultValue, out double value) {
			return ParseDouble(expression.File, expression.Row, expression.Column, expression.OriginalCommand, expression.Arguments, argumentIndex, argumentName, minimumValue, maximumValue, defaultValue, out value);
		}
		
		internal static bool ParseDouble(string fileName, int row, int column, string command, string[] arguments, int argumentIndex, string argumentName, double minimumValue, double maximumValue, double defaultValue, out double value) {
			if (ParseDouble(fileName, row, column, command, arguments, argumentIndex, argumentName, defaultValue, out value)) {
				if (value < minimumValue | value > maximumValue) {
					string location = "Argument " + (argumentIndex + 1).ToString(CultureInfo.InvariantCulture) + " in " + command;
					string description = "The argument is expected to be between " + minimumValue.ToString(InvariantCulture) + " and " + maximumValue.ToString(InvariantCulture) + ".";
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, column + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, location),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
					                      );
				}
				if (value < minimumValue) {
					value = minimumValue;
					return false;
				} else if (value > maximumValue) {
					value = maximumValue;
					return false;
				} else {
					return true;
				}
			} else {
				return false;
			}
		}
		
		internal static bool ParseDouble(string fileName, int row, int column, string command, string[] arguments, int argumentIndex, string argumentName, double defaultValue, out double value) {
			if (arguments != null && argumentIndex >= 0 && argumentIndex < arguments.Length && arguments[argumentIndex] != null && arguments[argumentIndex].Length != 0) {
				if (ParseDouble(arguments[argumentIndex], out value)) {
					return true;
				} else {
					string location = "Argument " + (argumentIndex + 1).ToString(CultureInfo.InvariantCulture) + " in " + command;
					string description = "The argument is not a valid floating-point number.";
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, column + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Location, location),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
					                      );
					value = defaultValue;
					return false;
				}
			} else {
				value = defaultValue;
				return false;
			}
		}
		
		private static bool ParseDouble(string text, out double value) {
			if (double.TryParse(text, NumberStyles.Float, InvariantCulture, out value)) {
				return true;
			} else {
				text = TrimInside(text);
				for (int n = text.Length; n >= 1; n--) {
					if (double.TryParse(text.Substring(0, n), NumberStyles.Float, InvariantCulture, out value)) {
						return true;
					}
				}
				return false;
			}
		}
		
		
		// --- helper functions ---
		
		private static string TrimInside(string text) {
			System.Text.StringBuilder builder = new System.Text.StringBuilder(text.Length);
			for (int i = 0; i < text.Length; i++) {
				char value = text[i];
				if (!char.IsWhiteSpace(value)) {
					builder.Append(value);
				}
			}
			return builder.ToString();
		}
		
		internal static bool IsTrackPosition(string text) {
			if (text.Length != 0) {
				string[] parts = text.Split(':');
				for (int i = 0; i < parts.Length; i++) {
					double value;
					if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
						return false;
					}
				}
				return true;
			} else {
				return false;
			}
		}
		
		internal static bool ParseTrackPosition(Parser.Expression expression, double[] factors, out double value) {
			string text = expression.CsvEquivalentCommand;
			int colon = text.IndexOf(':');
			if (colon >= 0) {
				string[] parts = text.Split(':');
				value = 0.0;
				for (int i = 0; i < parts.Length; i++) {
					double number;
					if (double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out number)) {
						int index = factors.Length - parts.Length + i;
						if (index >= 0) {
							value += number * factors[index];
						}
					} else {
						string description = "The track position is invalid.";
						Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
						                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, expression.File),
						                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, expression.Row + 1),
						                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Column, expression.Column + 1),
						                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
						                      );
						value = 0.0;
						return false;
					}
				}
				return true;
			} else {
				if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
					value *= factors[factors.Length - 1];
					return true;
				} else {
					value = 0.0;
					return false;
				}
			}
		}

		
	}
}