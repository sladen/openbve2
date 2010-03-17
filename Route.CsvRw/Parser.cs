using System;
using System.Globalization;
using System.Text;

/*
 * TODO: Make sure to check for circular dependencies with the $Include directive.
 * */

namespace Plugin {
	internal static partial class Parser {
		
		
		// --- structures ---
		
		/// <summary>Represents a $Sub variable.</summary>
		private struct Sub {
			// members
			/// <summary>The index of the $Sub variable.</summary>
			internal int Index;
			/// <summary>The value of the $Sub variable.</summary>
			internal string Value;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="index">The index of the $Sub variable.</param>
			/// <param name="value">The value of the $Sub variable.</param>
			internal Sub(int index, string value) {
				this.Index = index;
				this.Value = value;
			}
			// static functions
			/// <summary>Sets a $Sub variable to a specified value.</summary>
			/// <param name="subs">The list of $Sub variables.</param>
			/// <param name="subCount">The number of $Sub variables.</param>
			/// <param name="index">The non-negative index to the $Sub variable that is to be set.</param>
			/// <param name="value">The value for the $Sub variable to be set.</param>
			internal static void Set(ref Sub[] subs, ref int subCount, int index, string value) {
				for (int i = 0; i < subCount; i++) {
					if (subs[i].Index == index) {
						subs[i].Value = value;
						return;
					}
				}
				if (subCount == subs.Length) {
					Array.Resize<Sub>(ref subs, subs.Length << 1);
				}
				subs[subCount] = new Sub(index, value);
				subCount++;
			}
			/// <summary>Gets the value of a specified $Sub variable.</summary>
			/// <param name="subs">The list of $Sub variables.</param>
			/// <param name="subCount">The number of $Sub variables.</param>
			/// <param name="index">The non-negative index to the $Sub variable that is to be queried.</param>
			/// <param name="value">Receives the value of the queried $Sub variable on success.</param>
			/// <returns>A value indicating whether the $Sub variable could be retrieved.</returns>
			internal static bool Get(Sub[] subs, int subCount, int index, out string value) {
				for (int i = 0; i < subCount; i++) {
					if (subs[i].Index == index) {
						value = subs[i].Value;
						return true;
					}
				}
				value = null;
				return false;
			}
		}
		
		/// <summary>Represents options in the route file.</summary>
		internal class Options {
			// members
			/// <summary>The units of length, expressed as factors to multiply the target units by to arrive at meters.</summary>
			internal double[] UnitsOfLength;
			/// <summary>The unit of speed, expressed as a factor to multiply the target unit by to arrive at meters per second.</summary>
			internal double UnitOfSpeed;
			/// <summary>The block length.</summary>
			internal double BlockLength;
			/// <summary>The lighting conditions.</summary>
			internal OpenBveApi.Route.DirectionalLight Light;
			/// <summary>The initial camera position, e.g. at the first station.</summary>
			internal OpenBveApi.Math.Vector3 InitialPosition;
			/// <summary>The initial camera orientation, e.g. at the first station.</summary>
			internal OpenBveApi.Math.Orientation3 InitialOrientation;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			internal Options() {
				this.UnitsOfLength = new double[] { 1.0 };
				this.UnitOfSpeed = 0.277777777777777778;
				this.BlockLength = 25.0;
				this.Light = new OpenBveApi.Route.DirectionalLight(
					new OpenBveApi.Color.ColorRGB(0.625f, 0.625f, 0.625f),
					new OpenBveApi.Color.ColorRGB(0.625f, 0.625f, 0.625f),
					new OpenBveApi.Color.ColorRGB(0.0f, 0.0f, 0.0f),
					new OpenBveApi.Math.Vector3(-0.219185573394538, -0.86602540378444, 0.449397023149582)
				);
				this.InitialPosition = OpenBveApi.Math.Vector3.Null;
				this.InitialOrientation = OpenBveApi.Math.Orientation3.Default;
			}
		}
		
		// --- members ---
		
		/// <summary>The random number generator used by this parser.</summary>
		private static Random RandomNumberGenerator = new Random();
		
		
		// --- functions ---
		
