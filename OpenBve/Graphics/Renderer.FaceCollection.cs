using System;
using System.Collections;
using Tao.OpenGl;

namespace OpenBve {
	internal static partial class Renderer {
		
		// --- structures and classes ---
		
		/// <summary>Represents a reference to a face.</summary>
		private struct Face {
			// members
			/// <summary>An index by which to identify this face. The value is specific to the function that uses this data structure.</summary>
			internal int ObjectIndex;
			/// <summary>The face within the object.</summary>
			internal int FaceIndex;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="objectIndex">An index by which to identify this face. The value is specific to the algorithm that uses this data structure.</param>
			/// <param name="faceIndex">The face within the object.</param>
			internal Face(int objectIndex, int faceIndex) {
				this.ObjectIndex = objectIndex;
				this.FaceIndex = faceIndex;
			}
		}
		
		/// <summary>Represents a reference to a face along with information about its position and orientation.</summary>
		internal class PositionedFace {
			// members
			/// <summary>A reference to an object in the object library.</summary>
			internal int LibraryIndex;
			/// <summary>The face within the object.</summary>
			internal int FaceIndex;
			/// <summary>The absolute world position.</summary>
			internal OpenBveApi.Math.Vector3 Position;
			/// <summary>The absolute world orientation.</summary>
			internal OpenBveApi.Math.Orientation3 Orientation;
			/// <summary>The face normal pre-rotated into the absolute world orientation.</summary>
			internal OpenBveApi.Math.Vector3 Normal;
			/// <summary>An index by which to identify this face. The value is specific to the function that uses this data structure.</summary>
			internal int Index;
			/// <summary>A tag to store intermediate data. The value is specific to the function that uses this data structure.</summary>
			internal double Tag;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="libraryIndex">A reference to an object in the object library.</param>
			/// <param name="faceIndex">The face within the object.</param>
			/// <param name="position">The absolute world position.</param>
			/// <param name="orientation">The absolute world orientation.</param>
			/// <param name="index">An index by which to identify this face. The value is specific to the function that uses this data structure.</param>
			internal PositionedFace(int libraryIndex, int faceIndex, OpenBveApi.Math.Vector3 position, OpenBveApi.Math.Orientation3 orientation, int index) {
				this.LibraryIndex = libraryIndex;
				this.FaceIndex = faceIndex;
				this.Position = position;
				this.Orientation = orientation;
				OpenBveApi.Geometry.FaceVertexMesh mesh = ObjectLibrary.Library.Objects[libraryIndex] as OpenBveApi.Geometry.FaceVertexMesh;
				if (mesh != null && mesh.Faces[faceIndex].Vertices.Length >= 3) {
					OpenBveApi.Math.Vector3 vectorA = mesh.Vertices[mesh.Faces[faceIndex].Vertices[0]].SpatialCoordinates;
					OpenBveApi.Math.Vector3 vectorB = mesh.Vertices[mesh.Faces[faceIndex].Vertices[1]].SpatialCoordinates;
					OpenBveApi.Math.Vector3 vectorC = mesh.Vertices[mesh.Faces[faceIndex].Vertices[2]].SpatialCoordinates;
					if (OpenBveApi.Math.Vector3.CreateNormal(vectorA, vectorB, vectorC, out this.Normal)) {
						this.Normal.Rotate(orientation);
					} else {
						this.Normal = OpenBveApi.Math.Vector3.Up;
					}
				} else {
					this.Normal = OpenBveApi.Math.Vector3.Up;
				}
				this.Index = index;
				this.Tag = 0.0;
			}
		}
		
