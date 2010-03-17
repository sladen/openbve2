using System;
using System.Collections.Generic;

namespace OpenBve {
	/// <summary>Provides methods to manage static objects which are stored in a 2D quadtree structure.</summary>
	internal static class ObjectGrid {
		
		
		// --- structures and classes ---
		
		/// <summary>Represents an instance of a static object.</summary>
		internal class StaticOpaqueObject {
			// members
			/// <summary>A reference to an object stored in the object library.</summary>
			internal int LibraryIndex;
			/// <summary>The position of the object relative to the center of the grid node.</summary>
			internal OpenBveApi.Math.Vector3 GridPosition;
			/// <summary>The orientation of the object.</summary>
			internal OpenBveApi.Math.Orientation3 GridOrientation;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="libraryIndex">A reference to an object stored in the object library.</param>
			/// <param name="gridPosition">The position of the object relative to the center of the grid node.</param>
			/// <param name="gridOrientation">The orientation of the object.</param>
			internal StaticOpaqueObject(int libraryIndex, OpenBveApi.Math.Vector3 gridPosition, OpenBveApi.Math.Orientation3 gridOrientation) {
				this.LibraryIndex = libraryIndex;
				this.GridPosition = gridPosition;
				this.GridOrientation = gridOrientation;
			}
		}
		
		/// <summary>Represents some rectangular bounds on the grid.</summary>
		internal struct GridBounds {
			// members
			/// <summary>The left edge, i.e. the smallest x-coordinate.</summary>
			internal double Left;
			/// <summary>The right edge, i.e. the highest x-coordinate.</summary>
			internal double Right;
			/// <summary>The near edge, i.e. the smallest z-coordinate.</summary>
			internal double Near;
			/// <summary>The far edge, i.e. the highest z-coordinate.</summary>
			internal double Far;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="left">The left edge, i.e. the smallest x-coordinate.</param>
			/// <param name="right">The right edge, i.e. the highest x-coordinate.</param>
			/// <param name="near">The near edge, i.e. the smallest z-coordinate.</param>
			/// <param name="far">The far edge, i.e. the highest z-coordinate.</param>
			internal GridBounds(double left, double right, double near, double far) {
				this.Left = left;
				this.Right = right;
				this.Near = near;
				this.Far = far;
			}
			// read-only fields
			/// <summary>Represents a bounds that is invalid or has not been initialized.</summary>
			internal static readonly GridBounds Uninitialized = new GridBounds(double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);
		}
		
		/// <summary>Represents a grid node.</summary>
		internal abstract class GridNode {
			// members
			/// <summary>The parent of the grid node, or a null reference for the root node.</summary>
			internal GridInternalNode Parent;
			/// <summary>The bounds of the grid node in world coordinates.</summary>
			internal GridBounds Rectangle;
			/// <summary>The smallest rectangle that encloses all child nodes and attached objects in world coordinates.</summary>
			internal GridBounds BoundingRectangle;
		}
		
		/// <summary>Represents an internal grid node.</summary>
		internal class GridInternalNode : GridNode {
			// members
			/// <summary>A list of four child nodes or null references.</summary>
			/// <remarks>The child nodes are stored in the order: near-left, near-right, far-left and far-right. Individual members may be null references.</remarks>
			internal GridNode[] Children;
			// constructors
			internal GridInternalNode(GridInternalNode parent, GridBounds rectangle, GridNode[] children) {
				this.Parent = parent;
				this.Rectangle = rectangle;
				this.BoundingRectangle = GridBounds.Uninitialized;
				this.Children = children;
			}
			// instance functions
			internal void UpdateBoundingRectangle() {
				this.BoundingRectangle = new GridBounds();
				for (int i = 0; i < this.Children.Length; i++) {
					if (this.Children[i] != null) {
						if (this.Children[i].BoundingRectangle.Left < this.BoundingRectangle.Left) {
							this.BoundingRectangle.Left = this.Children[i].BoundingRectangle.Left;
						}
						if (this.Children[i].BoundingRectangle.Right > this.BoundingRectangle.Right) {
							this.BoundingRectangle.Right = this.Children[i].BoundingRectangle.Right;
						}
						if (this.Children[i].BoundingRectangle.Near < this.BoundingRectangle.Near) {
							this.BoundingRectangle.Near = this.Children[i].BoundingRectangle.Near;
						}
						if (this.Children[i].BoundingRectangle.Far > this.BoundingRectangle.Far) {
							this.BoundingRectangle.Far = this.Children[i].BoundingRectangle.Far;
						}
					}
				}
				if (this.Parent is GridInternalNode) {
					GridInternalNode intern = (GridInternalNode)this.Parent;
					intern.UpdateBoundingRectangle();
				}
			}
		}
		
