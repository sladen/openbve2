using System;

namespace Plugin {
	internal static class B3dCsvParser {
		
		// mesh builder
		private struct Vertex {
			// members
			internal OpenBveApi.Math.Vector3 SpatialCoordinates;
			internal OpenBveApi.Math.Vector2 TextureCoordinates;
			internal OpenBveApi.Math.Vector3 Normal;
		}
		private struct Face {
			// members
			internal FaceVertex[] Vertices;
			internal OpenBveApi.Color.ColorRGBA ReflectiveColor;
			internal OpenBveApi.Color.ColorRGB EmissiveColor;
			internal bool Flipped;
		}
		private struct FaceVertex {
			// members
			internal int Vertex;
			internal OpenBveApi.Math.Vector3 Normal;
		}
		private class MeshBuilder {
			// members
			internal Vertex[] Vertices;
			internal int VertexCount;
			internal Face[] Faces;
			internal int FaceCount;
			internal OpenBveApi.Texture.TextureHandle DaytimeTexture;
			internal OpenBveApi.Texture.TextureHandle NighttimeTexture;
			// constructors
			internal MeshBuilder() {
				this.Vertices = new Vertex[16];
				this.VertexCount = 0;
				this.Faces = new Face[4];
				this.FaceCount = 0;
				this.DaytimeTexture = OpenBveApi.Texture.TextureHandle.Null;
				this.NighttimeTexture = OpenBveApi.Texture.TextureHandle.Null;
			}
		}
		
