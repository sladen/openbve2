using System;

namespace OpenBveApi {
	/// <summary>Provides geometric structures.</summary>
	public static class Geometry {
		
		// objects
		/// <summary>Represents a geometric object. This is the ultimate base class for all objects.</summary>
		public abstract class GenericObject {
			// functions
			/// <summary>Translates the object.</summary>
			/// <param name="offset">The offset by which to translate.</param>
			public abstract void Translate(Math.Vector3 offset);
			/// <summary>Translates the object.</summary>
			/// <param name="orientation">The orientation along which to translate.</param>
			/// <param name="offset">The offset relative to the orientation by which to translate.</param>
			public abstract void Translate(Math.Orientation3 orientation, Math.Vector3 offset);
			/// <summary>Rotates the object.</summary>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle by which to rotate.</param>
			/// <param name="sineOfAngle">The sine of the angle by which to rotate.</param>
			public abstract void Rotate(Math.Vector3 direction, double cosineOfAngle, double sineOfAngle);
			/// <summary>Rotates the object from the default orientation into a specified orientation.</summary>
			/// <param name="orientation">The orientation.</param>
			/// <remarks>The default orientation is X = {1, 0, 0), Y = {0, 1, 0} and Z = {0, 0, 1}.</remarks>
			public abstract void Rotate(Math.Orientation3 orientation);
			/// <summary>Scales the object.</summary>
			/// <param name="factor">The factor by which to scale.</param>
			public abstract void Scale(Math.Vector3 factor);
			/// <summary>Creates a deep copy of this instance.</summary>
			/// <returns>The deep copy of this instance.</returns>
			public abstract GenericObject Clone();
		}
		
		// static objects
		/// <summary>Represents a static object. This is the base class for all static objects.</summary>
		public abstract class StaticObject : GenericObject { }
		
		// animated objects
		/// <summary>Represents an animated object. This is the base class for all animated objects.</summary>
		public abstract class AnimatedObject : GenericObject { }
		
		// vertex
		/// <summary>Represents a vertex.</summary>
		public struct Vertex {
			// members
			/// <summary>The spatial coordinates.</summary>
			public Math.Vector3 SpatialCoordinates;
			/// <summary>The texture coordinates.</summary>
			public Math.Vector2 TextureCoordinates;
			/// <summary>The surface normal.</summary>
			public Math.Vector3 Normal;
			/// <summary>The reflective color.</summary>
			public Color.ColorRGBA ReflectiveColor;
			/// <summary>A field to convey intermediate information.</summary>
			public int Tag;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="spatialCoordinates">The spatial coordinates.</param>
			/// <param name="normal">The surface normal.</param>
			/// <param name="textureCoordinates">The texture coordinates.</param>
			/// <param name="reflectiveColor">The reflective color.</param>
			/// <param name="tag">A field to convey intermediate information.</param>
			public Vertex(Math.Vector3 spatialCoordinates, Math.Vector2 textureCoordinates, Math.Vector3 normal, Color.ColorRGBA reflectiveColor, int tag) {
				this.SpatialCoordinates = spatialCoordinates;
				this.TextureCoordinates = textureCoordinates;
				this.Normal = normal;
				this.ReflectiveColor = reflectiveColor;
				this.Tag = tag;
			}
			// instance functions
			/// <summary>Creates a deep copy of this instance.</summary>
			/// <returns>The deep copy of this instance.</returns>
			public Vertex Clone() {
				return new Vertex(this.SpatialCoordinates, this.TextureCoordinates, this.Normal, this.ReflectiveColor, this.Tag);
			}
			// operators
			public static bool operator ==(Vertex a, Vertex b) {
				if (a.SpatialCoordinates != b.SpatialCoordinates) return false;
				if (a.TextureCoordinates != b.TextureCoordinates) return false;
				if (a.Normal != b.Normal) return false;
				if (a.ReflectiveColor != b.ReflectiveColor) return false;
				return true;
			}
			public static bool operator !=(Vertex a, Vertex b) {
				if (a.SpatialCoordinates != b.SpatialCoordinates) return true;
				if (a.TextureCoordinates != b.TextureCoordinates) return true;
				if (a.Normal != b.Normal) return true;
				if (a.ReflectiveColor != b.ReflectiveColor) return true;
				return false;
			}
		}
		
