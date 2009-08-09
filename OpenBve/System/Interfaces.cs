using System;
using System.Drawing;
using System.Text;

namespace OpenBve {
	internal static class Interfaces {
		
		// host interface
		internal class HostInterface : OpenBveApi.IHost10 {
			
			
			// --- general ---
			/// <summary>Reports a problem to the host application.</summary>
			/// <param name="type">The type of problem to be reported.</param>
			/// <param name="data">A list of key-value pairs containing information about the problem.</param>
			/// <remarks>Commonly used keys include "Source", "File", "Row", "Column", "Position" or "Text".</remarks>
			public void Report(OpenBveApi.General.ReportType type, params OpenBveApi.General.KeyValuePair[] data) {
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("Report:");
				for (int i = 0; i < data.Length; i++) {
					Console.Write(" " + data[i].Key + "=" + data[i].Value.ToString());
				}
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.Gray;
			}
			
			
			// --- path ---
			
			/// <summary>Resolves a path reference into a platform-specific absolute path.</summary>
			/// <param name="path">The path reference to resolve.</param>
			/// <returns>The platform-specific absolute path, or a null reference if the path could not be resolved.</returns>
			public string Resolve(OpenBveApi.Path.PathReference path) {
				string baseFolder;
				switch (path.Base) {
					case OpenBveApi.Path.PathBase.AbsolutePath:
						baseFolder = null;
						break;
					case OpenBveApi.Path.PathBase.PluginFolder:
						baseFolder = OpenBveApi.Path.CombineFolder(Program.Path, "Plugins");
						break;
					default:
						throw new NotImplementedException();
				}
				if (path.Type == OpenBveApi.Path.PathType.None) {
					return baseFolder;
				} else if (path.Path == null) {
					return null;
				} else {
					if (baseFolder == null) {
						if (path.Base == OpenBveApi.Path.PathBase.AbsolutePath) {
							return path.Path;
						} else {
							return null;
						}
					} else if (path.Type == OpenBveApi.Path.PathType.File) {
						return OpenBveApi.Path.CombineFile(baseFolder, path.Path);
					} else if (path.Type == OpenBveApi.Path.PathType.Folder) {
						return OpenBveApi.Path.CombineFolder(baseFolder, path.Path);
					} else {
						return null;
					}
				}
			}
			
			
			// --- textures ---
			
			/// <summary>Loads a texture suitable for post-processing.</summary>
			/// <param name="origin">The origin of the texture which includes a valid path.</param>
			/// <param name="texture">Receives the texture.</param>
			/// <returns>The success of the operation.</returns>
			public OpenBveApi.General.Result LoadTexture(OpenBveApi.General.Origin origin, out Bitmap texture) {
				if (origin.Path == null) {
					texture = null;
					return OpenBveApi.General.Result.InvalidArgument;
				} else {
					// collect information
					string pluginPath;
					object pluginData;
					if (origin.Plugin != null) {
						pluginData = origin.Plugin.Data;
						if (origin.Plugin.Path != null) {
							if (origin.Plugin.Path.Type != OpenBveApi.Path.PathType.File) {
								texture = null;
								return OpenBveApi.General.Result.InvalidArgument;
							} else {
								pluginPath = Resolve(origin.Plugin.Path);
								pluginData = origin.Plugin.Data;
							}
						} else {
							pluginPath = null;
						}
					} else {
						pluginPath = null;
						pluginData = null;
					}
					string path = Resolve(origin.Path);
					int skipIndex = -1;
					// use specific plugin if available
					if (pluginPath != null) {
						for (int i = 0; i < Plugins.TextureLoadingPlugins.Length; i++) {
							if (string.Equals(Plugins.TextureLoadingPlugins[i].Path, pluginPath, StringComparison.OrdinalIgnoreCase)) {
								OpenBveApi.General.Priority priority = Plugins.TextureLoadingPlugins[i].Api.CanLoadTexture(origin.Path.Type, path, origin.Encoding, pluginData);
								if (priority != OpenBveApi.General.Priority.NotCapable) {
									OpenBveApi.General.Result result = Plugins.TextureLoadingPlugins[i].Api.LoadTexture(origin.Path.Type, path, origin.Encoding, pluginData, out texture);
									return result;
								} else {
									skipIndex = i;
									break;
								}
							}
						}
					}
					// find all compatible plugins
					OpenBveApi.General.Priority[] pluginPriorities = new OpenBveApi.General.Priority[Plugins.TextureLoadingPlugins.Length];
					int[] pluginIndices = new int[Plugins.TextureLoadingPlugins.Length];
					int pluginCount = 0;
					for (int i = 0; i < Plugins.TextureLoadingPlugins.Length; i++) {
						if (i != skipIndex) {
							OpenBveApi.General.Priority priority = Plugins.TextureLoadingPlugins[i].Api.CanLoadTexture(origin.Path.Type, path, origin.Encoding, pluginData);
							if (priority != OpenBveApi.General.Priority.NotCapable) {
								pluginPriorities[pluginCount] = priority;
								pluginIndices[pluginCount] = i;
								pluginCount++;
							}
						}
					}
					// use plugin with highest priority if available
					if (pluginCount != 0) {
						Array.Sort<OpenBveApi.General.Priority, int>(pluginPriorities, pluginIndices, 0, pluginCount);
						int i = pluginIndices[pluginCount - 1];
						OpenBveApi.General.Result result = Plugins.TextureLoadingPlugins[i].Api.LoadTexture(origin.Path.Type, path, origin.Encoding, pluginData, out texture);
						return result;
					} else {
						texture = null;
						return OpenBveApi.General.Result.PluginNotFound;
					}
				}
			}
			