		/// <summary>Loads an RW/CSV route from a file and returns the success of the operation.</summary>
		/// <param name="file">The platform-specific absolute file name of the route.</param>
		/// <param name="fallback">The fallback encoding.</param>
		/// <param name="mesh">Receives the route data.</param>
		/// <returns>The success of the operation.</returns>
		internal static OpenBveApi.General.Result LoadCsvOrRwRoute(string file, System.Text.Encoding fallback, out OpenBveApi.Route.RouteData route) {
			/*
			 * Find the root folder.
			 * */
			string folder = System.IO.Path.GetDirectoryName(file);
			string rootFolder;
			if (GetRootFolder(folder, out rootFolder)) {
				/*
				 * Prepare the folders.
				 * */
				string railwayFolder = OpenBveApi.Path.CombineFolder(rootFolder, "Railway");
				string objectFolder = OpenBveApi.Path.CombineFolder(railwayFolder, "Object");
				string soundFolder = OpenBveApi.Path.CombineFolder(railwayFolder, "Sound");
				string trainFolder = OpenBveApi.Path.CombineFolder(rootFolder, "Train");
				string pluginDataFolder = OpenBveApi.Path.CombineFolder(GetStartupPath(), "Route.CsvRw");
				bool isRw = file.EndsWith(".rw", StringComparison.OrdinalIgnoreCase);
				/*
				 * Extract expressions from the file.
				 * */
				Expression[] expressions;
				GetExpressionsFromFile(folder, file, isRw, fallback, out expressions);
				/*
				 * Process the general expressions.
				 * */
				Options options;
				Structures structures;
				ProcessGeneralExpressions(expressions, objectFolder, pluginDataFolder, fallback, out options, out structures);
				/*
				 * Process track positions and associate
				 * them to the track expressions.
				 * */
				double lastPosition = 0.0;
				for (int i = 0; i < expressions.Length; i++) {
					if (expressions[i].Type == ExpressionType.Position) {
						double value;
						if (IO.ParseTrackPosition(expressions[i], options.UnitsOfLength, out value)) {
							lastPosition = value;
						}
					} else if (expressions[i].Type == ExpressionType.Track) {
						expressions[i].Position = lastPosition;
					}
				}
				/*
				 * Sort and process the track expressions.
				 * */
				int a = Environment.TickCount;
				SortExpressions(expressions, 0, expressions.Length);
				int b = Environment.TickCount;
				int t = b - a;
				ProcessTrackExpressions(expressions, options, structures, fallback, isRw);
				//WriteExpressionsToFile(@"C:\debug.txt", expressions);
				/*
				 * Submit the route data.
				 * */
				route = new OpenBveApi.Route.RouteData(options.InitialPosition + new OpenBveApi.Math.Vector3(0.0, 3.0, 0.0), options.InitialOrientation, options.Light);
				return OpenBveApi.General.Result.Successful;
			} else {
				/*
				 * The root folder could not be found.
				 * */
				IO.ReportMissingRootFolder(file);
				route = null;
				return OpenBveApi.General.Result.FolderNotFound;
			}
		}
		
		/// <summary>Gets the startup path for this program.</summary>
		/// <returns>The startup path for this program.</returns>
		/// <remarks>If retrieving the startup path fails, this returns either the working directory, or an empty string.</remarks>
		private static string GetStartupPath() {
			try {
				return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			} catch {
				try {
					return Environment.CurrentDirectory;
				} catch {
					return string.Empty;
				}
			}
		}
		
		/// <summary>Gets the the folder that contains a Railway and Train folder from a specified starting location.</summary>
		/// <param name="folder">The folder to start searching for the root folder.</param>
		/// <param name="root">Receives the path to the root folder on success.</param>
		/// <returns>A boolean indicating the success of the operation.</returns>
		private static bool GetRootFolder(string folder, out string root) {
			string current = folder;
			do {
				string railway = OpenBveApi.Path.CombineFolder(current, "Railway");
				if (System.IO.Directory.Exists(railway)) {
					string train = OpenBveApi.Path.CombineFolder(current, "Train");
					if (System.IO.Directory.Exists(train)) {
						root = current;
						return true;
					}
				}
				current = System.IO.Path.GetDirectoryName(current);
			} while (current != null);
			current = folder;
			do {
				string railway = OpenBveApi.Path.CombineFolder(current, "Railway");
				if (System.IO.Directory.Exists(railway)) {
					root = current;
					return true;
				}
				current = System.IO.Path.GetDirectoryName(current);
			} while (current != null);
			root = null;
			return false;
		}
		
