using System;
using System.Text;

namespace OpenBve {
	/// <summary>Provides the entry point for the application and stores application-specific information.</summary>
	internal static class Program {
		
		
		// --- members ---
		
		/// <summary>The absolute path to the folder where the executing assembly is located.</summary>
		internal static string StartupPath = null;
		
		/// <summary>The random number generator used by this application.</summary>
		internal static Random RandomNumberGenerator = new Random(0);
		
		/// <summary>The debug log for this application.</summary>
		internal static StringBuilder Log = new StringBuilder();
		
		/// <summary>The options for this application.</summary>
		internal static Options CurrentOptions = null;
		
		/// <summary>The maximum level of anisotropic filtering supported, or 0 to indicate no support.</summary>
		internal static float AnisotropicMaximum = 0.0f;
		
		
		// --- functions ---
		
		/// <summary>The entry procedure of this application.</summary>
		/// <param name="arguments">The list of command-line arguments.</param>
		internal static void Main(string[] arguments) {
			Console.ForegroundColor = ConsoleColor.Gray;
			#if !DEBUG
			try {
				#endif
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.WriteLine("Program started.");
				Initialize();
				Loop.Enter();
				Deinitialize();
				#if !DEBUG
			} catch (Exception ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex.Message);
				Console.ForegroundColor = ConsoleColor.Gray;
			}
			#endif
			Console.WriteLine("Program terminated. Press any key to continue...");
			Console.ReadKey(true);
		}
		
