using System;
using Tao.OpenGl;

namespace OpenBve {
	internal static class Camera {

		// members
		internal static OpenBveApi.Math.Vector3 Position = new OpenBveApi.Math.Vector3(0.0, 0.0, 0.0);
		internal static OpenBveApi.Math.Orientation3 Orientation = OpenBveApi.Math.Orientation3.Default;
		internal static OpenBveApi.Math.Vector3 GridLeafNodeCenter = new OpenBveApi.Math.Vector3(0.0, 0.0, 0.0);
		internal static ObjectGrid.GridLeafNode GridLeafNode = null;
		internal static ViewportOptions Viewport = new ViewportOptions(1.0, 1.0, 1.0, 2.0);
		
		// viewport options
		/// <summary>Stores options required to set up the viewport.</summary>
		internal struct ViewportOptions {
			// members
			/// <summary>The horizontal viewing angle in radians.</summary>
			internal double HorizontalViewingAngle;
			/// <summary>The vertical viewing angle in radians.</summary>
			internal double VerticalViewingAngle;
			internal double NearClippingPlane;
			internal double FarClippingPlane;
			// constructors
			/// <summary>Creates a new instance of the ViewportOptions structure.</summary>
			/// <param name="HorizontalViewingAngle">The horizontal viewing angle in radians.</param>
			/// <param name="VerticalViewingAngle">The vertical viewing angle in radians.</param>
			/// <param name="NearClippingPlane">The distance to the near clipping plane in meters.</param>
			/// <param name="FarClippingPlane">The distance to the far clipping plane in meters.</param>
			internal ViewportOptions(double horizontalViewingAngle, double verticalViewingAngle, double nearClippingPlane, double farClippingPlane) {
				this.HorizontalViewingAngle = horizontalViewingAngle;
				this.VerticalViewingAngle = verticalViewingAngle;
				this.NearClippingPlane = nearClippingPlane;
				this.FarClippingPlane = farClippingPlane;
			}
			/// <summary>Creates a new instance of the ViewportOptions structure.</summary>
			/// <param name="VerticalViewingAngle">The vertical viewing angle in radians.</param>
			/// <param name="NearClippingPlane">The distance to the near clipping plane in meters.</param>
			/// <param name="FarClippingPlane">The distance to the far clipping plane in meters.</param>
			/// <remarks>The horizontal viewing angle is determined automatically from the vertical viewing angle and the window's aspect ratio.</remarks>
			internal ViewportOptions(double verticalViewingAngle, double nearClippingPlane, double farClippingPlane) {
				this.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * verticalViewingAngle) * Screen.Properties.AspectRatio);
				this.VerticalViewingAngle = verticalViewingAngle;
				this.NearClippingPlane = nearClippingPlane;
				this.FarClippingPlane = farClippingPlane;
			}
		}
		
		// set viewport
		/// <summary>Sets the viewport.</summary>
		/// <param name="Options">The options to set up the viewport.</param>
		internal static void SetViewport(ViewportOptions options) {
			Gl.glViewport(0, 0, Screen.Properties.Width, Screen.Properties.Height);
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();
			const double inverseDegrees = 57.295779513082320877;
			double aspectRatio = Math.Tan(0.5 * options.HorizontalViewingAngle) / Math.Tan(0.5 * options.VerticalViewingAngle);
			Glu.gluPerspective(options.VerticalViewingAngle * inverseDegrees, -aspectRatio, options.NearClippingPlane, options.FarClippingPlane);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glLoadIdentity();
			Viewport = options;
		}
		
		// update grid leaf node
		internal static void UpdateGridLeafNode() {
			/*
			 * Find the leaf node the camera is currently in.
			 * */
			ObjectGrid.GridLeafNode leaf;
			if (ObjectGrid.Grid.GetLeafNode(Camera.Position, out leaf)) {
				double x = 0.5 * (leaf.Rectangle.Left + leaf.Rectangle.Right);
				double z = 0.5 * (leaf.Rectangle.Near + leaf.Rectangle.Far);
				Camera.GridLeafNodeCenter = new OpenBveApi.Math.Vector3(x, 0.0, z);
			} else {
				leaf = null;
				Camera.GridLeafNodeCenter = new OpenBveApi.Math.Vector3(0.0, 0.0, 0.0);
			}
			/*
			 * Check if the leaf node the camera is in has changed.
			 * */
			if (leaf != Camera.GridLeafNode) {
				if (leaf != null) {
					/*
					 * The camera is within the bounds of a leaf node.
					 * */
					ObjectGrid.GridPopulatedLeafNode[] oldLeafNodes;
					if (Camera.GridLeafNode != null) {
						oldLeafNodes = Camera.GridLeafNode.VisibleLeafNodes;
					} else {
						oldLeafNodes = null;
					}
					ObjectGrid.GridPopulatedLeafNode[] newLeafNodes = leaf.VisibleLeafNodes;
					/*
					 * Find leaf nodes that were visible before but are not any longer.
					 * */
					if (oldLeafNodes != null) {
						for (int i = 0; i < oldLeafNodes.Length; i++) {
							bool remove = true;
							for (int j = 0; j < newLeafNodes.Length; j++) {
								if (oldLeafNodes[i] == newLeafNodes[j]) {
									remove = false;
									break;
								}
							}
							if (remove) {
								/*
								 * This leaf node is not visible any longer. Remove its
								 * associated transparent faces from the renderer.
								 * */
								for (int j = 0; j < oldLeafNodes[i].TransparentFaceCount; j++) {
									Renderer.TransparentWorldFaces.Remove(oldLeafNodes[i].TransparentFaces[j]);
								}
								oldLeafNodes[i].TransparentFaceCount = 0;
							}
						}
					}
					/*
					 * Find leaf nodes that are visible now but were not before.
					 * */
					for (int i = 0; i < newLeafNodes.Length; i++) {
						bool add = true;
						if (oldLeafNodes != null) {
							for (int j = 0; j < oldLeafNodes.Length; j++) {
								if (newLeafNodes[i] == oldLeafNodes[j]) {
									add = false;
									break;
								}
							}
						}
						if (add) {
							/*
							 * This leaf node has become visible. Add all
							 * its transparent faces to the renderer.
							 * */
							ObjectGrid.GridPopulatedLeafNode visibleLeaf = newLeafNodes[i];
							if (visibleLeaf.TransparentFaceCount != 0) {
								throw new InvalidOperationException("#65517: A bug in the transparency management occured.");
							}
							visibleLeaf.LoadTextures();
							double x = 0.5 * (visibleLeaf.Rectangle.Left + visibleLeaf.Rectangle.Right);
							double z = 0.5 * (visibleLeaf.Rectangle.Near + visibleLeaf.Rectangle.Far);
							OpenBveApi.Math.Vector3 offset = new OpenBveApi.Math.Vector3(x, 0.0, z);
							for (int j = 0; j < visibleLeaf.StaticOpaqueObjectCount; j++) {
								int libraryIndex = visibleLeaf.StaticOpaqueObjects[j].LibraryIndex;
								OpenBveApi.Geometry.FaceVertexMesh mesh = ObjectLibrary.Library.Objects[libraryIndex] as OpenBveApi.Geometry.FaceVertexMesh;
								if (mesh != null) {
									for (int k = 0; k < mesh.Faces.Length; k++) {
										add = false;
										if (mesh.Materials[mesh.Faces[k].Material].BlendMode == OpenBveApi.Geometry.BlendMode.Additive) {
											add = true;
										}
										if (!add) {
											Textures.ApiHandle apiHandle = mesh.Materials[mesh.Faces[k].Material].DaytimeTexture as Textures.ApiHandle;
											if (apiHandle != null) {
												Textures.TextureType type = Textures.RegisteredTextures[apiHandle.TextureIndex].Type;
												if (type == Textures.TextureType.Unknown) {
													throw new InvalidOperationException("#31596: A bug in the texture management occured.");
												} else if (type == Textures.TextureType.Alpha) {
													add = true;
												}
											}
										}
										if (!add) {
											for (int h = 0; h < mesh.Faces[k].Vertices.Length; h++) {
												if (mesh.Vertices[mesh.Faces[k].Vertices[h]].ReflectiveColor.A != 1.0f) {
													add = true;
													break;
												}
											}
										}
										if (add) {
											OpenBveApi.Math.Vector3 position = visibleLeaf.StaticOpaqueObjects[j].GridPosition + offset;
											OpenBveApi.Math.Orientation3 orientation = visibleLeaf.StaticOpaqueObjects[j].GridOrientation;
											if (visibleLeaf.TransparentFaceCount == visibleLeaf.TransparentFaces.Length) {
												Array.Resize<object>(ref visibleLeaf.TransparentFaces, visibleLeaf.TransparentFaces.Length << 1);
											}
											visibleLeaf.TransparentFaces[visibleLeaf.TransparentFaceCount] = Renderer.TransparentWorldFaces.Add(libraryIndex, k, position, orientation);
											visibleLeaf.TransparentFaceCount++;
										}
									}
								}
							}
						}
					}
				} else if (Camera.GridLeafNode != null) {
					/*
					 * Before, the camera was inside the bounds of
					 * a leaf node, but now, it is not anymore.
					 * Remove the transparent faces associated
					 * to the old leaf node from the renderer.
					 * */
					ObjectGrid.GridPopulatedLeafNode[] oldLeafNodes = Camera.GridLeafNode.VisibleLeafNodes;
					for (int i = 0; i < oldLeafNodes.Length; i++) {
						for (int j = 0; j < oldLeafNodes[i].TransparentFaceCount; j++) {
							Renderer.TransparentWorldFaces.Remove(oldLeafNodes[i].TransparentFaces[j]);
						}
						oldLeafNodes[i].TransparentFaceCount = 0;
					}
				}
			}
			/*
			 * Apply the found leaf node.
			 * */
			Camera.GridLeafNode = leaf;
		}
		
	}
}