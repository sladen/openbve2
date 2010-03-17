using System;
using System.Text;

/*
 * TODO: Implement glow for BlendMode/SetBlendMode once the API has support for this.
 * TODO: Add missing XML annotation.
 * TODO: Update the style of code comments.
 * */

namespace Plugin {
	internal static partial class Parser {
		
		// load from file
		/// <summary>Loads a B3D/CSV object from a file and returns the success of the operation.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="fallback">The fallback encoding.</param>
		/// <param name="mesh">Receives the mesh of the object.</param>
		/// <returns>The success of the operation.</returns>
		internal static OpenBveApi.General.Result LoadFromFile(string fileName, System.Text.Encoding fallback, out OpenBveApi.Geometry.GenericObject obj) {
			/*
			 * Make some preparations.
			 * */
			string folder = System.IO.Path.GetDirectoryName(fileName);
			bool isB3d = fileName.EndsWith(".b3d", StringComparison.OrdinalIgnoreCase);
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			/*
			 * Get all lines from the file and remove
			 * comments as started by semicolons.
			 * */
			string[] lines = OpenBveApi.Text.GetLinesFromFile(fileName, fallback);
			for (int i = 0; i < lines.Length; i++) {
				int semicolon = lines[i].IndexOf(';');
				if (semicolon >= 0) {
					lines[i] = lines[i].Substring(0, semicolon).Trim();
				} else {
					lines[i] = lines[i].Trim();
				}
			}
			/*
			 * Parse the lines and add to a
			 * face-vertex mesh in the process.
			 * */
			MeshBuilder builder = new MeshBuilder();
			OpenBveApi.Geometry.FaceVertexMesh mesh = new OpenBveApi.Geometry.FaceVertexMesh();
			int totalNumberOfVertices = 0;
			for (int row = 0; row < lines.Length; row++) {
				if (lines[row].Length != 0) {
					/*
					 * Split the line into a command
					 * and an argument sequence.
					 * */
					string command;
					string argumentSequence;
					if (isB3d) {
						int space = lines[row].IndexOf(' ');
						if (space >= 0) {
							command = lines[row].Substring(0, space).TrimEnd();
							argumentSequence = lines[row].Substring(space + 1).TrimStart();
						} else {
							command = lines[row];
							argumentSequence = string.Empty;
						}
					} else {
						int comma = lines[row].IndexOf(',');
						if (comma >= 0) {
							command = lines[row].Substring(0, comma).TrimEnd();
							argumentSequence = lines[row].Substring(comma + 1).TrimStart();
						} else {
							command = lines[row];
							argumentSequence = string.Empty;
						}
					}
					/*
					 * Split the argument sequence into
					 * individual arguments and remove
					 * zero-length arguments from the
					 * end of the list.
					 * */
					string[] arguments = argumentSequence.Split(',');
					for (int j = 0; j < arguments.Length; j++) {
						arguments[j] = arguments[j].Trim();
					}
					int argumentCount = 0;
					for (int i = arguments.Length - 1; i >= 0; i--) {
						if (arguments[i].Length != 0) {
							argumentCount = i + 1;
							break;
						}
					}
					if (isB3d || command != string.Empty || argumentCount != 0) {
						if (argumentCount != arguments.Length) {
							Array.Resize<string>(ref arguments, argumentCount);
						}
						/*
						 * Now process the command and its arguments.
						 * */
						string commandLower = command.ToLowerInvariant();
						switch (commandLower) {
							case "[meshbuilder]":
							case "createmeshbuilder":
								{
									if (isB3d && commandLower != "[meshbuilder]") {
										IO.ReportNotAllowedCommand(fileName, row, command, "[MeshBuilder]");
									} else if (!isB3d && commandLower != "createmeshbuilder") {
										IO.ReportNotAllowedCommand(fileName, row, command, "CreateMeshBuilder");
									}
									IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 0, true);
									AddMeshBuilder(folder, mesh, builder, fallback, ref totalNumberOfVertices);
									builder = new MeshBuilder();
								}
								break;
							case "vertex":
							case "addvertex":
								{
									if (isB3d && commandLower != "vertex") {
										IO.ReportNotAllowedCommand(fileName, row, command, "Vertex");
									} else if (!isB3d && commandLower != "addvertex") {
										IO.ReportNotAllowedCommand(fileName, row, command, "AddVertex");
									}
									IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 8, true);
									OpenBveApi.Math.Vector3 spatial;
									IO.ParseDouble(fileName, row, command, arguments, 0, "spatialX", 0.0, out spatial.X);
									IO.ParseDouble(fileName, row, command, arguments, 1, "spatialY", 0.0, out spatial.Y);
									IO.ParseDouble(fileName, row, command, arguments, 2, "spatialZ", 0.0, out spatial.Z);
									OpenBveApi.Math.Vector3 normal;
									IO.ParseDouble(fileName, row, command, arguments, 3, "normalX", 0.0, out normal.X);
									IO.ParseDouble(fileName, row, command, arguments, 4, "normalY", 0.0, out normal.Y);
									IO.ParseDouble(fileName, row, command, arguments, 5, "normalZ", 0.0, out normal.Z);
									OpenBveApi.Math.Vector2 texture;
									IO.ParseDouble(fileName, row, command, arguments, 6, "textureX", 0.0, out texture.X);
									IO.ParseDouble(fileName, row, command, arguments, 7, "textureY", 0.0, out texture.Y);
									if (!normal.IsNullVector()) {
										normal.Normalize();
									}
									if (builder.Vertices.Length == builder.VertexCount) {
										Array.Resize<Vertex>(ref builder.Vertices, builder.Vertices.Length << 1);
									}
									builder.Vertices[builder.VertexCount].SpatialCoordinates = spatial;
									builder.Vertices[builder.VertexCount].TextureCoordinates = texture;
									builder.Vertices[builder.VertexCount].Normal = normal;
									builder.VertexCount++;
								}
								break;
							case "face":
							case "face2":
							case "addface":
							case "addface2":
								{
									if (IO.CheckArgumentCount(fileName, row, command, arguments.Length, 3, int.MaxValue, true)) {
										if (isB3d) {
											if (commandLower == "addface") {
												IO.ReportNotAllowedCommand(fileName, row, command, "Face");
											} else if (commandLower == "addface2") {
												IO.ReportNotAllowedCommand(fileName, row, command, "Face2");
											}
										} else {
											if (commandLower == "face") {
												IO.ReportNotAllowedCommand(fileName, row, command, "AddFace");
											} else if (commandLower == "face2") {
												IO.ReportNotAllowedCommand(fileName, row, command, "AddFace2");
											}
										}
										int[] vertices = new int[arguments.Length];
										bool valid = true;
										for (int i = 0; i < arguments.Length; i++) {
											if (IO.ParseInt(fileName, row, command, arguments, i, "vertex" + i.ToString(culture), -1, out vertices[i])) {
												if (vertices[i] < 0 | vertices[i] >= builder.VertexCount) {
													IO.ReportInvalidArgument(fileName, row, command, i, "vertex" + i.ToString(culture), "The argument references a non-existing vector.");
													valid = false;
													break;
												}
											} else {
												valid = false;
												break;
											}
										}
										if (valid) {
											int numberOfFaces = commandLower == "face2" || commandLower == "addface2" ? 2 : 1;
											for (int i = 0; i < numberOfFaces; i++) {
												if (builder.Faces.Length == builder.FaceCount) {
													Array.Resize<Face>(ref builder.Faces, builder.Faces.Length << 1);
												}
												builder.Faces[builder.FaceCount] = new Face();
												builder.Faces[builder.FaceCount].Vertices = new FaceVertex[vertices.Length];
												OpenBveApi.Math.Vector3 vertexA = builder.Vertices[vertices[0]].SpatialCoordinates;
												OpenBveApi.Math.Vector3 vertexB = builder.Vertices[vertices[1]].SpatialCoordinates;
												OpenBveApi.Math.Vector3 vertexC = builder.Vertices[vertices[2]].SpatialCoordinates;
												OpenBveApi.Math.Vector3 normal;
												OpenBveApi.Math.Vector3.CreateNormal(vertexA, vertexB, vertexC, out normal);
												for (int j = 0; j < vertices.Length; j++) {
													builder.Faces[builder.FaceCount].Vertices[j].Vertex = vertices[j];
													if (builder.Vertices[vertices[j]].Normal.IsNullVector()) {
														builder.Faces[builder.FaceCount].Vertices[j].Normal = normal;
													} else {
														builder.Faces[builder.FaceCount].Vertices[j].Normal = builder.Vertices[vertices[j]].Normal;
													}
												}
												builder.Faces[builder.FaceCount].ReflectiveColor = new OpenBveApi.Color.ColorRGBA(1.0f, 1.0f, 1.0f, 1.0f);
												builder.Faces[builder.FaceCount].EmissiveColor = new OpenBveApi.Color.ColorRGB(0.0f, 0.0f, 0.0f);
												builder.Faces[builder.FaceCount].Flipped = i == 1;
												builder.FaceCount++;
											}
										}
									}
								}
								break;
							case "cube":
								{
									if (IO.CheckArgumentCount(fileName, row, command, arguments.Length, 1, 3, true)) {
										double halfWidth, halfHeight, halfDepth;
										IO.ParseDouble(fileName, row, command, arguments, 0, "halfWidth", 0.0, out halfWidth);
										IO.ParseDouble(fileName, row, command, arguments, 1, "halfHeight", halfWidth, out halfHeight);
										IO.ParseDouble(fileName, row, command, arguments, 2, "halfDepth", halfWidth, out halfDepth);
										AddCube(builder, halfWidth, halfHeight, halfDepth);
									}
								}
								break;
							case "cylinder":
								{
									if (IO.CheckArgumentCount(fileName, row, command, arguments.Length, 4, 4, true)) {
										int faces;
										double upperRadius, lowerRadius, height;
										if (
											IO.ParseInt(fileName, row, command, arguments, 0, "faces", 3, int.MaxValue, -1, out faces) &&
											IO.ParseDouble(fileName, row, command, arguments, 1, "upperRadius", 0.0, out upperRadius) &&
											IO.ParseDouble(fileName, row, command, arguments, 2, "lowerRadius", 0.0, out lowerRadius) &&
											IO.ParseDouble(fileName, row, command, arguments, 3, "height", 0.0, out height)
										) {
											AddCylinder(builder, faces, upperRadius, lowerRadius, height);
										}
									}
								}
								break;
							case "[texture]":
							case "generatenormals":
								{
									if (isB3d && commandLower != "[texture]") {
										IO.ReportNotAllowedCommand(fileName, row, command, "[Texture]");
									} else if (!isB3d && commandLower != "generatenormals") {
										IO.ReportNotAllowedCommand(fileName, row, command, "GenerateNormals");
									}
									IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 0, true);
								}
								break;
							case "translate":
							case "translateall":
								{
									IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 3, true);
									OpenBveApi.Math.Vector3 offset;
									IO.ParseDouble(fileName, row, command, arguments, 0, "offsetX", 0.0, out offset.X);
									IO.ParseDouble(fileName, row, command, arguments, 1, "offsetY", 0.0, out offset.Y);
									IO.ParseDouble(fileName, row, command, arguments, 2, "offsetZ", 0.0, out offset.Z);
									builder.Translate(offset);
									if (commandLower == "translateall") {
										mesh.Translate(offset);
									}
								}
								break;
							case "scale":
							case "scaleall":
								{
									IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 3, true);
									OpenBveApi.Math.Vector3 factor;
									IO.ParseDouble(fileName, row, command, arguments, 0, "factorX", 1.0, out factor.X);
									IO.ParseDouble(fileName, row, command, arguments, 1, "factorY", 1.0, out factor.Y);
									IO.ParseDouble(fileName, row, command, arguments, 2, "factorZ", 1.0, out factor.Z);
									if (factor.X == 0.0) {
										IO.ReportInvalidArgument(fileName, row, command, 0, "factorX", "Value must not be zero.");
										factor.X = 1.0;
									}
									if (factor.Y == 0.0) {
										IO.ReportInvalidArgument(fileName, row, command, 1, "factorY", "Value must not be zero.");
										factor.Y = 1.0;
									}
									if (factor.Z == 0.0) {
										IO.ReportInvalidArgument(fileName, row, command, 2, "factorZ", "Value must not be zero.");
										factor.Z = 1.0;
									}
									builder.Scale(factor);
									if (commandLower == "scaleall") {
										mesh.Scale(factor);
									}
								}
								break;
							case "rotate":
							case "rotateall":
								{
									IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 4, true);
									OpenBveApi.Math.Vector3 direction;
									IO.ParseDouble(fileName, row, command, arguments, 0, "directionX", 0.0, out direction.X);
									IO.ParseDouble(fileName, row, command, arguments, 1, "directionY", 0.0, out direction.Y);
									IO.ParseDouble(fileName, row, command, arguments, 2, "directionZ", 0.0, out direction.Z);
									double angle;
									IO.ParseDouble(fileName, row, command, arguments, 3, "angle", 0.0, out angle);
									if (direction.IsNullVector()) {
										direction = OpenBveApi.Math.Vector3.Right;
									} else {
										direction.Normalize();
									}
									angle *= 0.0174532925199433;
									double cosineOfAngle = Math.Cos(angle);
									double sineOfAngle = Math.Sin(angle);
									builder.Rotate(direction, cosineOfAngle, sineOfAngle);
									if (commandLower == "rotateall") {
										mesh.Rotate(direction, cosineOfAngle, sineOfAngle);
									}
								}
								break;
							case "shear":
							case "shearall":
								{
									IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 7, true);
									OpenBveApi.Math.Vector3 direction;
									IO.ParseDouble(fileName, row, command, arguments, 0, "directionX", 0.0, out direction.X);
									IO.ParseDouble(fileName, row, command, arguments, 1, "directionY", 0.0, out direction.Y);
									IO.ParseDouble(fileName, row, command, arguments, 2, "directionZ", 0.0, out direction.Z);
									OpenBveApi.Math.Vector3 shift;
									IO.ParseDouble(fileName, row, command, arguments, 3, "shiftX", 0.0, out shift.X);
									IO.ParseDouble(fileName, row, command, arguments, 4, "shiftY", 0.0, out shift.Y);
									IO.ParseDouble(fileName, row, command, arguments, 5, "shiftZ", 0.0, out shift.Z);
									double ratio;
									IO.ParseDouble(fileName, row, command, arguments, 6, "ratio", 0.0, out ratio);
									Shear(builder, direction, shift, ratio);
									if (commandLower == "shearall") {
										Shear(mesh, direction, shift, ratio);
									}
								}
								break;
							case "color":
							case "setcolor":
								{
									if (isB3d && commandLower != "color") {
										IO.ReportNotAllowedCommand(fileName, row, command, "Color");
									} else if (!isB3d && commandLower != "setcolor") {
										IO.ReportNotAllowedCommand(fileName, row, command, "SetColor");
									}
									IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 4, true);
									double red, green, blue, alpha;
									IO.ParseDouble(fileName, row, command, arguments, 0, "red", 0.0, 255.0, 255.0, out red);
									IO.ParseDouble(fileName, row, command, arguments, 1, "green", 0.0, 255.0, 255.0, out green);
									IO.ParseDouble(fileName, row, command, arguments, 2, "blue", 0.0, 255.0, 255.0, out blue);
									IO.ParseDouble(fileName, row, command, arguments, 3, "alpha", 0.0, 255.0, 255.0, out alpha);
									red *= 0.00392156862745098;
									green *= 0.00392156862745098;
									blue *= 0.00392156862745098;
									alpha *= 0.00392156862745098;
									OpenBveApi.Color.ColorRGBA color = new OpenBveApi.Color.ColorRGBA((float)red, (float)green, (float)blue, (float)alpha);
									for (int j = 0; j < builder.FaceCount; j++) {
										builder.Faces[j].ReflectiveColor = color;
									}
								}
								break;
							case "emissivecolor":
							case "setemissivecolor":
								{
									if (isB3d && commandLower != "emissivecolor") {
										IO.ReportNotAllowedCommand(fileName, row, command, "EmissiveColor");
									} else if (!isB3d && commandLower != "setemissivecolor") {
										IO.ReportNotAllowedCommand(fileName, row, command, "SetEmissiveColor");
									}
									IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 3, true);
									double red, green, blue;
									IO.ParseDouble(fileName, row, command, arguments, 0, "red", 0.0, 255.0, 0.0, out red);
									IO.ParseDouble(fileName, row, command, arguments, 1, "green", 0.0, 255.0, 0.0, out green);
									IO.ParseDouble(fileName, row, command, arguments, 2, "blue", 0.0, 255.0, 0.0, out blue);
									red *= 0.00392156862745098;
									green *= 0.00392156862745098;
									blue *= 0.00392156862745098;
									OpenBveApi.Color.ColorRGB color = new OpenBveApi.Color.ColorRGB((float)red, (float)green, (float)blue);
									for (int j = 0; j < builder.FaceCount; j++) {
										builder.Faces[j].EmissiveColor = color;
									}
								}
								break;
							case "blendmode":
							case "setblendmode":
								{
									if (isB3d && commandLower != "blendmode") {
										IO.ReportNotAllowedCommand(fileName, row, command, "BlendMode");
									} else if (!isB3d && commandLower != "setblendmode") {
										IO.ReportNotAllowedCommand(fileName, row, command, "SetBlendMode");
									}
									if (IO.CheckArgumentCount(fileName, row, command, arguments.Length, 1, 3, true)) {
										OpenBveApi.Geometry.BlendMode blendMode;
										switch (arguments[0].ToLowerInvariant()) {
											case "normal":
												blendMode = OpenBveApi.Geometry.BlendMode.Normal;
												break;
											case "additive":
												blendMode = OpenBveApi.Geometry.BlendMode.Additive;
												break;
											default:
												IO.ReportInvalidArgument(fileName, row, command, 0, "blendMode", "The specified blend mode is not supported.");
												blendMode = OpenBveApi.Geometry.BlendMode.Normal;
												break;
										}
										for (int j = 0; j < builder.FaceCount; j++) {
											builder.Faces[j].BlendMode = blendMode;
										}
									}
								}
								break;
							case "load":
							case "loadtexture":
								{
									if (isB3d && commandLower != "load") {
										IO.ReportNotAllowedCommand(fileName, row, command, "Load");
									} else if (!isB3d && commandLower != "loadtexture") {
										IO.ReportNotAllowedCommand(fileName, row, command, "LoadTexture");
									}
									if (IO.CheckArgumentCount(fileName, row, command, arguments.Length, 1, 2, true)) {
										builder.DaytimeTexture = OpenBveApi.Path.CombineFile(folder, arguments[0]);
										builder.NighttimeTexture = arguments.Length >= 2 ? OpenBveApi.Path.CombineFile(folder, arguments[1]) : null;
										if (!System.IO.File.Exists(builder.DaytimeTexture)) {
											IO.ReportMissingFile(fileName, row, command, builder.DaytimeTexture);
										}
										if (builder.NighttimeTexture != null && !System.IO.File.Exists(builder.NighttimeTexture)) {
											IO.ReportMissingFile(fileName, row, command, builder.NighttimeTexture);
										}
									}
								}
								break;
							case "transparent":
							case "setdecaltransparentcolor":
								{
									if (isB3d && commandLower != "transparent") {
										IO.ReportNotAllowedCommand(fileName, row, command, "Transparent");
									} else if (!isB3d && commandLower != "setdecaltransparentcolor") {
										IO.ReportNotAllowedCommand(fileName, row, command, "SetDecalTransparentColor");
									}
									IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 3, true);
									int red, green, blue;
									IO.ParseInt(fileName, row, command, arguments, 0, "red", 0, 255, 0, out red);
									IO.ParseInt(fileName, row, command, arguments, 1, "green", 0, 255, 0, out green);
									IO.ParseInt(fileName, row, command, arguments, 2, "blue", 0, 255, 0, out blue);
									OpenBveApi.Color.TransparentColor color = new OpenBveApi.Color.TransparentColor((byte)red, (byte)green, (byte)blue, true);
									for (int j = 0; j < builder.FaceCount; j++) {
										builder.Faces[j].TransparentColor = color;
									}
								}
								break;
								/*
						case "normal":
						case "setnormal":
							{
								if (isB3d && commandLower != "normal") {
									IO.ReportNotAllowedCommand(fileName, row, command, "Normal");
								} else if (!isB3d && commandLower != "setnormal") {
									IO.ReportNotAllowedCommand(fileName, row, command, "SetNormal");
								}
								if (IO.CheckArgumentCount(fileName, row, command, arguments.Length, 1, 4, true)) {
									int vertex;
									if (IO.ParseInt(fileName, row, command, arguments, 0, "vertex", 0, builder.VertexCount - 1, -1, out vertex)) {
										OpenBveApi.Math.Vector3 normal;
										IO.ParseDouble(fileName, row, command, arguments, 0, "normalX", 0.0, out normal.X);
										IO.ParseDouble(fileName, row, command, arguments, 1, "normalY", 0.0, out normal.Y);
										IO.ParseDouble(fileName, row, command, arguments, 2, "normalZ", 0.0, out normal.Z);
										builder.Vertices[vertex].Normal = normal;
									}
								}
							}
							break;
								 */
							case "coordinates":
							case "settexturecoordinates":
								{
									if (isB3d && commandLower != "coordinates") {
										IO.ReportNotAllowedCommand(fileName, row, command, "Coordinates");
									} else if (!isB3d && commandLower != "settexturecoordinates") {
										IO.ReportNotAllowedCommand(fileName, row, command, "SetTextureCoordinate");
									}
									if (IO.CheckArgumentCount(fileName, row, command, arguments.Length, 1, 3, true)) {
										int vertex;
										if (IO.ParseInt(fileName, row, command, arguments, 0, "vertex", 0, builder.VertexCount - 1, -1, out vertex)) {
											OpenBveApi.Math.Vector2 texture;
											IO.ParseDouble(fileName, row, command, arguments, 1, "textureX", 0.0, out texture.X);
											IO.ParseDouble(fileName, row, command, arguments, 2, "textureY", 0.0, out texture.Y);
											builder.Vertices[vertex].TextureCoordinates = texture;
										}
									}
								}
								break;
							default:
								{
									IO.ReportNotSupportedCommand(fileName, row, command);
								}
								break;
						}
					}
				}
			}
			/*
			 * Add the last mesh builder to the
			 * face-vertex mesh and return the data.
			 * */
			AddMeshBuilder(folder, mesh, builder, fallback, ref totalNumberOfVertices);
			obj = (OpenBveApi.Geometry.GenericObject)mesh;
			return OpenBveApi.General.Result.Successful;
		}
		
		// add cube
		/// <summary>Adds a cube to a mesh builder.</summary>
		/// <param name="builder">The mesh builder to add the cube to.</param>
		/// <param name="halfWidth">Half the width of the cube.</param>
		/// <param name="halfHeight">Half the height of the cube.</param>
		/// <param name="halfDepth">Half the depth of the cube.</param>
		private static void AddCube(MeshBuilder builder, double halfWidth, double halfHeight, double halfDepth) {
			/*
			 * Create the vertices.
			 * */
			while (builder.VertexCount + 8 >= builder.Vertices.Length) {
				Array.Resize<Vertex>(ref builder.Vertices, builder.Vertices.Length << 1);
			}
			builder.Vertices[builder.VertexCount + 0].SpatialCoordinates = new OpenBveApi.Math.Vector3(halfWidth, halfHeight, -halfDepth);
			builder.Vertices[builder.VertexCount + 1].SpatialCoordinates = new OpenBveApi.Math.Vector3(halfWidth, -halfHeight, -halfDepth);
			builder.Vertices[builder.VertexCount + 2].SpatialCoordinates = new OpenBveApi.Math.Vector3(-halfWidth, -halfHeight, -halfDepth);
			builder.Vertices[builder.VertexCount + 3].SpatialCoordinates = new OpenBveApi.Math.Vector3(-halfWidth, halfHeight, -halfDepth);
			builder.Vertices[builder.VertexCount + 4].SpatialCoordinates = new OpenBveApi.Math.Vector3(halfWidth, halfHeight, halfDepth);
			builder.Vertices[builder.VertexCount + 5].SpatialCoordinates = new OpenBveApi.Math.Vector3(halfWidth, -halfHeight, halfDepth);
			builder.Vertices[builder.VertexCount + 6].SpatialCoordinates = new OpenBveApi.Math.Vector3(-halfWidth, -halfHeight, halfDepth);
			builder.Vertices[builder.VertexCount + 7].SpatialCoordinates = new OpenBveApi.Math.Vector3(-halfWidth, halfHeight, halfDepth);
			/*
			 * Create the faces.
			 * */
			while (builder.FaceCount + 6 >= builder.Faces.Length) {
				Array.Resize<Face>(ref builder.Faces, builder.Faces.Length << 1);
			}
			builder.Faces[builder.FaceCount + 0] = new Face(builder.VertexCount, new int[] { 0, 1, 2, 3 }, OpenBveApi.Math.Vector3.Backward);
			builder.Faces[builder.FaceCount + 1] = new Face(builder.VertexCount, new int[] { 0, 4, 5, 1 }, OpenBveApi.Math.Vector3.Right);
			builder.Faces[builder.FaceCount + 2] = new Face(builder.VertexCount, new int[] { 0, 3, 7, 4 }, OpenBveApi.Math.Vector3.Up);
			builder.Faces[builder.FaceCount + 3] = new Face(builder.VertexCount, new int[] { 6, 5, 4, 7 }, OpenBveApi.Math.Vector3.Forward);
			builder.Faces[builder.FaceCount + 4] = new Face(builder.VertexCount, new int[] { 6, 7, 3, 2 }, OpenBveApi.Math.Vector3.Left);
			builder.Faces[builder.FaceCount + 5] = new Face(builder.VertexCount, new int[] { 6, 2, 1, 5 }, OpenBveApi.Math.Vector3.Down);
			/*
			 * Increment the vertex and face counts.
			 * */
			builder.VertexCount += 8;
			builder.FaceCount += 6;
		}
		
		// add cylinder
		/// <summary>Adds a cylinder to a mesh builder.</summary>
		/// <param name="builder">The mesh builder to add the cylinder to.</param>
		/// <param name="segments">The number of segments of the cylinder.</param>
		/// <param name="upperRadius">The upper radius of the cylinder.</param>
		/// <param name="lowerRadius">The lower radius of the cylinder</param>
		/// <param name="height">The height of the cylinder</param>
		private static void AddCylinder(MeshBuilder builder, int segments, double upperRadius, double lowerRadius, double height) {
			/*
			 * Make some preparations.
			 * */
			bool upperCap = upperRadius > 0.0;
			bool lowerCap = lowerRadius > 0.0;
			upperRadius = Math.Abs(upperRadius);
			lowerRadius = Math.Abs(lowerRadius);
			double angleCurrent = 0.0;
			double angleDelta = 2.0 * Math.PI / (double)segments;
			double slope = height != 0.0 ? Math.Atan((lowerRadius - upperRadius) / height) : 0.0;
			double cosSlope = Math.Cos(slope);
			double sinSlope = Math.Sin(slope);
			double halfHeight = 0.5 * height;
			double signHeight = (double)Math.Sign(height);
			OpenBveApi.Math.Vector3 up, down;
			if (height >= 0.0) {
				up = OpenBveApi.Math.Vector3.Up;
				down = OpenBveApi.Math.Vector3.Down;
			} else {
				up = OpenBveApi.Math.Vector3.Down;
				down = OpenBveApi.Math.Vector3.Up;
			}
			/*
			 * Create the vertices and normals.
			 * */
			while (builder.VertexCount + 2 * segments >= builder.Vertices.Length) {
				Array.Resize<Vertex>(ref builder.Vertices, builder.Vertices.Length << 1);
			}
			OpenBveApi.Math.Vector3[] normals = new OpenBveApi.Math.Vector3[2 * segments];
			for (int i = 0; i < segments; i++) {
				double dirX = Math.Cos(angleCurrent);
				double dirZ = Math.Sin(angleCurrent);
				double lowerX = dirX * lowerRadius;
				double lowerZ = dirZ * lowerRadius;
				double upperX = dirX * upperRadius;
				double upperZ = dirZ * upperRadius;
				OpenBveApi.Math.Vector3 normal = new OpenBveApi.Math.Vector3(dirX * signHeight, 0.0, dirZ * signHeight);
				OpenBveApi.Math.Vector3 right = OpenBveApi.Math.Vector3.Cross(normal, OpenBveApi.Math.Vector3.Up);
				normal.Rotate(right, cosSlope, sinSlope);
				builder.Vertices[builder.VertexCount + 2 * i + 0].SpatialCoordinates = new OpenBveApi.Math.Vector3(upperX, halfHeight, upperZ);
				builder.Vertices[builder.VertexCount + 2 * i + 1].SpatialCoordinates = new OpenBveApi.Math.Vector3(lowerX, -halfHeight, lowerZ);
				normals[2 * i + 0] = normal;
				normals[2 * i + 1] = normal;
				angleCurrent += angleDelta;
			}
			/*
			 * Create the faces for the wall
			 * and increment the face count.
			 * */
			int faces = segments + (upperCap ? 1 : 0) + (lowerCap ? 1 : 0);
			while (builder.FaceCount + faces >= builder.Faces.Length) {
				Array.Resize<Face>(ref builder.Faces, builder.Faces.Length << 1);
			}
			for (int i = 0; i < segments; i++) {
				int index0 = (2 * i + 2) % (2 * segments);
				int index1 = (2 * i + 3) % (2 * segments);
				int index2 = 2 * i + 1;
				int index3 = 2 * i;
				builder.Faces[builder.FaceCount + i] = new Face(builder.VertexCount, new int[] { index0, index1, index2, index3 }, new OpenBveApi.Math.Vector3[] { normals[index0], normals[index1], normals[index2], normals[index3] });
			}
			builder.FaceCount += segments;
			/*
			 * Create the upper cap and
			 * increment the face count.
			 * */
			if (upperCap) {
				int[] indices = new int[segments];
				for (int i = 0; i < segments; i++) {
					indices[i] = 2 * i + 1;
				}
				builder.Faces[builder.FaceCount] = new Face(builder.VertexCount, indices, up);
				builder.FaceCount++;
			}
			/*
			 * Create the lower cap and
			 * increment the face count.
			 * */
			if (lowerCap) {
				int[] indices = new int[segments];
				for (int i = 0; i < segments; i++) {
					indices[i] = 2 * (segments - i - 1);
				}
				builder.Faces[builder.FaceCount] = new Face(builder.VertexCount, indices, down);
				builder.FaceCount++;
			}
			/*
			 * Increment the vertex count.
			 * */
			builder.VertexCount += 2 * segments;
		}
		
		// add mesh builder
		/// <summary>Adds a mesh builder to a mesh.</summary>
		/// <param name="folder">The absolute path to the folder where the object is stored.</param>
		/// <param name="mesh">The mesh to add the mesh builder to.</param>
		/// <param name="builder">The mesh builder to add.</param>
		/// <param name="encoding">The suggested encoding.</param>
		/// <param name="totalMeshBuilderVertices">The total number of vertices that were defined in previous mesh builders. This value will be modified to add the number of vertices in the specified mesh builder.</param>
		private static void AddMeshBuilder(string folder, OpenBveApi.Geometry.FaceVertexMesh mesh, MeshBuilder builder, System.Text.Encoding encoding, ref int totalMeshBuilderVertices) {
			// convert mesh builder to compatible mesh
			if (builder.FaceCount == 0) {
				return;
			}
			OpenBveApi.Geometry.FaceVertexMesh compatibleMesh = new OpenBveApi.Geometry.FaceVertexMesh();
			// vertices
			int vertices = 0;
			for (int i = 0; i < builder.FaceCount; i++) {
				vertices += builder.Faces[i].Vertices.Length;
			}
			compatibleMesh.Vertices = new OpenBveApi.Geometry.Vertex[vertices];
			vertices = 0;
			for (int i = 0; i < builder.FaceCount; i++) {
				// create normal
				int vertex0 = builder.Faces[i].Vertices[0].Vertex;
				int vertex1 = builder.Faces[i].Vertices[1].Vertex;
				int vertex2 = builder.Faces[i].Vertices[2].Vertex;
				OpenBveApi.Math.Vector3 normal;
				OpenBveApi.Math.Vector3.CreateNormal(
					builder.Vertices[vertex0].SpatialCoordinates,
					builder.Vertices[vertex1].SpatialCoordinates,
					builder.Vertices[vertex2].SpatialCoordinates,
					out normal
				);
				// apply vertices
				for (int j = 0; j < builder.Faces[i].Vertices.Length; j++) {
					int k = builder.Faces[i].Vertices[j].Vertex;
					compatibleMesh.Vertices[vertices].SpatialCoordinates = builder.Vertices[k].SpatialCoordinates;
					compatibleMesh.Vertices[vertices].TextureCoordinates = builder.Vertices[k].TextureCoordinates;
					if (builder.Faces[i].Vertices[j].Normal.IsNullVector()) {
						throw new InvalidOperationException();
					} else {
						compatibleMesh.Vertices[vertices].Normal = builder.Faces[i].Vertices[j].Normal;
					}
					compatibleMesh.Vertices[vertices].ReflectiveColor = builder.Faces[i].ReflectiveColor;
					compatibleMesh.Vertices[vertices].Tag = totalMeshBuilderVertices + k;
					vertices++;
				}
			}
			totalMeshBuilderVertices += builder.VertexCount;
			// materials
			compatibleMesh.Materials = new OpenBveApi.Geometry.Material[builder.FaceCount];
			for (int i = 0; i < builder.FaceCount; i++) {
				// daytime texture
				if (builder.DaytimeTexture != null) {
					OpenBveApi.Texture.TextureWrapMode horizontalWrapMode = OpenBveApi.Texture.TextureWrapMode.ClampToEdge;
					OpenBveApi.Texture.TextureWrapMode verticalWrapMode = OpenBveApi.Texture.TextureWrapMode.ClampToEdge;
					for (int j = 0; j < builder.Faces[i].Vertices.Length; j++) {
						int k = builder.Faces[i].Vertices[j].Vertex;
						if (builder.Vertices[k].TextureCoordinates.X < 0.0 | builder.Vertices[k].TextureCoordinates.X > 1.0) {
							horizontalWrapMode = OpenBveApi.Texture.TextureWrapMode.Repeat;
						}
						if (builder.Vertices[k].TextureCoordinates.Y < 0.0 | builder.Vertices[k].TextureCoordinates.Y > 1.0) {
							verticalWrapMode = OpenBveApi.Texture.TextureWrapMode.Repeat;
						}
					}
					OpenBveApi.Path.PathReference path = new OpenBveApi.Path.FileReference(builder.DaytimeTexture);
					OpenBveApi.General.Origin origin = new OpenBveApi.General.Origin(path, null, encoding);
					OpenBveApi.Texture.TextureParameters parameters = new OpenBveApi.Texture.TextureParameters(builder.Faces[i].TransparentColor, horizontalWrapMode, verticalWrapMode, null);
					OpenBveApi.Texture.TextureHandle handle;
					if (Interfaces.Host.RegisterTexture(origin, parameters, out handle) == OpenBveApi.General.Result.Successful) {
						compatibleMesh.Materials[i].DaytimeTexture = handle;
					} else {
						compatibleMesh.Materials[i].DaytimeTexture = null;
					}
				} else {
					compatibleMesh.Materials[i].DaytimeTexture = null;
				}
				// misc
				compatibleMesh.Materials[i].NighttimeTexture = null;
				compatibleMesh.Materials[i].EmissiveColor = builder.Faces[i].EmissiveColor;
				compatibleMesh.Materials[i].BlendMode = builder.Faces[i].BlendMode;
			}
			// faces
			compatibleMesh.Faces = new OpenBveApi.Geometry.Face[builder.FaceCount];
			vertices = 0;
			for (int i = 0; i < builder.FaceCount; i++) {
				compatibleMesh.Faces[i].Type = OpenBveApi.Geometry.FaceType.Polygon;
				compatibleMesh.Faces[i].Vertices = new int[builder.Faces[i].Vertices.Length];
				for (int j = 0; j < builder.Faces[i].Vertices.Length; j++) {
					compatibleMesh.Faces[i].Vertices[j] = vertices;
					vertices++;
				}
				compatibleMesh.Faces[i].Material = i;
				if (builder.Faces[i].Flipped) {
					compatibleMesh.Faces[i].Flip();
				}
			}
			// add mesh
			mesh.Add(compatibleMesh);
		}
		
	}
}