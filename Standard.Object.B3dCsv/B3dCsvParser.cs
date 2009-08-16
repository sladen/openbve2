using System;

namespace Plugin {
	internal static class B3dCsvParser {
		
		// vertex
		private struct Vertex {
			// members
			internal OpenBveApi.Math.Vector3 SpatialCoordinates;
			internal OpenBveApi.Math.Vector2 TextureCoordinates;
			internal OpenBveApi.Math.Vector3 Normal;
		}
		
		// face
		private struct Face {
			// members
			internal FaceVertex[] Vertices;
			internal OpenBveApi.Color.ColorRGBA ReflectiveColor;
			internal OpenBveApi.Color.ColorRGB EmissiveColor;
			internal OpenBveApi.Color.TransparentColor TransparentColor;
			internal bool Flipped;
			// constructors

			internal Face(int offset, int[] indices, OpenBveApi.Math.Vector3 normal) {
				FaceVertex[] vertices = new FaceVertex[indices.Length];
				for (int i = 0; i < indices.Length; i++) {
					vertices[i] = new FaceVertex(offset + indices[i], normal);
				}
				this.Vertices = vertices;
				this.ReflectiveColor = new OpenBveApi.Color.ColorRGBA(1.0f, 1.0f, 1.0f, 1.0f);
				this.EmissiveColor = new OpenBveApi.Color.ColorRGB(0.0f, 0.0f, 0.0f);
				this.TransparentColor = new OpenBveApi.Color.TransparentColor(0, 0, 0, false);
				this.Flipped = false;
			}
			internal Face(int offset, int[] indices, OpenBveApi.Math.Vector3[] normals) {
				if (indices.Length != normals.Length) {
					throw new ArgumentException();
				} else {
					FaceVertex[] vertices = new FaceVertex[indices.Length];
					for (int i = 0; i < indices.Length; i++) {
						vertices[i] = new FaceVertex(offset + indices[i], normals[i]);
					}
					this.Vertices = vertices;
					this.ReflectiveColor = new OpenBveApi.Color.ColorRGBA(1.0f, 1.0f, 1.0f, 1.0f);
					this.EmissiveColor = new OpenBveApi.Color.ColorRGB(0.0f, 0.0f, 0.0f);
					this.TransparentColor = new OpenBveApi.Color.TransparentColor(0, 0, 0, false);
					this.Flipped = false;
				}
			}
		}
		
		// face vertex
		private struct FaceVertex {
			// members
			internal int Vertex;
			internal OpenBveApi.Math.Vector3 Normal;
			// constructors
			internal FaceVertex(int vertex, OpenBveApi.Math.Vector3 normal) {
				this.Vertex = vertex;
				this.Normal = normal;
			}
		}
		
		// mesh builder
		private class MeshBuilder {
			// members
			internal Vertex[] Vertices;
			internal int VertexCount;
			internal Face[] Faces;
			internal int FaceCount;
			internal string DaytimeTexture;
			internal string NighttimeTexture;
			// constructors
			internal MeshBuilder() {
				this.Vertices = new Vertex[16];
				this.VertexCount = 0;
				this.Faces = new Face[4];
				this.FaceCount = 0;
				this.DaytimeTexture = null;
				this.NighttimeTexture = null;
			}
			// functions
			internal void Translate(OpenBveApi.Math.Vector3 offset) {
				for (int i = 0; i < this.VertexCount; i++) {
					this.Vertices[i].SpatialCoordinates += offset;
				}
			}
			internal void Rotate(OpenBveApi.Math.Vector3 direction, double cosineOfAngle, double sineOfAngle) {
				for (int i = 0; i < this.VertexCount; i++) {
					OpenBveApi.Math.Vector3.Rotate(ref this.Vertices[i].SpatialCoordinates, direction, cosineOfAngle, sineOfAngle);
				}
			}
		}
		