		/// <summary>Initializes all subsystems.</summary>
		private static void Initialize() {
			/*
			 * Initialize the subsystems.
			 * */
			StartupPath = GetStartupPath();
			CurrentOptions = Options.LoadFromFile(OpenBveApi.Path.CombineFile(StartupPath, "settings.cfg"));
			Platform.Initialize();
			string pluginPath = OpenBveApi.Path.CombineFolder(StartupPath, "Plugins");
			Plugins.Initialize(pluginPath);
			Screen.ScreenOptions windowOptions = new Screen.ScreenOptions(
				CurrentOptions.Width,
				CurrentOptions.Height,
				32,
				CurrentOptions.Fullscreen,
				CurrentOptions.VSync
			);
			Screen.Initialize(windowOptions);
			const double degrees = 0.0174532925199433;
			double verticalViewingAngle = 45.0 * degrees;
			Camera.ViewportOptions viewportOptions = new Camera.ViewportOptions(verticalViewingAngle, 0.5, 1.4142 * Program.CurrentOptions.ViewingDistance);
			Camera.SetViewport(viewportOptions);
			Renderer.Initialize();
			Timing.Initialize();
			ObjectGrid.Grid = new ObjectGrid.GridCollection(CurrentOptions.GridSize);
			/*
			 * Do some stuff for debugging.
			 * */
			if (CurrentOptions.ContentType.Equals("object", StringComparison.OrdinalIgnoreCase)) {
				/*
				 * Load the specified object.
				 * */
				string title = System.IO.Path.GetFileName(CurrentOptions.ContentFile);
				OpenBveApi.Path.PathReference path = new OpenBveApi.Path.FileReference(CurrentOptions.ContentFile);
				System.Text.Encoding encoding = System.Text.Encoding.UTF8;
				OpenBveApi.General.Origin origin = new OpenBveApi.General.Origin(path, null, encoding);
				OpenBveApi.Geometry.GenericObject obj;
				OpenBveApi.Geometry.FaceVertexMesh mesh;
				if (Interfaces.Host10.LoadObject(origin, out obj) == OpenBveApi.General.Result.Successful) {
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Object loaded successfully: " + title);
					Console.ForegroundColor = ConsoleColor.Gray;
					mesh = (OpenBveApi.Geometry.FaceVertexMesh)obj;
				} else {
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Object could not be loaded: " + title);
					Console.ForegroundColor = ConsoleColor.Gray;
					mesh = new OpenBveApi.Geometry.FaceVertexMesh();
				}
				OpenBveApi.Geometry.ObjectHandle handle;
				Interfaces.Host10.RegisterObject(mesh, out handle);
				ObjectLibrary.ApiHandle apiHandle = handle as ObjectLibrary.ApiHandle;
				if (apiHandle != null) {
					if (CurrentOptions.ContentCount <= 1) {
						ObjectGrid.Grid.Add(apiHandle.LibraryIndex, OpenBveApi.Math.Vector3.Null, OpenBveApi.Math.Orientation3.Default);
					} else {
						for (int i = 0; i < CurrentOptions.ContentCount; i++) {
							double x = 100.0 * (2.0 * Program.RandomNumberGenerator.NextDouble() - 1.0);
							double y = 0.0;
							double z = 100.0 * (2.0 * Program.RandomNumberGenerator.NextDouble() - 1.0);
							OpenBveApi.Math.Vector3 position = new OpenBveApi.Math.Vector3(x, y, z);
							OpenBveApi.Math.Orientation3 orientation = OpenBveApi.Math.Orientation3.Default;
							double angle = 2.0 * Math.PI * Program.RandomNumberGenerator.NextDouble();
							orientation.Rotate(OpenBveApi.Math.Vector3.Up, Math.Cos(angle), Math.Sin(angle));
							ObjectGrid.Grid.Add(apiHandle.LibraryIndex, position, orientation);
						}
					}
				}
			} else if (CurrentOptions.ContentType.Equals("route", StringComparison.OrdinalIgnoreCase)) {
				/*
				 * Load the specified route.
				 * */
				string title = System.IO.Path.GetFileName(CurrentOptions.ContentFile);
				OpenBveApi.Path.PathReference path = new OpenBveApi.Path.FileReference(CurrentOptions.ContentFile);
				System.Text.Encoding encoding = System.Text.Encoding.UTF8;
				OpenBveApi.General.Origin origin = new OpenBveApi.General.Origin(path, null, encoding);
				OpenBveApi.Route.RouteData route;
				if (Interfaces.Host10.LoadRoute(origin, out route) == OpenBveApi.General.Result.Successful) {
					OpenBveApi.Route.DirectionalLight light = route.LightingModel as OpenBveApi.Route.DirectionalLight;
					if (light != null) {
						Renderer.InitializeLighting(light);
					}
					Camera.Position = route.Position;
					Camera.Orientation = route.Orientation;
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Route loaded successfully: " + title);
					Console.ForegroundColor = ConsoleColor.Gray;
				} else {
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Route could not be loaded: " + title);
					Console.ForegroundColor = ConsoleColor.Gray;
				}
			}
			ObjectGrid.Grid.CreateVisibilityLists(Program.CurrentOptions.ViewingDistance);
			
			Console.WriteLine("GL_TRIANGLES: " + ObjectLibrary.Triangles.ToString());
			Console.WriteLine("GL_TRIANGLE_STRIP: " + ObjectLibrary.TriangleStrips.ToString());
			Console.WriteLine("GL_TRIANGLE_FAN: " + ObjectLibrary.TriangleFans.ToString());
			Console.WriteLine("GL_QUADS: " + ObjectLibrary.Quads.ToString());
			Console.WriteLine("GL_QUAD_STRIP: " + ObjectLibrary.QuadStrips.ToString());
			Console.WriteLine("GL_POLYGON: " + ObjectLibrary.Polygons.ToString());
			
		}
		
		/// <summary>Gets the startup path for this program.</summary>
		/// <returns>The startup path for this program.</returns>
		/// <remarks>If retrieving the startup path fails, this returns either the working directory, or an empty string.</remarks>
		private static string GetStartupPath() {
			try {
				return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			} catch {
				try {
					return Environment.CurrentDirectory;
				} catch {
					return string.Empty;
				}
			}
		}
		
		/// <summary>Deinitializes all subsystems.</summary>
		private static void Deinitialize() {
			Renderer.Deinitialize();
			Screen.Deinitialize();
			Plugins.Deinitialize();
			string file = System.IO.Path.Combine(StartupPath, "log.txt");
			System.IO.File.WriteAllText(file, Log.ToString(), new UTF8Encoding(true));
		}
		
	}
}