			/// <summary>Registers a texture with the host application.</summary>
			/// <param name="origin">The origin of the texture which includes a valid path.</param>
			/// <param name="parameters">The parameters for the texture.</param>
			/// <param name="handle">Receives a handle to the texture.</param>
			/// <returns>The success of the operation.</returns>
			public OpenBveApi.General.Result RegisterTexture(OpenBveApi.General.Origin origin, OpenBveApi.Texture.TextureParameters parameters, out OpenBveApi.Texture.TextureHandle handle) {
				handle = OpenBveApi.Texture.TextureHandle.Null;
				return OpenBveApi.General.Result.NotSupported;
			}
			
			/// <summary>Registers a texture with the host application.</summary>
			/// <param name="texture">The texture to register.</param>
			/// <param name="parameters">The parameters for the texture.</param>
			/// <param name="handle">Receives a handle to the texture.</param>
			/// <returns>The success of the operation.</returns>
			public OpenBveApi.General.Result RegisterTexture(Bitmap texture, OpenBveApi.Texture.TextureParameters parameters, out OpenBveApi.Texture.TextureHandle handle) {
				handle = OpenBveApi.Texture.TextureHandle.Null;
				return OpenBveApi.General.Result.NotSupported;
			}
			
			
			// --- objects ---
			