		// blend mode
		/// <summary>Represents the way a face is blended onto the screen.</summary>
		public enum BlendMode {
			/// <summary>The face is blended normally, i.e. face pixels replace screen pixels.</summary>
			Normal = 0,
			/// <summary>The face is blended additively, i.e. face pixels are added to screen pixels.</summary>
			Additive = 1
		}
		
		// material
		/// <summary>Represents a material.</summary>
		public struct Material {
			// members
			/// <summary>The emissive color.</summary>
			public Color.ColorRGB EmissiveColor;
			/// <summary>A handle to the daytime texture.</summary>
			public Texture.TextureHandle DaytimeTexture;
			/// <summary>A handle to the nighttime texture.</summary>
			/// <remarks>If a nighttime texture is given, a daytime texture must also be given.</remarks>
			public Texture.TextureHandle NighttimeTexture;
			/// <summary>The blend type.</summary>
			public BlendMode BlendMode;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="emissiveColor">The emissive color.</param>
			/// <param name="daytimeTexture">A handle to the daytime texture.</param>
			/// <param name="nighttimeTexture">A handle to the nighttime texture.</param>
			/// <param name="blendMode">The blend type.</param>
			public Material(Color.ColorRGB emissiveColor, Texture.TextureHandle daytimeTexture, Texture.TextureHandle nighttimeTexture, BlendMode blendMode) {
				this.EmissiveColor = emissiveColor;
				this.DaytimeTexture = daytimeTexture;
				this.NighttimeTexture = nighttimeTexture;
				this.BlendMode = blendMode;
			}
			// instance functions
			/// <summary>Creates a deep copy of this instance.</summary>
			/// <returns>The deep copy of this instance.</returns>
			public Material Clone() {
				return new Material(this.EmissiveColor, this.DaytimeTexture, this.NighttimeTexture, this.BlendMode);
			}
			// comparisons
			public static bool operator ==(Material a, Material b) {
				if (a.EmissiveColor != b.EmissiveColor) return false;
				if (a.DaytimeTexture != b.DaytimeTexture) return false;
				if (a.NighttimeTexture != b.NighttimeTexture) return false;
				if (a.BlendMode != b.BlendMode) return false;
				return true;
			}
			public static bool operator !=(Material a, Material b) {
				if (a.EmissiveColor != b.EmissiveColor) return true;
				if (a.DaytimeTexture != b.DaytimeTexture) return true;
				if (a.NighttimeTexture != b.NighttimeTexture) return true;
				if (a.BlendMode != b.BlendMode) return true;
				return false;
			}
		}
		
		// face type
		/// <summary>Specifies how vertices in a face are organized.</summary>
		/// <remarks>If a face is not fully opaque, it should be organized as a Polygon.</remarks>
		public enum FaceType {
			/// <summary>Represents a series of triangles. The vertex count must be a multiple of 3.</summary>
			Triangles = 0,
			/// <summary>Represents a triangle strip. There must be at least 3 vertices.</summary>
			TriangleStrip = 1,
			/// <summary>Represents a triangle fan. There must be at least 3 vertices.</summary>
			TriangleFan = 2,
			/// <summary>Represents a series of quads. The vertex count must be a multiple of 4.</summary>
			Quads = 3,
			/// <summary>Represents a quad strip. The vertex count must be a multiple of 2. There must be at least 4 vertices.</summary>
			QuadStrip = 4,
			/// <summary>Represents a polygon. There must be at least 3 vertices.</summary>
			Polygon = 5
		}
		
