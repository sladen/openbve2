using System;
using Tao.OpenGl;
using Tao.Sdl;

namespace OpenBve {
	/// <summary>Provides structures and methods to perform rendering tasks.</summary>
	internal static partial class Renderer {
		
		// --- members ---
		
		/// <summary>A collection of transparent faces currently visible in the world.</summary>
		/// <remarks>This collection does not include faces from the overlay cab layer.</remarks>
		internal static FaceCollection TransparentWorldFaces = null;
		
		
		// --- initialization and deinitialization ---
		
		/// <summary>Initializes the renderer.</summary>
		internal static void Initialize() {
			
			TransparentWorldFaces = new FaceCollection(Program.CurrentOptions.FacesPerDisplayList, Program.CurrentOptions.SortInterval);
			
			// TODO: Isolate global settings from dynamic settings.
			//       Global settings should be initialized here, dynamic ones via OpenGlState.Initialize().
			Gl.glClearColor(0.5f, 0.5f, 0.5f, 1.0f);
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			Gl.glEnable(Gl.GL_DEPTH_TEST);
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
			Gl.glDepthFunc(Gl.GL_LEQUAL);
			Gl.glHint(Gl.GL_FOG_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_LINE_SMOOTH_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_POINT_SMOOTH_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_POLYGON_SMOOTH_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_GENERATE_MIPMAP_HINT, Gl.GL_NICEST);
			Gl.glDisable(Gl.GL_DITHER);
			Gl.glCullFace(Gl.GL_FRONT);
			
			if (true) {
//				Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[] { LightAmbientColor.R, LightAmbientColor.G, LightAmbientColor.B, 1.0f });
//				Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[] { LightDiffuseColor.R, LightDiffuseColor.G, LightDiffuseColor.B, 1.0f });
				Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
				Gl.glEnable(Gl.GL_LIGHTING);
				Gl.glEnable(Gl.GL_LIGHT0);
				Gl.glEnable(Gl.GL_COLOR_MATERIAL);
				Gl.glColorMaterial(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT_AND_DIFFUSE);
				Gl.glShadeModel(Gl.GL_SMOOTH);
			} else {
				Gl.glShadeModel(Gl.GL_FLAT);
			}
			
			Gl.glCullFace(Gl.GL_FRONT);
			Gl.glEnable(Gl.GL_CULL_FACE);
			
			
			OpenGlState.Initialize();
			
		}
		
		/// <summary>Deinitializes the renderer.</summary>
		internal static void Deinitialize() {
			// TODO: Do deinitialization here.
		}
		
		
		// --- render the whole scene ---