			/// <summary>Loads an object from a file, suitable for post-processing.</summary>
			/// <param name="origin">The origin of the object which includes a valid path.</param>
			/// <param name="obj">Receives the object.</param>
			/// <returns>The success of the operation.</returns>
			public OpenBveApi.General.Result LoadObject(OpenBveApi.General.Origin origin, out OpenBveApi.Geometry.GenericObject obj) {
				if (origin.Path == null) {
					obj = null;
					return OpenBveApi.General.Result.InvalidArgument;
				} else {
					// collect information
					string pluginPath;
					object pluginData;
					if (origin.Plugin != null) {
						pluginData = origin.Plugin.Data;
						if (origin.Plugin.Path != null) {
							if (origin.Plugin.Path.Type != OpenBveApi.Path.PathType.File) {
								obj = null;
								return OpenBveApi.General.Result.InvalidArgument;
							} else {
								pluginPath = Resolve(origin.Plugin.Path);
								pluginData = origin.Plugin.Data;
							}
						} else {
							pluginPath = null;
						}
					} else {
						pluginPath = null;
						pluginData = null;
					}
					string path = Resolve(origin.Path);
					int skipIndex = -1;
					// use specific plugin if available
					if (pluginPath != null) {
						for (int i = 0; i < Plugins.ObjectLoadingPlugins.Length; i++) {
							if (string.Equals(Plugins.ObjectLoadingPlugins[i].Path, pluginPath, StringComparison.OrdinalIgnoreCase)) {
								OpenBveApi.General.Priority priority = Plugins.ObjectLoadingPlugins[i].Api.CanLoadObject(origin.Path.Type, path, origin.Encoding, pluginData);
								if (priority != OpenBveApi.General.Priority.NotCapable) {
									OpenBveApi.General.Result result = Plugins.ObjectLoadingPlugins[i].Api.LoadObject(origin.Path.Type, path, origin.Encoding, pluginData, out obj);
									return result;
								} else {
									skipIndex = i;
									break;
								}
							}
						}
					}
					// find all compatible plugins
					OpenBveApi.General.Priority[] pluginPriorities = new OpenBveApi.General.Priority[Plugins.ObjectLoadingPlugins.Length];
					int[] pluginIndices = new int[Plugins.ObjectLoadingPlugins.Length];
					int pluginCount = 0;
					for (int i = 0; i < Plugins.ObjectLoadingPlugins.Length; i++) {
						if (i != skipIndex) {
							OpenBveApi.General.Priority priority = Plugins.ObjectLoadingPlugins[i].Api.CanLoadObject(origin.Path.Type, path, origin.Encoding, pluginData);
							if (priority != OpenBveApi.General.Priority.NotCapable) {
								pluginPriorities[pluginCount] = priority;
								pluginIndices[pluginCount] = i;
								pluginCount++;
							}
						}
					}
					// use plugin with highest priority if available
					if (pluginCount != 0) {
						Array.Sort<OpenBveApi.General.Priority, int>(pluginPriorities, pluginIndices, 0, pluginCount);
						int i = pluginIndices[pluginCount - 1];
						OpenBveApi.General.Result result = Plugins.ObjectLoadingPlugins[i].Api.LoadObject(origin.Path.Type, path, origin.Encoding, pluginData, out obj);
						return result;
					} else {
						obj = null;
						return OpenBveApi.General.Result.PluginNotFound;
					}
				}
			}
			
			
			// --- sound ---
			
			/// <summary>Loads a sound from a file, suitable for post-processing.</summary>
			/// <param name="origin">The origin of the sound which includes a valid path.</param>
			/// <param name="sound">Receives the sound data.</param>
			/// <returns>The success of the operation.</returns>
			public OpenBveApi.General.Result LoadSound(OpenBveApi.General.Origin origin, out OpenBveApi.Sound.SoundData sound) {
				sound = null;
				return OpenBveApi.General.Result.NotSupported;
			}
			
			/// <summary>Registers a sound with the host application.</summary>
			/// <param name="origin">The origin of the sound which includes a valid path.</param>
			/// <param name="handle">Receives a handle to the sound.</param>
			/// <returns>The success of the operation.</returns>
			public OpenBveApi.General.Result RegisterSound(OpenBveApi.General.Origin origin, out OpenBveApi.Sound.SoundHandle handle) {
				handle = OpenBveApi.Sound.SoundHandle.Null;
				return OpenBveApi.General.Result.NotSupported;
			}
			
			/// <summary>Registers a sound with the host application.</summary>
			/// <param name="sound">The sound data to register.</param>
			/// <param name="handle">Receives a handle to the sound.</param>
			/// <returns>The success of the operation.</returns>
			public OpenBveApi.General.Result RegisterSound(OpenBveApi.Sound.SoundData sound, out OpenBveApi.Sound.SoundHandle handle) {
				handle = OpenBveApi.Sound.SoundHandle.Null;
				return OpenBveApi.General.Result.NotSupported;
			}

		}
		
		// members
		internal static HostInterface Host10 = new HostInterface();
		
	}
}