		// face
		/// <summary>Represents a face.</summary>
		public struct Face {
			// members
			/// <summary>The type of the face, describing how vertices are organized.</summary>
			/// <remarks>If a face is not fully opaque, it should be organized as a Polygon.</remarks>
			public FaceType Type;
			/// <summary>A list of references to vertices of the underlying mesh.</summary>
			/// <remarks>The order of the vertices depends on the type of face.</remarks>
			public int[] Vertices;
			/// <summary>A reference to a material of the underlying mesh.</summary>
			public int Material;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="type">The type of the face, describing how vertices are organized.</param>
			/// <param name="vertices">A list of references to vertices of the underlying mesh.</param>
			/// <param name="material">A reference to a material of the underlying mesh.</param>
			public Face(FaceType type, int[] vertices, int material) {
				this.Type = type;
				this.Vertices = vertices;
				this.Material = material;
			}
			// instance functions
			/// <summary>Flips the front and back of the face.</summary>
			public void Flip() {
				if (this.Type == FaceType.TriangleFan) {
					// triangle fan
					for (int j = 1; j < (this.Vertices.Length - 1 >> 1); j++) {
						int k = this.Vertices.Length - j;
						int vertex = this.Vertices[j];
						this.Vertices[j] = this.Vertices[k];
						this.Vertices[k] = vertex;
					}
				} else if (this.Type == FaceType.QuadStrip) {
					// quad strip
					for (int j = 0; j < this.Vertices.Length; j += 2) {
						int vertex = this.Vertices[j];
						this.Vertices[j] = this.Vertices[j + 1];
						this.Vertices[j] = vertex;
					}
				} else {
					// others
					for (int j = 0; j < (this.Vertices.Length >> 1); j++) {
						int k = this.Vertices.Length - j - 1;
						int vertex = this.Vertices[j];
						this.Vertices[j] = this.Vertices[k];
						this.Vertices[k] = vertex;
					}
				}
			}
			/// <summary>Creates a deep copy of this instance.</summary>
			/// <returns>The deep copy of this instance.</returns>
			public Face Clone() {
				int[] vertices = new int[this.Vertices.Length];
				for (int i = 0; i < this.Vertices.Length; i++) {
					vertices[i] = this.Vertices[i];
				}
				return new Face(this.Type, vertices, this.Material);
			}
		}
		