		/// <summary>Represents an abstract leaf node.</summary>
		internal abstract class GridLeafNode : GridNode {
			// members
			/// <summary>The list of all leaf nodes in the grid collection that are visible from within the bounding rectangle of this filler node.</summary>
			internal GridPopulatedLeafNode[] VisibleLeafNodes;
		}
		
		/// <summary>Represents an unpopulated leaf node.</summary>
		internal class GridUnpopulatedLeafNode : GridLeafNode {
			// constructors
			internal GridUnpopulatedLeafNode(GridInternalNode parent, GridBounds rectangle) {
				this.Parent = parent;
				this.Rectangle = rectangle;
				this.BoundingRectangle = GridBounds.Uninitialized;
				this.VisibleLeafNodes = null;
			}
		}
		
		/// <summary>Represents a populated leaf node.</summary>
		internal class GridPopulatedLeafNode : GridLeafNode {
			// members
			/// <summary>A list of static objects attached to this grid node.</summary>
			internal StaticOpaqueObject[] StaticOpaqueObjects;
			/// <summary>The number of static objects attached to this grid node.</summary>
			internal int StaticOpaqueObjectCount;
			/// <summary>The display list that renders the attached static objects.</summary>
			internal DisplayList DisplayList;
			/// <summary>A list of handles to transparent faces as obtained from the renderer.</summary>
			internal object[] TransparentFaces;
			/// <summary>The number of handles to transparent faces.</summary>
			internal int TransparentFaceCount;
			// constructors
			internal GridPopulatedLeafNode(GridInternalNode parent, GridBounds rectangle, StaticOpaqueObject initialStaticOpaqueObject) {
				this.Parent = parent;
				this.Rectangle = rectangle;
				this.BoundingRectangle = GridBounds.Uninitialized;
				this.VisibleLeafNodes = null;
				if (initialStaticOpaqueObject != null) {
					this.StaticOpaqueObjects = new StaticOpaqueObject[] { initialStaticOpaqueObject };
					this.StaticOpaqueObjectCount = 1;
				} else {
					this.StaticOpaqueObjects = new StaticOpaqueObject[] { null };
					this.StaticOpaqueObjectCount = 0;
				}
				this.DisplayList = new DisplayList();
				this.TransparentFaces = new object[1];
				this.TransparentFaceCount = 0;
			}
			internal GridPopulatedLeafNode(GridUnpopulatedLeafNode unpopulated) {
				this.Parent = unpopulated.Parent;
				this.Rectangle = unpopulated.Rectangle;
				this.BoundingRectangle = GridBounds.Uninitialized;
				this.VisibleLeafNodes = null;
				this.StaticOpaqueObjects = new StaticOpaqueObject[] { null };
				this.StaticOpaqueObjectCount = 0;
				this.DisplayList = new DisplayList();
				this.TransparentFaces = new object[1];
				this.TransparentFaceCount = 0;
			}
			// instance functions
			/// <summary>Takes an object, its position and orientation on the grid node, and then updates the bounding rectangle accordingly.</summary>
			/// <param name="libraryIndex">A reference to an object stored in the object library.</param>
			/// <param name="gridPosition">The position of the object relative to the center of the contained grid node.</param>
			/// <param name="gridOrientation">The orientation of the object.</param>
			internal void UpdateBoundingRectangle(int libraryIndex, OpenBveApi.Math.Vector3 gridPosition, OpenBveApi.Math.Orientation3 gridOrientation) {
				OpenBveApi.Geometry.FaceVertexMesh mesh = ObjectLibrary.Library.Objects[libraryIndex] as OpenBveApi.Geometry.FaceVertexMesh;
				if (mesh != null) {
					OpenBveApi.Math.Vector3 gridCenter = new OpenBveApi.Math.Vector3(
						0.5 * (this.Rectangle.Left + this.Rectangle.Right),
						0.0,
						0.5 * (this.Rectangle.Near + this.Rectangle.Far)
					);
					OpenBveApi.Math.Vector3 absolutePosition = gridCenter + gridPosition;
					for (int i = 0; i < mesh.Vertices.Length; i++) {
						OpenBveApi.Math.Vector3 vector = absolutePosition + OpenBveApi.Math.Vector3.Rotate(mesh.Vertices[i].SpatialCoordinates, gridOrientation);
						if (vector.X < this.BoundingRectangle.Left) {
							this.BoundingRectangle.Left = vector.X;
						}
						if (vector.X > this.BoundingRectangle.Right) {
							this.BoundingRectangle.Right = vector.X;
						}
						if (vector.Z < this.BoundingRectangle.Near) {
							this.BoundingRectangle.Near = vector.Z;
						}
						if (vector.Z > this.BoundingRectangle.Far) {
							this.BoundingRectangle.Far = vector.Z;
						}
					}
				}
				if (this.Parent is GridInternalNode) {
					GridInternalNode intern = (GridInternalNode)this.Parent;
					intern.UpdateBoundingRectangle();
				}
			}
			/// <summary>Ensures that all textures that are used by the static objects in this leaf node have been loaded.</summary>
			internal void LoadTextures() {
				for (int i = 0; i < this.StaticOpaqueObjectCount; i++) {
					int libraryIndex = this.StaticOpaqueObjects[i].LibraryIndex;
					OpenBveApi.Geometry.FaceVertexMesh mesh = ObjectLibrary.Library.Objects[libraryIndex] as OpenBveApi.Geometry.FaceVertexMesh;
					if (mesh != null) {
						for (int j = 0; j < mesh.Faces.Length; j++) {
							int material = mesh.Faces[j].Material;
							Textures.ApiHandle apiHandle = mesh.Materials[material].DaytimeTexture as Textures.ApiHandle;
							if (apiHandle != null) {
								Textures.LoadTexture(apiHandle.TextureIndex, true);
							}
						}
					}
				}
			}
			/// <summary>Creates or updates the display list for the static objects. If the display list already exists, it is recreated.</summary>
			internal void CreateOrUpdateDisplayList() {
				/*
				 * Load all textures used by the objects in this leaf node.
				 * */
				this.LoadTextures();
				/*
				 * Begin rendering to the display list.
				 * */
				Renderer.OpenGlState state;
				this.DisplayList.Begin(out state);
				/*
				 * Render all attached static objects.
				 * */
				Renderer.RenderStaticOpaqueObjects(this.StaticOpaqueObjects, this.StaticOpaqueObjectCount, ref state);
				if (Program.CurrentOptions.ShowGrid) {
					/*
					 * Render the rectangle and bounding rectangle for debugging purposes.
					 * */
					OpenBveApi.Color.ColorRGB brightColor = new OpenBveApi.Color.ColorRGB(
						(float)Program.RandomNumberGenerator.NextDouble(),
						(float)Program.RandomNumberGenerator.NextDouble(),
						(float)Program.RandomNumberGenerator.NextDouble()
					);
					OpenBveApi.Color.ColorRGB darkColor = new OpenBveApi.Color.ColorRGB(
						0.5f * brightColor.R,
						0.5f * brightColor.G,
						0.5f * brightColor.B
					);
					{ /* Render the rectangle. */
						double x = 0.5 * (this.Rectangle.Right - this.Rectangle.Left);
						double z = 0.5 * (this.Rectangle.Far - this.Rectangle.Near);
						OpenBveApi.Math.Vector3[] vertices = new OpenBveApi.Math.Vector3[] {
							new OpenBveApi.Math.Vector3(-x, -1.1, -z),
							new OpenBveApi.Math.Vector3(-x, -1.1, z),
							new OpenBveApi.Math.Vector3(x, -1.1, z),
							new OpenBveApi.Math.Vector3(x, -1.1, -z)
						};
						Renderer.RenderPolygonFromVertices(vertices, darkColor, OpenBveApi.Color.ColorRGB.Black, ref state);
					}
					{ /* Render the bounding rectangle. */
						OpenBveApi.Math.Vector2 center = new OpenBveApi.Math.Vector2(
							0.5 * (this.Rectangle.Left + this.Rectangle.Right),
							0.5 * (this.Rectangle.Near + this.Rectangle.Far)
						);
						OpenBveApi.Math.Vector2 nearLeft = new OpenBveApi.Math.Vector2(
							this.BoundingRectangle.Left - center.X,
							this.BoundingRectangle.Near - center.Y
						);
						OpenBveApi.Math.Vector2 nearRight = new OpenBveApi.Math.Vector2(
							this.BoundingRectangle.Right - center.X,
							this.BoundingRectangle.Near - center.Y
						);
						OpenBveApi.Math.Vector2 farLeft = new OpenBveApi.Math.Vector2(
							this.BoundingRectangle.Left - center.X,
							this.BoundingRectangle.Far - center.Y
						);
						OpenBveApi.Math.Vector2 farRight = new OpenBveApi.Math.Vector2(
							this.BoundingRectangle.Right - center.X,
							this.BoundingRectangle.Far - center.Y
						);
						OpenBveApi.Math.Vector3[] vertices = new OpenBveApi.Math.Vector3[] {
							new OpenBveApi.Math.Vector3(nearLeft.X, -1.0, nearLeft.Y),
							new OpenBveApi.Math.Vector3(farLeft.X, -1.0, farLeft.Y),
							new OpenBveApi.Math.Vector3(farRight.X, -1.0, farRight.Y),
							new OpenBveApi.Math.Vector3(nearRight.X, -1.0, nearRight.Y)
						};
						Renderer.RenderPolygonFromVertices(vertices, brightColor, OpenBveApi.Color.ColorRGB.Black, ref state);
					}
				}
				
				/*
				 * End rendering to the display list.
				 * */
				this.DisplayList.End(ref state);
			}
			/// <summary>Destroys the display list for the static objects.</summary>
			internal void DestroyDisplayList() {
				this.DisplayList.Destroy();
			}
		}
		
