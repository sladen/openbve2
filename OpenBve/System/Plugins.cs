using System;
using System.Reflection;

namespace OpenBve {
	/// <summary>Provides functions to manage plugins.</summary>
	internal static class Plugins {
		
		// plugin information
		internal class PluginInformation {
			// members
			internal string File;
			internal Assembly Dll;
			internal OpenBveApi.IPlugin10 Api;
			// constructors
			internal PluginInformation(string file, Assembly dll, OpenBveApi.IPlugin10 api) {
				this.File = file;
				this.Dll = dll;
				this.Api = api;
			}
		}
		
		// members
		internal static PluginInformation[] AllAvailablePlugins = null;
		internal static PluginInformation[] TextureLoadingPlugins = null;
		internal static PluginInformation[] ObjectLoadingPlugins = null;
		internal static PluginInformation[] SoundLoadingPlugins = null;
		internal static PluginInformation[] RouteLoadingPlugins = null;
		
		// initialize
		internal static void Initialize(string folder) {
			// all available plugins
			{
				int count = 0;
				AllAvailablePlugins = new PluginInformation[16];
				string[] files = System.IO.Directory.GetFiles(folder);
				for (int i = 0; i < files.Length; i++) {
					if (files[i].EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) {
						try {
							Assembly dll = Assembly.LoadFile(files[i]);
							Type[] types = dll.GetTypes();
							for (int j = 0; j < types.Length; j++) {
								if (types[j].IsPublic & (types[j].Attributes & TypeAttributes.Abstract) == 0) {
									object instance = dll.CreateInstance(types[j].FullName);
									if (instance is OpenBveApi.IPlugin10) {
										OpenBveApi.IPlugin10 api = (OpenBveApi.IPlugin10)instance;
										OpenBveApi.IHost[] hosts = new OpenBveApi.IHost[] { Interfaces.Host10 };
										if (api.Load(hosts)) {
											PluginInformation information = new PluginInformation(files[i], dll, api);
											if (AllAvailablePlugins.Length == count) {
												Array.Resize<PluginInformation>(ref AllAvailablePlugins, AllAvailablePlugins.Length << 1);
											}
											AllAvailablePlugins[count] = information;
											count++;
											Console.WriteLine("Plugin loaded: " + System.IO.Path.GetFileName(files[i]));
										}
									}
								}
							}
						} catch (Exception ex) {
							ConsoleColor color = Console.ForegroundColor;
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("Error while loading plugin " + System.IO.Path.GetFileName(files[i]) + ": " + ex.Message);
							Console.ForegroundColor = color;
						}
					}
				}
				Array.Resize<PluginInformation>(ref AllAvailablePlugins, count);
			}
			// texture-loading plugins
			{
				int count = 0;
				TextureLoadingPlugins = new PluginInformation[AllAvailablePlugins.Length];
				for (int i = 0; i < AllAvailablePlugins.Length; i++) {
					if (AllAvailablePlugins[i].Api.CanLoadTextures()) {
						TextureLoadingPlugins[count] = AllAvailablePlugins[i];
						Console.WriteLine("Plugin can load textures: " + System.IO.Path.GetFileName(AllAvailablePlugins[i].File));
						count++;
					}
				}
				Array.Resize<PluginInformation>(ref TextureLoadingPlugins, count);
			}
			// object-loading plugins
			{
				int count = 0;
				ObjectLoadingPlugins = new PluginInformation[AllAvailablePlugins.Length];
				for (int i = 0; i < AllAvailablePlugins.Length; i++) {
					if (AllAvailablePlugins[i].Api.CanLoadObjects()) {
						ObjectLoadingPlugins[count] = AllAvailablePlugins[i];
						Console.WriteLine("Plugin can load objects: " + System.IO.Path.GetFileName(AllAvailablePlugins[i].File));
						count++;
					}
				}
				Array.Resize<PluginInformation>(ref ObjectLoadingPlugins, count);
			}
			// sound-loading plugins
			{
				int count = 0;
				SoundLoadingPlugins = new PluginInformation[AllAvailablePlugins.Length];
				for (int i = 0; i < AllAvailablePlugins.Length; i++) {
					if (AllAvailablePlugins[i].Api.CanLoadSounds()) {
						SoundLoadingPlugins[count] = AllAvailablePlugins[i];
						Console.WriteLine("Plugin can load sounds: " + System.IO.Path.GetFileName(AllAvailablePlugins[i].File));
						count++;
					}
				}
				Array.Resize<PluginInformation>(ref SoundLoadingPlugins, count);
			}
			// route-loading plugins
			{
				int count = 0;
				RouteLoadingPlugins = new PluginInformation[AllAvailablePlugins.Length];
				for (int i = 0; i < AllAvailablePlugins.Length; i++) {
					if (AllAvailablePlugins[i].Api.CanLoadRoutes()) {
						RouteLoadingPlugins[count] = AllAvailablePlugins[i];
						Console.WriteLine("Plugin can load routes: " + System.IO.Path.GetFileName(AllAvailablePlugins[i].File));
						count++;
					}
				}
				Array.Resize<PluginInformation>(ref RouteLoadingPlugins, count);
			}
		}
		
		// deinitialize
		internal static void Deinitialize() {
			for (int i = 0; i < AllAvailablePlugins.Length; i++) {
				AllAvailablePlugins[i].Api.Unload();
				Console.WriteLine("Plugin unloaded: " + System.IO.Path.GetFileName(AllAvailablePlugins[i].File));
			}
			AllAvailablePlugins = null;
			TextureLoadingPlugins = null;
			ObjectLoadingPlugins = null;
			SoundLoadingPlugins = null;
			RouteLoadingPlugins = null;
		}
		
	}
}