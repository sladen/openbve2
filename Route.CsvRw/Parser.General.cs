using System;
using System.Globalization;
using System.Text;

namespace Plugin {
	internal static partial class Parser {
		
		/// <summary>Process all general expressions.</summary>
		/// <param name="expressions">The list of expressions.</param>
		/// <param name="objectFolder">The platform-specific absolute path to the Object folder.</param>
		/// <param name="pluginDataFolder">The platform-specific absolute path to the data folder of the plugin.</param>
		/// <param name="fallback">The fallback encoding.</param>
		/// <param name="structures">Receives the structures used in the route.</param>
		private static void ProcessGeneralExpressions(Expression[] expressions, string objectFolder, string pluginDataFolder, Encoding fallback, out Options options, out Structures structures) {
			options = new Options();
			structures = new Structures(pluginDataFolder, fallback);
			for (int i = 0; i < expressions.Length; i++) {
				if (expressions[i].Type == ExpressionType.General) {
					string command = expressions[i].CsvEquivalentCommand.ToLowerInvariant();
					switch (command) {
						case "options.unitoflength":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, int.MaxValue)) {
								if (expressions[i].Arguments != null && expressions[i].Arguments.Length != 0) {
									options.UnitsOfLength = new double[expressions[i].Arguments.Length];
									for (int j = 0; j < expressions[i].Arguments.Length; j++) {
										IO.ParseDoubleFromArgument(expressions[i], j, "factor" + j.ToString(CultureInfo.InvariantCulture), double.MinValue, double.MaxValue, j == 0 ? 1.0 : 0.0, out options.UnitsOfLength[j]);
									}
								} else {
									options.UnitsOfLength = new double[] { 1.0 };
								}
							}
							break;
						case "options.unitofspeed":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 1)) {
								IO.ParseDoubleFromArgument(expressions[i], 0, "factor", double.MinValue, double.MaxValue, 1.0, out options.UnitOfSpeed);
								options.UnitOfSpeed *= 0.277777777777777778;
							}
							break;
						case "options.blocklength":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 1)) {
								IO.ParseDoubleFromArgumentExtended(expressions[i], 0, "length", 25.0, options.UnitsOfLength, out options.BlockLength);
							}
							break;
						case "route.ambientlight":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 3)) {
								double red, green, blue;
								IO.ParseDoubleFromArgument(expressions[i], 0, "red", 0.0, 255.0, 160.0, out red);
								IO.ParseDoubleFromArgument(expressions[i], 1, "green", 0.0, 255.0, 160.0, out green);
								IO.ParseDoubleFromArgument(expressions[i], 2, "blue", 0.0, 255.0, 160.0, out blue);
								const double factor = 1.0 / 255.0;
								options.Light.AmbientLight = new OpenBveApi.Color.ColorRGB((float)(factor * red), (float)(factor * green), (float)(factor * blue));
							}
							break;
						case "route.directionallight":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 3)) {
								double red, green, blue;
								IO.ParseDoubleFromArgument(expressions[i], 0, "red", 0.0, 255.0, 160.0, out red);
								IO.ParseDoubleFromArgument(expressions[i], 1, "green", 0.0, 255.0, 160.0, out green);
								IO.ParseDoubleFromArgument(expressions[i], 2, "blue", 0.0, 255.0, 160.0, out blue);
								const double factor = 0.00392156862745098039;
								options.Light.DiffuseLight = new OpenBveApi.Color.ColorRGB((float)(factor * red), (float)(factor * green), (float)(factor * blue));
							}
							break;
						case "route.lightdirection":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 2)) {
								double theta, phi;
								IO.ParseDoubleFromArgument(expressions[i], 0, "theta", double.MinValue, double.MaxValue, 60.0, out theta);
								IO.ParseDoubleFromArgument(expressions[i], 1, "phi", double.MinValue, double.MaxValue, -26.0, out phi);
								const double factor = 0.0174532925199432958;
								theta *= factor;
								phi *= factor;
								double x = Math.Cos(theta) * Math.Sin(phi);
								double y = -Math.Sin(theta);
								double z = Math.Cos(theta) * Math.Cos(phi);
								options.Light.LightDirection = new OpenBveApi.Math.Vector3(x, y, z);
							}
							break;
						case "structure.ground":
						case "structure.rail":
						case "structure.walll":
						case "structure.wallr":
						case "structure.dikel":
						case "structure.diker":
						case "structure.forml":
						case "structure.formr":
						case "structure.formcl":
						case "structure.formcr":
						case "structure.roofl":
						case "structure.roofr":
						case "structure.roofcl":
						case "structure.roofcr":
						case "structure.crackl":
						case "structure.crackr":
						case "structure.freeobj":
						case "structure.beacon":
							if (IO.CheckExpression(expressions[i], 0, 1, true, 1, 1)) {
								if (expressions[i].Suffix == null || expressions[i].Suffix.Equals(".load", StringComparison.OrdinalIgnoreCase)) {
									int structureIndex;
									int minimum = 0;
									if (command == "structure.roofl" | command == "structure.roofr" | command == "structure.roofcl" | command == "structure.roofcr") {
										minimum = 1;
									}
									IO.ParseIntFromIndex(expressions[i], 0, "structureIndex", minimum, int.MaxValue, minimum, out structureIndex);
									if (OpenBveApi.Path.ContainsInvalidPathCharacters(expressions[i].Arguments[0])) {
										IO.ReportInvalidArgument(expressions[i], 0, "fileName", "Invalid path characters.");
									} else {
										string fileName = OpenBveApi.Path.CombineFile(objectFolder, RemoveLeadingSlashOrBackslash(expressions[i].Arguments[0]));
										switch (command) {
											case "structure.ground":
												structures.Ground.Set(structureIndex, 0, fileName, false);
												structures.Cycle.Set(structureIndex, new int[] { structureIndex });
												break;
											case "structure.rail":
												structures.Rail.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.walll":
												structures.WallL.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.wallr":
												structures.WallR.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.dikel":
												structures.DikeL.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.diker":
												structures.DikeR.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.forml":
												structures.FormL.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.formr":
												structures.FormR.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.formcl":
												structures.FormCL.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.formcr":
												structures.FormCR.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.roofl":
												structures.RoofL.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.roofr":
												structures.RoofR.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.roofcl":
												structures.RoofCL.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.roofcr":
												structures.RoofCR.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.crackl":
												structures.CrackL.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.crackr":
												structures.CrackR.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.freeobj":
												structures.FreeObj.Set(structureIndex, 0, fileName, false);
												break;
											case "structure.beacon":
												structures.Beacon.Set(structureIndex, 0, fileName, false);
												break;
											default:
												throw new InvalidOperationException();
										}
									}
								} else {
									IO.ReportInvalidSuffix(expressions[i]);
								}
							}
							break;
						case "structure.pole":
							if (IO.CheckExpression(expressions[i], 0, 2, true, 1, 1)) {
								if (expressions[i].Suffix == null || expressions[i].Suffix.Equals(".load", StringComparison.OrdinalIgnoreCase)) {
									int structureIndex2;
									IO.ParseIntFromIndex(expressions[i], 0, "structureIndex2", 0, int.MaxValue, 0, out structureIndex2);
									int structureIndex1;
									IO.ParseIntFromIndex(expressions[i], 1, "structureIndex1", 0, int.MaxValue, 0, out structureIndex1);
									if (OpenBveApi.Path.ContainsInvalidPathCharacters(expressions[i].Arguments[0])) {
										IO.ReportInvalidArgument(expressions[i], 0, "fileName", "Invalid path characters.");
									} else {
										string fileName = OpenBveApi.Path.CombineFile(objectFolder, RemoveLeadingSlashOrBackslash(expressions[i].Arguments[0]));
										structures.Pole.Set(structureIndex1, structureIndex2, fileName, false);
										if (structureIndex2 == 0) {
											structures.PoleMirrored.Set(structureIndex1, 0, fileName, true);
										}
									}
								} else {
									IO.ReportInvalidSuffix(expressions[i]);
								}
							}
							break;
						case "cycle.ground":
							if (IO.CheckExpression(expressions[i], 0, 1, true, 1, int.MaxValue)) {
								if (expressions[i].Suffix == null || expressions[i].Suffix.Equals(".params", StringComparison.OrdinalIgnoreCase)) {
									int cycleIndex;
									IO.ParseIntFromIndex(expressions[i], 0, "cycleIndex", 0, int.MaxValue, 0, out cycleIndex);
									int[] groundStructures = new int[expressions[i].Arguments.Length];
									bool success = true;
									for (int j = 0; j < expressions[i].Arguments.Length; j++) {
										if (!IO.ParseIntFromArgument(expressions[i], j, "structureIndex" + j.ToString(CultureInfo.InvariantCulture), 0, int.MaxValue, 0, out groundStructures[j])) {
											success = false;
											break;
										} else if (!structures.Ground.Exists(groundStructures[j], 0)) {
											IO.ReportInvalidData(expressions[i], "A ground structure does not exist.");
										}
									}
									if (success) {
										structures.Cycle.Set(cycleIndex, groundStructures);
									}
								} else {
									IO.ReportInvalidSuffix(expressions[i]);
								}
							}
							break;
					}
				}
			}
		}

		
	}
}