		/// <summary>Gets an array of all expressions contained in an RW/CSV route.</summary>
		/// <param name="folder">The platform-specific absolute path to the folder where the main route file is stored.</param>
		/// <param name="file">The platform-specific absolute path to the file that contains the cell.</param>
		/// <param name="isRw">Whether the route is of RW format.</param>
		/// <param name="fallback">The fallback encoding.</param>
		/// <param name="expressions">Receives the expressions.</param>
		/// <returns>An array of expressions.</returns>
		private static void GetExpressionsFromFile(string folder, string file, bool isRw, Encoding fallback, out Expression[] expressions) {
			/*
			 * First, extract expressiosn from the file.
			 * */
			expressions = new Expression[4096];
			int expressionCount = 0;
			Sub[] subs = new Sub[16];
			int subCount = 0;
			if (isRw) {
				string section = "Invalid";
				GetExpressionsFromRwFile(folder, file, fallback, ref expressions, ref expressionCount, ref subs, ref subCount, ref section);
			} else {
				string with = null;
				GetExpressionsFromCsvFile(folder, file, fallback, ref expressions, ref expressionCount, ref subs, ref subCount, ref with);
			}
			for (int i = 0; i < expressionCount; i++) {
				if (IO.IsTrackPosition(expressions[i].CsvEquivalentCommand)) {
					expressions[i].Type = ExpressionType.Position;
				} else if (expressions[i].CsvEquivalentCommand.StartsWith("Track.", StringComparison.OrdinalIgnoreCase)) {
					expressions[i].Type = ExpressionType.Track;
				} else {
					expressions[i].Type = ExpressionType.General;
				}
			}
			Array.Resize<Expression>(ref expressions, expressionCount);
		}
		
