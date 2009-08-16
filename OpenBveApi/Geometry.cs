using System;

namespace OpenBveApi {
	/// <summary>Provides geometric structures.</summary>
	public static class Geometry {
		
		// objects
		/// <summary>Represents an geometric object. This is the ultimate base class for all objects.</summary>
		public abstract class GenericObject {
			// functions
			/// <summary>Translates the object.</summary>
			/// <param name="offset">The offset by which to translate.</param>
			public abstract void Translate(Math.Vector3 offset);
			/// <summary>Translates the object.</summary>
			/// <param name="orientation">The orientation along which to translate.</param>
			/// <param name="offset">The offset relative to the orientation by which to translate.</param>
			public abstract void Translate(Math.Orientation3 orientation, Math.Vector3 offset);
			/// <summary>Scales the object.</summary>
			/// <param name="factor">The factor by which to scale.</param>
			public abstract void Scale(Math.Vector3 factor);
			/// <summary>Rotates the object.</summary>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle by which to rotate.</param>
			/// <param name="sineOfAngle">The sine of the angle by which to rotate.</param>
			public abstract void Rotate(Math.Vector3 direction, double cosineOfAngle, double sineOfAngle);
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
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="spatialCoordinates">The spatial coordinates.</param>
			/// <param name="normal">The surface normal.</param>
			/// <param name="textureCoordinates">The texture coordinates.</param>
			/// <param name="reflectiveColor">The reflective color.</param>
			public Vertex(Math.Vector3 spatialCoordinates, Math.Vector3 normal, Math.Vector2 textureCoordinates, Color.ColorRGBA reflectiveColor) {
				this.SpatialCoordinates = spatialCoordinates;
				this.Normal = normal;
				this.TextureCoordinates = textureCoordinates;
				this.ReflectiveColor = reflectiveColor;
			}
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
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="emissiveColor">The emissive color.</param>
			/// <param name="daytimeTexture">A handle to the daytime texture.</param>
			/// <param name="nighttimeTexture">A handle to the nighttime texture.</param>
			public Material(Color.ColorRGB emissiveColor, Texture.TextureHandle daytimeTexture, Texture.TextureHandle nighttimeTexture) {
				this.EmissiveColor = emissiveColor;
				this.DaytimeTexture = daytimeTexture;
				this.NighttimeTexture = nighttimeTexture;
			}
		}
		
		// face vertex
		/// <summary>Represents a vertex of a face.</summary>
		public struct FaceVertex {
			// members
			/// <summary>A reference to a vertex of the underlying mesh.</summary>
			public int Vertex;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="vertex">A reference to a vertex of the underlying mesh.</param>
			public FaceVertex(int vertex) {
				this.Vertex = vertex;
			}
		}
		
		// face type
		/// <summary>Specifies how vertices in a face are organized.</summary>
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
			/// <remarks>For polygons with 3 or 4 vertices, consider using Triangles or Quads, respectively.</remarks>
			Polygon = 5
		}
		
		// face
		/// <summary>Represents a face.</summary>
		public struct Face {
			// members
			/// <summary>The type of the face, describing how vertices are organized.</summary>
			public FaceType Type;
			/// <summary>A list of face vertices.</summary>
			public FaceVertex[] Vertices;
			/// <summary>A reference to a material of the underlying mesh.</summary>
			public int Material;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="type">The type of the face, describing how vertices are organized.</param>
			/// <param name="vertices">A list of face vertices.</param>
			/// <param name="material">A reference to a material of the underlying mesh.</param>
			public Face(FaceType type, FaceVertex[] vertices, int material) {
				this.Type = type;
				this.Vertices = vertices;
				this.Material = material;
			}
			// functions
			/// <summary>Flips the front and back of the face.</summary>
			public void Flip() {
				if (this.Type == FaceType.TriangleFan) {
					// triangle fan
					for (int j = 1; j < (this.Vertices.Length - 1 >> 1); j++) {
						int k = this.Vertices.Length - j;
						FaceVertex vertex = this.Vertices[j];
						this.Vertices[j] = this.Vertices[k];
						this.Vertices[k] = vertex;
					}
				} else if (this.Type == FaceType.QuadStrip) {
					// quad strip
					for (int j = 0; j < this.Vertices.Length; j += 2) {
						FaceVertex vertex = this.Vertices[j];
						this.Vertices[j] = this.Vertices[j + 1];
						this.Vertices[j] = vertex;
					}
				} else {
					// others
					for (int j = 0; j < (this.Vertices.Length >> 1); j++) {
						int k = this.Vertices.Length - j - 1;
						FaceVertex vertex = this.Vertices[j];
						this.Vertices[j] = this.Vertices[k];
						this.Vertices[k] = vertex;
					}
				}
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
			// functions
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
			/// <summary>Scales the mesh.</summary>
			/// <param name="factor">The factor by which to scale.</param>
			public override void Scale(Math.Vector3 factor) {
				// vertices
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
				// reverse winding
				if (factor.X * factor.Y * factor.Z < 0.0) {
					for (int i = 0; i < this.Faces.Length; i++) {
						this.Faces[i].Flip();
					}
				}
			}
			/// <summary>Rotates the mesh.</summary>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle by which to rotate.</param>
			/// <param name="sineOfAngle">The sine of the angle by which to rotate.</param>
			public override void Rotate(Math.Vector3 direction, double cosineOfAngle, double sineOfAngle) {
				for (int i = 0; i < this.Vertices.Length; i++) {
					Math.Vector3.Rotate(ref this.Vertices[i].SpatialCoordinates, direction, cosineOfAngle, sineOfAngle);
					Math.Vector3.Rotate(ref this.Vertices[i].Normal, direction, cosineOfAngle, sineOfAngle);
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
					this.Faces[faces + i].Vertices = new FaceVertex[mesh.Faces[i].Vertices.Length];
					for (int j = 0; j < mesh.Faces[i].Vertices.Length; j++) {
						this.Faces[faces + i].Vertices[j].Vertex = mesh.Faces[i].Vertices[j].Vertex + vertices;
					}
				}
			}
		}
		
	}
}