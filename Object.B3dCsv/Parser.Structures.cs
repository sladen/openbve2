using System;

/*
 * TODO: Add missing XML annotation.
 * */

namespace Plugin {
	internal static partial class Parser {
		
		// vertex
		/// <summary>Represents a vertex.</summary>
		private struct Vertex {
			// members
			/// <summary>The spatial coordinates of this vertex.</summary>
			internal OpenBveApi.Math.Vector3 SpatialCoordinates;
			/// <summary>The texture coordinates of this vector.</summary>
			internal OpenBveApi.Math.Vector2 TextureCoordinates;
			/// <summary>The normal at this vertex. If set to Vector3.Null, ... TODO</summary>
			internal OpenBveApi.Math.Vector3 Normal;
		}
		
		// face
		/// <summary>Represents a face.</summary>
		private struct Face {
			// members
			/// <summary>The array of face vertices.</summary>
			internal FaceVertex[] Vertices;
			/// <summary>The reflective color of this face.</summary>
			internal OpenBveApi.Color.ColorRGBA ReflectiveColor;
			/// <summary>The emissive color of this face.</summary>
			internal OpenBveApi.Color.ColorRGB EmissiveColor;
			/// <summary>The transparent color of this face.</summary>
			internal OpenBveApi.Color.TransparentColor TransparentColor;
			/// <summary>The blend type.</summary>
			internal OpenBveApi.Geometry.BlendMode BlendMode;
			/// <summary>Indicates whether this face is to be flipped.</summary>
			internal bool Flipped;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="offset">The value to offset the indices by.</param>
			/// <param name="indices">An array of indices which when offset by offset point to the vertices of the underlying mesh builder.</param>
			/// <param name="normal">The normal of this face.</param>
			internal Face(int offset, int[] indices, OpenBveApi.Math.Vector3 normal) {
				FaceVertex[] vertices = new FaceVertex[indices.Length];
				for (int i = 0; i < indices.Length; i++) {
					vertices[i] = new FaceVertex(offset + indices[i], normal);
				}
				this.Vertices = vertices;
				this.ReflectiveColor = new OpenBveApi.Color.ColorRGBA(1.0f, 1.0f, 1.0f, 1.0f);
				this.EmissiveColor = new OpenBveApi.Color.ColorRGB(0.0f, 0.0f, 0.0f);
				this.TransparentColor = new OpenBveApi.Color.TransparentColor(0, 0, 0, false);
				this.BlendMode = OpenBveApi.Geometry.BlendMode.Normal;
				this.Flipped = false;
			}
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="offset">The value to offset the indices by.</param>
			/// <param name="indices">An array of indices which when offset by offset point to the vertices of the underlying mesh builder.</param>
			/// <param name="normal">An array of indices containing the normals for the vertices corresponding to the indices array.</param>
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
					this.BlendMode = OpenBveApi.Geometry.BlendMode.Normal;
					this.Flipped = false;
				}
			}
		}
		
		// face vertex
		/// <summary>Represents a face vertex.</summary>
		private struct FaceVertex {
			// members
			/// <summary>A reference to a vertex of the underyling mesh builder.</summary>
			internal int Vertex;
			/// <summary>The normal at this vertex. If set to Vector3.Null, ... TODO</summary>
			internal OpenBveApi.Math.Vector3 Normal;
			// constructors
			/// <summary></summary>
			/// <param name="vertex">A reference to a vertex of the underyling mesh builder.</param>
			/// <param name="normal">The normal at this vertex. If set to Vector3.Null, ... TODO</param>
			internal FaceVertex(int vertex, OpenBveApi.Math.Vector3 normal) {
				this.Vertex = vertex;
				this.Normal = normal;
			}
		}
		
		// mesh builder
		/// <summary>Represents a mesh builder.</summary>
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
					this.Vertices[i].SpatialCoordinates.Rotate(direction, cosineOfAngle, sineOfAngle);
				}
			}
			internal void Scale(OpenBveApi.Math.Vector3 factor) {
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
					for (int i = 0; i < this.FaceCount; i++) {
						Array.Reverse(this.Faces[i].Vertices);
					}
				}
			}
		}
		
	}
}