		// load from file
		/// <summary>Loads a B3D/CSV object from a file and returns the success of the operation.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="encoding">The suggested encoding.</param>
		/// <param name="mesh">Receives the mesh of the object.</param>
		/// <returns>The success of the operation.</returns>
		internal static OpenBveApi.General.Result LoadFromFile(string fileName, System.Text.Encoding encoding, out OpenBveApi.Geometry.FaceVertexMesh mesh) {
			// prepare
			string folder = System.IO.Path.GetDirectoryName(fileName);
			bool isB3D = string.Equals(System.IO.Path.GetExtension(fileName), ".b3d", StringComparison.OrdinalIgnoreCase);
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			// load lines
			string[] lines = System.IO.File.ReadAllLines(fileName, encoding);
			for (int i = 0; i < lines.Length; i++) {
				int semicolon = lines[i].IndexOf(';');
				if (semicolon >= 0) {
					lines[i] = lines[i].Substring(0, semicolon).Trim();
				} else {
					lines[i] = lines[i].Trim();
				}
			}
			// parse lines
			MeshBuilder builder = new MeshBuilder();
			mesh = new OpenBveApi.Geometry.FaceVertexMesh();
			for (int row = 0; row < lines.Length; row++) {
				if (lines[row].Length != 0) {
					// separate command and arguments
					string command;
					string argumentSequence;
					if (isB3D) {
						int space = lines[row].IndexOf(' ');
						if (space >= 0) {
							command = lines[row].Substring(0, space).TrimEnd();
							argumentSequence = lines[row].Substring(space + 1);
						} else {
							command = lines[row];
							argumentSequence = "";
						}
					} else {
						int comma = lines[row].IndexOf(',');
						if (comma >= 0) {
							command = lines[row].Substring(0, comma).TrimEnd();
							argumentSequence = lines[row].Substring(comma + 1);
						} else {
							command = lines[row];
							argumentSequence = "";
						}
					}
					string[] arguments = argumentSequence.Split(',');
					for (int j = 0; j < arguments.Length; j++) {
						arguments[j] = arguments[j].Trim();
					}
					string commandLower = command.ToLowerInvariant();
					// process commands
					switch (commandLower) {
						case "[meshbuilder]":
						case "createmeshbuilder":
							// meshbuilder
							{
								AddMeshBuilder(folder, mesh, builder, encoding);
								builder = new MeshBuilder();
							}
							break;
						case "vertex":
						case "addvertex":
							// vertex
							{
								if (isB3D & commandLower == "createmeshbuilder") {
									IO.ReportNotAllowedCommand(fileName, row, command, "[MeshBuilder]");
								} else if (!isB3D & commandLower == "[meshbuilder]") {
									IO.ReportNotAllowedCommand(fileName, row, command, "CreateMeshBuilder");
								}
								IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 6, true);
								double spatialX, spatialY, spatialZ;
								IO.ParseDouble(fileName, row, command, arguments, 0, "spatialX", 0.0, out spatialX);
								IO.ParseDouble(fileName, row, command, arguments, 1, "spatialY", 0.0, out spatialY);
								IO.ParseDouble(fileName, row, command, arguments, 2, "spatialZ", 0.0, out spatialZ);
								double normalX, normalY, normalZ;
								IO.ParseDouble(fileName, row, command, arguments, 3, "normalX", 0.0, out normalX);
								IO.ParseDouble(fileName, row, command, arguments, 4, "normalY", 0.0, out normalY);
								IO.ParseDouble(fileName, row, command, arguments, 5, "normalZ", 0.0, out normalZ);
								OpenBveApi.Math.Vector3 spatial = new OpenBveApi.Math.Vector3(spatialX, spatialY, spatialZ);
								OpenBveApi.Math.Vector3 normal = new OpenBveApi.Math.Vector3(normalX, normalY, normalZ);
								if (!normal.IsNullVector()) {
									OpenBveApi.Math.Vector3.Normalize(ref normal);
								}
								if (builder.Vertices.Length == builder.VertexCount) {
									Array.Resize<Vertex>(ref builder.Vertices, builder.Vertices.Length << 1);
								}
								builder.Vertices[builder.VertexCount].SpatialCoordinates = spatial;
								builder.Vertices[builder.VertexCount].TextureCoordinates = new OpenBveApi.Math.Vector2(0.0, 0.0);
								builder.Vertices[builder.VertexCount].Normal = normal;
								builder.VertexCount++;
							}
							break;
						case "face":
						case "face2":
						case "addface":
						case "addface2":
							// face
							{
								if (IO.CheckArgumentCount(fileName, row, command, arguments.Length, 3, int.MaxValue, true)) {
									if (isB3D & commandLower == "addface") {
										IO.ReportNotAllowedCommand(fileName, row, command, "Face");
									} else if (isB3D & commandLower == "addface2") {
										IO.ReportNotAllowedCommand(fileName, row, command, "Face2");
									} else if (!isB3D & commandLower == "face") {
										IO.ReportNotAllowedCommand(fileName, row, command, "AddFace");
									} else if (!isB3D & commandLower == "face2") {
										IO.ReportNotAllowedCommand(fileName, row, command, "AddFace2");
									}
									int[] vertices = new int[arguments.Length];
									bool valid = true;
									for (int i = 0; i < arguments.Length; i++) {
										if (IO.ParseInt(fileName, row + 1, command, arguments, i, "vertex" + (i + 1).ToString(culture), -1, out vertices[i])) {
											if (vertices[i] < 0 | vertices[i] >= builder.VertexCount) {
												IO.ReportInvalidArgument(fileName, row, command, i, "vertex" + (i + 1).ToString(culture), arguments[i], "The argument references a vertex that does not exist.");
												valid = false;
											}
										} else {
											valid = false;
											break;
										}
									}
									if (valid) {
										// create face
										if (valid) {
											int numberOfFaces = commandLower == "face2" | commandLower == "addface2" ? 2 : 1;
											for (int i = 0; i < numberOfFaces; i++) {
												if (builder.Faces.Length == builder.FaceCount) {
													Array.Resize<Face>(ref builder.Faces, builder.Faces.Length << 1);
												}
												builder.Faces[builder.FaceCount] = new Face();
												builder.Faces[builder.FaceCount].Vertices = new FaceVertex[vertices.Length];
												OpenBveApi.Math.Vector3 vertexA = builder.Vertices[vertices[0]].SpatialCoordinates;
												OpenBveApi.Math.Vector3 vertexB = builder.Vertices[vertices[1]].SpatialCoordinates;
												OpenBveApi.Math.Vector3 vertexC = builder.Vertices[vertices[2]].SpatialCoordinates;
												OpenBveApi.Math.Vector3 normal = OpenBveApi.Math.Vector3.CreateNormal(vertexA, vertexB, vertexC);
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
							}
							break;
						case "cube":
							// cube
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
							// cylinder
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
						case "texture":
						case "generatenormals":
							// texture
							{
								if (isB3D & commandLower == "generatenormals") {
									IO.ReportNotAllowedCommand(fileName, row, command, "[Texture]");
								} else if (!isB3D & commandLower == "[texture]") {
									IO.ReportNotAllowedCommand(fileName, row, command, "GenerateNormals");
								}
							}
							break;
						case "translate":
						case "translateall":
							// translate
							{
								IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 3, true);
								double offsetX, offsetY, offsetZ;
								IO.ParseDouble(fileName, row, command, arguments, 0, "offsetX", 0.0, out offsetX);
								IO.ParseDouble(fileName, row, command, arguments, 1, "offsetY", 0.0, out offsetY);
								IO.ParseDouble(fileName, row, command, arguments, 2, "offsetZ", 0.0, out offsetZ);
								OpenBveApi.Math.Vector3 offset = new OpenBveApi.Math.Vector3(offsetX, offsetY, offsetZ);
								builder.Translate(offset);
								if (commandLower == "translateall") {
									mesh.Translate(offset);
								}
							}
							break;
						case "scale":
						case "scaleall":
							// TODO: Implement this.
							break;
						case "rotate":
						case "rotateall":
							// rotate
							{
								IO.CheckArgumentCount(fileName, row, command, arguments.Length, 0, 4, true);
								double directionX, directionY, directionZ, angle;
								IO.ParseDouble(fileName, row, command, arguments, 0, "directionX", 0.0, out directionX);
								IO.ParseDouble(fileName, row, command, arguments, 1, "directionY", 0.0, out directionY);
								IO.ParseDouble(fileName, row, command, arguments, 2, "directionZ", 0.0, out directionZ);
								IO.ParseDouble(fileName, row, command, arguments, 3, "angle", 0.0, out angle);
								OpenBveApi.Math.Vector3 direction = new OpenBveApi.Math.Vector3(directionX, directionY, directionZ);
								OpenBveApi.Math.Vector3.Normalize(ref direction, OpenBveApi.Math.Vector3.Right);
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
							// TODO: Implement this.
							break;
						case "color":
						case "setcolor":
							// color
							{
								if (isB3D & commandLower == "setcolor") {
									IO.ReportNotAllowedCommand(fileName, row, command, "Color");
								} else if (!isB3D & commandLower == "color") {
									IO.ReportNotAllowedCommand(fileName, row, command, "SetColor");
								}
								double colorR, colorG, colorB, colorA;
								IO.ParseDouble(fileName, row, command, arguments, 0, "colorR", 0.0, 255.0, 255.0, out colorR);
								IO.ParseDouble(fileName, row, command, arguments, 1, "colorG", 0.0, 255.0, 255.0, out colorG);
								IO.ParseDouble(fileName, row, command, arguments, 2, "colorB", 0.0, 255.0, 255.0, out colorB);
								IO.ParseDouble(fileName, row, command, arguments, 3, "colorA", 0.0, 255.0, 255.0, out colorA);
								colorR *= 0.00392156862745098;
								colorG *= 0.00392156862745098;
								colorB *= 0.00392156862745098;
								colorA *= 0.00392156862745098;
								for (int j = 0; j < builder.FaceCount; j++) {
									builder.Faces[j].ReflectiveColor = new OpenBveApi.Color.ColorRGBA((float)colorR, (float)colorG, (float)colorB, (float)colorA);
								}
							}
							break;
						case "emissivecolor":
						case "setemissivecolor":
							// emissive color
							{
								if (isB3D & commandLower == "setemissivecolor") {
									IO.ReportNotAllowedCommand(fileName, row, command, "EmissiveColor");
								} else if (!isB3D & commandLower == "emissivecolor") {
									IO.ReportNotAllowedCommand(fileName, row, command, "SetEmissiveColor");
								}
								double colorR, colorG, colorB;
								IO.ParseDouble(fileName, row, command, arguments, 0, "colorR", 0.0, 255.0, 0.0, out colorR);
								IO.ParseDouble(fileName, row, command, arguments, 1, "colorG", 0.0, 255.0, 0.0, out colorG);
								IO.ParseDouble(fileName, row, command, arguments, 2, "colorB", 0.0, 255.0, 0.0, out colorB);
								colorR *= 0.00392156862745098;
								colorG *= 0.00392156862745098;
								colorB *= 0.00392156862745098;
								for (int j = 0; j < builder.FaceCount; j++) {
									builder.Faces[j].EmissiveColor = new OpenBveApi.Color.ColorRGB((float)colorR, (float)colorG, (float)colorB);
								}
							}
							break;
						case "blendmode":
						case "setblendmode":
							// TODO: Implement this.
							break;
						case "load":
						case "loadtexture":
							// load texture
							{
								if (isB3D & commandLower == "loadtexture") {
									IO.ReportNotAllowedCommand(fileName, row, command, "Load");
								} else if (!isB3D & commandLower == "load") {
									IO.ReportNotAllowedCommand(fileName, row, command, "LoadTexture");
								}
								if (IO.CheckArgumentCount(fileName, row, command, arguments.Length, 1, 2, true)) {
									builder.DaytimeTexture = arguments[0];
								}
							}
							break;
						case "transparentcolor":
						case "setdecaltransparentcolor":
							// emissive color
							{
								if (isB3D & commandLower == "setdecaltransparentcolor") {
									IO.ReportNotAllowedCommand(fileName, row, command, "TransparentColor");
								} else if (!isB3D & commandLower == "transparentcolor") {
									IO.ReportNotAllowedCommand(fileName, row, command, "SetDecalTransparentColor");
								}
								int colorR, colorG, colorB;
								IO.ParseInt(fileName, row, command, arguments, 0, "colorR", 0, 255, 0, out colorR);
								IO.ParseInt(fileName, row, command, arguments, 1, "colorG", 0, 255, 0, out colorG);
								IO.ParseInt(fileName, row, command, arguments, 2, "colorB", 0, 255, 0, out colorB);
								for (int j = 0; j < builder.FaceCount; j++) {
									builder.Faces[j].TransparentColor = new OpenBveApi.Color.TransparentColor((byte)colorR, (byte)colorG, (byte)colorB, true);
								}
							}
							break;
						case "coordinates":
						case "settexturecoordinates":
							{
								if (isB3D & commandLower == "settexturecoordinates") {
									IO.ReportNotAllowedCommand(fileName, row, command, "Coordinates");
								} else if (!isB3D & commandLower == "coordinates") {
									IO.ReportNotAllowedCommand(fileName, row, command, "SetTextureCoordinate");
								}
								int vertex;
								if (IO.ParseInt(fileName, row, command, arguments, 0, "vertex", 0, builder.VertexCount - 1, -1, out vertex)) {
									double texCoordX, texCoordY;
									IO.ParseDouble(fileName, row, command, arguments, 1, "texCoorX", 0.0, out texCoordX);
									IO.ParseDouble(fileName, row, command, arguments, 2, "texCoorY", 0.0, out texCoordY);
									builder.Vertices[vertex].TextureCoordinates = new OpenBveApi.Math.Vector2(texCoordX, texCoordY);
								}
							}
							break;
						default:
							// default
							{
								IO.ReportNotSupportedCommand(fileName, row, command);
							}
							break;
					}
				}
			}
			// finish
			AddMeshBuilder(folder, mesh, builder, encoding);
			return OpenBveApi.General.Result.Successful;
		}
		
		// add cube
		/// <summary>Adds a cube to a mesh builder.</summary>
		/// <param name="builder">The mesh builder to add the cube to.</param>
		/// <param name="halfWidth">Half the width of the cube.</param>
		/// <param name="halfHeight">Half the height of the cube.</param>
		/// <param name="halfDepth">Half the depth of the cube.</param>
		private static void AddCube(MeshBuilder builder, double halfWidth, double halfHeight, double halfDepth) {
			// vertices
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
			// faces
			while (builder.FaceCount + 6 >= builder.Faces.Length) {
				Array.Resize<Face>(ref builder.Faces, builder.Faces.Length << 1);
			}
			builder.Faces[builder.FaceCount + 0] = new Face(builder.VertexCount, new int[] { 0, 1, 2, 3 }, OpenBveApi.Math.Vector3.Backward);
			builder.Faces[builder.FaceCount + 1] = new Face(builder.VertexCount, new int[] { 0, 4, 5, 1 }, OpenBveApi.Math.Vector3.Right);
			builder.Faces[builder.FaceCount + 2] = new Face(builder.VertexCount, new int[] { 0, 3, 7, 4 }, OpenBveApi.Math.Vector3.Up);
			builder.Faces[builder.FaceCount + 3] = new Face(builder.VertexCount, new int[] { 6, 5, 4, 7 }, OpenBveApi.Math.Vector3.Forward);
			builder.Faces[builder.FaceCount + 4] = new Face(builder.VertexCount, new int[] { 6, 7, 3, 2 }, OpenBveApi.Math.Vector3.Left);
			builder.Faces[builder.FaceCount + 5] = new Face(builder.VertexCount, new int[] { 6, 2, 1, 5 }, OpenBveApi.Math.Vector3.Down);
			// finish
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
			// prepare
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
			// vertices and normals
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
				OpenBveApi.Math.Vector3.Rotate(ref normal, right, cosSlope, sinSlope);
				builder.Vertices[builder.VertexCount + 2 * i + 0].SpatialCoordinates = new OpenBveApi.Math.Vector3(upperX, halfHeight, upperZ);
				builder.Vertices[builder.VertexCount + 2 * i + 1].SpatialCoordinates = new OpenBveApi.Math.Vector3(lowerX, -halfHeight, lowerZ);
				normals[2 * i + 0] = normal;
				normals[2 * i + 1] = normal;
				angleCurrent += angleDelta;
			}
			// faces
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
			// caps
			if (upperCap) {
				int[] indices = new int[segments];
				for (int i = 0; i < segments; i++) {
					indices[i] = 2 * i + 1;
				}
				builder.Faces[builder.FaceCount] = new Face(builder.VertexCount, indices, up);
				builder.FaceCount++;
			}
			if (lowerCap) {
				int[] indices = new int[segments];
				for (int i = 0; i < segments; i++) {
					indices[i] = 2 * (segments - i - 1);
				}
				builder.Faces[builder.FaceCount] = new Face(builder.VertexCount, indices, down);
				builder.FaceCount++;
			}
			// finish
			builder.VertexCount += 2 * segments;
		}
		
		// add mesh builder
		/// <summary>Adds a mesh builder to a mesh.</summary>
		/// <param name="folder">The absolute path to the folder where the object is stored.</param>
		/// <param name="mesh">The mesh to add the mesh builder to.</param>
		/// <param name="builder">The mesh builder to add.</param>
		/// <param name="encoding">The suggested encoding.</param>
		private static void AddMeshBuilder(string folder, OpenBveApi.Geometry.FaceVertexMesh mesh, MeshBuilder builder, System.Text.Encoding encoding) {
			// convert mesh builder to compatible mesh
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
				OpenBveApi.Math.Vector3 normal = OpenBveApi.Math.Vector3.CreateNormal(
					builder.Vertices[vertex0].SpatialCoordinates,
					builder.Vertices[vertex1].SpatialCoordinates,
					builder.Vertices[vertex2].SpatialCoordinates
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
					vertices++;
				}
			}
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
					string textureFile = OpenBveApi.Path.CombineFile(folder, builder.DaytimeTexture);
					OpenBveApi.Path.PathReference path = new OpenBveApi.Path.PathReference(OpenBveApi.Path.PathBase.AbsolutePath, OpenBveApi.Path.PathType.File, textureFile);
					OpenBveApi.General.Origin origin = new OpenBveApi.General.Origin(path, null, encoding);
					OpenBveApi.Texture.TextureParameters parameters = new OpenBveApi.Texture.TextureParameters(builder.Faces[i].TransparentColor, horizontalWrapMode, verticalWrapMode, null);
					OpenBveApi.Texture.TextureHandle handle;
					if (Interfaces.Host.RegisterTexture(origin, parameters, out handle) == OpenBveApi.General.Result.Successful) {
						compatibleMesh.Materials[i].DaytimeTexture = handle;
					} else {
						compatibleMesh.Materials[i].DaytimeTexture = OpenBveApi.Texture.TextureHandle.Null;
					}
				} else {
					compatibleMesh.Materials[i].DaytimeTexture = OpenBveApi.Texture.TextureHandle.Null;
				}
				// misc
				compatibleMesh.Materials[i].NighttimeTexture = OpenBveApi.Texture.TextureHandle.Null;
				compatibleMesh.Materials[i].EmissiveColor = builder.Faces[i].EmissiveColor;
			}
			// faces
			compatibleMesh.Faces = new OpenBveApi.Geometry.Face[builder.FaceCount];
			vertices = 0;
			for (int i = 0; i < builder.FaceCount; i++) {
				compatibleMesh.Faces[i].Type = OpenBveApi.Geometry.FaceType.Polygon;
				compatibleMesh.Faces[i].Vertices = new OpenBveApi.Geometry.FaceVertex[builder.Faces[i].Vertices.Length];
				for (int j = 0; j < builder.Faces[i].Vertices.Length; j++) {
					compatibleMesh.Faces[i].Vertices[j].Vertex = vertices;
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