		/// <summary>Renders the whole screen, including the scene and the interface components.</summary>
		internal static void Render(double elapsedTime) {
			/*
			 * Initialize rendering this frame.
			 * */
			Gl.glClearColor(LightAmbientColor.R, LightAmbientColor.G, LightAmbientColor.B, 1.0f);
			//Gl.glClearColor(0.5f, 0.5f, 0.5f, 1.0f);
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			Gl.glLoadIdentity();
			
			if (false) {
				const double radius = 30.0;
				double angle = 0.001 * (double)System.Environment.TickCount;
				Camera.Position = new OpenBveApi.Math.Vector3(Math.Sin(angle) * radius, 10.0, -Math.Cos(angle) * radius);
				OpenBveApi.Math.Vector3 center = new OpenBveApi.Math.Vector3(0.0, 0.0, 0.0);
				OpenBveApi.Math.Vector3 direction = OpenBveApi.Math.Vector3.Normalize(center - Camera.Position);
				OpenBveApi.Math.Vector3 side = new OpenBveApi.Math.Vector3(direction.Z, 0.0, -direction.X);
				OpenBveApi.Math.Vector3 up = OpenBveApi.Math.Vector3.Cross(direction, side);
				Camera.Orientation = new OpenBveApi.Math.Orientation3(side, up, direction);
			}
			
			
			/*
			 * Ensure that the leaf node the camera is currently in
			 * has been updated to reflect the latest camera position.
			 * */
			Camera.UpdateGridLeafNode();
			
			/*
			 * Apply the camera's position and orientation to OpenGL.
			 * */
			{
				OpenBveApi.Math.Vector3 position = Camera.Position - Camera.GridLeafNodeCenter;
				OpenBveApi.Math.Vector3 direction = Camera.Orientation.Z;
				OpenBveApi.Math.Vector3 up = Camera.Orientation.Y;
				OpenBveApi.Math.Vector3 center = position + direction;
				Glu.gluLookAt(position.X, position.Y, position.Z, center.X, center.Y, center.Z, up.X, up.Y, up.Z);
			}
			
			/*
			 * Set lighting conditions
			 * */
			Gl.glEnable(Gl.GL_LIGHTING);
			UpdateLighting(Timing.SecondsSinceMidnight);
			Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[] { (float)LightPosition.X, (float)LightPosition.Y, (float)LightPosition.Z, 0.0f });
			Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[] { LightAmbientColor.R, LightAmbientColor.G, LightAmbientColor.B, 1.0f });
			Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[] { LightDiffuseColor.R, LightDiffuseColor.G, LightDiffuseColor.B, 1.0f });
			
			/*
			 * Render all leaf nodes visible from the leaf node the camera is currently in.
			 * */
			Triangle[] triangles = null;
			if (Program.CurrentOptions.BlockClipping) {
				InitializeBlockClipping(out triangles);
			}
			int blockCount = 0;
			int blocksClipped = 0;
			int objectCount = 0;
			if (Camera.GridLeafNode != null) {
				int length = Camera.GridLeafNode.VisibleLeafNodes.Length;
				for (int i = 0; i < Camera.GridLeafNode.VisibleLeafNodes.Length; i++) {
					bool visible;
					if (Program.CurrentOptions.BlockClipping) {
						visible = BlockIntersectsTriangles(Camera.GridLeafNode.VisibleLeafNodes[i].BoundingRectangle, triangles);
					} else {
						visible = true;
					}
					if (visible) {
						RenderGridLeafNode(Camera.GridLeafNode.VisibleLeafNodes[i], false);
						objectCount += Camera.GridLeafNode.VisibleLeafNodes[i].StaticOpaqueObjectCount;
					} else {
						blocksClipped++;
					}
					blockCount++;
				}
			}
			//Windows.UpdateDebugText("blockCount=" + blockCount.ToString() + ", blocksClipped=" + blocksClipped.ToString());
			
