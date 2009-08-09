using System;

namespace OpenBve {
	internal static class Program {
		
		// members
		internal static string Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		
		// main
		internal static void Main(string[] Arguments) {
			
			// initialize
			Platform.Initialize();
			OpenBveApi.Path.PathReference pluginPath = new OpenBveApi.Path.PathReference(OpenBveApi.Path.PathBase.PluginFolder, OpenBveApi.Path.PathType.None, null);
			Plugins.Initialize(Interfaces.Host10.Resolve(pluginPath));
			Timer.Initialize();
			Window.WindowOptions windowOptions = new Window.WindowOptions(640, 480, 32, false, true);
			Window.Initialize(windowOptions);
			const double degrees = 0.0174532925199433;
			double verticalViewingAngle = 45.0 * degrees;
			Camera.ViewportOptions viewportOptions = new Camera.ViewportOptions(verticalViewingAngle, 0.2, 600.0);
			Camera.SetViewport(viewportOptions);
			Renderer.Initialize();
			
			// loop
			Loop.Enter();
			
			// deinitialize
			Renderer.Deinitialize();
			Window.Deinitialize();
			Plugins.Deinitialize();
			
			// finalize
			Console.WriteLine("Program terminated. Press any key to continue...");
			Console.ReadKey(true);
			
		}
		
	}
}