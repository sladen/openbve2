using System;
using System.Reflection;
using Tao.OpenGl;
using Tao.Sdl;

namespace OpenBve {

	internal static class Renderer {
		
		// initialize
		internal static void Initialize() {
			
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
			
			Gl.glLoadIdentity();
			Glu.gluLookAt(0.0, 0.0, -5.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			
			#if false
			Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[] { 0.6f, 0.55f, 0.5f, 1.0f });
			Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[] { 0.5f, 0.45f, 0.4f, 1.0f });
			Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
			Gl.glEnable(Gl.GL_LIGHTING);
			Gl.glEnable(Gl.GL_LIGHT0);
			Gl.glEnable(Gl.GL_COLOR_MATERIAL);
			Gl.glColorMaterial(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT_AND_DIFFUSE);
			Gl.glShadeModel(Gl.GL_SMOOTH);
			#else
			Gl.glShadeModel(Gl.GL_FLAT);
			#endif
			
			Gl.glCullFace(Gl.GL_FRONT);
			Gl.glEnable(Gl.GL_CULL_FACE);

			string fileName = OpenBveApi.Path.CombineFile(Program.Path, "object.csv");
			OpenBveApi.Path.PathReference path = new OpenBveApi.Path.PathReference(OpenBveApi.Path.PathBase.AbsolutePath, OpenBveApi.Path.PathType.File, fileName);
			System.Text.Encoding encoding = System.Text.Encoding.UTF8;
			OpenBveApi.General.Origin origin = new OpenBveApi.General.Origin(path, null, encoding);
			OpenBveApi.Geometry.GenericObject obj;
			if (Interfaces.Host10.LoadObject(origin, out obj) == OpenBveApi.General.Result.Successful) {
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Object loaded successfully.");
				Console.ForegroundColor = ConsoleColor.Gray;
			} else {
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Object could not be loaded.");
				Console.ForegroundColor = ConsoleColor.Gray;
			}
			OpenBveApi.Geometry.Mesh mesh = (OpenBveApi.Geometry.Mesh)obj;

			DISPLAY_LIST = Gl.glGenLists(1);
			RenderToDisplayList(mesh, DISPLAY_LIST);

		}
		
		// DEBUG
		private static int DISPLAY_LIST = 0;
		
		// deinitialize
		internal static void Deinitialize() {
			Gl.glDeleteLists(DISPLAY_LIST, 1);
		}
		
		// render
		internal static void Render() {
			
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			Gl.glLoadIdentity();
			const double radius = 20.0;
			double angle = 0.001 * (double)System.Environment.TickCount;
			Glu.gluLookAt(Math.Cos(angle) * radius, 0.0, Math.Sin(angle) * radius, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
			
			Gl.glCallList(DISPLAY_LIST);
			
			Sdl.SDL_GL_SwapBuffers();
			
			debugCheckError("Renderer.Render");
			
		}
		
		// DEBUG
		private static void RenderToDisplayList(OpenBveApi.Geometry.Mesh mesh, int list) {
			Gl.glNewList(DISPLAY_LIST, Gl.GL_COMPILE);
			for (int i = 0; i < mesh.Faces.Length; i++) {
				switch (mesh.Faces[i].Type) {
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
				for (int j = 0; j < mesh.Faces[i].Vertices.Length; j++) {
					int vertex = mesh.Faces[i].Vertices[j].Vertex;
					Gl.glColor3f(mesh.Vertices[vertex].ReflectiveColor.R, mesh.Vertices[vertex].ReflectiveColor.G, mesh.Vertices[vertex].ReflectiveColor.B);
					Gl.glTexCoord2d(mesh.Vertices[vertex].TextureCoordinates.X, mesh.Vertices[vertex].TextureCoordinates.Y);
					Gl.glVertex3d(mesh.Vertices[vertex].SpatialCoordinates.X, mesh.Vertices[vertex].SpatialCoordinates.Y, mesh.Vertices[vertex].SpatialCoordinates.Z);
				}
				Gl.glEnd();
			}
			Gl.glEndList();
		}
		
		// DEBUG
		private static void debugCheckError(string Location) {
			int error = Gl.glGetError();
			if (error != Gl.GL_NO_ERROR) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(Location);
				switch (error) {
					case Gl.GL_INVALID_ENUM:
						Console.WriteLine("GL_INVALID_ENUM");
						break;
					case Gl.GL_INVALID_VALUE:
						Console.WriteLine("GL_INVALID_VALUE");
						break;
					case Gl.GL_INVALID_OPERATION:
						Console.WriteLine("GL_INVALID_OPERATION");
						break;
					case Gl.GL_STACK_OVERFLOW:
						Console.WriteLine("GL_STACK_OVERFLOW");
						break;
					case Gl.GL_STACK_UNDERFLOW:
						Console.WriteLine("GL_STACK_UNDERFLOW");
						break;
					case Gl.GL_OUT_OF_MEMORY:
						Console.WriteLine("GL_OUT_OF_MEMORY");
						break;
					case Gl.GL_TABLE_TOO_LARGE:
						Console.WriteLine("GL_TABLE_TOO_LARGE");
						break;
					default:
						Console.WriteLine(error.ToString());
						break;
				}
				Console.ForegroundColor = ConsoleColor.Gray;
				Loop.Leave();
			}
		}
		
	}
}