		// load from file
		internal static OpenBveApi.General.Result LoadFromFile(string FileName, System.Text.Encoding Encoding, out OpenBveApi.Geometry.Mesh Mesh) {
			bool isB3D = string.Equals(System.IO.Path.GetExtension(FileName), ".b3d", StringComparison.OrdinalIgnoreCase);
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			// load lines
			string[] lines = System.IO.File.ReadAllLines(FileName, Encoding);
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
			Mesh = new OpenBveApi.Geometry.Mesh();
			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].Length != 0) {
					// separate command and arguments
					string command;
					string argumentSequence;
					if (isB3D) {
						int space = lines[i].IndexOf(' ');
						if (space >= 0) {
							command = lines[i].Substring(0, space).TrimEnd();
							argumentSequence = lines[i].Substring(space + 1);
						} else {
							command = lines[i];
							argumentSequence = "";
						}
					} else {
						int comma = lines[i].IndexOf(',');
						if (comma >= 0) {
							command = lines[i].Substring(0, comma).TrimEnd();
							argumentSequence = lines[i].Substring(comma + 1);
						} else {
							command = lines[i];
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
							AddMeshBuilder(ref Mesh, builder);
							builder = new MeshBuilder();
							break;
						case "vertex":
						case "addvertex":
							// vertex
							{
								if (isB3D & commandLower == "createmeshbuilder") {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "CreateMeshBuilder is not allowed in B3D files. Did you mean [MeshBuilder]?")
									                      );
								} else if (!isB3D & commandLower == "[meshbuilder]") {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "[MeshBuilder] is not allowed in CSV files. Did you mean CreateMeshBuilder?")
									                      );
								}
								double spatialX = 0.0, spatialY = 0.0, spatialZ = 0.0;
								if (arguments.Length >= 1 && arguments[0].Length != 0 && !IO.Parse(arguments[0], out spatialX)) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "spatialX is not a valid floating-point number.")
									                      );
									spatialX = 0.0;
								}
								if (arguments.Length >= 2 && arguments[1].Length != 0 && !IO.Parse(arguments[1], out spatialY)) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "spatialY is not a valid floating-point number.")
									                      );
									spatialY = 0.0;
								}
								if (arguments.Length >= 3 && arguments[2].Length != 0 && !IO.Parse(arguments[2], out spatialZ)) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "spatialZ is not a valid floating-point number.")
									                      );
									spatialZ = 0.0;
								}
								double normalX = 0.0, normalY = 0.0, normalZ = 0.0;
								if (arguments.Length >= 4 && arguments[3].Length != 0 && !IO.Parse(arguments[3], out normalX)) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "normalX is not a valid floating-point number.")
									                      );
									normalX = 0.0;
								}
								if (arguments.Length >= 5 && arguments[4].Length != 0 && !IO.Parse(arguments[4], out normalY)) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "normalY is not a valid floating-point number.")
									                      );
									normalY = 0.0;
								}
								if (arguments.Length >= 6 && arguments[5].Length != 0 && !IO.Parse(arguments[5], out normalZ)) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "normalZ is not a valid floating-point number.")
									                      );
									normalZ = 0.0;
								}
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
								if (arguments.Length < 3) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "At least 3 arguments are expected.")
									                      );
								} else {
									if (isB3D & commandLower == "addface") {
										Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
										                       new OpenBveApi.General.KeyValuePair("Source", FileName),
										                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
										                       new OpenBveApi.General.KeyValuePair("Command", command),
										                       new OpenBveApi.General.KeyValuePair("Text", "AddFace is not allowed in B3D files. Did you mean Face?")
										                      );
									} else if (isB3D & commandLower == "addface2") {
										Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
										                       new OpenBveApi.General.KeyValuePair("Source", FileName),
										                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
										                       new OpenBveApi.General.KeyValuePair("Command", command),
										                       new OpenBveApi.General.KeyValuePair("Text", "AddFace2 is not allowed in B3D files. Did you mean Face2?")
										                      );
									} else if (!isB3D & commandLower == "face") {
										Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
										                       new OpenBveApi.General.KeyValuePair("Source", FileName),
										                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
										                       new OpenBveApi.General.KeyValuePair("Command", command),
										                       new OpenBveApi.General.KeyValuePair("Text", "Face is not allowed in CSV files. Did you mean AddFace?")
										                      );
									} else if (!isB3D & commandLower == "face2") {
										Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
										                       new OpenBveApi.General.KeyValuePair("Source", FileName),
										                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
										                       new OpenBveApi.General.KeyValuePair("Command", command),
										                       new OpenBveApi.General.KeyValuePair("Text", "Face2 is not allowed in CSV files. Did you mean AddFace2?")
										                      );
									}
									int[] vertices = new int[arguments.Length];
									bool valid = true;
									for (int j = 0; j < arguments.Length; j++) {
										vertices[j] = 0;
										if (arguments[j].Length != 0 && !IO.Parse(arguments[j], out vertices[j])) {
											Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
											                       new OpenBveApi.General.KeyValuePair("Source", FileName),
											                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
											                       new OpenBveApi.General.KeyValuePair("Command", command),
											                       new OpenBveApi.General.KeyValuePair("Text", "vertex" + j.ToString(culture) + " is not a valid integer.")
											                      );
											vertices[j] = 0;
										}
										if (vertices[j] < 0 | vertices[j] >= builder.VertexCount) {
											Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
											                       new OpenBveApi.General.KeyValuePair("Source", FileName),
											                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
											                       new OpenBveApi.General.KeyValuePair("Command", command),
											                       new OpenBveApi.General.KeyValuePair("Text", "vertex" + j.ToString(culture) + " references a vertex that does not exist in the current mesh builder.")
											                      );
											valid = false;
										}
									}
									if (valid) {
										int faces = commandLower == "face2" | commandLower == "addface2" ? 2 : 1;
										for (int j = 0; j < faces; j++) {
											if (builder.Faces.Length == builder.FaceCount) {
												Array.Resize<Face>(ref builder.Faces, builder.Faces.Length << 1);
											}
											builder.Faces[builder.FaceCount] = new Face();
											builder.Faces[builder.FaceCount].Vertices = new FaceVertex[vertices.Length];
											OpenBveApi.Math.Vector3 vertexA = builder.Vertices[vertices[0]].SpatialCoordinates;
											OpenBveApi.Math.Vector3 vertexB = builder.Vertices[vertices[1]].SpatialCoordinates;
											OpenBveApi.Math.Vector3 vertexC = builder.Vertices[vertices[2]].SpatialCoordinates;
											OpenBveApi.Math.Vector3 normal = OpenBveApi.Math.Vector3.CreateNormal(vertexA, vertexB, vertexC);
											for (int k = 0; k < vertices.Length; k++) {
												builder.Faces[builder.FaceCount].Vertices[k].Vertex = vertices[k];
												if (builder.Vertices[vertices[k]].Normal.IsNullVector()) {
													builder.Faces[builder.FaceCount].Vertices[k].Normal = normal;
												} else {
													builder.Faces[builder.FaceCount].Vertices[k].Normal = builder.Vertices[vertices[k]].Normal;
												}
											}
											builder.Faces[builder.FaceCount].ReflectiveColor = new OpenBveApi.Color.ColorRGBA(1.0f, 1.0f, 1.0f, 1.0f);
											builder.Faces[builder.FaceCount].EmissiveColor = new OpenBveApi.Color.ColorRGB(0.0f, 0.0f, 0.0f);
											builder.Faces[builder.FaceCount].Flipped = j == 1;
											builder.FaceCount++;
										}
									}
								}
							}
							break;
						case "color":
						case "setcolor":
							{
								// color
								if (isB3D & commandLower == "setcolor") {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "SetColor is not allowed in B3D files. Did you mean Color?")
									                      );
								} else if (!isB3D & commandLower == "color") {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "Color is not allowed in CSV files. Did you mean SetColor?")
									                      );
								}
								double colorR = 1.0, colorG = 1.0, colorB = 1.0, colorA = 1.0;
								if (arguments.Length >= 1 && arguments[0].Length != 0 && !IO.Parse(arguments[0], out colorR)) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "colorR is not a valid floating-point number.")
									                      );
									colorR = 1.0;
								}
								if (arguments.Length >= 2 && arguments[1].Length != 0 && !IO.Parse(arguments[1], out colorG)) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "colorR is not a valid floating-point number.")
									                      );
									colorG = 1.0;
								}
								if (arguments.Length >= 3 && arguments[2].Length != 0 && !IO.Parse(arguments[2], out colorB)) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "colorR is not a valid floating-point number.")
									                      );
									colorB = 1.0;
								}
								if (arguments.Length >= 4 && arguments[3].Length != 0 && !IO.Parse(arguments[3], out colorA)) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "colorA is not a valid floating-point number.")
									                      );
									colorA = 1.0;
								}
								if (colorR < 0.0 | colorR > 255.0) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "colorR must be between 0 and 255.")
									                      );
									colorR = colorR < 128.0 ? 0.0: 255.0;
								}
								if (colorG < 0.0 | colorG > 255.0) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "colorG must be between 0 and 255.")
									                      );
									colorG = colorG < 128.0 ? 0.0: 255.0;
								}
								if (colorB < 0.0 | colorB > 255.0) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "colorB must be between 0 and 255.")
									                      );
									colorB = colorB < 128.0 ? 0.0: 255.0;
								}
								if (colorA < 0.0 | colorA > 255.0) {
									Interfaces.Host.Report(OpenBveApi.General.ReportType.InvalidData,
									                       new OpenBveApi.General.KeyValuePair("Source", FileName),
									                       new OpenBveApi.General.KeyValuePair("Row", i + 1),
									                       new OpenBveApi.General.KeyValuePair("Command", command),
									                       new OpenBveApi.General.KeyValuePair("Text", "colorA must be between 0 and 255.")
									                      );
									colorA = colorA < 128.0 ? 0.0: 255.0;
								}
								for (int j = 0; j < builder.FaceCount; j++) {
									builder.Faces[j].ReflectiveColor = new OpenBveApi.Color.ColorRGBA((float)colorR, (float)colorG, (float)colorB, (float)colorA);
								}
							}
							break;
						default:
							break;
					}
				}
			}
			AddMeshBuilder(ref Mesh, builder);
			return OpenBveApi.General.Result.Successful;
		}
		
		// add mesh builder
		private static void AddMeshBuilder(ref OpenBveApi.Geometry.Mesh mesh, MeshBuilder builder) {
			// convert mesh builder to compatible mesh
			OpenBveApi.Geometry.Mesh compatibleMesh = new OpenBveApi.Geometry.Mesh();
			// vertices
			int vertices = 0;
			for (int i = 0; i < builder.FaceCount; i++) {
				vertices += builder.Faces[i].Vertices.Length;
			}
			compatibleMesh.Vertices = new OpenBveApi.Geometry.Vertex[vertices];
			vertices = 0;
			for (int i = 0; i < builder.FaceCount; i++) {
				for (int j = 0; j < builder.Faces[i].Vertices.Length; j++) {
					int k = builder.Faces[i].Vertices[j].Vertex;
					compatibleMesh.Vertices[vertices].SpatialCoordinates = builder.Vertices[k].SpatialCoordinates;
					compatibleMesh.Vertices[vertices].TextureCoordinates = builder.Vertices[k].TextureCoordinates;
					compatibleMesh.Vertices[vertices].Normal = builder.Faces[i].Vertices[j].Normal;
					compatibleMesh.Vertices[vertices].ReflectiveColor = builder.Faces[i].ReflectiveColor;
					vertices++;
				}
			}
			// materials
			compatibleMesh.Materials = new OpenBveApi.Geometry.Material[builder.FaceCount];
			for (int i = 0; i < builder.FaceCount; i++) {
				compatibleMesh.Materials[i].DaytimeTexture = builder.DaytimeTexture;
				compatibleMesh.Materials[i].NighttimeTexture = builder.NighttimeTexture;
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