		/// <summary>Gets an array of all expressions contained in a CSV route.</summary>
		/// <param name="folder">The platform-specific absolute path to the folder where the main route file is stored.</param>
		/// <param name="file">The platform-specific absolute path to the file that contains the cell.</param>
		/// <param name="fallback">The fallback encoding.</param>
		/// <param name="expressions">The list of expressions.</param>
		/// <param name="expressionCount">The number of expressions.</param>
		/// <param name="with">The last argument to the With command.</param>
		private static void GetExpressionsFromCsvFile(string folder, string file, Encoding fallback, ref Expression[] expressions, ref int expressionCount, ref Sub[] subs, ref int subCount, ref string with) {
			/*
			 * Read all lines from the file.
			 * */
			string[] lines = OpenBveApi.Text.GetLinesFromFile(file, fallback);
			/*
			 * Now, split the lines on comma boundaries.
			 * Ignore comments, then preprocess and add
			 * expressions from all remaining cells.
			 * */
			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].Length != 0) {
					string[] cells = lines[i].Split(',');
					for (int j = 0; j < cells.Length; j++) {
						if (cells[j].Length != 0) {
							cells[j] = cells[j].Trim();
							if (cells[j].Length != 0) {
								if (cells[j][0] != ';') {
									PreprocessAndAddExpressions(folder, file, false, fallback, i, j, cells[j], ref expressions, ref expressionCount, ref subs, ref subCount, ref with);
								}
							}
						}
					}
				}
			}
		}
		
		/// <summary>Gets an array of all expressions contained in a RW route.</summary>
		/// <param name="folder">The platform-specific absolute path to the folder where the main route file is stored.</param>
		/// <param name="file">The platform-specific absolute path to the file that contains the cell.</param>
		/// <param name="fallback">The fallback encoding.</param>
		/// <param name="expressions">The list of expressions.</param>
		/// <param name="expressionCount">The number of expressions.</param>
		/// <param name="section">The last opened section.</param>
		private static void GetExpressionsFromRwFile(string folder, string file, Encoding fallback, ref Expression[] expressions, ref int expressionCount, ref Sub[] subs, ref int subCount, ref string section) {
			/*
			 * Read all lines from the file.
			 * */
			string[] lines = OpenBveApi.Text.GetLinesFromFile(file, fallback);
			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].Length != 0) {
					lines[i] = lines[i].Trim();
				}
			}
			/*
			 * Find the first section in the file.
			 * The text prior to the first section
			 * is the file's comments.
			 * */
			int start;
			StringBuilder descriptions = new StringBuilder();
			for (start = 0; start < lines.Length; start++) {
				if (lines[start].Length != 0 && lines[start][0] == '[') {
					break;
				} else {
					descriptions.AppendLine(lines[start]);
				}
			}
			if (descriptions.Length != 0) {
				if (expressions.Length == expressionCount) {
					Array.Resize<Expression>(ref expressions, expressions.Length << 1);
				}
				expressions[expressionCount] = new Expression(file, 0, 0, "Route.Comment", null, null, new string[] { descriptions.ToString().Trim() });
				expressionCount++;
			}
			/*
			 * For the rest of the file, the syntax of each
			 * line depends on the section.
			 * */
			bool isRailwaySection = section.Equals("Railway", StringComparison.OrdinalIgnoreCase);
			for (int i = start; i < lines.Length; i++) {
				if (lines[i].Length != 0) {
					if (lines[i][0] == '[') {
						/*
						 * A new section is started.
						 * */
						int semicolon = lines[i].IndexOf(';');
						if (semicolon >= 0) {
							lines[i] = lines[i].Substring(0, semicolon).TrimEnd();
						}
						if (lines[i][lines[i].Length - 1] == ']') {
							section = lines[i].Substring(1, lines[i].Length - 2).Trim();
							string sectionLower = section.ToLowerInvariant();
							switch (sectionLower) {
								case "structure":
									IO.ReportInvalidData(file, i, 0, "The [Structure] section is invalid in RW routes. Use [Object] instead.");
									break;
								case "track":
									IO.ReportInvalidData(file, i, 0, "The [Track] section is invalid in RW routes. Use [Railway] instead.");
									break;
							}
							isRailwaySection = sectionLower == "railway";
						} else {
							IO.ReportInvalidData(file, i, 0, "A section must end in a closing bracket.");
							section = "Invalid";
							isRailwaySection = false;
						}
					} else if (isRailwaySection) {
						/*
						 * The line from the [Railway] section.
						 * */
						int semicolon = lines[i].IndexOf(';');
						if (semicolon >= 0) {
							lines[i] = lines[i].Substring(0, semicolon).TrimEnd();
						}
						if (lines[i].Length != 0) {
							PreprocessAndAddExpressions(folder, file, true, fallback, i, 0, lines[i], ref expressions, ref expressionCount, ref subs, ref subCount, ref section);
						}
					} else {
						/*
						 * A line from a other section.
						 * */
						int semicolon = lines[i].IndexOf(';');
						if (semicolon >= 0) {
							int equals = lines[i].IndexOf('=');
							if (equals == -1 | equals > semicolon) {
								lines[i] = lines[i].Substring(0, semicolon).TrimEnd();
							}
						}
						if (lines[i].Length != 0) {
							PreprocessAndAddExpressions(folder, file, true, fallback, i, 0, lines[i], ref expressions, ref expressionCount, ref subs, ref subCount, ref section);
						}
					}
				}
			}
		}
		
		/// <summary>Removes a leading slash or backslash character from a path if present.</summary>
		/// <param name="path">The path.</param>
		/// <returns>The path without the leading slash or backslash character.</returns>
		private static string RemoveLeadingSlashOrBackslash(string path) {
			if (path.Length != 0 && (path[0] == '/' || path[0] == '\\')) {
				return path.Substring(1);
			} else {
				return path;
			}
		}
		
	}
}