using System;

namespace Plugin
{
	internal static partial class Parser {
		
		// process structure
		/// <summary>Loads binary .X object data and returns a compatible mesh.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="structure">The object structure to process.</param>
		/// <param name="obj">Receives the compatible mesh.</param>
		/// <param name="encoding">The fallback encoding.</param>
		private static bool ProcessStructure(string fileName, Structure structure, out OpenBveApi.Geometry.GenericObject obj, System.Text.Encoding encoding) {
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			OpenBveApi.Geometry.FaceVertexMesh compatibleMesh = new OpenBveApi.Geometry.FaceVertexMesh();
			obj = null;
			/*
			 * file
			 */
			for (int i = 0; i < structure.Data.Length; i++) {
				Structure f = structure.Data[i] as Structure;
				if (f == null) {
					IO.ReportError(fileName, "Top-level inlined arguments are invalid");
					return false;
				}
				switch (f.Name) {
					case "Mesh":
						{
							/*
							 * mesh
							 */
							if (f.Data.Length < 4) {
								IO.ReportError(fileName, "Mesh is expected to have at least 4 arguments");
								return false;
							} else if (!(f.Data[0] is int)) {
								IO.ReportError(fileName, "nVertices is expected to be a DWORD in Mesh");
								return false;
							} else if (!(f.Data[1] is Structure[])) {
								IO.ReportError(fileName, "vertices[nVertices] is expected to be a Vector array in Mesh");
								return false;
							} else if (!(f.Data[2] is int)) {
								IO.ReportError(fileName, "nFaces is expected to be a DWORD in Mesh");
								return false;
							} else if (!(f.Data[3] is Structure[])) {
								IO.ReportError(fileName, "faces[nFaces] is expected to be a MeshFace array in Mesh");
								return false;
							}
							int nVertices = (int)f.Data[0];
							if (nVertices < 0) {
								IO.ReportError(fileName, "nVertices is expected to be non-negative in Mesh");
								return false;
							}
							Structure[] sVertices = (Structure[])f.Data[1];
							if (nVertices != sVertices.Length) {
								IO.ReportError(fileName, "nVertices does not match with the length of array vertices in Mesh");
								return false;
							}
							int nFaces = (int)f.Data[2];
							if (nFaces < 0) {
								IO.ReportError(fileName, "nFaces is expected to be non-negative in Mesh");
								return false;
							}
							Structure[] sFaces = (Structure[])f.Data[3];
							if (nFaces != sFaces.Length) {
								IO.ReportError(fileName, "nFaces does not match with the length of array faces in Mesh");
								return false;
							}
							/*
							 * collect vertices
							 */
							OpenBveApi.Geometry.Vertex[] vertices = new OpenBveApi.Geometry.Vertex[nVertices];
							for (int j = 0; j < nVertices; j++) {
								if (sVertices[j].Name != "Vector") {
									IO.ReportError(fileName, "vertices[" + j.ToString(culture) + "] is expected to be of template Vertex in Mesh");
									return false;
								} else if (sVertices[j].Data.Length != 3) {
									IO.ReportError(fileName, "vertices[" + j.ToString(culture) + "] is expected to have 3 arguments in Mesh");
									return false;
								} else if (!(sVertices[j].Data[0] is double)) {
									IO.ReportError(fileName, "x is expected to be a float in vertices[" + j.ToString(culture) + "] in Mesh");
									return false;
								} else if (!(sVertices[j].Data[1] is double)) {
									IO.ReportError(fileName, "y is expected to be a float in vertices[" + j.ToString(culture) + "] in Mesh");
									return false;
								} else if (!(sVertices[j].Data[2] is double)) {
									IO.ReportError(fileName, "z is expected to be a float in vertices[" + j.ToString(culture) + "] in Mesh");
									return false;
								}
								double x = (double)sVertices[j].Data[0];
								double y = (double)sVertices[j].Data[1];
								double z = (double)sVertices[j].Data[2];
								vertices[j].SpatialCoordinates = new OpenBveApi.Math.Vector3(x, y, z);
							}
							/*
							 * collect faces
							 */
							int[][] faces = new int[nFaces][];
							OpenBveApi.Math.Vector3[][] faceNormals = new OpenBveApi.Math.Vector3[nFaces][];
							int[] faceMaterials = new int[nFaces];
							for (int j = 0; j < nFaces; j++) {
								faceMaterials[j] = -1;
							}
							for (int j = 0; j < nFaces; j++) {
								if (sFaces[j].Name != "MeshFace") {
									IO.ReportError(fileName, "faces[" + j.ToString(culture) + "] is expected to be of template MeshFace in Mesh");
									return false;
								} else if (sFaces[j].Data.Length != 2) {
									IO.ReportError(fileName, "face[" + j.ToString(culture) + "] is expected to have 2 arguments in Mesh");
									return false;
								} else if (!(sFaces[j].Data[0] is int)) {
									IO.ReportError(fileName, "nFaceVertexIndices is expected to be a DWORD in face[" + j.ToString(culture) + "] in Mesh");
									return false;
								} else if (!(sFaces[j].Data[1] is int[])) {
									IO.ReportError(fileName, "faceVertexIndices[nFaceVertexIndices] is expected to be a DWORD array in face[" + j.ToString(culture) + "] in Mesh");
									return false;
								}
								int nFaceVertexIndices = (int)sFaces[j].Data[0];
								if (nFaceVertexIndices < 0) {
									IO.ReportError(fileName, "nFaceVertexIndices is expected to be non-negative in MeshFace in Mesh");
									return false;
								}
								int[] faceVertexIndices = (int[])sFaces[j].Data[1];
								if (nFaceVertexIndices != faceVertexIndices.Length) {
									IO.ReportError(fileName, "nFaceVertexIndices does not match with the length of array faceVertexIndices in face[" + j.ToString(culture) + "] in Mesh");
									return false;
								}
								faces[j] = new int[nFaceVertexIndices];
								faceNormals[j] = new OpenBveApi.Math.Vector3[nFaceVertexIndices];
								for (int k = 0; k < nFaceVertexIndices; k++) {
									if (faceVertexIndices[k] < 0 | faceVertexIndices[k] >= nVertices) {
										IO.ReportError(fileName, "faceVertexIndices[" + k.ToString(culture) + "] does not reference a valid vertex in face[" + j.ToString(culture) + "] in Mesh");
										return false;
									}
									faces[j][k] = faceVertexIndices[k];
									faceNormals[j][k] = new OpenBveApi.Math.Vector3(0.0f, 0.0f, 0.0f);
								}
							}
							/*
							 * collect additional templates
							 */
							Material[] materials = new Material[] { };
							for (int j = 4; j < f.Data.Length; j++) {
								Structure g = f.Data[j] as Structure;
								if (g == null) {
									IO.ReportError(fileName, "Unexpected inlined argument encountered in Mesh");
									return false;
								}
								switch (g.Name) {
									case "MeshMaterialList":
										{
											/*
											 * meshmateriallist
											 */
											if (g.Data.Length < 3) {
												IO.ReportError(fileName, "MeshMaterialList is expected to have at least 3 arguments in Mesh");
												return false;
											} else if (!(g.Data[0] is int)) {
												IO.ReportError(fileName, "nMaterials is expected to be a DWORD in MeshMaterialList in Mesh");
												return false;
											} else if (!(g.Data[1] is int)) {
												IO.ReportError(fileName, "nFaceIndexes is expected to be a DWORD in MeshMaterialList in Mesh");
												return false;
											} else if (!(g.Data[2] is int[])) {
												IO.ReportError(fileName, "faceIndexes[nFaceIndexes] is expected to be a DWORD array in MeshMaterialList in Mesh");
												return false;
											}
											int nMaterials = (int)g.Data[0];
											if (nMaterials < 0) {
												IO.ReportError(fileName, "nMaterials is expected to be non-negative in MeshMaterialList in Mesh");
												return false;
											}
											int nFaceIndexes = (int)g.Data[1];
											if (nFaceIndexes < 0) {
												IO.ReportError(fileName, "nFaceIndexes is expected to be non-negative in MeshMaterialList in Mesh");
												return false;
											} else if (nFaceIndexes > nFaces) {
												IO.ReportError(fileName, "nFaceIndexes does not reference valid faces in MeshMaterialList in Mesh");
												return false;
											}
											int[] faceIndexes = (int[])g.Data[2];
											if (nFaceIndexes != faceIndexes.Length) {
												IO.ReportError(fileName, "nFaceIndexes does not match with the length of array faceIndexes in face[" + j.ToString(culture) + "] in Mesh");
												return false;
											}
											for (int k = 0; k < nFaceIndexes; k++) {
												if (faceIndexes[k] < 0 | faceIndexes[k] >= nMaterials) {
													IO.ReportError(fileName, "faceIndexes[" + k.ToString(culture) + "] does not reference a valid Material template in MeshMaterialList in Mesh");
													return false;
												}
											}
											/*
											 * collect material templates
											 */
											int mn = materials.Length;
											Array.Resize<Material>(ref materials, mn + nMaterials);
											for (int k = 0; k < nMaterials; k++) {
												materials[mn + k].FaceColor = new OpenBveApi.Color.ColorRGBA(1.0f, 1.0f, 1.0f, 1.0f);
												materials[mn + k].SpecularColor = new OpenBveApi.Color.ColorRGB(0.0f, 0.0f, 0.0f);
												materials[mn + k].EmissiveColor = new OpenBveApi.Color.ColorRGB(0.0f, 0.0f, 0.0f);
												materials[mn + k].TextureFilename = null;
											}
											int materialIndex = mn;
											for (int k = 3; k < g.Data.Length; k++) {
												Structure h = g.Data[k] as Structure;
												if (h == null) {
													IO.ReportError(fileName, "Unexpected inlined argument encountered in MeshMaterialList in Mesh");
													return false;
												} else if (h.Name != "Material") {
													IO.ReportError(fileName, "Material template expected in MeshMaterialList in Mesh");
													return false;
												} else {
													/*
													 * material
													 */
													if (h.Data.Length < 4) {
														IO.ReportError(fileName, "Material is expected to have at least 4 arguments in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(h.Data[0] is Structure)) {
														IO.ReportError(fileName, "faceColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(h.Data[1] is double)) {
														IO.ReportError(fileName, "power is expected to be a float in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(h.Data[2] is Structure)) {
														IO.ReportError(fileName, "specularColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(h.Data[3] is Structure)) {
														IO.ReportError(fileName, "emissiveColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh");
														return false;
													}
													Structure faceColor = (Structure)h.Data[0];
													Structure specularColor = (Structure)h.Data[2];
													Structure emissiveColor = (Structure)h.Data[3];
													double red, green, blue, alpha;
													/*
													 * collect face color
													 */
													if (faceColor.Name != "ColorRGBA") {
														IO.ReportError(fileName, "faceColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh");
														return false;
													} else if (faceColor.Data.Length != 4) {
														IO.ReportError(fileName, "faceColor is expected to have 4 arguments in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(faceColor.Data[0] is double)) {
														IO.ReportError(fileName, "red is expected to be a float in faceColor in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(faceColor.Data[1] is double)) {
														IO.ReportError(fileName, "green is expected to be a float in faceColor in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(faceColor.Data[2] is double)) {
														IO.ReportError(fileName, "blue is expected to be a float in faceColor in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(faceColor.Data[3] is double)) {
														IO.ReportError(fileName, "alpha is expected to be a float in faceColor in Material in MeshMaterialList in Mesh");
														return false;
													}
													red = (double)faceColor.Data[0];
													green = (double)faceColor.Data[1];
													blue = (double)faceColor.Data[2];
													alpha = (double)faceColor.Data[3];
													if (red < 0.0 | red > 1.0) {
														IO.ReportError(fileName, "red is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh");
														red = red < 0.5 ? 0.0 : 1.0;
													}
													if (green < 0.0 | green > 1.0) {
														IO.ReportError(fileName, "green is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh");
														green = green < 0.5 ? 0.0 : 1.0;
													}
													if (blue < 0.0 | blue > 1.0) {
														IO.ReportError(fileName, "blue is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh");
														blue = blue < 0.5 ? 0.0 : 1.0;
													}
													if (alpha < 0.0 | alpha > 1.0) {
														IO.ReportError(fileName, "alpha is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh");
														alpha = alpha < 0.5 ? 0.0 : 1.0;
													}
													materials[materialIndex].FaceColor = new OpenBveApi.Color.ColorRGBA((float)red, (float)green, (float)blue, (float)alpha);
													/*
													 * collect specular color
													 */
													if (specularColor.Name != "ColorRGB") {
														IO.ReportError(fileName, "specularColor is expected to be a ColorRGB in Material in MeshMaterialList in Mesh");
														return false;
													} else if (specularColor.Data.Length != 3) {
														IO.ReportError(fileName, "specularColor is expected to have 3 arguments in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(specularColor.Data[0] is double)) {
														IO.ReportError(fileName, "red is expected to be a float in specularColor in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(specularColor.Data[1] is double)) {
														IO.ReportError(fileName, "green is expected to be a float in specularColor in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(specularColor.Data[2] is double)) {
														IO.ReportError(fileName, "blue is expected to be a float in specularColor in Material in MeshMaterialList in Mesh");
														return false;
													}
													red = (double)specularColor.Data[0];
													green = (double)specularColor.Data[1];
													blue = (double)specularColor.Data[2];
													if (red < 0.0 | red > 1.0) {
														IO.ReportError(fileName, "red is expected to be in the range from 0.0 to 1.0 in specularColor in Material in MeshMaterialList in Mesh");
														red = red < 0.5 ? 0.0 : 1.0;
													}
													if (green < 0.0 | green > 1.0) {
														IO.ReportError(fileName, "green is expected to be in the range from 0.0 to 1.0 in specularColor in Material in MeshMaterialList in Mesh");
														green = green < 0.5 ? 0.0 : 1.0;
													}
													if (blue < 0.0 | blue > 1.0) {
														IO.ReportError(fileName, "blue is expected to be in the range from 0.0 to 1.0 in specularColor in Material in MeshMaterialList in Mesh");
														blue = blue < 0.5 ? 0.0 : 1.0;
													}
													materials[materialIndex].SpecularColor = new OpenBveApi.Color.ColorRGB((float)red, (float)green, (float)blue);
													/*
													 * collect emissive color
													 */
													if (emissiveColor.Name != "ColorRGB") {
														IO.ReportError(fileName, "emissiveColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh");
														return false;
													} else if (emissiveColor.Data.Length != 3) {
														IO.ReportError(fileName, "emissiveColor is expected to have 3 arguments in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(emissiveColor.Data[0] is double)) {
														IO.ReportError(fileName, "red is expected to be a float in emissiveColor in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(emissiveColor.Data[1] is double)) {
														IO.ReportError(fileName, "green is expected to be a float in emissiveColor in Material in MeshMaterialList in Mesh");
														return false;
													} else if (!(emissiveColor.Data[2] is double)) {
														IO.ReportError(fileName, "blue is expected to be a float in emissiveColor in Material in MeshMaterialList in Mesh");
														return false;
													}
													red = (double)emissiveColor.Data[0];
													green = (double)emissiveColor.Data[1];
													blue = (double)emissiveColor.Data[2];
													if (red < 0.0 | red > 1.0) {
														IO.ReportError(fileName, "red is expected to be in the range from 0.0 to 1.0 in emissiveColor in Material in MeshMaterialList in Mesh");
														red = red < 0.5 ? 0.0 : 1.0;
													}
													if (green < 0.0 | green > 1.0) {
														IO.ReportError(fileName, "green is expected to be in the range from 0.0 to 1.0 in emissiveColor in Material in MeshMaterialList in Mesh");
														green = green < 0.5 ? 0.0 : 1.0;
													}
													if (blue < 0.0 | blue > 1.0) {
														IO.ReportError(fileName, "blue is expected to be in the range from 0.0 to 1.0 in emissiveColor in Material in MeshMaterialList in Mesh");
														blue = blue < 0.5 ? 0.0 : 1.0;
													}
													materials[materialIndex].EmissiveColor = new OpenBveApi.Color.ColorRGB((float)red, (float)green, (float)blue);
													/*
													 * collect additional templates
													 */
													for (int l = 4; l < h.Data.Length; l++) {
														Structure e = h.Data[l] as Structure;
														if (e == null) {
															IO.ReportError(fileName, "Unexpected inlined argument encountered in Material in MeshMaterialList in Mesh");
															return false;
														}
														switch (e.Name) {
															case "TextureFilename":
																{
																	/*
																	 * texturefilename
																	 */
																	if (e.Data.Length != 1) {
																		IO.ReportError(fileName, "filename is expected to have 1 argument in TextureFilename in Material in MeshMaterialList in Mesh");
																		return false;
																	} else if (!(e.Data[0] is string)) {
																		IO.ReportError(fileName, "filename is expected to be a string in TextureFilename in Material in MeshMaterialList in Mesh");
																		return false;
																	}
																	string filename = (string)e.Data[0];
																	materials[materialIndex].TextureFilename = filename;
																} break;
															default:
																/*
																 * unknown
																 */
																IO.ReportError(fileName, "Unsupported template " + e.Name + " encountered in MeshMaterialList in Mesh");
																break;
														}
													}
													/*
													 * finish
													 */
													materialIndex++;
												}
											}
											if (materialIndex != mn + nMaterials) {
												IO.ReportError(fileName, "nMaterials does not match the number of Material templates encountered in Material in MeshMaterialList in Mesh");
												return false;
											}
											/*
											 * assign materials
											 */
											for (int k = 0; k < nFaceIndexes; k++) {
												faceMaterials[k] = faceIndexes[k];
											}
											if (nMaterials != 0) {
												for (int k = 0; k < nFaces; k++) {
													if (faceMaterials[k] == -1) {
														faceMaterials[k] = 0;
													}
												}
											}
											/*
											 * assign reflective colors to the vertices within each face which uses each material
											 */
											for (int currentMaterial = 0; currentMaterial < materials.Length; currentMaterial++) {
												for (int faceIndex = 0; faceIndex < faces.Length; faceIndex++) {
													if (faceMaterials[faceIndex] == currentMaterial) {
														foreach (int vertex in faces[faceIndex]) {
															vertices[vertex].ReflectiveColor = materials[currentMaterial].FaceColor;
														}
													}
												}
											}
										} break;
									case "MeshTextureCoords":
										{
											/*
											 * meshtexturecoords
											 */
											if (g.Data.Length != 2) {
												IO.ReportError(fileName, "MeshTextureCoords is expected to have 2 arguments in Mesh");
												return false;
											} else if (!(g.Data[0] is int)) {
												IO.ReportError(fileName, "nTextureCoords is expected to be a DWORD in MeshTextureCoords in Mesh");
												return false;
											} else if (!(g.Data[1] is Structure[])) {
												IO.ReportError(fileName, "textureCoords[nTextureCoords] is expected to be a Coords2d array in MeshTextureCoords in Mesh");
												return false;
											}
											int nTextureCoords = (int)g.Data[0];
											Structure[] textureCoords = (Structure[])g.Data[1];
											if (nTextureCoords < 0 | nTextureCoords > nVertices) {
												IO.ReportError(fileName, "nTextureCoords does not reference valid vertices in MeshTextureCoords in Mesh");
												return false;
											}
											for (int k = 0; k < nTextureCoords; k++) {
												if (textureCoords[k].Name != "Coords2d") {
													IO.ReportError(fileName, "textureCoords[" + k.ToString(culture) + "] is expected to be a Coords2d in MeshTextureCoords in Mesh");
													return false;
												} else if (textureCoords[k].Data.Length != 2) {
													IO.ReportError(fileName, "textureCoords[" + k.ToString(culture) + "] is expected to have 2 arguments in MeshTextureCoords in Mesh");
													return false;
												} else if (!(textureCoords[k].Data[0] is double)) {
													IO.ReportError(fileName, "u is expected to be a float in textureCoords[" + k.ToString(culture) + "] in MeshTextureCoords in Mesh");
													return false;
												} else if (!(textureCoords[k].Data[1] is double)) {
													IO.ReportError(fileName, "v is expected to be a float in textureCoords[" + k.ToString(culture) + "] in MeshTextureCoords in Mesh");
													return false;
												}
												double u = (double)textureCoords[k].Data[0];
												double v = (double)textureCoords[k].Data[1];
												vertices[k].TextureCoordinates = new OpenBveApi.Math.Vector2(u, v);
											}
										} break;
									case "MeshNormals":
										{
											/*
											 * meshnormals
											 */
											if (g.Data.Length != 4) {
												IO.ReportError(fileName, "MeshNormals is expected to have 4 arguments in Mesh");
												return false;
											} else if (!(g.Data[0] is int)) {
												IO.ReportError(fileName, "nNormals is expected to be a DWORD in MeshNormals in Mesh");
												return false;
											} else if (!(g.Data[1] is Structure[])) {
												IO.ReportError(fileName, "normals is expected to be a Vector array in MeshNormals in Mesh");
												return false;
											} else if (!(g.Data[2] is int)) {
												IO.ReportError(fileName, "nFaceNormals is expected to be a DWORD in MeshNormals in Mesh");
												return false;
											} else if (!(g.Data[3] is Structure[])) {
												IO.ReportError(fileName, "faceNormals is expected to be a MeshFace array in MeshNormals in Mesh");
												return false;
											}
											int nNormals = (int)g.Data[0];
											if (nNormals < 0) {
												IO.ReportError(fileName, "nNormals is expected to be non-negative in MeshNormals in Mesh");
												return false;
											}
											Structure[] sNormals = (Structure[])g.Data[1];
											if (nNormals != sNormals.Length) {
												IO.ReportError(fileName, "nNormals does not match with the length of array normals in MeshNormals in Mesh");
												return false;
											}
											int nFaceNormals = (int)g.Data[2];
											if (nFaceNormals < 0 | nFaceNormals > nFaces) {
												IO.ReportError(fileName, "nNormals does not reference valid vertices in MeshNormals in Mesh");
												return false;
											}
											Structure[] sFaceNormals = (Structure[])g.Data[3];
											if (nFaceNormals != sFaceNormals.Length) {
												IO.ReportError(fileName, "nFaceNormals does not match with the length of array faceNormals in MeshNormals in Mesh");
												return false;
											}
											/*
											 * collect normals
											 */
											OpenBveApi.Math.Vector3[] normals = new OpenBveApi.Math.Vector3[nNormals];
											for (int k = 0; k < nNormals; k++) {
												if (sNormals[k].Name != "Vector") {
													IO.ReportError(fileName, "normals[" + k.ToString(culture) + "] is expected to be of template Vertex in MeshNormals in Mesh");
													return false;
												} else if (sNormals[k].Data.Length != 3) {
													IO.ReportError(fileName, "normals[" + k.ToString(culture) + "] is expected to have 3 arguments in MeshNormals in Mesh");
													return false;
												} else if (!(sNormals[k].Data[0] is double)) {
													IO.ReportError(fileName, "x is expected to be a float in normals[" + k.ToString(culture) + "] in MeshNormals in Mesh");
													return false;
												} else if (!(sNormals[k].Data[1] is double)) {
													IO.ReportError(fileName, "y is expected to be a float in normals[" + k.ToString(culture) + " ]in MeshNormals in Mesh");
													return false;
												} else if (!(sNormals[k].Data[2] is double)) {
													IO.ReportError(fileName, "z is expected to be a float in normals[" + k.ToString(culture) + "] in MeshNormals in Mesh");
													return false;
												}
												double x = (double)sNormals[k].Data[0];
												double y = (double)sNormals[k].Data[1];
												double z = (double)sNormals[k].Data[2];
												OpenBveApi.Math.Vector3 normal = new OpenBveApi.Math.Vector3(x, y, z);
												if (!normal.IsNullVector()) {
													normal.Normalize();
												}
												normals[k] = normal;
											}
											/*
											 * collect faces
											 */
											for (int k = 0; k < nFaceNormals; k++) {
												if (sFaceNormals[k].Name != "MeshFace") {
													IO.ReportError(fileName, "faceNormals[" + k.ToString(culture) + "] is expected to be of template MeshFace in MeshNormals in Mesh");
													return false;
												} else if (sFaceNormals[k].Data.Length != 2) {
													IO.ReportError(fileName, "faceNormals[" + k.ToString(culture) + "] is expected to have 2 arguments in MeshNormals in Mesh");
													return false;
												} else if (!(sFaceNormals[k].Data[0] is int)) {
													IO.ReportError(fileName, "nFaceVertexIndices is expected to be a DWORD in faceNormals[" + k.ToString(culture) + "] in MeshNormals in Mesh");
													return false;
												} else if (!(sFaceNormals[k].Data[1] is int[])) {
													IO.ReportError(fileName, "faceVertexIndices[nFaceVertexIndices] is expected to be a DWORD array in faceNormals[" + k.ToString(culture) + "] in MeshNormals in Mesh");
													return false;
												}
												int nFaceVertexIndices = (int)sFaceNormals[k].Data[0];
												if (nFaceVertexIndices < 0 | nFaceVertexIndices > faces[k].Length) {
													IO.ReportError(fileName, "nFaceVertexIndices does not reference a valid vertex in MeshFace in MeshNormals in Mesh");
													return false;
												}
												int[] faceVertexIndices = (int[])sFaceNormals[k].Data[1];
												if (nFaceVertexIndices != faceVertexIndices.Length) {
													IO.ReportError(fileName, "nFaceVertexIndices does not match with the length of array faceVertexIndices in faceNormals[" + k.ToString(culture) + "] in MeshFace in MeshNormals in Mesh");
													return false;
												}
												for (int l = 0; l < nFaceVertexIndices; l++) {
													if (faceVertexIndices[l] < 0 | faceVertexIndices[l] >= nNormals) {
														IO.ReportError(fileName, "faceVertexIndices[" + l.ToString(culture) + "] does not reference a valid normal in faceNormals[" + k.ToString(culture) + "] in MeshFace in MeshNormals in Mesh");
														return false;
													}
													faceNormals[k][l] = normals[faceVertexIndices[l]];
												}
											}
										} break;
									default:
										/*
										 * unknown
										 */
										IO.ReportError(fileName, "Unsupported template " + g.Name + " encountered in Mesh");
										break;
								}
							}
							/*
							 * default material
							 */
							if (materials.Length == 0) {
								materials = new Material[1];
								materials[0].FaceColor = new OpenBveApi.Color.ColorRGBA(1.0f, 1.0f, 1.0f, 1.0f);
								materials[0].EmissiveColor = new OpenBveApi.Color.ColorRGB(0.0f, 0.0f, 0.0f);
								materials[0].SpecularColor = new OpenBveApi.Color.ColorRGB(0.0f, 0.0f, 0.0f);
								materials[0].TextureFilename = null;
								for (int j = 0; j < nFaces; j++) {
									faceMaterials[j] = 0;
								}
							}
							/*
							 * eliminate non-visible faces and associated properties
							 */
							int[][] visibleFaces = new int[nFaces][];
							int newFaceCount = 0;
							for (int currentFace = 0; currentFace < faces.Length; currentFace++) {
								if (faces[currentFace].Length >= 3) {
									visibleFaces[newFaceCount] = faces[currentFace];
									faceNormals[newFaceCount] = faceNormals[currentFace];
									faceMaterials[newFaceCount] = faceMaterials[currentFace];
									newFaceCount++;
								}
							}
							Array.Resize<int[]>(ref visibleFaces, newFaceCount);
							Array.Resize<OpenBveApi.Math.Vector3[]>(ref faceNormals, newFaceCount);
							Array.Resize<int>(ref faceMaterials, newFaceCount);
							/*
							 * prepare compatible mesh for data
							 */
							int vertexCount = 0;
							for (int j = 0; j < visibleFaces.Length; j++) {
								foreach (int face in visibleFaces[j]) {
									vertexCount++;
								}
							}
							Array.Resize<OpenBveApi.Geometry.Vertex>(ref compatibleMesh.Vertices, vertexCount);
							Array.Resize<OpenBveApi.Geometry.Face>(ref compatibleMesh.Faces, visibleFaces.Length);
							Array.Resize<OpenBveApi.Geometry.Material>(ref compatibleMesh.Materials, materials.Length);
							/*
							 * apply vertex data
							 */
							vertexCount = 0;
							for (int faceIndex = 0; faceIndex < visibleFaces.Length; faceIndex++) {
								compatibleMesh.Faces[faceIndex].Vertices = new int[visibleFaces[faceIndex].Length];
								/*
								 *  go through each list of vertices in face and get vertex data
								 */
								for (int vertexIndex = 0; vertexIndex < visibleFaces[faceIndex].Length; vertexIndex++) {
									OpenBveApi.Geometry.Vertex newVertex = new OpenBveApi.Geometry.Vertex();
									newVertex.SpatialCoordinates = vertices[visibleFaces[faceIndex][vertexIndex]].SpatialCoordinates;
									newVertex.ReflectiveColor = vertices[visibleFaces[faceIndex][vertexIndex]].ReflectiveColor;
									newVertex.TextureCoordinates = vertices[visibleFaces[faceIndex][vertexIndex]].TextureCoordinates;
									newVertex.Tag = visibleFaces[faceIndex][vertexIndex];
									/*
									 * check for null vector normal and create face derived normal if necessary
									 */
									if (!faceNormals[faceIndex][vertexIndex].IsNullVector()) {
										newVertex.Normal = faceNormals[faceIndex][vertexIndex];
									} else if (visibleFaces[faceIndex].Length >= 3) {
										OpenBveApi.Math.Vector3 vertexA = vertices[visibleFaces[faceIndex][0]].SpatialCoordinates;
										OpenBveApi.Math.Vector3 vertexB = vertices[visibleFaces[faceIndex][1]].SpatialCoordinates;
										OpenBveApi.Math.Vector3 vertexC = vertices[visibleFaces[faceIndex][2]].SpatialCoordinates;
										if (!OpenBveApi.Math.Vector3.CreateNormal(vertexA, vertexB, vertexC, out newVertex.Normal)) {
											newVertex.Normal = OpenBveApi.Math.Vector3.Up;
										}
									}
									/*
									 * assign vertex to mesh and finalise faces
									 */
									compatibleMesh.Vertices[vertexCount] = newVertex;
									compatibleMesh.Faces[faceIndex].Vertices[vertexIndex] = vertexCount;
									compatibleMesh.Faces[faceIndex].Type = OpenBveApi.Geometry.FaceType.Polygon;
									vertexCount++;
								}
							}
							/*
							 * assign texture wrap mode, texture handles, X format transparent color, and emissive colors
							 */
							for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++) {
								bool emissive = materials[materialIndex].EmissiveColor.R != 0 | materials[materialIndex].EmissiveColor.G != 0 | materials[materialIndex].EmissiveColor.B != 0;
								if (materials[materialIndex].TextureFilename != null) {
									/*
									 * default to clamp to edge and override if necessary
									 */
									OpenBveApi.Texture.TextureWrapMode horizontalWrapMode, verticalWrapMode;
									horizontalWrapMode = OpenBveApi.Texture.TextureWrapMode.ClampToEdge;
									verticalWrapMode = OpenBveApi.Texture.TextureWrapMode.ClampToEdge;
									for (int j = 0; j < compatibleMesh.Faces.Length; j++) {
										for (int k = 0; k < visibleFaces[j].Length; k++) {
											if (vertices[visibleFaces[j][k]].TextureCoordinates.X < 0.0 | vertices[visibleFaces[j][k]].TextureCoordinates.X > 1.0) {
												horizontalWrapMode = OpenBveApi.Texture.TextureWrapMode.Repeat;
											}
											if (vertices[visibleFaces[j][k]].TextureCoordinates.Y < 0.0 | vertices[visibleFaces[j][k]].TextureCoordinates.Y > 1.0) {
												verticalWrapMode = OpenBveApi.Texture.TextureWrapMode.Repeat;
											}
										}
									}
									/*
									 * register texture and get handle
									 */
									string folder = System.IO.Path.GetDirectoryName(fileName);
									string textureFile = OpenBveApi.Path.CombineFile(folder, materials[materialIndex].TextureFilename);
									OpenBveApi.Path.PathReference path = new OpenBveApi.Path.FileReference(textureFile);
									OpenBveApi.General.Origin origin = new OpenBveApi.General.Origin(path, null, encoding);
									OpenBveApi.Color.TransparentColor transparentColor = new OpenBveApi.Color.TransparentColor(0, 0, 0, true);
									OpenBveApi.Texture.TextureParameters parameters = new OpenBveApi.Texture.TextureParameters(transparentColor, horizontalWrapMode, verticalWrapMode, null);
									OpenBveApi.Texture.TextureHandle handle;
									if (Interfaces.Host.RegisterTexture(origin, parameters, out handle) == OpenBveApi.General.Result.Successful) {
										compatibleMesh.Materials[materialIndex].DaytimeTexture = handle;
									} else {
										compatibleMesh.Materials[materialIndex].DaytimeTexture = null;
									}
									if (!System.IO.File.Exists(textureFile)) {
										IO.ReportError(fileName, materialIndex.ToString(), textureFile.ToString());
									}
								}
								compatibleMesh.Materials[materialIndex].EmissiveColor = materials[materialIndex].EmissiveColor;
								compatibleMesh.Materials[materialIndex].NighttimeTexture = null;
							}
							/*
							 * assign material reference to face
							 */
							for (int j = 0; j < faceMaterials.Length; j++) {
								compatibleMesh.Faces[j].Material = faceMaterials[j];
							}
							break;
						}
					case "Header":
						break;
					default:
						/*
						 * unknown
						 */
						IO.ReportError(fileName, "Unsupported template " + f.Name + " encountered");
						break;
				}
			}
			/*
			 * return
			 */
			obj = compatibleMesh;
			return true;
		}
	}
}