//			if (Camera.GridLeafNode == null) {
//				Windows.UpdateDebugText("NULL");
//			} else if (Camera.GridLeafNode is ObjectGrid.GridPopulatedLeafNode) {
//				ObjectGrid.GridLeafNode leaf = (ObjectGrid.GridLeafNode)Camera.GridLeafNode;
//				Windows.UpdateDebugText("GridPopulatedLeafNode, rect(left=" + leaf.Rectangle.Left.ToString("0") + ", right=" + leaf.Rectangle.Right.ToString("0") + ", near=" + leaf.Rectangle.Near.ToString("0") + ", far=" + leaf.Rectangle.Far.ToString("0") + "), bounding(left=" + leaf.BoundingRectangle.Left.ToString("0") + ", right=" + leaf.BoundingRectangle.Right.ToString("0") + ", near=" + leaf.BoundingRectangle.Near.ToString("0") + ", far=" + leaf.BoundingRectangle.Far.ToString("0") + "), count=" + leaf.VisibleLeafNodes.Length.ToString());
//			} else if (Camera.GridLeafNode is ObjectGrid.GridUnpopulatedLeafNode) {
//				ObjectGrid.GridLeafNode leaf = (ObjectGrid.GridLeafNode)Camera.GridLeafNode;
//				Windows.UpdateDebugText("GridUnpopulatedLeafNode, rect(left=" + leaf.Rectangle.Left.ToString("0") + ", right=" + leaf.Rectangle.Right.ToString("0") + ", near=" + leaf.Rectangle.Near.ToString("0") + ", far=" + leaf.Rectangle.Far.ToString("0") + "), bounding(left=" + leaf.BoundingRectangle.Left.ToString("0") + ", right=" + leaf.BoundingRectangle.Right.ToString("0") + ", near=" + leaf.BoundingRectangle.Near.ToString("0") + ", far=" + leaf.BoundingRectangle.Far.ToString("0") + "), count=" + leaf.VisibleLeafNodes.Length.ToString());
//			} else {
//				Windows.UpdateDebugText("invalid");
//			}
			if (Camera.GridLeafNode == null) {
				Windows.UpdateDebugText("You have left the bounds of the grid root node.");
			}
			
			/* 
			 * Render transparent world faces.
			 * */
			TransparentWorldFaces.SortAllFaces(elapsedTime, false);
			TransparentWorldFaces.RenderAllLists();
			/*
			 * Render overlays in screen coordinates.
			 * */
			{
				/*
				 * Prepare the projection matrix to work
				 * temporarily in screen coordinates.
				 * */
				Gl.glMatrixMode(Gl.GL_PROJECTION);
				Gl.glPushMatrix();
				Gl.glLoadIdentity();
				Gl.glOrtho(0.0, (double)Screen.Properties.Width, (double)Screen.Properties.Height, 0.0, -1.0, 1.0);
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
				Gl.glPushMatrix();
				Gl.glLoadIdentity();
				Gl.glDisable(Gl.GL_LIGHTING);
				/*
				 * Render GUI.
				 * */
				Windows.Render();
				/*
				 * Undo the matrix transformation.
				 * */
				Gl.glPopMatrix();
				Gl.glMatrixMode(Gl.GL_PROJECTION);
				Gl.glPopMatrix();
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
			}
			/* 
			 * Perform frame-finalizing tasks.
			 * */
			Sdl.SDL_GL_SwapBuffers();
			#if DEBUG
			CheckForOpenGlError("Renderer.Render");
			#endif
		}

		/// <summary>Renders the objects attached to a specified populated leaf node.</summary>
		/// <param name="leaf">The leaf node to render.</param>
		private static void RenderGridLeafNode(ObjectGrid.GridPopulatedLeafNode leaf, bool includeVisibleLeafNodes) {
			/* Create the display list for the leaf node if not yet available. */
			if (leaf.DisplayList.IsUnavailable()) {
				leaf.CreateOrUpdateDisplayList();
			}
			/* Render the leaf node if the display list is available. */
			if (leaf.DisplayList.IsAvailable()) {
				double x = 0.5 * (leaf.Rectangle.Left + leaf.Rectangle.Right) - Camera.GridLeafNodeCenter.X;
				double y = -Camera.GridLeafNodeCenter.Y;
				double z = 0.5 * (leaf.Rectangle.Near + leaf.Rectangle.Far) - Camera.GridLeafNodeCenter.Z;
				Gl.glPushMatrix();
				Gl.glTranslated(x, y, z);
				leaf.DisplayList.Call();
				Gl.glPopMatrix();
			}
		}
		
		
		// --- render specific kinds of objects ---

		/// <summary>Renders a solid-color polygon from an array of vertices.</summary>
		/// <param name="vertices">The vertices that describe the polygon.</param>
		/// <param name="reflectiveColor">The reflective color.</param>
		/// <param name="emissiveColor">The emissive color.</param>
		/// <remarks>This function may unbind the texture or change the emissive color in the current OpenGL state.</remarks>
		internal static void RenderPolygonFromVertices(OpenBveApi.Math.Vector3[] vertices, OpenBveApi.Color.ColorRGB reflectiveColor, OpenBveApi.Color.ColorRGB emissiveColor, ref OpenGlState state) {
			/*
			 * Begin rendering the polygon.
			 * */
			state.UnbindTexture();
			Gl.glBegin(Gl.GL_POLYGON);
			state.SetEmissiveColor(emissiveColor);
			/*
			 * Render the vertices.
			 * */
			OpenBveApi.Math.Vector3 normal;
			OpenBveApi.Math.Vector3.CreateNormal(vertices[0], vertices[1], vertices[2], out normal);
			for (int i = 0; i < vertices.Length; i++) {
				Gl.glColor3f(reflectiveColor.R, reflectiveColor.G, reflectiveColor.B);
				Gl.glNormal3d(normal.X, normal.Y, normal.Z);
				Gl.glVertex3d(vertices[i].X, vertices[i].Y, vertices[i].Z);
			}
			/*
			 * End rendering the polygon.
			 * */
			Gl.glEnd();
		}
		
		/// <summary>Renders the opaque part of a set of specified static objects.</summary>
		/// <param name="objects">The list of static objects.</param>
		/// <param name="count">The number of static objects.</param>
		/// <param name="count">The current OpenGL state.</param>
		/// <remarks>This function may change the texture or emissive color in the current OpenGL state.</remarks>
		internal static void RenderStaticOpaqueObjects(ObjectGrid.StaticOpaqueObject[] objects, int count, ref OpenGlState state) {
			/* 
			 * Count the number of faces.
			 * */
			int faceCount = 0;
			for (int i = 0; i < count; i++) {
				OpenBveApi.Geometry.FaceVertexMesh mesh = ObjectLibrary.Library.Objects[objects[i].LibraryIndex] as OpenBveApi.Geometry.FaceVertexMesh;
				if (mesh != null) {
					faceCount += mesh.Faces.Length;
				}
			}
			/* 
			 * Sort the faces by shared material properties. This will later increase
			 * rendering speed as fewer state changes will be involved in OpenGL.
			 * */
			Face[] faces = new Face[faceCount];
			long[] keys = new long[faceCount];
			faceCount = 0;
			for (int i = 0; i < count; i++) {
				OpenBveApi.Geometry.FaceVertexMesh mesh = ObjectLibrary.Library.Objects[objects[i].LibraryIndex] as OpenBveApi.Geometry.FaceVertexMesh;
				if (mesh != null) {
					for (int j = 0; j < mesh.Faces.Length; j++) {
						int material = mesh.Faces[j].Material;
						long texture =
							mesh.Materials[material].DaytimeTexture is Textures.ApiHandle ?
							(long)(((Textures.ApiHandle)mesh.Materials[material].DaytimeTexture).TextureIndex) + 1 :
							(long)0;
						long emissive =
							(long)(255.0f * mesh.Materials[material].EmissiveColor.R) |
							(long)(255.0f * mesh.Materials[material].EmissiveColor.G) << 8 |
							(long)(255.0f * mesh.Materials[material].EmissiveColor.B) << 16;
						faces[faceCount] = new Face(i, j);
						keys[faceCount] = texture | emissive << 32;
						faceCount++;
					}
				}
			}
			Array.Sort<long, Face>(keys, faces);
			/*
			 * Render the faces.
			 * */
			state.SetAlphaFunction(Gl.GL_EQUAL, 1.0f);
			for (int i = 0; i < faceCount; i++) {
				int objectIndex = faces[i].ObjectIndex;
				OpenBveApi.Geometry.FaceVertexMesh mesh = ObjectLibrary.Library.Objects[objects[objectIndex].LibraryIndex] as OpenBveApi.Geometry.FaceVertexMesh;
				if (mesh != null) {
					int material = mesh.Faces[faces[i].FaceIndex].Material;
					if (mesh.Materials[material].BlendMode == OpenBveApi.Geometry.BlendMode.Normal) {
						RenderFace(mesh, faces[i].FaceIndex, objects[objectIndex].GridPosition, objects[objectIndex].GridOrientation, ref state);
					}
				}
			}
		}

		
		// --- render a single face ---
		
		/// <summary>Renders a face of a face-vertex mesh.</summary>
		/// <param name="mesh">The face-vertex mesh.</param>
		/// <param name="faceIndex">The face of the mesh to render.</param>
		/// <param name="position">The position of the face.</param>
		/// <param name="orientation">The orientation of the face.</param>
		/// <param name="state">The current OpenGL state.</param>
		/// <remarks>This function may change the texture or emissive color in the current OpenGL state.</remarks>
		private static void RenderFace(OpenBveApi.Geometry.FaceVertexMesh mesh, int faceIndex, OpenBveApi.Math.Vector3 position, OpenBveApi.Math.Orientation3 orientation, ref OpenGlState state) {
			int material = mesh.Faces[faceIndex].Material;
			/*
			 * Set the texture.
			 * */
			Textures.ApiHandle apiHandle = mesh.Materials[material].DaytimeTexture as Textures.ApiHandle;
			int textureIndex = apiHandle != null ? apiHandle.TextureIndex : -1;
			if (textureIndex >= 0 && Textures.RegisteredTextures[textureIndex].Status == Textures.TextureStatus.Loaded) {
				int openGlTextureIndex = Textures.RegisteredTextures[textureIndex].OpenGlTextureIndex;
				state.BindTexture(openGlTextureIndex);
			} else {
				state.UnbindTexture();
			}
			/*
			 * Begin rendering the face.
			 * */
			switch (mesh.Faces[faceIndex].Type) {
				case OpenBveApi.Geometry.FaceType.Triangles:
					Gl.glBegin(Gl.GL_TRIANGLES);
					break;
				case OpenBveApi.Geometry.FaceType.TriangleStrip:
					Gl.glBegin(Gl.GL_TRIANGLE_STRIP);
					break;
				case OpenBveApi.Geometry.FaceType.TriangleFan:
					Gl.glBegin(Gl.GL_TRIANGLE_FAN);
					break;
				case OpenBveApi.Geometry.FaceType.Quads:
					Gl.glBegin(Gl.GL_QUADS);
					break;
				case OpenBveApi.Geometry.FaceType.QuadStrip:
					Gl.glBegin(Gl.GL_QUAD_STRIP);
					break;
				case OpenBveApi.Geometry.FaceType.Polygon:
					Gl.glBegin(Gl.GL_POLYGON);
					break;
				default:
					throw new InvalidOperationException();
			}
			/*
			 * Set the emissive color.
			 * */
			OpenBveApi.Color.ColorRGB emissiveColor = mesh.Materials[material].EmissiveColor;
			state.SetEmissiveColor(emissiveColor);
			/*
			 * Render the vertices of the face.
			 * */
			for (int j = 0; j < mesh.Faces[faceIndex].Vertices.Length; j++) {
				int vertex = mesh.Faces[faceIndex].Vertices[j];
				OpenBveApi.Math.Vector3 spatialCoordinates = mesh.Vertices[vertex].SpatialCoordinates;
				spatialCoordinates.Rotate(orientation);
				spatialCoordinates.Translate(position);
				OpenBveApi.Math.Vector3 normal = mesh.Vertices[vertex].Normal;
				normal.Rotate(orientation);
				Gl.glColor4f(mesh.Vertices[vertex].ReflectiveColor.R, mesh.Vertices[vertex].ReflectiveColor.G, mesh.Vertices[vertex].ReflectiveColor.B, mesh.Vertices[vertex].ReflectiveColor.A);
				Gl.glNormal3d(normal.X, normal.Y, normal.Z);
				Gl.glTexCoord2d(mesh.Vertices[vertex].TextureCoordinates.X, mesh.Vertices[vertex].TextureCoordinates.Y);
				Gl.glVertex3d(spatialCoordinates.X, spatialCoordinates.Y, spatialCoordinates.Z);
			}
			/*
			 * End rendering the face.
			 * */
			Gl.glEnd();
		}
		
		
		// --- functions for debugging
		
		#if DEBUG
		/// <summary>Checks if an OpenGL error is reported. If so, the error is written to the console and the program is terminated.</summary>
		/// <param name="location">A description indicating the location at which this function was called, and thus, where the error about occured.</param>
		private static void CheckForOpenGlError(string location) {
			int error = Gl.glGetError();
			if (error != Gl.GL_NO_ERROR) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("OpenGL error ");
				switch (error) {
					case Gl.GL_INVALID_ENUM:
						Console.Write("GL_INVALID_ENUM");
						break;
					case Gl.GL_INVALID_VALUE:
						Console.Write("GL_INVALID_VALUE");
						break;
					case Gl.GL_INVALID_OPERATION:
						Console.Write("GL_INVALID_OPERATION");
						break;
					case Gl.GL_STACK_OVERFLOW:
						Console.Write("GL_STACK_OVERFLOW");
						break;
					case Gl.GL_STACK_UNDERFLOW:
						Console.Write("GL_STACK_UNDERFLOW");
						break;
					case Gl.GL_OUT_OF_MEMORY:
						Console.Write("GL_OUT_OF_MEMORY");
						break;
					case Gl.GL_TABLE_TOO_LARGE:
						Console.Write("GL_TABLE_TOO_LARGE");
						break;
					default:
						Console.Write(error.ToString());
						break;
				}
				Console.Write(" at ");
				Console.Write(location);
				Console.ForegroundColor = ConsoleColor.Gray;
				Loop.Leave();
			}
		}
		#endif
		
	}
}