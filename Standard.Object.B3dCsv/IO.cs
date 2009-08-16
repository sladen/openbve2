using System;
using System.Globalization;

namespace Plugin {
	internal static class IO {
		
		// members
		private static CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

		// report now allowed command
		internal static void ReportNotAllowedCommand(string fileName, int row, string command, string supposedCommand) {
			bool isB3D = string.Equals(System.IO.Path.GetExtension(fileName), ".b3d", StringComparison.OrdinalIgnoreCase);
			string format = isB3D ? "B3D" : "CSV";
			string description = "This command is not allowed in " + format + " files. Did you mean " + supposedCommand + "?";
			Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Command, command),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
			                      );
		}
		
		// report now supported command
		internal static void ReportNotSupportedCommand(string fileName, int row, string command) {
			string description = "This command is not supported.";
			Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Command, command),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
			                      );
		}
		
		// report not enough arguments
		internal static bool CheckArgumentCount(string fileName, int row, string command, int argumentsProvided, int argumentsMinimum, int argumentsMaximum, bool allowTrailingArguments) {
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
					description = provided + " provided, but " + expected + " expected.";
				} else {
					string provided = argumentsProvided == 1 ? "1 argument was" : argumentsProvided.ToString(InvariantCulture) + " arguments were";
					string minimum = argumentsMinimum.ToString(InvariantCulture);
					string maximum = argumentsMaximum.ToString(InvariantCulture);
					description = provided + " provided, but between " + minimum + " and " + maximum + " arguments are expected.";
				}
				Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
				                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
				                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
				                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Command, command),
				                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
				                      );
				if (allowTrailingArguments & argumentsProvided >= argumentsMinimum) {
					return true;
				} else {
					return false;
				}
			}
		}

		// report invalid argument
		internal static void ReportInvalidArgument(string fileName, int row, string command, int argumentIndex, string argumentName, string argumentContent, string description) {
			Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Command, command),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentIndex, argumentIndex + 1),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentName, argumentName),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentContent, argumentContent),
			                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Description, description)
			                      );
		}

		// parse (int)
		internal static bool ParseInt(string fileName, int row, string command, string[] arguments, int argumentIndex, string argumentName, int minimumValue, int maximumValue, int defaultValue, out int value) {
			if (ParseInt(fileName, row, command, arguments, argumentIndex, argumentName, defaultValue, out value)) {
				if (value < minimumValue | value > maximumValue) {
					string description = "The argument is expected to be between " + minimumValue.ToString(InvariantCulture) + " and " + maximumValue.ToString(InvariantCulture) + ".";
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Command, command),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentIndex, argumentIndex + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentName, argumentName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentContent, arguments[argumentIndex]),
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
		internal static bool ParseInt(string fileName, int row, string command, string[] arguments, int argumentIndex, string argumentName, int defaultValue, out int value) {
			if (argumentIndex >= 0 && argumentIndex < arguments.Length && arguments[argumentIndex] != null && arguments[argumentIndex].Length != 0) {
				if (ParseInt(arguments[argumentIndex], out value)) {
					return true;
				} else {
					string description = "The argument is not a valid integer.";
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Command, command),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentIndex, argumentIndex + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentName, argumentName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentContent, arguments[argumentIndex]),
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
		private static bool ParseInt(string expression, out int value) {
			if (int.TryParse(expression, NumberStyles.Integer, InvariantCulture, out value)) {
				return true;
			} else {
				expression = TrimInside(expression);
				for (int n = expression.Length; n >= 1; n--) {
					if (int.TryParse(expression.Substring(0, n), NumberStyles.Integer, InvariantCulture, out value)) {
						return true;
					}
				}
				return false;
			}
		}
		
		// parse (double)
		internal static bool ParseDouble(string fileName, int row, string command, string[] arguments, int argumentIndex, string argumentName, double minimumValue, double maximumValue, double defaultValue, out double value) {
			if (ParseDouble(fileName, row, command, arguments, argumentIndex, argumentName, defaultValue, out value)) {
				if (value < minimumValue | value > maximumValue) {
					string description = "The argument is expected to be between " + minimumValue.ToString(InvariantCulture) + " and " + maximumValue.ToString(InvariantCulture) + ".";
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Command, command),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentIndex, argumentIndex + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentName, argumentName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentContent, arguments[argumentIndex]),
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
		internal static bool ParseDouble(string fileName, int row, string command, string[] arguments, int argumentIndex, string argumentName, double defaultValue, out double value) {
			if (argumentIndex >= 0 && argumentIndex < arguments.Length && arguments[argumentIndex] != null && arguments[argumentIndex].Length != 0) {
				if (ParseDouble(arguments[argumentIndex], out value)) {
					return true;
				} else {
					string description = "The argument is not a valid floating-point number.";
					Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.SourcePath, fileName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Row, row + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.Command, command),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentIndex, argumentIndex + 1),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentName, argumentName),
					                       new OpenBveApi.General.ReportKeyValuePair(OpenBveApi.General.ReportKey.ArgumentContent, arguments[argumentIndex]),
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
		private static bool ParseDouble(string expression, out double value) {
			if (double.TryParse(expression, NumberStyles.Float, InvariantCulture, out value)) {
				return true;
			} else {
				expression = TrimInside(expression);
				for (int n = expression.Length; n >= 1; n--) {
					if (double.TryParse(expression.Substring(0, n), NumberStyles.Float, InvariantCulture, out value)) {
						return true;
					}
				}
				return false;
			}
		}
		
		// trim inside
		private static string TrimInside(string expression) {
			System.Text.StringBuilder builder = new System.Text.StringBuilder(expression.Length);
			for (int i = 0; i < expression.Length; i++) {
				char c = expression[i];
				if (!char.IsWhiteSpace(c)) {
					builder.Append(c);
				}
			}
			return builder.ToString();
		}
		
	}
}