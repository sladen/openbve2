using System;
using System.Text;

namespace Plugin {
	internal static partial class Parser {
		
		/// <summary>Process all track expressions.</summary>
		/// <param name="expressions">The list of expressions.</param>
		/// <param name="options">The options used in the route.</param>
		/// <param name="structures">The structures used in the route.</param>
		/// <param name="encoding">The fallback encoding to load objects.</param>
		/// <param name="isRw">Whether the route is of RW format.</param>
		private static void ProcessTrackExpressions(Expression[] expressions, Options options, Structures structures, Encoding encoding, bool isRw) {
			/*
			 * Initialize the blocks. Make sure that the
			 * first block exists and has rail 0 defined.
			 * */
			BlockCollection blocks = new BlockCollection(options.BlockLength);
			{
				Block block = blocks.GetCurrentBlock(0.0);
				block.Height = isRw ? 0.3 : 0.0;
				block.HeightDefined = true;
				Rail rail = block.GetRail(0);
				rail.Status = RailStatus.Continuous;
				rail.StartX = 0.0;
				rail.StartY = 0.0;
				rail.EndX = 0.0;
				rail.EndY = 0.0;
				rail.Type = 0;
			}
			/*
			 * Process the expressions.
			 * */
			Block initialBlock = null;
			for (int i = 0; i < expressions.Length; i++) {
				if (expressions[i].Type == ExpressionType.Track) {
					Block block = blocks.GetCurrentBlock(expressions[i].Position);
					string command = expressions[i].CsvEquivalentCommand.ToLowerInvariant();
					switch (command) {
							
							/*
							 * --- Track geometry ---
							 * */
						case "track.turn":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 1)) {
								double ratio;
								IO.ParseDoubleFromArgument(expressions[i], 0, "ratio", double.MinValue, double.MaxValue, 0.0, out ratio);
								block.TurnRatio = ratio;
							}
							break;
						case "track.curve":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 2)) {
								double radius, cant;
								IO.ParseDoubleFromArgumentExtended(expressions[i], 0, "radius", 0.0, options.UnitsOfLength, out radius);
								IO.ParseDoubleFromArgument(expressions[i], 1, "cant", double.MinValue, double.MaxValue, 0.0, out cant);
								block.CurveRadius = radius;
								block.CurveCant = cant;
							}
							break;
						case "track.pitch":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 2)) {
								double pitch;
								IO.ParseDoubleFromArgument(expressions[i], 0, "pitch", double.MinValue, double.MaxValue, 0.0, out pitch);
								block.Pitch = -0.001 * pitch;
							}
							break;
						case "track.height":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 2)) {
								double height;
								IO.ParseDoubleFromArgumentExtended(expressions[i], 0, "height", 0.0, options.UnitsOfLength, out height);
								if (isRw) {
									height += 0.3;
								}
								block.Height = height;
								block.HeightDefined = true;
							}
							break;
							
							/* 
							 * --- Rail creation and ending ---
							 *  */
						case "track.railstart":
						case "track.rail":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 4)) {
								int index;
								IO.ParseIntFromArgument(expressions[i], 0, "index", 0, int.MaxValue, 0, out index);
								Rail rail = block.GetRail(index);
								if (rail.Updated) {
									IO.ReportInvalidData(expressions[i], "The rail was already updated in the same block.");
								} else {
									double x, y;
									IO.ParseDoubleFromArgumentExtended(expressions[i], 1, "x", rail.StartX, options.UnitsOfLength, out x);
									IO.ParseDoubleFromArgumentExtended(expressions[i], 2, "y", rail.StartY, options.UnitsOfLength, out y);
									int type;
									IO.ParseIntFromArgument(expressions[i], 3, "type", 0, int.MaxValue, rail.Type, out type);
									if (rail.Status != RailStatus.NotAvailable & command == "track.railstart") {
										IO.ReportInvalidData(expressions[i], "The rail already exists.");
									}
									rail.Status = RailStatus.Continuous;
									rail.Updated = true;
									rail.StartX = x;
									rail.StartY = y;
									rail.EndX = x;
									rail.EndY = y;
									rail.Type = type;
									if (blocks.GetPreviousBlock(expressions[i].Position, out block)) {
										Rail previousRail = block.GetRail(index);
										if (previousRail.Status == RailStatus.Continuous) {
											previousRail.EndX = x;
											previousRail.EndY = y;
										}
									}
								}
							}
							break;
						case "track.railend":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 3)) {
								int index;
								IO.ParseIntFromArgument(expressions[i], 0, "index", 0, int.MaxValue, 0, out index);
								Block previousBlock;
								if (blocks.GetPreviousBlock(expressions[i].Position, out previousBlock)) {
									Rail previousRail = previousBlock.GetRail(index);
									if (previousRail.Status == RailStatus.Continuous) {
										double x, y;
										IO.ParseDoubleFromArgumentExtended(expressions[i], 1, "x", previousRail.EndX, options.UnitsOfLength, out x);
										IO.ParseDoubleFromArgumentExtended(expressions[i], 2, "y", previousRail.EndY, options.UnitsOfLength, out y);
										previousRail.Status = RailStatus.Discontinuous;
										previousRail.EndX = x;
										previousRail.EndY = y;
										Rail rail = block.GetRail(index);
										rail.Status = RailStatus.NotAvailable;
										rail.StartX = x;
										rail.StartY = y;
										rail.EndX = x;
										rail.EndY = y;
									} else {
										IO.ReportInvalidData(expressions[i], "The rail does not exist.");
									}
								} else {
									IO.ReportInvalidData(expressions[i], "The command is used at a location where accessing the previous block is not possible.");
								}
							}
							break;
						case "track.railtype":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 2)) {
								int index;
								IO.ParseIntFromArgument(expressions[i], 0, "index", 0, int.MaxValue, 0, out index);
								Rail rail = block.GetRail(index);
								if (rail.Status != RailStatus.NotAvailable) {
									int type;
									IO.ParseIntFromArgument(expressions[i], 1, "type", 0, int.MaxValue, 0, out type);
									rail.Type = type;
								} else {
									IO.ReportInvalidData(expressions[i], "The rail does not exist.");
								}
							}
							break;
							
							/*
							 * --- Repeating structures ---
							 * */
						case "track.ground":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 1)) {
								int cycleIndex;
								IO.ParseIntFromArgument(expressions[i], 0, "cycleIndex", 0, int.MaxValue, 0, out cycleIndex);
								block.GroundCycle = cycleIndex;
							}
							break;
						case "track.wall":
						case "track.dike":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 3)) {
								int railIndex;
								IO.ParseIntFromArgument(expressions[i], 0, "railIndex", 0, int.MaxValue, 0, out railIndex);
								Rail rail = block.GetRail(railIndex);
								if (rail.Status == RailStatus.NotAvailable) {
									IO.ReportInvalidData(expressions[i], "The rail does not exist.");
								}
								int side;
								IO.ParseIntFromArgument(expressions[i], 1, "side", -1, 1, 0, out side);
								int structureIndex;
								IO.ParseIntFromArgument(expressions[i], 2, "structureIndex", 0, int.MaxValue, 0, out structureIndex);
								if (command == "track.wall") {
									rail.WallType = structureIndex;
									rail.WallSide = side;
								} else {
									rail.DikeType = structureIndex;
									rail.DikeSide = side;
								}
							}
							break;
						case "track.wallend":
						case "track.dikeend":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 1)) {
								int railIndex;
								IO.ParseIntFromArgument(expressions[i], 0, "railIndex", 0, int.MaxValue, 0, out railIndex);
								Block previousBlock;
								if (blocks.GetPreviousBlock(expressions[i].Position, out previousBlock)) {
									Rail previousRail = previousBlock.GetRail(railIndex);
									if (previousRail.Status == RailStatus.NotAvailable) {
										IO.ReportInvalidData(expressions[i], "The rail does not exist.");
									}
								} else {
									IO.ReportInvalidData(expressions[i], "The command is used at a location where accessing the previous block is not possible.");
								}
								Rail rail = block.GetRail(railIndex);
								if (command == "track.wallend") {
									if (rail.WallType >= 0) {
										rail.WallType = -1;
									} else {
										IO.ReportInvalidData(expressions[i], "The wall does not exist.");
									}
								} else {
									if (rail.DikeType >= 0) {
										rail.DikeType = -1;
									} else {
										IO.ReportInvalidData(expressions[i], "The dike does not exist.");
									}
								}
							}
							break;
						case "track.pole":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 5)) {
								int railIndex;
								IO.ParseIntFromArgument(expressions[i], 0, "railIndex", 0, int.MaxValue, 0, out railIndex);
								Rail rail = block.GetRail(railIndex);
								if (rail.Status == RailStatus.NotAvailable) {
									IO.ReportInvalidData(expressions[i], "The rail does not exist.");
								}
								int structureIndex2;
								IO.ParseIntFromArgument(expressions[i], 1, "structureIndex2", 0, int.MaxValue, 0, out structureIndex2);
								double location, interval;
								IO.ParseDoubleFromArgument(expressions[i], 2, "location", double.MinValue, double.MaxValue, 0, out location);
								IO.ParseDoubleFromArgument(expressions[i], 3, "interval", 0.0, double.MaxValue, 0, out interval);
								int structureIndex1;
								IO.ParseIntFromArgument(expressions[i], 4, "structureIndex1", 0, int.MaxValue, 0, out structureIndex1);
								int spacing = (int)Math.Round(interval / blocks.BlockLength);
								if (spacing < 1) {
									spacing = 1;
								}
								rail.Pole = new Pole(structureIndex1, structureIndex2, location, spacing);
							}
							break;
						case "track.poleend":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 1)) {
								int railIndex;
								IO.ParseIntFromArgument(expressions[i], 0, "railIndex", 0, int.MaxValue, 0, out railIndex);
								Block previousBlock;
								if (blocks.GetPreviousBlock(expressions[i].Position, out previousBlock)) {
									Rail previousRail = previousBlock.GetRail(railIndex);
									if (previousRail.Status == RailStatus.NotAvailable) {
										IO.ReportInvalidData(expressions[i], "The rail does not exist.");
									}
								} else {
									IO.ReportInvalidData(expressions[i], "The command is used at a location where accessing the previous block is not possible.");
								}
								Rail rail = block.GetRail(railIndex);
								if (rail.Pole.StructureIndex1 >= 0) {
									rail.Pole.StructureIndex1 = -1;
								} else {
									IO.ReportInvalidData(expressions[i], "The pole does not exist.");
								}
							}
							break;
							
							/*
							 * --- Station-related ---
							 * */
						case "track.form":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 4)) {
								int railIndex1;
								IO.ParseIntFromArgument(expressions[i], 0, "railIndex1", 0, int.MaxValue, 0, out railIndex1);
								Rail rail1 = block.GetRail(railIndex1);
								if (rail1.Status != RailStatus.NotAvailable) {
									FormLayout layout = FormLayout.Invalid;
									if (expressions[i].Arguments != null && expressions[i].Arguments.Length >= 2) {
										if (expressions[i].Arguments[1].Equals("L", StringComparison.OrdinalIgnoreCase)) {
											layout = FormLayout.Left;
										} else if (expressions[i].Arguments[1].Equals("R", StringComparison.OrdinalIgnoreCase)) {
											layout = FormLayout.Right;
										}
									}
									int railIndex2 = 0;
									if (layout == FormLayout.Invalid) {
										IO.ParseIntFromArgument(expressions[i], 1, "railIndex2", isRw ? -9 : -2, int.MaxValue, 0, out railIndex2);
										if (railIndex2 == -9 & isRw | railIndex2 == -1) {
											layout = FormLayout.Left;
										} else if (railIndex2 == 9 & isRw | railIndex2 == -2) {
											layout = FormLayout.Right;
										} else if (railIndex2 < 0) {
											if (isRw) {
												IO.ReportInvalidData(expressions[i], "Invalid railIndex2. Allowed values are -9, -2, -1, 9, and existing rails.");
											} else {
												IO.ReportInvalidData(expressions[i], "Invalid railIndex2. Allowed values are -2, -1, and existing rails.");
											}
										} else {
											layout = railIndex1 == railIndex2 ? FormLayout.Stub : FormLayout.SecondaryRail;
										}
									}
									if (layout == FormLayout.SecondaryRail) {
										Rail rail2 = block.GetRail(railIndex2);
										if (rail2.Status == RailStatus.NotAvailable) {
											IO.ReportInvalidData(expressions[i], "The rail does not exist.");
											layout = FormLayout.Invalid;
										}
									}
									if (layout != FormLayout.Invalid) {
										int roofStructureIndex, formStructureIndex;
										IO.ParseIntFromArgument(expressions[i], 2, "roofStructureIndex", 0, int.MaxValue, 0, out roofStructureIndex);
										IO.ParseIntFromArgument(expressions[i], 3, "formStructureIndex", 0, int.MaxValue, 0, out formStructureIndex);
										if (rail1.Forms == null) {
											rail1.Forms = new Form[] { null, null, null, null };
										} else if (rail1.FormCount == rail1.Forms.Length) {
											Array.Resize<Form>(ref rail1.Forms, rail1.Forms.Length << 1);
										}
										rail1.Forms[rail1.FormCount] = new Form(formStructureIndex, roofStructureIndex, layout, railIndex2);
										rail1.FormCount++;
									}
								} else {
									IO.ReportInvalidData(expressions[i], "The rail does not exist.");
								}
							}
							break;
						case "track.stop":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 4)) {
								const int railIndex = 0;
								Rail rail = block.GetRail(railIndex);
								if (rail.Status != RailStatus.NotAvailable) {
									int side, cars;
									double backwardTolerance, forwardTolerance;
									IO.ParseIntFromArgument(expressions[i], 0, "side", -1, 1, 0, out side);
									IO.ParseDoubleFromArgument(expressions[i], 1, "backwardTolerance", double.MinValue, double.MaxValue, 5.0, out backwardTolerance);
									IO.ParseDoubleFromArgument(expressions[i], 1, "forwardTolerance", double.MinValue, double.MaxValue, 5.0, out forwardTolerance);
									if (backwardTolerance <= 0.0) {
										IO.ReportInvalidData(expressions[i].File, expressions[i].Row, expressions[i].Column, expressions[i].OriginalCommand, "The backward tolerance must be positive.");
										if (backwardTolerance == 0.0) {
											backwardTolerance = 5.0;
										} else {
											backwardTolerance = -backwardTolerance;
										}
									}
									if (forwardTolerance <= 0.0) {
										IO.ReportInvalidData(expressions[i].File, expressions[i].Row, expressions[i].Column, expressions[i].OriginalCommand, "The forward tolerance must be positive.");
										if (forwardTolerance == 0.0) {
											forwardTolerance = 5.0;
										} else {
											forwardTolerance = -forwardTolerance;
										}
									}
									IO.ParseIntFromArgument(expressions[i], 3, "cars", 0, int.MaxValue, 0, out cars);
									if (cars == 0) {
										cars = int.MaxValue;
									}
									if (side != 0) {
										if (rail.Stops == null) {
											rail.Stops = new Parser.BlockObject[] { null, null, null, null };
										} else if (rail.StopCount == rail.Stops.Length) {
											Array.Resize<BlockObject>(ref rail.Stops, rail.Stops.Length << 1);
										}
										rail.Stops[rail.StopCount] = new Parser.BlockObject(0, 1.8 * (double)side, 0.0, 0.0, 0.0, 0.0, 0.0);
										rail.StopCount++;
									}
									if (initialBlock == null) {
										initialBlock = block;
									}
								} else {
									IO.ReportInvalidData(expressions[i], "The rail does not exist.");
								}
							}
							break;
							
							/*
							 * --- Other structures ---
							 * */
						case "track.crack":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 3)) {
								int railIndex1;
								IO.ParseIntFromArgument(expressions[i], 0, "railIndex1", 0, int.MaxValue, 0, out railIndex1);
								Rail rail1 = block.GetRail(railIndex1);
								if (rail1.Status != RailStatus.NotAvailable) {
									int railIndex2;
									IO.ParseIntFromArgument(expressions[i], 1, "railIndex2", 0, int.MaxValue, 0, out railIndex2);
									if (railIndex1 != railIndex2) {
										Rail rail2 = block.GetRail(railIndex2);
										if (rail2.Status != RailStatus.NotAvailable) {
											int structureIndex;
											IO.ParseIntFromArgument(expressions[i], 2, "structureIndex", 0, int.MaxValue, 0, out structureIndex);
											if (rail1.Cracks == null) {
												rail1.Cracks = new Crack[] { null, null, null, null };
											} else if (rail1.CrackCount == rail1.Cracks.Length) {
												Array.Resize<Crack>(ref rail1.Cracks, rail1.Cracks.Length << 1);
											}
											rail1.Cracks[rail1.CrackCount] = new Crack(structureIndex, railIndex2);
											rail1.CrackCount++;
										} else {
											IO.ReportInvalidData(expressions[i], "The rail does not exist.");
										}
									} else {
										IO.ReportInvalidData(expressions[i], "The two rails must not be the same.");
									}
								} else {
									IO.ReportInvalidData(expressions[i], "The rail does not exist.");
								}
							}
							break;
						case "track.freeobj":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 7)) {
								int railIndex;
								IO.ParseIntFromArgument(expressions[i], 0, "railIndex", -1, int.MaxValue, 0, out railIndex);
								int structureIndex;
								IO.ParseIntFromArgument(expressions[i], 1, "structureIndex", 0, int.MaxValue, 0, out structureIndex);
								double x, y;
								IO.ParseDoubleFromArgumentExtended(expressions[i], 2, "x", 0.0, options.UnitsOfLength, out x);
								IO.ParseDoubleFromArgumentExtended(expressions[i], 3, "y", 0.0, options.UnitsOfLength, out y);
								double yaw, pitch, roll;
								IO.ParseDoubleFromArgument(expressions[i], 4, "yaw", double.MinValue, double.MaxValue, 0.0, out yaw);
								IO.ParseDoubleFromArgument(expressions[i], 5, "pitch", double.MinValue, double.MaxValue, 0.0, out pitch);
								IO.ParseDoubleFromArgument(expressions[i], 6, "roll", double.MinValue, double.MaxValue, 0.0, out roll);
								yaw *= 0.0174532925199432958;
								pitch *= -0.0174532925199432958;
								roll *= -0.0174532925199432958;
								BlockObject freeObj = new BlockObject(structureIndex, x, y, expressions[i].Position - block.Location, yaw, pitch, roll);
								if (railIndex == -1) {
									/*
									 * Associate to ground.
									 * */
									if (block.FreeObjs == null) {
										block.FreeObjs = new BlockObject[] { null, null, null, null };
									} else if (block.FreeObjCount == block.FreeObjs.Length) {
										Array.Resize<BlockObject>(ref block.FreeObjs, block.FreeObjs.Length << 1);
									}
									block.FreeObjs[block.FreeObjCount] = freeObj;
									block.FreeObjCount++;
								} else {
									/*
									 * Associate to rail.
									 * */
									Rail rail = block.GetRail(railIndex);
									if (rail.Status != RailStatus.NotAvailable) {
										if (rail.FreeObjs == null) {
											rail.FreeObjs = new BlockObject[] { null, null, null, null };
										} else if (rail.FreeObjCount == rail.FreeObjs.Length) {
											Array.Resize<BlockObject>(ref rail.FreeObjs, rail.FreeObjs.Length << 1);
										}
										rail.FreeObjs[rail.FreeObjCount] = freeObj;
										rail.FreeObjCount++;
									} else {
										IO.ReportInvalidData(expressions[i], "The rail does not exist.");
									}
								}
							}
							break;
						case "track.beacon":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 9)) {
								const int railIndex = 0;
								Rail rail = block.GetRail(railIndex);
								if (rail.Status != RailStatus.NotAvailable) {
									int type, structureIndex, section, data;
									IO.ParseIntFromArgument(expressions[i], 0, "type", 0, int.MaxValue, 0, out type);
									IO.ParseIntFromArgument(expressions[i], 1, "structureIndex", -1, int.MaxValue, 0, out structureIndex);
									IO.ParseIntFromArgument(expressions[i], 2, "section", -1, int.MaxValue, 0, out section);
									IO.ParseIntFromArgument(expressions[i], 3, "data", int.MinValue, int.MaxValue, 0, out data);
									double x, y;
									IO.ParseDoubleFromArgumentExtended(expressions[i], 4, "x", 0.0, options.UnitsOfLength, out x);
									IO.ParseDoubleFromArgumentExtended(expressions[i], 5, "y", 0.0, options.UnitsOfLength, out y);
									double yaw, pitch, roll;
									IO.ParseDoubleFromArgument(expressions[i], 6, "yaw", double.MinValue, double.MaxValue, 0.0, out yaw);
									IO.ParseDoubleFromArgument(expressions[i], 7, "pitch", double.MinValue, double.MaxValue, 0.0, out pitch);
									IO.ParseDoubleFromArgument(expressions[i], 8, "roll", double.MinValue, double.MaxValue, 0.0, out roll);
									yaw *= 0.0174532925199432958;
									pitch *= -0.0174532925199432958;
									roll *= -0.0174532925199432958;
									if (structureIndex >= 0) {
										BlockObject beacon = new BlockObject(structureIndex, x, y, expressions[i].Position - block.Location, yaw, pitch, roll);
										if (rail.Beacons == null) {
											rail.Beacons = new BlockObject[] { null, null, null, null };
										} else if (rail.BeaconCount == rail.Beacons.Length) {
											Array.Resize<BlockObject>(ref rail.Beacons, rail.Beacons.Length << 1);
										}
										rail.Beacons[rail.BeaconCount] = beacon;
										rail.BeaconCount++;
									}
								} else {
									IO.ReportInvalidData(expressions[i], "The rail does not exist.");
								}
							}
							break;
						case "track.transponder":
						case "track.tr":
						case "track.atssn":
						case "track.atsp":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 8)) {
								const int railIndex = 0;
								Rail rail = block.GetRail(railIndex);
								if (rail.Status != RailStatus.NotAvailable) {
									int type, signal, switchSystem;
									double x, y;
									double yaw, pitch, roll;
									if (command == "track.atssn" | command == "track.atsp") {
										type = command == "track.atssn" ? 0 : 3;
										signal = 0;
										switchSystem = 0;
										x = 0.0;
										y = 0.0;
										yaw = 0.0;
										pitch = 0.0;
										roll = 0.0;
									} else {
										IO.ParseIntFromArgument(expressions[i], 0, "type", 0, 4, 0, out type);
										IO.ParseIntFromArgument(expressions[i], 1, "signal", -1, int.MaxValue, 0, out signal);
										IO.ParseIntFromArgument(expressions[i], 2, "switchSystem", -1, 0, 0, out switchSystem);
										IO.ParseDoubleFromArgumentExtended(expressions[i], 3, "x", 0.0, options.UnitsOfLength, out x);
										IO.ParseDoubleFromArgumentExtended(expressions[i], 4, "y", 0.0, options.UnitsOfLength, out y);
										IO.ParseDoubleFromArgument(expressions[i], 5, "yaw", double.MinValue, double.MaxValue, 0.0, out yaw);
										IO.ParseDoubleFromArgument(expressions[i], 6, "pitch", double.MinValue, double.MaxValue, 0.0, out pitch);
										IO.ParseDoubleFromArgument(expressions[i], 7, "roll", double.MinValue, double.MaxValue, 0.0, out roll);
										yaw *= 0.0174532925199432958;
										pitch *= -0.0174532925199432958;
										roll *= -0.0174532925199432958;
									}
									BlockObject beacon = new BlockObject(-type - 1, x, y, expressions[i].Position - block.Location, yaw, pitch, roll);
									if (rail.Beacons == null) {
										rail.Beacons = new BlockObject[] { null, null, null, null };
									} else if (rail.BeaconCount == rail.Beacons.Length) {
										Array.Resize<BlockObject>(ref rail.Beacons, rail.Beacons.Length << 1);
									}
									rail.Beacons[rail.BeaconCount] = beacon;
									rail.BeaconCount++;
								} else {
									IO.ReportInvalidData(expressions[i], "The rail does not exist.");
								}
							}
							break;
						case "track.limit":
							if (IO.CheckExpression(expressions[i], 0, 0, false, 0, 3)) {
								const int railIndex = 0;
								Rail rail = block.GetRail(railIndex);
								if (rail.Status != RailStatus.NotAvailable) {
									double speed;
									IO.ParseDoubleFromArgument(expressions[i], 0, "speed", 0.0, double.MaxValue, 0.0, out speed);
									int side, cource;
									IO.ParseIntFromArgument(expressions[i], 1, "side", -1, 1, 0, out side);
									IO.ParseIntFromArgument(expressions[i], 2, "cource", -1, 1, 0, out cource);
									if (side != 0) {
										if (rail.Limits == null) {
											rail.Limits = new Parser.BlockObject[] { null, null, null, null };
										} else {
											while (rail.Limits.Length - rail.LimitCount < 4) {
												Array.Resize<BlockObject>(ref rail.Limits, rail.Limits.Length << 1);
											}
										}
										double x = 2.2 * (double)side;
										if (speed == 0.0) {
											rail.Limits[rail.LimitCount] = new Parser.BlockObject(10, x, 0.0, 0.0, 0.0, 0.0, 0.0);
											rail.LimitCount++;
										} else {
											rail.Limits[rail.LimitCount] = new Parser.BlockObject(12 + cource, x, 0.0, 0.0, 0.0, 0.0, 0.0);
											rail.LimitCount++;
											int displayValue = speed < 999.0 ? (int)Math.Ceiling(speed) : 999;
											if (displayValue < 10) {
												int ones = displayValue;
												rail.Limits[rail.LimitCount] = new Parser.BlockObject(ones, x, 0.0, 0.0, 0.0, 0.0, 0.0);
												rail.LimitCount++;
											} else if (displayValue < 100) {
												int tens = displayValue / 10;
												int ones = displayValue % 10;
												rail.Limits[rail.LimitCount + 0] = new Parser.BlockObject(ones, x + 0.09375, 0.0, 0.0, 0.0, 0.0, 0.0);
												rail.Limits[rail.LimitCount + 1] = new Parser.BlockObject(tens, x - 0.09375, 0.0, 0.0, 0.0, 0.0, 0.0);
												rail.LimitCount += 2;
											} else {
												int hundreds = displayValue / 100;
												int tens = (displayValue / 10) % 10;
												int ones = displayValue % 10;
												rail.Limits[rail.LimitCount + 0] = new Parser.BlockObject(ones, x + 0.1875, 0.0, 0.0, 0.0, 0.0, 0.0);
												rail.Limits[rail.LimitCount + 1] = new Parser.BlockObject(tens, x, 0.0, 0.0, 0.0, 0.0, 0.0);
												rail.Limits[rail.LimitCount + 2] = new Parser.BlockObject(hundreds, x - 0.1875, 0.0, 0.0, 0.0, 0.0, 0.0);
												rail.LimitCount += 3;
											}
										}
									}
								} else {
									IO.ReportInvalidData(expressions[i], "The rail does not exist.");
								}
							}
							break;
					}
				}
			}
			/*
			 * Ensure that all blocks until the location
			 * of the last expression have been created.
			 * */
			blocks.GetCurrentBlock(expressions[expressions.Length - 1].Position);
			/*
			 * Interpolate the height for blocks
			 * without a defined height.
			 * */
			{
				int start = 0;
				double height = blocks.Blocks[0].Height;
				for (int i = 1; i < blocks.BlockCount; i++) {
					if (blocks.Blocks[i].HeightDefined) {
						for (int j = start + 1; j < i; j++) {
							double t = (double)(j - start) / (double)(i - start);
							blocks.Blocks[j].Height = (1.0 - t) * blocks.Blocks[start].Height + t * blocks.Blocks[i].Height;
						}
						start = i;
						height = blocks.Blocks[i].Height;
					}
				}
				for (int i = start + 1; i < blocks.BlockCount; i++) {
					blocks.Blocks[i].Height = height;
				}
			}
			/*
			 * Compute the position and orientation
			 * for the beginning of each block.
			 * */
			{
				OpenBveApi.Math.Vector3 position = OpenBveApi.Math.Vector3.Null;
				OpenBveApi.Math.Orientation3 orientation = OpenBveApi.Math.Orientation3.Default;
				for (int i = 0; i < blocks.BlockCount; i++) {
					double turnAngle = Math.Atan(blocks.Blocks[i].TurnRatio);
					if (blocks.Blocks[i].CurveRadius != 0.0) {
						/*
						 * Curved track.
						 * */
						double pitchAngle = Math.Atan(blocks.Blocks[i].Pitch);
						if (i != 0) {
							pitchAngle -= Math.Atan(blocks.Blocks[i - 1].Pitch);
						}
						if (pitchAngle != 0.0) {
							orientation.RotateAroundXAxis(Math.Cos(pitchAngle), Math.Sin(pitchAngle));
						}
						double curvedLengthOnGround = blocks.BlockLength / Math.Sqrt(1.0 + blocks.Blocks[i].Pitch * blocks.Blocks[i].Pitch);
						double curveAngle = 0.5 * curvedLengthOnGround / blocks.Blocks[i].CurveRadius;
						double straightLengthOnGround = 2.0 * blocks.Blocks[i].CurveRadius * Math.Sin(curveAngle);
						double straightLengthOnSlope = Math.Sqrt(straightLengthOnGround * straightLengthOnGround + curvedLengthOnGround * curvedLengthOnGround * blocks.Blocks[i].Pitch * blocks.Blocks[i].Pitch);
						double cosineOfAngle = Math.Cos(curveAngle + turnAngle);
						double sineOfAngle = Math.Sin(curveAngle + turnAngle);
						orientation.Rotate(OpenBveApi.Math.Vector3.Up, cosineOfAngle, sineOfAngle);
						blocks.Blocks[i].Position = position;
						blocks.Blocks[i].Orientation = orientation;
						position += orientation.Z * straightLengthOnSlope;
						cosineOfAngle = Math.Cos(curveAngle);
						sineOfAngle = Math.Sin(curveAngle);
						orientation.Rotate(OpenBveApi.Math.Vector3.Up, cosineOfAngle, sineOfAngle);
					} else {
						/*
						 * Straight track.
						 * */
						double pitchAngle = Math.Atan(blocks.Blocks[i].Pitch);
						if (i != 0) {
							pitchAngle -= Math.Atan(blocks.Blocks[i - 1].Pitch);
						}
						if (pitchAngle != 0.0) {
							orientation.RotateAroundXAxis(Math.Cos(pitchAngle), Math.Sin(pitchAngle));
						}
						double cosineOfAngle = Math.Cos(turnAngle);
						double sineOfAngle = Math.Sin(turnAngle);
						orientation.Rotate(OpenBveApi.Math.Vector3.Up, cosineOfAngle, sineOfAngle);
						blocks.Blocks[i].Position = position;
						blocks.Blocks[i].Orientation = orientation;
						position += orientation.Z * blocks.BlockLength;
					}
					if (blocks.Blocks[i] == initialBlock) {
						options.InitialPosition = position;
						options.InitialOrientation = orientation;
					}
				}
			}
			/*
			 * For each block, create the associated objects.
			 * */
			for (int blockIndex = 0; blockIndex < blocks.BlockCount; blockIndex++) {
				Block block = blocks.Blocks[blockIndex];
				/*
				 * Create objects associated to the ground.
				 * */
				{
					OpenBveApi.Math.Orientation3 groundOrientation;
					{
						OpenBveApi.Math.Vector3 z = OpenBveApi.Math.Vector3.Normalize(new OpenBveApi.Math.Vector3(block.Orientation.Z.X, 0.0, block.Orientation.Z.Z));
						OpenBveApi.Math.Vector3 x = block.Orientation.X;
						OpenBveApi.Math.Vector3 y = OpenBveApi.Math.Vector3.Cross(z, x);
						groundOrientation = new OpenBveApi.Math.Orientation3(x, y, z);
					}
					/*
					 * Create the ground object.
					 * */
					if (block.GroundCycle >= 0) {
						int[] groundStructures;
						if (structures.Cycle.Get(block.GroundCycle, out groundStructures)) {
							OpenBveApi.Geometry.ObjectHandle handle;
							int groundIndex = groundStructures[blockIndex % groundStructures.Length];
							if (structures.Ground.Get(groundIndex, 0, encoding, out handle)) {
								OpenBveApi.Math.Vector3 position = block.Position;
								position.Y -= block.Height;
								Interfaces.Host.CreateObject(handle, position, groundOrientation);
							}
						}
					}
					/*
					 * Create the free objects.
					 * */
					for (int freeObjIndex = 0; freeObjIndex < block.FreeObjCount; freeObjIndex++) {
						BlockObject freeObj = block.FreeObjs[freeObjIndex];
						OpenBveApi.Geometry.ObjectHandle handle;
						if (structures.FreeObj.Get(freeObj.StructureIndex, 0, encoding, out handle)) {
							OpenBveApi.Math.Vector3 position = block.Position + groundOrientation.X * freeObj.X + groundOrientation.Y * freeObj.Y + groundOrientation.Z * freeObj.Z;
							position.Y -= block.Height;
							OpenBveApi.Math.Orientation3 orientation = groundOrientation;
							orientation.RotateAroundYAxis(Math.Cos(freeObj.Yaw), Math.Sin(freeObj.Yaw));
							orientation.RotateAroundXAxis(Math.Cos(freeObj.Pitch), Math.Sin(freeObj.Pitch));
							orientation.RotateAroundZAxis(Math.Cos(freeObj.Roll), Math.Sin(freeObj.Roll));
							Interfaces.Host.CreateObject(handle, position, orientation);
						}
					}
				}
				/*
				 * Create objects associated to the rails.
				 * */
				for (int railIndex = 0; railIndex < block.RailCount; railIndex++) {
					Rail rail = block.Rails[railIndex];
					if (rail.Status != RailStatus.NotAvailable) {
						/*
						 * Compute the position of this rail at the
						 * beginning and end of this block. Take
						 * the orientation of the next block into
						 * account where possible.
						 * */
						OpenBveApi.Math.Vector3 railStart = block.Position + block.Orientation.X * rail.StartX + block.Orientation.Y * rail.StartY;
						OpenBveApi.Math.Vector3 railEnd;
						if (blockIndex < blocks.BlockCount - 1) {
							Block nextBlock = blocks.Blocks[blockIndex + 1];
							railEnd = nextBlock.Position + nextBlock.Orientation.X * rail.EndX + nextBlock.Orientation.Y * rail.EndY;
						} else {
							railEnd = block.Position + block.Orientation.X * rail.EndX + block.Orientation.Y * rail.EndY + block.Orientation.Z * blocks.BlockLength;
						}
						OpenBveApi.Math.Orientation3 railOrientation;
						{
							OpenBveApi.Math.Vector3 z = OpenBveApi.Math.Vector3.Normalize(railEnd - railStart);
							OpenBveApi.Math.Vector3 x = OpenBveApi.Math.Vector3.Normalize(new OpenBveApi.Math.Vector3(z.Z, 0.0, -z.X));
							OpenBveApi.Math.Vector3 y = OpenBveApi.Math.Vector3.Cross(z, x);
							railOrientation = new OpenBveApi.Math.Orientation3(x, y, z);
						}
						/*
						 * Prepare creating objects.
						 * */
						OpenBveApi.Geometry.ObjectHandle handle;
						/*
						 * Create the rail object.
						 * */
						if (structures.Rail.Get(rail.Type, 0, encoding, out handle)) {
							Interfaces.Host.CreateObject(handle, railStart, railOrientation);
						}
						/*
						 * Create the walls.
						 * */
						if (rail.WallType >= 0) {
							if (rail.WallSide <= 0) {
								if (structures.WallL.Get(rail.WallType, 0, encoding, out handle)) {
									Interfaces.Host.CreateObject(handle, railStart, railOrientation);
								}
							}
							if (rail.WallSide >= 0) {
								if (structures.WallR.Get(rail.WallType, 0, encoding, out handle)) {
									Interfaces.Host.CreateObject(handle, railStart, railOrientation);
								}
							}
						}
						/*
						 * Create the dikes.
						 * */
						if (rail.DikeType >= 0) {
							if (rail.DikeSide <= 0) {
								if (structures.DikeL.Get(rail.DikeType, 0, encoding, out handle)) {
									Interfaces.Host.CreateObject(handle, railStart, railOrientation);
								}
							}
							if (rail.DikeSide >= 0) {
								if (structures.DikeR.Get(rail.DikeType, 0, encoding, out handle)) {
									Interfaces.Host.CreateObject(handle, railStart, railOrientation);
								}
							}
						}
						/*
						 * Create the pole.
						 * */
						if (rail.Pole.StructureIndex1 >= 0) {
							if (blockIndex % rail.Pole.Spacing == 0) {
								if (rail.Pole.StructureIndex2 == 0) {
									/*
									 * A pole spanning just one rail (by design).
									 * */
									if (rail.Pole.Location <= 0.0) {
										/*
										 * Place the pole as-is.
										 * */
										if (structures.Pole.Get(rail.Pole.StructureIndex1, 0, encoding, out handle)) {
											Interfaces.Host.CreateObject(handle, railStart, railOrientation);
										}
									} else {
										/*
										 * Place the pole mirrored on its x-axis.
										 * */
										if (structures.PoleMirrored.Get(rail.Pole.StructureIndex1, 0, encoding, out handle)) {
											Interfaces.Host.CreateObject(handle, railStart, railOrientation);
										}
									}
								} else {
									/*
									 * A pole spanning at least two rails (by design).
									 * */
									if (structures.Pole.Get(rail.Pole.StructureIndex1, rail.Pole.StructureIndex2, encoding, out handle)) {
										OpenBveApi.Math.Vector3 position = railStart;
										position -= 3.8 * rail.Pole.Location * block.Orientation.X;
										Interfaces.Host.CreateObject(handle, position, railOrientation);
									}
								}
							}
						}
						/*
						 * Create the forms and roofs.
						 * */
						for (int formIndex = 0; formIndex < rail.FormCount; formIndex++) {
							Form form = rail.Forms[formIndex];
							if (form.Layout == FormLayout.Stub) {
								/* 
								 * Place as a stub.
								 *  */
								if (form.FormType >= 0) {
									if (structures.FormL.Get(form.FormType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
									if (structures.FormR.Get(form.FormType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
								}
								if (form.RoofType >= 0) {
									if (structures.RoofL.Get(form.RoofType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
									if (structures.RoofR.Get(form.RoofType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
								}
							} else if (form.Layout == FormLayout.Left) {
								/* 
								 * Place on the left side.
								 *  */
								if (form.FormType >= 0) {
									if (structures.FormL.Get(form.FormType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
									if (structures.FormCL.Get(form.FormType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
								}
								if (form.RoofType >= 0) {
									if (structures.RoofL.Get(form.RoofType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
									if (structures.RoofCL.Get(form.RoofType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
								}
							} else if (form.Layout == FormLayout.Right) {
								/* 
								 * Place on the right side.
								 *  */
								if (form.FormType >= 0) {
									if (structures.FormR.Get(form.FormType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
									if (structures.FormCR.Get(form.FormType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
								}
								if (form.RoofType >= 0) {
									if (structures.RoofR.Get(form.RoofType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
									if (structures.RoofCR.Get(form.RoofType, 0, encoding, out handle)) {
										Interfaces.Host.CreateObject(handle, railStart, railOrientation);
									}
								}
							} else if (form.Layout == FormLayout.SecondaryRail) {
								/*
								 * Place in-between rails.
								 * */
								Rail secondaryRail = block.GetRail(form.SecondaryRail);
								if (secondaryRail.Status != RailStatus.NotAvailable) {
									/*
									 * Determine position of the secondary rail at
									 * the beginning and the end of this block.
									 * */
									OpenBveApi.Math.Vector3 secondaryRailStart = block.Position + block.Orientation.X * secondaryRail.StartX + block.Orientation.Y * secondaryRail.StartY;
									OpenBveApi.Math.Vector3 secondaryRailEnd;
									if (blockIndex < blocks.BlockCount - 1) {
										Block nextBlock = blocks.Blocks[blockIndex + 1];
										secondaryRailEnd = nextBlock.Position + nextBlock.Orientation.X * secondaryRail.EndX + nextBlock.Orientation.Y * secondaryRail.EndY;
									} else {
										secondaryRailEnd = block.Position + block.Orientation.X * secondaryRail.EndX + block.Orientation.Y * secondaryRail.EndY + block.Orientation.Z * blocks.BlockLength;
									}
									OpenBveApi.Math.Orientation3 secondaryRailOrientation;
									{
										OpenBveApi.Math.Vector3 z = OpenBveApi.Math.Vector3.Normalize(secondaryRailEnd - secondaryRailStart);
										OpenBveApi.Math.Vector3 x = OpenBveApi.Math.Vector3.Normalize(new OpenBveApi.Math.Vector3(z.Z, 0.0, -z.X));
										OpenBveApi.Math.Vector3 y = OpenBveApi.Math.Vector3.Cross(z, x);
										secondaryRailOrientation = new OpenBveApi.Math.Orientation3(x, y, z);
									}
									if (
										rail.StartX < secondaryRail.StartX |
										rail.StartX == secondaryRail.StartX & rail.EndX < secondaryRail.EndX
									) {
										/*
										 * Current rail is to the left, secondary rail to the right.
										 * */
										if (form.FormType >= 0) {
											if (structures.FormR.Get(form.FormType, 0, encoding, out handle)) {
												Interfaces.Host.CreateObject(handle, railStart, railOrientation);
											}
											if (structures.FormL.Get(form.FormType, 0, encoding, out handle)) {
												Interfaces.Host.CreateObject(handle, secondaryRailStart, secondaryRailOrientation);
											}
											OpenBveApi.Geometry.GenericObject obj;
											if (structures.FormCR.Get(form.FormType, 0, encoding, out obj)) {
												obj = GetTransformedObject(obj, secondaryRail.StartX - rail.StartX, secondaryRail.EndX - rail.EndX);
												if (Interfaces.Host.RegisterObject(obj, out handle) == OpenBveApi.General.Result.Successful) {
													Interfaces.Host.CreateObject(handle, railStart, railOrientation);
												}
											}
										}
										if (form.RoofType >= 0) {
											if (structures.RoofR.Get(form.RoofType, 0, encoding, out handle)) {
												Interfaces.Host.CreateObject(handle, railStart, railOrientation);
											}
											if (structures.RoofL.Get(form.RoofType, 0, encoding, out handle)) {
												Interfaces.Host.CreateObject(handle, secondaryRailStart, secondaryRailOrientation);
											}
											OpenBveApi.Geometry.GenericObject obj;
											if (structures.RoofCR.Get(form.RoofType, 0, encoding, out obj)) {
												obj = GetTransformedObject(obj, secondaryRail.StartX - rail.StartX, secondaryRail.EndX - rail.EndX);
												if (Interfaces.Host.RegisterObject(obj, out handle) == OpenBveApi.General.Result.Successful) {
													Interfaces.Host.CreateObject(handle, railStart, railOrientation);
												}
											}
										}
									} else if (
										rail.StartX > secondaryRail.StartX |
										rail.StartX == secondaryRail.StartX & rail.EndX > secondaryRail.EndX
									) {
										/*
										 * Current rail is to the right, secondary rail to the left.
										 * */
										if (form.FormType >= 0) {
											if (structures.FormL.Get(form.FormType, 0, encoding, out handle)) {
												Interfaces.Host.CreateObject(handle, railStart, railOrientation);
											}
											if (structures.FormR.Get(form.FormType, 0, encoding, out handle)) {
												Interfaces.Host.CreateObject(handle, secondaryRailStart, secondaryRailOrientation);
											}
											OpenBveApi.Geometry.GenericObject obj;
											if (structures.FormCL.Get(form.FormType, 0, encoding, out obj)) {
												obj = GetTransformedObject(obj, secondaryRail.StartX - rail.StartX, secondaryRail.EndX - rail.EndX);
												if (Interfaces.Host.RegisterObject(obj, out handle) == OpenBveApi.General.Result.Successful) {
													Interfaces.Host.CreateObject(handle, railStart, railOrientation);
												}
											}
										}
										if (form.RoofType >= 0) {
											if (structures.RoofL.Get(form.RoofType, 0, encoding, out handle)) {
												Interfaces.Host.CreateObject(handle, railStart, railOrientation);
											}
											if (structures.RoofR.Get(form.RoofType, 0, encoding, out handle)) {
												Interfaces.Host.CreateObject(handle, secondaryRailStart, secondaryRailOrientation);
											}
											OpenBveApi.Geometry.GenericObject obj;
											if (structures.RoofCL.Get(form.RoofType, 0, encoding, out obj)) {
												obj = GetTransformedObject(obj, secondaryRail.StartX - rail.StartX, secondaryRail.EndX - rail.EndX);
												if (Interfaces.Host.RegisterObject(obj, out handle) == OpenBveApi.General.Result.Successful) {
													Interfaces.Host.CreateObject(handle, railStart, railOrientation);
												}
											}
										}
									}
								}
							}
						}
						/*
						 * Create the cracks.
						 * */
						for (int crackIndex = 0; crackIndex < rail.CrackCount; crackIndex++) {
							Crack crack = rail.Cracks[crackIndex];
							Rail secondaryRail = block.GetRail(crack.SecondaryRail);
							if (secondaryRail.Status != RailStatus.NotAvailable) {
								/*
								 * Determine position of the secondary rail at
								 * the beginning and the end of this block.
								 * */
								OpenBveApi.Math.Vector3 secondaryRailStart = block.Position + block.Orientation.X * secondaryRail.StartX + block.Orientation.Y * secondaryRail.StartY;
								OpenBveApi.Math.Vector3 secondaryRailEnd;
								if (blockIndex < blocks.BlockCount - 1) {
									Block nextBlock = blocks.Blocks[blockIndex + 1];
									secondaryRailEnd = nextBlock.Position + nextBlock.Orientation.X * secondaryRail.EndX + nextBlock.Orientation.Y * secondaryRail.EndY;
								} else {
									secondaryRailEnd = block.Position + block.Orientation.X * secondaryRail.EndX + block.Orientation.Y * secondaryRail.EndY + block.Orientation.Z * blocks.BlockLength;
								}
								OpenBveApi.Math.Orientation3 secondaryRailOrientation;
								{
									OpenBveApi.Math.Vector3 z = OpenBveApi.Math.Vector3.Normalize(secondaryRailEnd - secondaryRailStart);
									OpenBveApi.Math.Vector3 x = OpenBveApi.Math.Vector3.Normalize(new OpenBveApi.Math.Vector3(z.Z, 0.0, -z.X));
									OpenBveApi.Math.Vector3 y = OpenBveApi.Math.Vector3.Cross(z, x);
									secondaryRailOrientation = new OpenBveApi.Math.Orientation3(x, y, z);
								}
								if (
									rail.StartX < secondaryRail.StartX |
									rail.StartX == secondaryRail.StartX & rail.EndX < secondaryRail.EndX
								) {
									/*
									 * Current rail is to the left, secondary rail to the right.
									 * */
									OpenBveApi.Geometry.GenericObject obj;
									if (structures.CrackR.Get(crack.Type, 0, encoding, out obj)) {
										obj = GetTransformedObject(obj, secondaryRail.StartX - rail.StartX, secondaryRail.EndX - rail.EndX);
										if (Interfaces.Host.RegisterObject(obj, out handle) == OpenBveApi.General.Result.Successful) {
											Interfaces.Host.CreateObject(handle, railStart, railOrientation);
										}
									}
								} else if (
									rail.StartX > secondaryRail.StartX |
									rail.StartX == secondaryRail.StartX & rail.EndX > secondaryRail.EndX
								) {
									/*
									 * Current rail is to the right, secondary rail to the left.
									 * */
									OpenBveApi.Geometry.GenericObject obj;
									if (structures.CrackL.Get(crack.Type, 0, encoding, out obj)) {
										obj = GetTransformedObject(obj, secondaryRail.StartX - rail.StartX, secondaryRail.EndX - rail.EndX);
										if (Interfaces.Host.RegisterObject(obj, out handle) == OpenBveApi.General.Result.Successful) {
											Interfaces.Host.CreateObject(handle, railStart, railOrientation);
										}
									}

								}
								
							}
						}
						/*
						 * Create the free objects.
						 * */
						for (int freeObjIndex = 0; freeObjIndex < rail.FreeObjCount; freeObjIndex++) {
							BlockObject freeObj = rail.FreeObjs[freeObjIndex];
							if (structures.FreeObj.Get(freeObj.StructureIndex, 0, encoding, out handle)) {
								OpenBveApi.Math.Vector3 position = railStart + railOrientation.X * freeObj.X + railOrientation.Y * freeObj.Y + railOrientation.Z * freeObj.Z;
								OpenBveApi.Math.Orientation3 orientation = railOrientation;
								orientation.RotateAroundYAxis(Math.Cos(freeObj.Yaw), Math.Sin(freeObj.Yaw));
								orientation.RotateAroundXAxis(Math.Cos(freeObj.Pitch), Math.Sin(freeObj.Pitch));
								orientation.RotateAroundZAxis(Math.Cos(freeObj.Roll), Math.Sin(freeObj.Roll));
								Interfaces.Host.CreateObject(handle, position, orientation);
							}
						}
						/*
						 * Create the beacons.
						 * */
						for (int beaconIndex = 0; beaconIndex < rail.BeaconCount; beaconIndex++) {
							BlockObject beacon = rail.Beacons[beaconIndex];
							if (structures.Beacon.Get(beacon.StructureIndex, 0, encoding, out handle)) {
								OpenBveApi.Math.Vector3 position = railStart + railOrientation.X * beacon.X + railOrientation.Y * beacon.Y + railOrientation.Z * beacon.Z;
								OpenBveApi.Math.Orientation3 orientation = railOrientation;
								orientation.RotateAroundYAxis(Math.Cos(beacon.Yaw), Math.Sin(beacon.Yaw));
								orientation.RotateAroundXAxis(Math.Cos(beacon.Pitch), Math.Sin(beacon.Pitch));
								orientation.RotateAroundZAxis(Math.Cos(beacon.Roll), Math.Sin(beacon.Roll));
								Interfaces.Host.CreateObject(handle, position, orientation);
							}
						}
						/*
						 * Create the stop posts.
						 * */
						for (int stopIndex = 0; stopIndex < rail.StopCount; stopIndex++) {
							BlockObject stop = rail.Stops[stopIndex];
							if (structures.Stop.Get(encoding, out handle)) {
								OpenBveApi.Math.Vector3 position = railStart + railOrientation.X * stop.X + railOrientation.Y * stop.Y + railOrientation.Z * stop.Z;
								OpenBveApi.Math.Orientation3 orientation = railOrientation;
								orientation.RotateAroundYAxis(Math.Cos(stop.Yaw), Math.Sin(stop.Yaw));
								orientation.RotateAroundXAxis(Math.Cos(stop.Pitch), Math.Sin(stop.Pitch));
								orientation.RotateAroundZAxis(Math.Cos(stop.Roll), Math.Sin(stop.Roll));
								Interfaces.Host.CreateObject(handle, position, orientation);
							}
						}
						/*
						 * Create the limit posts.
						 * */
						for (int limitIndex = 0; limitIndex < rail.LimitCount; limitIndex++) {
							BlockObject limit = rail.Limits[limitIndex];
							if (structures.Limits.Get(limit.StructureIndex, 0, encoding, out handle)) {
								OpenBveApi.Math.Vector3 position = railStart + railOrientation.X * limit.X + railOrientation.Y * limit.Y + railOrientation.Z * limit.Z;
								OpenBveApi.Math.Orientation3 orientation = railOrientation;
								orientation.RotateAroundYAxis(Math.Cos(limit.Yaw), Math.Sin(limit.Yaw));
								orientation.RotateAroundXAxis(Math.Cos(limit.Pitch), Math.Sin(limit.Pitch));
								orientation.RotateAroundZAxis(Math.Cos(limit.Roll), Math.Sin(limit.Roll));
								Interfaces.Host.CreateObject(handle, position, orientation);
							}
						}
					}
				}
			}
		}

		
	}
}