		// mesh
		/// <summary>Represents a face-vertex mesh.</summary>
		public class FaceVertexMesh : StaticObject {
			// members
			/// <summary>A list of vertices.</summary>
			public Vertex[] Vertices;
			/// <summary>A list of materials.</summary>
			public Material[] Materials;
			/// <summary>A list of faces.</summary>
			public Face[] Faces;
			// constructors
			/// <summary>Creates an empty mesh.</summary>
			public FaceVertexMesh() {
				this.Vertices = new Vertex[] { };
				this.Materials = new Material[] { };
				this.Faces = new Face[] { };
			}
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="vertices">A list of vertices.</param>
			/// <param name="materials">A list of materials.</param>
			/// <param name="faces">A list of faces.</param>
			public FaceVertexMesh(Vertex[] vertices, Material[] materials, Face[] faces) {
				this.Vertices = vertices;
				this.Materials = materials;
				this.Faces = faces;
			}
			// instance functions
			/// <summary>Translates the mesh.</summary>
			/// <param name="offset">The offset by which to translate.</param>
			public override void Translate(Math.Vector3 offset) {
				for (int i = 0; i < this.Vertices.Length; i++) {
					this.Vertices[i].SpatialCoordinates += offset;
				}
			}
			/// <summary>Translates the object.</summary>
			/// <param name="orientation">The orientation along which to translate.</param>
			/// <param name="offset">The offset relative to the orientation by which to translate.</param>
			public override void Translate(Math.Orientation3 orientation, Math.Vector3 offset) {
				double x = orientation.X.X * offset.X + orientation.Y.X * offset.Y + orientation.Z.X * offset.Z;
				double y = orientation.X.Y * offset.X + orientation.Y.Y * offset.Y + orientation.Z.Y * offset.Z;
				double z = orientation.X.Z * offset.X + orientation.Y.Z * offset.Y + orientation.Z.Z * offset.Z;
				for (int i = 0; i < this.Vertices.Length; i++) {
					this.Vertices[i].SpatialCoordinates.X += x;
					this.Vertices[i].SpatialCoordinates.Y += y;
					this.Vertices[i].SpatialCoordinates.Z += z;
				}
			}
			/// <summary>Rotates the mesh.</summary>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle by which to rotate.</param>
			/// <param name="sineOfAngle">The sine of the angle by which to rotate.</param>
			public override void Rotate(Math.Vector3 direction, double cosineOfAngle, double sineOfAngle) {
				for (int i = 0; i < this.Vertices.Length; i++) {
					this.Vertices[i].SpatialCoordinates.Rotate(direction, cosineOfAngle, sineOfAngle);
					this.Vertices[i].Normal.Rotate(direction, cosineOfAngle, sineOfAngle);
				}
			}
			/// <summary>Rotates the mesh from the default orientation into a specified orientation.</summary>
			/// <param name="orientation">The orientation.</param>
			/// <remarks>The default orientation is X = {1, 0, 0), Y = {0, 1, 0} and Z = {0, 0, 1}.</remarks>
			public override void Rotate(Math.Orientation3 orientation) {
				for (int i = 0; i < this.Vertices.Length; i++) {
					this.Vertices[i].SpatialCoordinates.Rotate(orientation);
					this.Vertices[i].Normal.Rotate(orientation);
				}
			}
			/// <summary>Scales the object.</summary>
			/// <param name="factor">The factor by which to scale.</param>
			public override void Scale(Math.Vector3 factor) {
				double inverseFactorX = 1.0 / factor.X;
				double inverseFactorY = 1.0 / factor.Y;
				double inverseFactorZ = 1.0 / factor.Z;
				double inverseFactorSquaredX = inverseFactorX * inverseFactorX;
				double inverseFactorSquaredY = inverseFactorY * inverseFactorY;
				double inverseFactorSquaredZ = inverseFactorZ * inverseFactorZ;
				for (int i = 0; i < this.Vertices.Length; i++) {
					this.Vertices[i].SpatialCoordinates *= factor;
					double normalSquaredX = this.Vertices[i].Normal.X * this.Vertices[i].Normal.X;
					double normalSquaredY = this.Vertices[i].Normal.Y * this.Vertices[i].Normal.Y;
					double normalSquaredZ = this.Vertices[i].Normal.Z * this.Vertices[i].Normal.Z;
					double norm = normalSquaredX * inverseFactorSquaredX + normalSquaredY * inverseFactorSquaredY + normalSquaredZ * inverseFactorSquaredZ;
					if (norm != 0.0) {
						double scalar = System.Math.Sqrt((normalSquaredX + normalSquaredY + normalSquaredZ) / norm);
						this.Vertices[i].Normal.X *= inverseFactorX * scalar;
						this.Vertices[i].Normal.Y *= inverseFactorY * scalar;
						this.Vertices[i].Normal.Z *= inverseFactorZ * scalar;
					}
				}
				if (factor.X * factor.Y * factor.Z < 0.0) {
					for (int i = 0; i < this.Faces.Length; i++) {
						this.Faces[i].Flip();
					}
				}
			}
			/// <summary>Adds another mesh to this instance of a mesh.</summary>
			/// <param name="mesh">The mesh to add.</param>
			public void Add(FaceVertexMesh mesh) {
				int vertices = this.Vertices.Length;
				int materials = this.Materials.Length;
				int faces = this.Faces.Length;
				// vertices
				Array.Resize<Vertex>(ref this.Vertices, vertices + mesh.Vertices.Length);
				for (int i = 0; i < mesh.Vertices.Length; i++) {
					this.Vertices[vertices + i] = mesh.Vertices[i];
				}
				// materials
				Array.Resize<Material>(ref this.Materials, materials + mesh.Materials.Length);
				for (int i = 0; i < mesh.Materials.Length; i++) {
					this.Materials[materials + i] = mesh.Materials[i];
				}
				// faces
				Array.Resize<Face>(ref this.Faces, faces + mesh.Faces.Length);
				for (int i = 0; i < mesh.Faces.Length; i++) {
					this.Faces[faces + i].Material = mesh.Faces[i].Material + materials;
					this.Faces[faces + i].Type = mesh.Faces[i].Type;
					this.Faces[faces + i].Vertices = new int[mesh.Faces[i].Vertices.Length];
					for (int j = 0; j < mesh.Faces[i].Vertices.Length; j++) {
						this.Faces[faces + i].Vertices[j] = mesh.Faces[i].Vertices[j] + vertices;
					}
				}
			}
			/// <summary>Creates a deep copy of this instance.</summary>
			/// <returns>The deep copy of this instance.</returns>
			public override GenericObject Clone() {
				Vertex[] vertices = new Vertex[this.Vertices.Length];
				for (int i = 0; i < this.Vertices.Length; i++) {
					vertices[i] = this.Vertices[i].Clone();
				}
				Material[] materials = new Material[this.Materials.Length];
				for (int i = 0; i < materials.Length; i++) {
					materials[i] = this.Materials[i].Clone();
				}
				Face[] faces = new Face[this.Faces.Length];
				for (int i = 0; i < this.Faces.Length; i++) {
					faces[i] = this.Faces[i].Clone();
				}
				return new FaceVertexMesh(vertices, materials, faces);
			}
			/// <summary>Optimizes the object by eliminating unused or duplicate vertices and materials, and optionally, by creating triangle and quad strips.</summary>
			/// <param name="createStrips">Whether to try to create triangle strips and quad strips.</param>
			/// <remarks>This function should generally only be called by the host application, and only if it wishes to permanently store object geometry in the FaceVertexMesh.</remarks>
			public void Optimize(bool createStrips) {
				//return;
				int vertexCount = this.Vertices.Length;
				int materialCount = this.Materials.Length;
				int faceCount = this.Faces.Length;
				/* 
				 * Eliminate duplicate vertices
				 * */
				for (int i = 0; i < vertexCount; i++) {
					for (int j = i + 1; j < vertexCount; j++) {
						if (this.Vertices[j] == this.Vertices[i]) {
							this.Vertices[j] = this.Vertices[vertexCount - 1];
							for (int k = 0; k < faceCount; k++) {
								for (int h = 0; h < this.Faces[k].Vertices.Length; h++) {
									if (this.Faces[k].Vertices[h] == j) {
										this.Faces[k].Vertices[h] = i;
									} else if (this.Faces[k].Vertices[h] == vertexCount - 1) {
										this.Faces[k].Vertices[h] = j;
									}
								}
							}
							vertexCount--;
							j--;
						}
					}
				}
				/* 
				 * Eliminate duplicate materials
				 * */
				for (int i = 0; i < materialCount; i++) {
					for (int j = i + 1; j < materialCount; j++) {
						if (this.Materials[j] == this.Materials[i]) {
							this.Materials[j] = this.Materials[materialCount - 1];
							for (int k = 0; k < faceCount; k++) {
								if (this.Faces[k].Material == j) {
									this.Faces[k].Material = i;
								} else if (this.Faces[k].Material == materialCount - 1) {
									this.Faces[k].Material = j;
								}
							}
							materialCount--;
							j--;
						}
					}
				}
				/*
				 * Eliminate unused vertices and materials.
				 * */
				bool[] vertexUsed = new bool[vertexCount];
				bool[] materialUsed = new bool[materialCount];
				for (int i = 0; i < faceCount; i++) {
					materialUsed[this.Faces[i].Material] = true;
					for (int j = 0; j < this.Faces[i].Vertices.Length; j++) {
						vertexUsed[this.Faces[i].Vertices[j]] = true;
					}
				}
				for (int i = 0; i < vertexCount; i++) {
					if (!vertexUsed[i]) {
						this.Vertices[i] = this.Vertices[vertexCount - 1];
						for (int j = 0; j < faceCount; j++) {
							for (int k = 0; k < this.Faces[j].Vertices.Length; k++) {
								if (this.Faces[j].Vertices[k] == vertexCount - 1) {
									this.Faces[j].Vertices[k] = i;
								}
							}
						}
						vertexCount--;
						i--;
					}
				}
				for (int i = 0; i < materialCount; i++) {
					if (!materialUsed[i]) {
						this.Materials[i] = this.Materials[materialCount - 1];
						for (int j = 0; j < faceCount; j++) {
							if (this.Faces[j].Material == materialCount - 1) {
								this.Faces[j].Material = i;
							}
						}
						materialCount--;
						i--;
					}
				}
				/*
				 * Optimizations regarding creating strips.
				 * */
				if (createStrips) {
					/*
					 * Split multiple triangles from GL_TRIANGLES
					 * and multiple quads from GL_QUADS into
					 * individual faces.
					 * */
					for (int i = 0; i < faceCount; i++) {
						if (this.Faces[i].Type == FaceType.Triangles) {
							if (this.Faces[i].Vertices.Length >= 6) {
								int n = this.Faces[i].Vertices.Length / 3 - 1;
								while (faceCount + n > this.Faces.Length) {
									Array.Resize<Face>(ref this.Faces, this.Faces.Length << 1);
								}
								for (int j = 0; j < n; j++) {
									int offset = 3 * (j + 1);
									int[] vertices = new int[] { this.Faces[i].Vertices[offset], this.Faces[i].Vertices[offset + 1], this.Faces[i].Vertices[offset + 2] };
									this.Faces[faceCount + j] = new Face(FaceType.Triangles, vertices, this.Faces[i].Material);
								}
								this.Faces[i].Vertices = new int[] { this.Faces[i].Vertices[0], this.Faces[i].Vertices[1], this.Faces[i].Vertices[2] };
								faceCount += n;
							}
						} else if (this.Faces[i].Type == FaceType.Quads) {
							if (this.Faces[i].Vertices.Length >= 8) {
								int n = this.Faces[i].Vertices.Length / 4 - 1;
								while (faceCount + n > this.Faces.Length) {
									Array.Resize<Face>(ref this.Faces, this.Faces.Length << 1);
								}
								for (int j = 0; j < n; j++) {
									int offset = 4 * (j + 1);
									int[] vertices = new int[] { this.Faces[i].Vertices[offset], this.Faces[i].Vertices[offset + 1], this.Faces[i].Vertices[offset + 2], this.Faces[i].Vertices[offset + 3] };
									this.Faces[faceCount + j] = new Face(FaceType.Quads, vertices, this.Faces[i].Material);
								}
								this.Faces[i].Vertices = new int[] { this.Faces[i].Vertices[0], this.Faces[i].Vertices[1], this.Faces[i].Vertices[2], this.Faces[i].Vertices[3] };
								faceCount += n;
							}
						}
					}
					/*
					 * Convert all GL_POLYGON with three vertices into GL_TRIANGLES
					 * and all GL_POLYGON with four vertices into GL_QUADS.
					 * */
					for (int i = 0; i < faceCount; i++) {
						if (this.Faces[i].Type == FaceType.Polygon) {
							if (this.Faces[i].Vertices.Length == 3) {
								this.Faces[i].Type = FaceType.Triangles;
							} else if (this.Faces[i].Vertices.Length == 4) {
								this.Faces[i].Type = FaceType.Quads;
							}
						}
					}
					/* 
					 * Create or extend GL_TRIANGLE_STRIP structures.
					 * */
					// TODO
					/* 
					 * Create or extend GL_QUAD_STRIP structures.
					 * */
					while (true) {
						/*
						 * Join GL_QUAD_STRIP structures that have
						 * the same material if they fit together.
						 * */
						for (int i = 0; i < faceCount; i++) {
							if (this.Faces[i].Type == FaceType.QuadStrip) {
								int n = this.Faces[i].Vertices.Length;
								for (int j = i + 1; j < faceCount; j++) {
									if (this.Faces[j].Type == FaceType.QuadStrip) {
										int m = this.Faces[j].Vertices.Length;
										if (this.Faces[i].Material == this.Faces[j].Material) {
											if (this.Faces[i].Vertices[0] == this.Faces[j].Vertices[1] & this.Faces[i].Vertices[1] == this.Faces[j].Vertices[0]) {
												int[] vertices = new int[n + m - 2];
												for (int k = 0; k < n; k++) {
													vertices[k] = this.Faces[i].Vertices[n - k - 1];
												}
												for (int k = 0; k < m - 2; k++) {
													vertices[n + k] = this.Faces[j].Vertices[k + 2];
												}
												this.Faces[i].Vertices = vertices;
												this.Faces[j] = this.Faces[faceCount - 1];
												faceCount--;
												i--;
												break;
											} else if (this.Faces[i].Vertices[0] == this.Faces[j].Vertices[m - 2] & this.Faces[i].Vertices[1] == this.Faces[j].Vertices[m - 1]) {
												int[] vertices = new int[n + m - 2];
												for (int k = 0; k < n; k++) {
													vertices[k] = this.Faces[i].Vertices[n - k - 1];
												}
												for (int k = 0; k < m - 2; k++) {
													vertices[n + k] = this.Faces[j].Vertices[m - k - 3];
												}
												this.Faces[i].Vertices = vertices;
												this.Faces[j] = this.Faces[faceCount - 1];
												faceCount--;
												i--;
												break;
											} else if (this.Faces[i].Vertices[n - 2] == this.Faces[j].Vertices[0] & this.Faces[i].Vertices[n - 1] == this.Faces[j].Vertices[1]) {
												int[] vertices = new int[n + m - 2];
												for (int k = 0; k < n; k++) {
													vertices[k] = this.Faces[i].Vertices[k];
												}
												for (int k = 0; k < m - 2; k++) {
													vertices[n + k] = this.Faces[j].Vertices[k + 2];
												}
												this.Faces[i].Vertices = vertices;
												this.Faces[j] = this.Faces[faceCount - 1];
												faceCount--;
												i--;
												break;
											} else if (this.Faces[i].Vertices[n - 2] == this.Faces[j].Vertices[m - 1] & this.Faces[i].Vertices[n - 1] == this.Faces[j].Vertices[m - 2]) {
												int[] vertices = new int[n + m - 2];
												for (int k = 0; k < n; k++) {
													vertices[k] = this.Faces[i].Vertices[k];
												}
												for (int k = 0; k < m - 2; k++) {
													vertices[n + k] = this.Faces[j].Vertices[m - k - 3];
												}
												this.Faces[i].Vertices = vertices;
												this.Faces[j] = this.Faces[faceCount - 1];
												faceCount--;
												i--;
												break;
											}
										}
									}
								}
							}
						}
						/*
						 * 
						 * For each GL_QUADS, try to find a matching
						 * GL_QUAD_STRIP and join the quad with the
						 * strip. If no matching strip is found, try to
						 * form a new GL_QUAD_STRIP with another GL_QUADS.
						 * */
						bool changed = false;
						for (int i = 0; i < faceCount; i++) {
							if (this.Faces[i].Type == FaceType.Quads) {
								bool added = false;
								for (int j = 0; j < faceCount; j++) {
									if (this.Faces[j].Type == FaceType.QuadStrip) {
										if (this.Faces[i].Material == this.Faces[j].Material) {
											int n = this.Faces[j].Vertices.Length;
											for (int k = 0; k < 4; k++) {
												if (this.Faces[i].Vertices[k] == this.Faces[j].Vertices[0] & this.Faces[i].Vertices[(k + 3) % 4] == this.Faces[j].Vertices[1]) {
													Array.Resize<int>(ref this.Faces[j].Vertices, n + 2);
													for (int h = this.Faces[j].Vertices.Length - 1; h >= 2; h--) {
														this.Faces[j].Vertices[h] = this.Faces[j].Vertices[h - 2];
													}
													this.Faces[j].Vertices[0] = this.Faces[i].Vertices[(k + 1) % 4];
													this.Faces[j].Vertices[1] = this.Faces[i].Vertices[(k + 2) % 4];
													this.Faces[i] = this.Faces[faceCount - 1];
													faceCount--;
													i--;
													added = true;
													changed = true;
													break;
												} else if (this.Faces[i].Vertices[k] == this.Faces[j].Vertices[n - 1] & this.Faces[i].Vertices[(k + 3) % 4] == this.Faces[j].Vertices[n - 2]) {
													Array.Resize<int>(ref this.Faces[j].Vertices, n + 2);
													this.Faces[j].Vertices[n] = this.Faces[i].Vertices[(k + 2) % 4];
													this.Faces[j].Vertices[n + 1] = this.Faces[i].Vertices[(k + 1) % 4];
													this.Faces[i] = this.Faces[faceCount - 1];
													faceCount--;
													i--;
													added = true;
													changed = true;
													break;
												}
											}
											if (added) {
												break;
											}
										}
									}
								}
								if (!added) {
									for (int j = i + 1; j < faceCount; j++) {
										if (this.Faces[j].Type == FaceType.Quads) {
											if (this.Faces[i].Material == this.Faces[j].Material) {
												for (int k = 0; k < 4; k++) {
													for (int h = 0; h < 4; h++) {
														if (this.Faces[i].Vertices[k] == this.Faces[j].Vertices[h] & this.Faces[i].Vertices[(k + 3) % 4] == this.Faces[j].Vertices[(h + 1) % 4]) {
															int[] vertices = new int[] { this.Faces[i].Vertices[(k + 1) % 4], this.Faces[i].Vertices[(k + 2) % 4], this.Faces[i].Vertices[k], this.Faces[i].Vertices[(k + 3) % 4], this.Faces[j].Vertices[(h + 3) % 4], this.Faces[j].Vertices[(h + 2) % 4] };
															this.Faces[i].Type = FaceType.QuadStrip;
															this.Faces[i].Vertices = vertices;
															this.Faces[j] = this.Faces[faceCount - 1];
															faceCount--;
															added = true;
															changed = true;
															break;
														}
													}
													if (added) {
														break;
													}
												}
												if (added) {
													break;
												}
											}
										}
									}
								}
							}
						}
						if (!changed) {
							break;
						}
					}
					/*
					 * Join all GL_TRIANGLE structures or GL_QUADS structures
					 * that have the same material together.
					 * */
					for (int i = 0; i < faceCount; i++) {
						if (this.Faces[i].Type == FaceType.Triangles | this.Faces[i].Type == FaceType.Quads) {
							for (int j = i + 1; j < faceCount; j++) {
								if (this.Faces[i].Type == this.Faces[j].Type) {
									if (this.Faces[i].Material == this.Faces[j].Material) {
										int n = this.Faces[i].Vertices.Length;
										int m = this.Faces[j].Vertices.Length;
										Array.Resize<int>(ref this.Faces[i].Vertices, n + m);
										for (int k = 0; k < m; k++) {
											this.Faces[i].Vertices[n + k] = this.Faces[j].Vertices[k];
										}
										this.Faces[j] = this.Faces[faceCount - 1];
										faceCount--;
										j--;
									}
								}
							}
						}
					}
				}
				/* 
				 * Finalize the optimization
				 * */
				if (vertexCount != this.Vertices.Length) {
					Array.Resize<Vertex>(ref this.Vertices, vertexCount);
				}
				if (materialCount != this.Materials.Length) {
					Array.Resize<Material>(ref this.Materials, materialCount);
				}
				if (faceCount != this.Faces.Length) {
					Array.Resize<Face>(ref this.Faces, faceCount);
				}
			}
		}
		
		// object handle
		/// <summary>Represents a handle to an object as obtained from the host application.</summary>
		public abstract class ObjectHandle { }
		
	}
}