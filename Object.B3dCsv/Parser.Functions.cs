using System;

/*
 * TODO:
 * Add XML annotation.
 * */

namespace Plugin {
	internal static partial class Parser {
		
		private static void Shear(MeshBuilder builder, OpenBveApi.Math.Vector3 direction, OpenBveApi.Math.Vector3 shift, double ratio) {
			for (int i = 0; i < builder.Vertices.Length; i++) {
				double factor = ratio * OpenBveApi.Math.Vector3.Dot(builder.Vertices[i].SpatialCoordinates, direction);
				builder.Vertices[i].SpatialCoordinates += shift * factor;
				if (!builder.Vertices[i].Normal.IsNullVector()) {
					factor = ratio * OpenBveApi.Math.Vector3.Dot(builder.Vertices[i].Normal, shift);
					builder.Vertices[i].Normal -= direction * factor;
					if (!builder.Vertices[i].Normal.IsNullVector()) {
						builder.Vertices[i].Normal.Normalize();
					}
				}
			}
			for (int i = 0; i < builder.FaceCount; i++) {
				for (int j = 0; j < builder.Faces[i].Vertices.Length; j++) {
					if (!builder.Faces[i].Vertices[j].Normal.IsNullVector()) {
						double factor = ratio * OpenBveApi.Math.Vector3.Dot(builder.Faces[i].Vertices[j].Normal, shift);
						builder.Faces[i].Vertices[j].Normal -= direction * factor;
						if (!builder.Faces[i].Vertices[j].Normal.IsNullVector()) {
							builder.Faces[i].Vertices[j].Normal.Normalize();
						}
					}
				}
			}
		}
		
		private static void Shear(OpenBveApi.Geometry.FaceVertexMesh mesh, OpenBveApi.Math.Vector3 direction, OpenBveApi.Math.Vector3 shift, double ratio) {
			for (int i = 0; i < mesh.Vertices.Length; i++) {
				double factor = ratio * OpenBveApi.Math.Vector3.Dot(mesh.Vertices[i].SpatialCoordinates, direction);
				mesh.Vertices[i].SpatialCoordinates += shift * factor;
				if (!mesh.Vertices[i].Normal.IsNullVector()) {
					factor = ratio * OpenBveApi.Math.Vector3.Dot(mesh.Vertices[i].Normal, shift);
					mesh.Vertices[i].Normal -= direction * factor;
					if (mesh.Vertices[i].Normal.IsNullVector()) {
						mesh.Vertices[i].Normal = OpenBveApi.Math.Vector3.Up;
					} else {
						mesh.Vertices[i].Normal.Normalize();
					}
				}
			}
		}
		
	}
}