		/// <summary>Represents a collection of faces which are rendered to several display lists.</summary>
		internal class FaceCollection {
			// members
			/// <summary>A list of faces.</summary>
			private PositionedFace[] Faces;
			/// <summary>The number of faces.</summary>
			private int FaceCount;
			/// <summary>A list of display lists.</summary>
			private DisplayList[] Lists;
			/// <summary>The number of display lists.</summary>
			private bool[] ListsChanged;
			/// <summary>The number of faces per display list.</summary>
			private int FacesPerDisplayList;
			/// <summary>The interval in seconds in which to sort the collection.</summary>
			private double SortInterval;
			/// <summary>The time in seconds that elapsed since the list was last depth-sorted.</summary>
			internal double ElapsedTime;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="facePerDisplayList">The number of faces per display list.</param>
			/// <param name="sortInterval">The interval in seconds in which to sort the collection.</param>
			internal FaceCollection(int facesPerDisplayList, double sortInterval) {
				this.Faces = new PositionedFace[facesPerDisplayList];
				this.FaceCount = 0;
				this.Lists = new DisplayList[] { new DisplayList() };
				this.ListsChanged = new bool[] { false };
				this.FacesPerDisplayList = facesPerDisplayList;
				this.SortInterval = sortInterval;
				this.ElapsedTime = 0.0;
			}
			// instance functions
			/// <summary>Gets the number of faces currently in this collection.</summary>
			/// <returns>The number of faces currently in this collection.</returns>
			internal int GetCount() {
				return this.FaceCount;
			}
			/// <summary>Adds the face of an object from the object library to this collection.</summary>
			/// <param name="libraryIndex">The object in the object library.</param>
			/// <param name="faceIndex">The face of the object.</param>
			/// <param name="position">The absolute position of the face.</param>
			/// <param name="orientation">The absolute orientation of the face.</param>
			/// <returns>A reference to the added face which can be used later to remove it again.</returns>
			internal object Add(int libraryIndex, int faceIndex, OpenBveApi.Math.Vector3 position, OpenBveApi.Math.Orientation3 orientation) {
				/*
				 * Only add face-vertex meshes.
				 * */
				OpenBveApi.Geometry.FaceVertexMesh mesh = ObjectLibrary.Library.Objects[libraryIndex] as OpenBveApi.Geometry.FaceVertexMesh;
				if (mesh != null) {
					/*
					 * Add the face.
					 * */
					if (this.FaceCount == this.Faces.Length) {
						Array.Resize<PositionedFace>(ref this.Faces, this.Faces.Length << 1);
						int count = this.Lists.Length;
						Array.Resize<DisplayList>(ref this.Lists, count << 1);
						for (int i = count; i < this.Lists.Length; i++) {
							this.Lists[i] = new DisplayList();
						}
						Array.Resize<bool>(ref this.ListsChanged, this.ListsChanged.Length << 1);
					}
					PositionedFace face = new PositionedFace(libraryIndex, faceIndex, position, orientation, this.FaceCount);
					this.Faces[this.FaceCount] = face;
					this.ListsChanged[this.FaceCount / this.FacesPerDisplayList] = true;
					this.FaceCount++;
					/*
					 * Make sure the textures used by the face have been loaded.
					 * */
					for (int i = 0; i < mesh.Faces.Length; i++) {
						int material = mesh.Faces[i].Material;
						Textures.ApiHandle apiHandle = mesh.Materials[material].DaytimeTexture as Textures.ApiHandle;
						if (apiHandle != null) {
							Textures.LoadTexture(apiHandle.TextureIndex, true);
						}
					}
					return (object)face;
				} else {
					return null;
				}
			}
			/// <summary>Removes a specified face from this collection.</summary>
			/// <param name="handle">A reference to a face as obtained from the Add method.</param>
			internal void Remove(object handle) {
				PositionedFace face = handle as PositionedFace;
				if (face != null) {
					int index = face.Index;
					if (index >= 0 & index < this.FaceCount) {
						this.Faces[index] = this.Faces[this.FaceCount - 1];
						this.Faces[index].Index = index;
						this.ListsChanged[index / this.FacesPerDisplayList] = true;
						this.ListsChanged[(this.FaceCount - 1) / this.FacesPerDisplayList] = true;
						this.FaceCount--;
						if (this.FaceCount % this.FacesPerDisplayList == 0) {
							this.Lists[this.Lists.Length - 1].Destroy();
						}
					} else {
						throw new InvalidOperationException("#56482: A bug in the transparency management occured.");
					}
				}
			}
			/// <summary>Sorts all faces in this collection.</summary>
			/// <param name="elapsedTime">The time that elapsed since the last call to this function.</param>
			/// <param name="force">Whether to force sorting the collection. If this flag is not set, sorting only occurs sporadically.</param>
			internal void SortAllFaces(double elapsedTime, bool force) {
				this.ElapsedTime += elapsedTime;
				if (this.ElapsedTime >= SortInterval) {
					this.ElapsedTime = 0.0;
					if (this.FaceCount != 0) {
						for (int i = 0; i < this.FaceCount; i++) {
							OpenBveApi.Geometry.FaceVertexMesh mesh = ObjectLibrary.Library.Objects[this.Faces[i].LibraryIndex] as OpenBveApi.Geometry.FaceVertexMesh;
							if (mesh != null) {
								int faceIndex = this.Faces[i].FaceIndex;
								int[] vertices = mesh.Faces[faceIndex].Vertices;
								if (vertices.Length >= 3) {
									OpenBveApi.Math.Vector3 vectorA = mesh.Vertices[vertices[0]].SpatialCoordinates;
									OpenBveApi.Math.Vector3 normal = this.Faces[i].Normal;
									OpenBveApi.Math.Vector3 position = this.Faces[i].Position + OpenBveApi.Math.Vector3.Rotate(vectorA, this.Faces[i].Orientation);
									double dot = OpenBveApi.Math.Vector3.Dot(normal, position - Camera.Position);
									this.Faces[i].Tag = -dot * dot;
								} else {
									this.Faces[i].Tag = double.MinValue;
								}
							} else {
								this.Faces[i].Tag = double.MinValue;
							}
							this.Faces[i].Index = i;
						}
						SortPositionedFaces(this.Faces, 0, this.FaceCount);
						for (int i = 0; i < this.FaceCount; i++) {
							if (i != this.Faces[i].Index) {
								this.ListsChanged[i / this.FacesPerDisplayList] = true;
							}
						}
						for (int i = 0; i < this.FaceCount; i++) {
							this.Faces[i].Index = i;
						}
					}
				}
			}
			/// <summary>Renders all display lists in this collection. This process may invoke recreating individual display lists.</summary>
			internal void RenderAllLists() {
				if (this.FaceCount != 0) {
					int listCount = (this.FaceCount + this.FacesPerDisplayList - 1) / this.FacesPerDisplayList;
					int faceCount = 0;
					for (int i = 0; i < listCount; i++) {
						if (this.ListsChanged[i]) {
							OpenGlState state;
							this.Lists[i].Begin(out state);
							state.SetDepthMask(false);
							for (int j = 0; j < this.FacesPerDisplayList; j++) {
								if (faceCount == this.FaceCount) {
									break;
								} else {
									int libraryIndex = this.Faces[faceCount].LibraryIndex;
									OpenBveApi.Geometry.FaceVertexMesh mesh = ObjectLibrary.Library.Objects[libraryIndex] as OpenBveApi.Geometry.FaceVertexMesh;
									if (mesh != null) {
										int faceIndex = this.Faces[faceCount].FaceIndex;
										if (mesh.Materials[mesh.Faces[faceIndex].Material].BlendMode == OpenBveApi.Geometry.BlendMode.Additive) {
											state.UnsetAlphaFunction();
											state.SetBlend(Gl.GL_SRC_ALPHA, Gl.GL_ONE);
										} else {
											state.SetAlphaFunction(Gl.GL_LESS, 1.0f);
											state.SetBlend(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
										}
										RenderFace(mesh, faceIndex, this.Faces[faceCount].Position, this.Faces[faceCount].Orientation, ref state);
									}
									faceCount++;
								}
							}
							this.Lists[i].End(ref state);
							this.ListsChanged[i] = false;
						} else {
							faceCount += this.FacesPerDisplayList;
						}
						double x = -Camera.GridLeafNodeCenter.X;
						double y = -Camera.GridLeafNodeCenter.Y;
						double z = -Camera.GridLeafNodeCenter.Z;
						Gl.glPushMatrix();
						Gl.glTranslated(x, y, z);
						this.Lists[i].Call();
						Gl.glPopMatrix();
					}
				}
			}
			// static functions
			/// <summary>Sorts a list of faces.</summary>
			/// <param name="faces">The list of faces.</param>
			/// <param name="index">The index in the list at which to start sorting.</param>
			/// <param name="count">The number of items in the list which to sort.</param>
			private static void SortPositionedFaces(PositionedFace[] faces, int index, int count) {
				PositionedFace[] dummy = new PositionedFace[faces.Length];
				SortPositionedFaces(faces, dummy, index, count);
			}
			/// <summary>Sorts a list of faces.</summary>
			/// <param name="faces">The list of faces.</param>
			/// <param name="dummy">A dummy list of the same length as the list of faces.</param>
			/// <param name="index">The index in the list at which to start sorting.</param>
			/// <param name="count">The number of items in the list which to sort.</param>
			/// <remarks>This method implements a stable merge sort that switches to an in-place stable insertion sort with sufficiently few elements.</remarks>
			private static void SortPositionedFaces(PositionedFace[] faces, PositionedFace[] dummy, int index, int count) {
				if (count < 25) {
					/*
					 * Use an insertion sort for less than 25 elements
					 * */
					for (int i = 1; i < count; i++) {
						int j;
						for (j = i - 1; j >= 0; j--) {
							if (faces[index + i].Tag >= faces[index + j].Tag) {
								break;
							}
						}
						PositionedFace temp = faces[index + i];
						for (int k = i; k > j + 1; k--) {
							faces[index + k] = faces[index + k - 1];
						}
						faces[index + j + 1] = temp;
					}
				} else {
					/*
					 * For more than three elements, split the list in
					 * half, recursively sort the two lists, then merge
					 * them back together.
					 * */
					int halfCount = count / 2;
					SortPositionedFaces(faces, dummy, index, halfCount);
					SortPositionedFaces(faces, dummy, index + halfCount, count - halfCount);
					int left = index;
					int right = index + halfCount;
					for (int i = index; i < index + count; i++) {
						if (left == index + halfCount) {
							while (right != index + count) {
								dummy[i] = faces[right];
								right++;
								i++;
							}
							break;
						} else if (right == index + count) {
							while (left != index + halfCount) {
								dummy[i] = faces[left];
								left++;
								i++;
							}
							break;
						}
						if (faces[left].Tag <= faces[right].Tag) {
							dummy[i] = faces[left];
							left++;
						} else {
							dummy[i] = faces[right];
							right++;
						}
					}
					for (int i = index; i < index + count; i++) {
						faces[i] = dummy[i];
					}
				}
			}
		}

	}
}