		// grid collection
		/// <summary>Represents a collection of grid nodes.</summary>
		internal class GridCollection {
			// members
			/// <summary>The root node that encapsulates all other grid nodes.</summary>
			internal GridNode Root;
			/// <summary>The side length of a leaf node.</summary>
			internal double SideLength;
			// constructors
			/// <summary>Creates an empty grid with a null root node.</summary>
			/// <param name="sideLength">The side length of a leaf node.</param>
			internal GridCollection(double sideLength) {
				this.Root = null;
				this.SideLength = sideLength;
			}
			// instance functions
			/// <summary>Adds a new instance of a static object to the grid.</summary>
			/// <param name="libraryIndex">The index to a library object.</param>
			/// <param name="position">The absolute world position of the object.</param>
			/// <param name="orientation">The absolute world orientation of the object.</param>
			internal void Add(int libraryIndex, OpenBveApi.Math.Vector3 position, OpenBveApi.Math.Orientation3 orientation) {
				if (this.Root == null) {
					// the root node does not exist yet
					OpenBveApi.Math.Vector3 gridPosition = new OpenBveApi.Math.Vector3(0.0, position.Y, 0.0);
					GridPopulatedLeafNode leaf = new GridPopulatedLeafNode(
						null,
						new GridBounds(
							position.X - 0.5 * this.SideLength,
							position.X + 0.5 * this.SideLength,
							position.Z - 0.5 * this.SideLength,
							position.Z + 0.5 * this.SideLength
						),
						new StaticOpaqueObject(libraryIndex, gridPosition, orientation)
					);
					leaf.UpdateBoundingRectangle(libraryIndex, gridPosition, orientation);
					this.Root = leaf;
				} else {
					// the root node exists
					while (true) {
						if (position.X >= this.Root.Rectangle.Left & position.X <= this.Root.Rectangle.Right & position.Z >= this.Root.Rectangle.Near & position.Z <= this.Root.Rectangle.Far) {
							// the position is within the bounds of the root node
							GridNode node = this.Root;
							double left = this.Root.Rectangle.Left;
							double right = this.Root.Rectangle.Right;
							double near = this.Root.Rectangle.Near;
							double far = this.Root.Rectangle.Far;
							while (true) {
								if (node is GridPopulatedLeafNode) {
									// populated leaf node
									OpenBveApi.Math.Vector3 gridPosition = new OpenBveApi.Math.Vector3(
										position.X - 0.5 * (left + right),
										position.Y,
										position.Z - 0.5 * (near + far)
									);
									GridPopulatedLeafNode leaf = (GridPopulatedLeafNode)node;
									if (leaf.StaticOpaqueObjectCount == leaf.StaticOpaqueObjects.Length) {
										Array.Resize<StaticOpaqueObject>(ref leaf.StaticOpaqueObjects, leaf.StaticOpaqueObjects.Length << 1);
									}
									leaf.VisibleLeafNodes = null;
									leaf.StaticOpaqueObjects[leaf.StaticOpaqueObjectCount] = new StaticOpaqueObject(libraryIndex, gridPosition, orientation);
									leaf.StaticOpaqueObjectCount++;
									leaf.UpdateBoundingRectangle(libraryIndex, gridPosition, orientation);
									break;
								} else if (node is GridInternalNode) {
									// internal node
									GridInternalNode intern = (GridInternalNode)node;
									int index;
									double centerX = 0.5 * (left + right);
									double centerZ = 0.5 * (near + far);
									if (position.Z <= centerZ) {
										if (position.X <= centerX) {
											index = 0;
											right = centerX;
											far = centerZ;
										} else {
											index = 1;
											left = centerX;
											far = centerZ;
										}
									} else {
										if (position.X <= centerX) {
											index = 2;
											right = centerX;
											near = centerZ;
										} else {
											index = 3;
											left = centerX;
											near = centerZ;
										}
									}
									if (intern.Children[index] is GridUnpopulatedLeafNode) {
										double sideLength = 0.5 * (right - left + far - near);
										const double toleranceFactor = 1.01;
										if (sideLength < toleranceFactor * this.SideLength) {
											// create populated leaf child
											GridPopulatedLeafNode child = new GridPopulatedLeafNode(
												intern,
												new GridBounds(left, right, near, far),
												null
											);
											child.BoundingRectangle = GridBounds.Uninitialized;
											intern.Children[index] = child;
											node = child;
										} else {
											// create internal child
											GridInternalNode child = new GridInternalNode(
												intern,
												new GridBounds(left, right, near, far),
												new GridNode[] { null, null, null, null }
											);
											child.Children[0] = new GridUnpopulatedLeafNode(child, new GridBounds(left, 0.5 * (left + right), near, 0.5 * (near + far)));
											child.Children[1] = new GridUnpopulatedLeafNode(child, new GridBounds(0.5 * (left + right), right, near, 0.5 * (near + far)));
											child.Children[2] = new GridUnpopulatedLeafNode(child, new GridBounds(left, 0.5 * (left + right), 0.5 * (near + far), far));
											child.Children[3] = new GridUnpopulatedLeafNode(child, new GridBounds(0.5 * (left + right), right, 0.5 * (near + far), far));
											intern.Children[index] = child;
											node = child;
										}
									} else {
										// go to child
										node = intern.Children[index];
									}
								} else {
									throw new InvalidOperationException();
								}
							}
							break;
						} else {
							// the position is outside the bounds of the root node
							if (position.Z <= 0.5 * (this.Root.Rectangle.Near + this.Root.Rectangle.Far)) {
								if (position.X <= 0.5 * (this.Root.Rectangle.Left + this.Root.Rectangle.Right)) {
									// expand toward near-left
									GridInternalNode intern = new GridInternalNode(
										null,
										new GridBounds(
											2.0 * this.Root.Rectangle.Left - this.Root.Rectangle.Right,
											this.Root.Rectangle.Right,
											2.0 * this.Root.Rectangle.Near - this.Root.Rectangle.Far,
											this.Root.Rectangle.Far
										),
										new GridNode[] { null, null, null, this.Root }
									);
									this.Root.Parent = intern;
									intern.Children[0] = new GridUnpopulatedLeafNode(intern, new GridBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
									intern.Children[1] = new GridUnpopulatedLeafNode(intern, new GridBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
									intern.Children[2] = new GridUnpopulatedLeafNode(intern, new GridBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
									intern.UpdateBoundingRectangle();
									this.Root = intern;
								} else {
									// expand toward near-right
									GridInternalNode intern = new GridInternalNode(
										null,
										new GridBounds(
											this.Root.Rectangle.Left,
											2.0 * this.Root.Rectangle.Right - this.Root.Rectangle.Left,
											2.0 * this.Root.Rectangle.Near - this.Root.Rectangle.Far,
											this.Root.Rectangle.Far
										),
										new GridNode[] { null, null, this.Root, null }
									);
									this.Root.Parent = intern;
									intern.Children[0] = new GridUnpopulatedLeafNode(intern, new GridBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
									intern.Children[1] = new GridUnpopulatedLeafNode(intern, new GridBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
									intern.Children[3] = new GridUnpopulatedLeafNode(intern, new GridBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
									intern.UpdateBoundingRectangle();
									this.Root = intern;
								}
							} else {
								if (position.X <= 0.5 * (this.Root.Rectangle.Left + this.Root.Rectangle.Right)) {
									// expand toward far-left
									GridInternalNode intern = new GridInternalNode(
										null,
										new GridBounds(
											2.0 * this.Root.Rectangle.Left - this.Root.Rectangle.Right,
											this.Root.Rectangle.Right,
											this.Root.Rectangle.Near,
											2.0 * this.Root.Rectangle.Far - this.Root.Rectangle.Near
										),
										new GridNode[] { null, this.Root, null, null }
									);
									this.Root.Parent = intern;
									intern.Children[0] = new GridUnpopulatedLeafNode(intern, new GridBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
									intern.Children[2] = new GridUnpopulatedLeafNode(intern, new GridBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
									intern.Children[3] = new GridUnpopulatedLeafNode(intern, new GridBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
									intern.UpdateBoundingRectangle();
									this.Root = intern;
								} else {
									// expand toward far-right
									GridInternalNode intern = new GridInternalNode(
										null,
										new GridBounds(
											this.Root.Rectangle.Left,
											2.0 * this.Root.Rectangle.Right - this.Root.Rectangle.Left,
											this.Root.Rectangle.Near,
											2.0 * this.Root.Rectangle.Far - this.Root.Rectangle.Near
										),
										new GridNode[] { this.Root, null, null, null }
									);
									this.Root.Parent = intern;
									intern.Children[1] = new GridUnpopulatedLeafNode(intern, new GridBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
									intern.Children[2] = new GridUnpopulatedLeafNode(intern, new GridBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
									intern.Children[3] = new GridUnpopulatedLeafNode(intern, new GridBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
									intern.UpdateBoundingRectangle();
									this.Root = intern;
								}
							}
						}
					}
				}
			}
			/// <summary>Creates the visibility lists for all nodes in this grid collection.</summary>
			/// <param name="viewingDistance">The viewing distance.</param>
			/// <remarks>Call this function whenever the viewing distance changes.</remarks>
			internal void CreateVisibilityLists(double viewingDistance) {
				FinalizeBoundingRectangles(this.Root);
				CreateVisibilityLists(this.Root, viewingDistance);
			}
			/// <summary>Finalizes the bounding rectangles for the specified node and all its child nodes.</summary>
			/// <param name="node">The node for which to finalize visiblity lists.</param>
			/// <remarks>This function is used to get rid of uninitialized bounding rectangles.</remarks>
			private void FinalizeBoundingRectangles(GridNode node) {
				if (node is GridInternalNode) {
					GridInternalNode intern = (GridInternalNode)node;
					for (int i = 0; i < intern.Children.Length; i++) {
						FinalizeBoundingRectangles(intern.Children[i]);
					}
				} else if (node is GridLeafNode) {
					GridLeafNode leaf = (GridLeafNode)node;
					if (leaf.BoundingRectangle.Left > leaf.BoundingRectangle.Right | leaf.BoundingRectangle.Near > leaf.BoundingRectangle.Far) {
						if (leaf is GridPopulatedLeafNode) {
							GridUnpopulatedLeafNode unpopulated = new GridUnpopulatedLeafNode(leaf.Parent, leaf.Rectangle);
							unpopulated.BoundingRectangle = unpopulated.Rectangle;
							for (int i = 0; i < leaf.Parent.Children.Length; i++) {
								if (unpopulated.Parent.Children[i] == leaf) {
									unpopulated.Parent.Children[i] = unpopulated;
								}
							}
							unpopulated.Parent.UpdateBoundingRectangle();
						} else {
							leaf.BoundingRectangle = leaf.Rectangle;
							leaf.Parent.UpdateBoundingRectangle();
						}
					}
				}
			}
			/// <summary>Creates the visibility lists for the specified node and all its child nodes.</summary>
			/// <param name="node">The node for which to create visiblity lists.</param>
			/// <param name="viewingDistance">The viewing distance.</param>
			private void CreateVisibilityLists(GridNode node, double viewingDistance) {
				if (node is GridInternalNode) {
					GridInternalNode intern = (GridInternalNode)node;
					for (int i = 0; i < intern.Children.Length; i++) {
						CreateVisibilityLists(intern.Children[i], viewingDistance);
					}
				} else if (node is GridLeafNode) {
					GridLeafNode leaf = (GridLeafNode)node;
					CreateVisibilityList(leaf, viewingDistance);
				}
			}
			/// <summary>Creates the visibility list for the specified leaf node.</summary>
			/// <param name="leaf">The leaf node for which to create its visibility list.</param>
			/// <param name="viewingDistance">The viewing distance.</param>
			private void CreateVisibilityList(GridLeafNode leaf, double viewingDistance) {
				List<GridPopulatedLeafNode> nodes = new List<GridPopulatedLeafNode>();
				CreateVisibilityList(leaf, this.Root, nodes, viewingDistance);
				leaf.VisibleLeafNodes = nodes.ToArray();
			}
			/// <summary>Creates the visibility list for the specified leaf node.</summary>
			/// <param name="leaf">The leaf node for which to create its visibility list.</param>
			/// <param name="node">The node to potentially include in the visiblity list if visible.</param>
			/// <param name="nodes">The list of visible leaf nodes.</param>
			/// <param name="viewingDistance">The viewing distance.</param>
			private void CreateVisibilityList(GridLeafNode leaf, GridNode node, List<GridPopulatedLeafNode> nodes, double viewingDistance) {
				if (node != null) {
					bool visible;
					if (
						leaf.BoundingRectangle.Left <= node.BoundingRectangle.Right &
						leaf.BoundingRectangle.Right >= node.BoundingRectangle.Left &
						leaf.BoundingRectangle.Near <= node.BoundingRectangle.Far &
						leaf.BoundingRectangle.Far >= node.BoundingRectangle.Near
					) {
						/*
						 * If the bounding rectangles intersect directly, the node is
						 * definately visible from at least some point inside the leaf.
						 * */
						visible = true;
					} else if (
						leaf.BoundingRectangle.Left - viewingDistance <= node.BoundingRectangle.Right &
						leaf.BoundingRectangle.Right + viewingDistance >= node.BoundingRectangle.Left &
						leaf.BoundingRectangle.Near - viewingDistance <= node.BoundingRectangle.Far &
						leaf.BoundingRectangle.Far + viewingDistance >= node.BoundingRectangle.Near
					) {
						/*
						 * If the leaf bounding rectangle extended by the viewing distance
						 * in all directions intersects with the node bounding rectangle,
						 * visibility is at least a possibility.
						 * */
						if (
							leaf.BoundingRectangle.Left <= node.BoundingRectangle.Right &
							leaf.BoundingRectangle.Right >= node.BoundingRectangle.Left |
							leaf.BoundingRectangle.Near <= node.BoundingRectangle.Far &
							leaf.BoundingRectangle.Far >= node.BoundingRectangle.Near
						) {
							/*
							 * The bounding rectangles intersect, but either only on
							 * the x-axis, or on the y-axis. This case is always visible
							 * given that the above constraint (extension by viewing
							 * distance) is also met.
							 * */
							visible = true;
						} else {
							/*
							 * The bounding rectangles don't intersect on either axis.
							 * Visibility is given if the smallest vertex-to-vertex
							 * distance is smaller than the viewing distance.
							 * */
							if (leaf.BoundingRectangle.Right <= node.BoundingRectangle.Left) {
								if (leaf.BoundingRectangle.Far <= node.BoundingRectangle.Near) {
									double x = leaf.BoundingRectangle.Right - node.BoundingRectangle.Left;
									double y = leaf.BoundingRectangle.Far - node.BoundingRectangle.Near;
									visible = x * x + y * y <= viewingDistance * viewingDistance;
								} else {
									double x = leaf.BoundingRectangle.Right - node.BoundingRectangle.Left;
									double y = leaf.BoundingRectangle.Near - node.BoundingRectangle.Far;
									visible = x * x + y * y <= viewingDistance * viewingDistance;
								}
							} else {
								if (leaf.BoundingRectangle.Far <= node.BoundingRectangle.Near) {
									double x = leaf.BoundingRectangle.Left - node.BoundingRectangle.Right;
									double y = leaf.BoundingRectangle.Far - node.BoundingRectangle.Near;
									visible = x * x + y * y <= viewingDistance * viewingDistance;
								} else {
									double x = leaf.BoundingRectangle.Left - node.BoundingRectangle.Right;
									double y = leaf.BoundingRectangle.Near - node.BoundingRectangle.Far;
									visible = x * x + y * y <= viewingDistance * viewingDistance;
								}
							}
						}
					} else {
						/*
						 * If the leaf bounding rectangle extended by the viewing distance
						 * in all directions doesn't intersect with the node bounding rectangle,
						 * visibility is not possible.
						 * */
						visible = false;
					}
					if (visible) {
						/*
						 * The node is visible and is either added to the list of visible nodes if
						 * a leaf node, or recursively processed for all children if an internal node.
						 * */
						if (node is GridInternalNode) {
							GridInternalNode intern = (GridInternalNode)node;
							for (int i = 0; i < intern.Children.Length; i++) {
								CreateVisibilityList(leaf, intern.Children[i], nodes, viewingDistance);
							}
						} else if (node is GridPopulatedLeafNode) {
							nodes.Add((GridPopulatedLeafNode)node);
						}
					}
				}
			}
			/// <summary>Gets the leaf node for a specified position.</summary>
			/// <param name="position">The position.</param>
			/// <param name="leaf">Receives the leaf node on success.</param>
			/// <returns>The success of the operation.</returns>
			internal bool GetLeafNode(OpenBveApi.Math.Vector3 position, out GridLeafNode leaf) {
				return GetLeafNode(position, this.Root, out leaf);
			}
			private bool GetLeafNode(OpenBveApi.Math.Vector3 position, GridNode node, out GridLeafNode leaf) {
				if (node != null) {
					if (
						position.X >= node.Rectangle.Left &
						position.X <= node.Rectangle.Right &
						position.Z >= node.Rectangle.Near &
						position.Z <= node.Rectangle.Far
					) {
						if (node is GridInternalNode) {
							GridInternalNode intern = (GridInternalNode)node;
							for (int i = 0; i < intern.Children.Length; i++) {
								if (GetLeafNode(position, intern.Children[i], out leaf)) {
									return true;
								}
							}
							leaf = null;
							return false;
						} else if (node is GridLeafNode) {
							leaf = (GridLeafNode)node;
							return true;
						} else {
							throw new InvalidOperationException();
						}
					} else {
						leaf = null;
						return false;
					}
				} else {
					leaf = null;
					return false;
				}
			}
		}
		
		// members
		/// <summary>The current grid collection in the world.</summary>
		internal static GridCollection Grid = null;
		
	}
}