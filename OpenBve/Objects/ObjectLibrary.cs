using System;

namespace OpenBve {
	internal static class ObjectLibrary {
		
		// api handle
		/// <summary>Represents a handle to an object registered in the object library.</summary>
		/// <remarks>This class is used for interaction with the API.</remarks>
		internal class ApiHandle : OpenBveApi.Geometry.ObjectHandle {
			/// <summary>The index to the library object.</summary>
			internal int LibraryIndex;
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="libraryIndex">The index to the library object.</param>
			internal ApiHandle(int libraryIndex) {
				this.LibraryIndex = libraryIndex;
			}
		}
		
		// mesh library
		internal class MeshLibrary {
			// members
			internal OpenBveApi.Geometry.GenericObject[] Objects;
			internal int ObjectCount;
			// constructors
			internal MeshLibrary() {
				this.Objects = new OpenBveApi.Geometry.GenericObject[256];
				this.ObjectCount = 0;
			}
			// instance functions
			/// <summary>Adds an object to the library and returns the index at which the object is stored in the library.</summary>
			/// <param name="obj">The object to add to the library.</param>
			/// <returns>The index at which the object is stored in the library.</returns>
			internal int Add(OpenBveApi.Geometry.GenericObject obj) {
				if (obj is OpenBveApi.Geometry.FaceVertexMesh) {
					OpenBveApi.Geometry.FaceVertexMesh mesh = (OpenBveApi.Geometry.FaceVertexMesh)obj;
					if (Program.CurrentOptions.ObjectOptimization == 1) {
						mesh.Optimize(false);
					} else if (Program.CurrentOptions.ObjectOptimization == 2) {
						mesh.Optimize(true);
					}
					for (int i = 0; i < mesh.Faces.Length; i++) {
						switch (mesh.Faces[i].Type) {
							case OpenBveApi.Geometry.FaceType.Triangles:
								Triangles++;
								break;
							case OpenBveApi.Geometry.FaceType.TriangleStrip:
								TriangleStrips++;
								break;
							case OpenBveApi.Geometry.FaceType.TriangleFan:
								TriangleFans++;
								break;
							case OpenBveApi.Geometry.FaceType.Quads:
								Quads++;
								break;
							case OpenBveApi.Geometry.FaceType.QuadStrip:
								QuadStrips += (mesh.Faces[i].Vertices.Length - 2) / 2;
								break;
							case OpenBveApi.Geometry.FaceType.Polygon:
								Polygons++;
								break;
						}
					}
				}
				if (this.Objects.Length == this.ObjectCount) {
					Array.Resize<OpenBveApi.Geometry.GenericObject>(ref this.Objects, this.Objects.Length << 1);
				}
				int n = this.ObjectCount;
				this.Objects[n] = obj;
				this.ObjectCount++;
				return n;
			}
		}
		
		// current mesh library
		internal static MeshLibrary Library = new MeshLibrary();
		internal static int Triangles = 0;
		internal static int TriangleStrips = 0;
		internal static int TriangleFans = 0;
		internal static int Quads = 0;
		internal static int QuadStrips = 0;
		internal static int Polygons = 0;
		
	}
}