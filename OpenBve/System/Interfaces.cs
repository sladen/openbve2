using OpenBveApi;
using System;

namespace OpenBve {
	internal static class Interfaces {
		
		// host interface
		internal class HostInterface : IHost10 {
			
			
			// --- general ---
			
			[Obsolete]
			public void Report(General.ReportType type, params General.ReportKeyValuePair[] keyValuePairs) {
				Program.Log.Append(type.ToString()).AppendLine(":");
				for (int i = 0; i < keyValuePairs.Length; i++) {
					Program.Log.Append('\t').Append(keyValuePairs[i].Key.ToString()).Append(" = ").Append(keyValuePairs[i].Value.ToString()).AppendLine();
				}
				Program.Log.AppendLine();
			}
			
			/// <summary>Reports a problem to the host application.</summary>
			/// <param name="data">Information about the problem, e.g. the location where it occured, and a description.</param>
			public void Report(params General.ReportData[] data) {
				Program.Log.AppendLine("Report:");
				for (int i = 0; i < data.Length; i++) {
					string reportType = data[i].GetType().ToString();
					if (reportType.StartsWith("OpenBveApi.General+Report")) {
						reportType = reportType.Substring(25);
					}
					string reportValue = data[i].ToString();
					Program.Log.Append('\t').Append(reportType).Append(": ").AppendLine(reportValue);
				}
				Program.Log.AppendLine();
			}
			

			// --- textures (loading) ---
			
			/// <summary>Loads a texture suitable for post-processing.</summary>
			/// <param name="origin">The origin of the texture.</param>
			/// <param name="texture">Receives the texture.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result LoadTexture(General.Origin origin, out Texture.TextureData texture) {
				if (origin.Path == null) {
					texture = null;
					return General.Result.InvalidArgument;
				} else {
					int skipIndex = -1;
					object data;
					// use specific plugin if available
					if (origin.Plugin != null) {
						data = origin.Plugin.Data;
						for (int i = 0; i < Plugins.TextureLoadingPlugins.Length; i++) {
							if (string.Equals(Plugins.TextureLoadingPlugins[i].File == origin.Plugin.File, StringComparison.OrdinalIgnoreCase)) {
								General.Priority priority = Plugins.TextureLoadingPlugins[i].Api.CanLoadTexture(origin.Path, origin.Encoding, data);
								if (priority != General.Priority.NotCapable) {
									General.Result result = Plugins.TextureLoadingPlugins[i].Api.LoadTexture(origin.Path, origin.Encoding, data, out texture);
									return result;
								} else {
									skipIndex = i;
									break;
								}
							}
						}
					} else {
						data = null;
					}
					// find all compatible plugins
					General.Priority[] pluginPriorities = new General.Priority[Plugins.TextureLoadingPlugins.Length];
					int[] pluginIndices = new int[Plugins.TextureLoadingPlugins.Length];
					int pluginCount = 0;
					for (int i = 0; i < Plugins.TextureLoadingPlugins.Length; i++) {
						if (i != skipIndex) {
							General.Priority priority = Plugins.TextureLoadingPlugins[i].Api.CanLoadTexture(origin.Path, origin.Encoding, data);
							if (priority != General.Priority.NotCapable) {
								pluginPriorities[pluginCount] = priority;
								pluginIndices[pluginCount] = i;
								pluginCount++;
							}
						}
					}
					// use plugin with highest priority if available
					if (pluginCount == 1) {
						General.Result result = Plugins.TextureLoadingPlugins[pluginIndices[0]].Api.LoadTexture(origin.Path, origin.Encoding, data, out texture);
						return result;
					} else if (pluginCount != 0) {
						Array.Sort<General.Priority, int>(pluginPriorities, pluginIndices, 0, pluginCount);
						int i = pluginIndices[pluginCount - 1];
						General.Result result = Plugins.TextureLoadingPlugins[pluginIndices[i]].Api.LoadTexture(origin.Path, origin.Encoding, data, out texture);
						return result;
					} else {
						texture = null;
						return General.Result.PluginNotFound;
					}
				}
			}
			
			/// <summary>Registers a texture with the host application.</summary>
			/// <param name="origin">The origin of the texture which includes a valid path.</param>
			/// <param name="parameters">The parameters for the texture.</param>
			/// <param name="handle">Receives a handle to the texture.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result RegisterTexture(General.Origin origin, Texture.TextureParameters parameters, out Texture.TextureHandle handle) {
				int index;
				Textures.RegisterTexture(origin, parameters, out index);
				handle = new Textures.ApiHandle(index);
				return General.Result.Successful;
			}
			
			/// <summary>Registers a texture with the host application.</summary>
			/// <param name="texture">The texture to register.</param>
			/// <param name="parameters">The parameters for the texture.</param>
			/// <param name="handle">Receives a handle to the texture.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result RegisterTexture(Texture.TextureData texture, Texture.TextureParameters parameters, out Texture.TextureHandle handle) {
				// TODO: Implement this.
				handle = null;
				return General.Result.NotSupported;
			}
			
			
			// --- objects (loading) ---
			
			/// <summary>Loads an object from a file, suitable for post-processing.</summary>
			/// <param name="origin">The origin of the object which includes a valid path.</param>
			/// <param name="obj">Receives the object.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result LoadObject(General.Origin origin, out Geometry.GenericObject obj) {
				if (origin.Path == null) {
					obj = null;
					return General.Result.InvalidArgument;
				} else {
					int skipIndex = -1;
					object data;
					// use specific plugin if available
					if (origin.Plugin != null) {
						data = origin.Plugin.Data;
						for (int i = 0; i < Plugins.ObjectLoadingPlugins.Length; i++) {
							if (string.Equals(Plugins.ObjectLoadingPlugins[i].File == origin.Plugin.File, StringComparison.OrdinalIgnoreCase)) {
								General.Priority priority = Plugins.ObjectLoadingPlugins[i].Api.CanLoadObject(origin.Path, origin.Encoding, data);
								if (priority != General.Priority.NotCapable) {
									General.Result result = Plugins.ObjectLoadingPlugins[i].Api.LoadObject(origin.Path, origin.Encoding, data, out obj);
									return result;
								} else {
									skipIndex = i;
									break;
								}
							}
						}
					} else {
						data = null;
					}
					// find all compatible plugins
					General.Priority[] pluginPriorities = new General.Priority[Plugins.ObjectLoadingPlugins.Length];
					int[] pluginIndices = new int[Plugins.ObjectLoadingPlugins.Length];
					int pluginCount = 0;
					for (int i = 0; i < Plugins.ObjectLoadingPlugins.Length; i++) {
						if (i != skipIndex) {
							General.Priority priority = Plugins.ObjectLoadingPlugins[i].Api.CanLoadObject(origin.Path, origin.Encoding, data);
							if (priority != General.Priority.NotCapable) {
								pluginPriorities[pluginCount] = priority;
								pluginIndices[pluginCount] = i;
								pluginCount++;
							}
						}
					}
					// use plugin with highest priority if available
					if (pluginCount == 1) {
						General.Result result = Plugins.ObjectLoadingPlugins[pluginIndices[0]].Api.LoadObject(origin.Path, origin.Encoding, data, out obj);
						return result;
					} else if (pluginCount != 0) {
						Array.Sort<General.Priority, int>(pluginPriorities, pluginIndices, 0, pluginCount);
						int i = pluginIndices[pluginCount - 1];
						General.Result result = Plugins.ObjectLoadingPlugins[pluginIndices[i]].Api.LoadObject(origin.Path, origin.Encoding, data, out obj);
						return result;
					} else {
						obj = null;
						return General.Result.PluginNotFound;
					}
				}
			}
			
			/// <summary>Registers an object with the host application.</summary>
			/// <param name="obj">The object to register.</param>
			/// <param name="handle">Receives a handle to the object.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result RegisterObject(Geometry.GenericObject obj, out Geometry.ObjectHandle handle) {
				int libraryIndex = ObjectLibrary.Library.Add(obj);
				handle = new ObjectLibrary.ApiHandle(libraryIndex);
				return General.Result.Successful;
			}
			
			/// <summary>Creates a new instance of an object at a specified position with a specified orietation.</summary>
			/// <param name="handle">The handle to the object.</param>
			/// <param name="position">The position of the object.</param>
			/// <param name="orientation">The orientation of the object.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result CreateObject(Geometry.ObjectHandle handle, OpenBveApi.Math.Vector3 position, OpenBveApi.Math.Orientation3 orientation) {
				// TODO: This operation should not be allowed at runtime.
				ObjectLibrary.ApiHandle apiHandle = handle as ObjectLibrary.ApiHandle;
				if (apiHandle != null) {
					ObjectGrid.Grid.Add(apiHandle.LibraryIndex, position, orientation);
					return General.Result.Successful;
				} else {
					return General.Result.InvalidArgument;
				}
			}

			
			// --- sound (loading) ---
			
			/// <summary>Loads a sound from a file, suitable for post-processing.</summary>
			/// <param name="origin">The origin of the sound which includes a valid path.</param>
			/// <param name="sound">Receives the sound data.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result LoadSound(General.Origin origin, out Sound.SoundData sound) {
				// TODO: Implement this.
				sound = null;
				return General.Result.NotSupported;
			}
			
			/// <summary>Registers a sound with the host application.</summary>
			/// <param name="origin">The origin of the sound which includes a valid path.</param>
			/// <param name="handle">Receives a handle to the sound.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result RegisterSound(General.Origin origin, out Sound.SoundBufferHandle handle) {
				// TODO: Implement this.
				handle = null;
				return General.Result.NotSupported;
			}
			
			/// <summary>Registers a sound with the host application.</summary>
			/// <param name="sound">The sound data to register.</param>
			/// <param name="handle">Receives a handle to the sound.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result RegisterSound(Sound.SoundData sound, out Sound.SoundBufferHandle handle) {
				// TODO: Implement this.
				handle = null;
				return General.Result.NotSupported;
			}
			
			
			// --- route (loading) ---
			
			/// <summary>Loads a route.</summary>
			/// <param name="origin">The origin of the route which includes a valid path.</param>
			/// <param name="route">Receives the route data.</param>
			/// <returns>The success of the operation.</returns>
			internal General.Result LoadRoute(General.Origin origin, out Route.RouteData route) {
				if (origin.Path == null) {
					route = null;
					return General.Result.InvalidArgument;
				} else {
					int skipIndex = -1;
					object data;
					// use specific plugin if available
					if (origin.Plugin != null) {
						data = origin.Plugin.Data;
						for (int i = 0; i < Plugins.RouteLoadingPlugins.Length; i++) {
							if (string.Equals(Plugins.RouteLoadingPlugins[i].File == origin.Plugin.File, StringComparison.OrdinalIgnoreCase)) {
								General.Priority priority = Plugins.RouteLoadingPlugins[i].Api.CanLoadRoute(origin.Path, origin.Encoding, data);
								if (priority != General.Priority.NotCapable) {
									General.Result result = Plugins.RouteLoadingPlugins[i].Api.LoadRoute(origin.Path, origin.Encoding, data, out route);
									return result;
								} else {
									skipIndex = i;
									break;
								}
							}
						}
					} else {
						data = null;
					}
					// find all compatible plugins
					General.Priority[] pluginPriorities = new General.Priority[Plugins.RouteLoadingPlugins.Length];
					int[] pluginIndices = new int[Plugins.RouteLoadingPlugins.Length];
					int pluginCount = 0;
					for (int i = 0; i < Plugins.RouteLoadingPlugins.Length; i++) {
						if (i != skipIndex) {
							General.Priority priority = Plugins.RouteLoadingPlugins[i].Api.CanLoadRoute(origin.Path, origin.Encoding, data);
							if (priority != General.Priority.NotCapable) {
								pluginPriorities[pluginCount] = priority;
								pluginIndices[pluginCount] = i;
								pluginCount++;
							}
						}
					}
					// use plugin with highest priority if available
					if (pluginCount == 1) {
						General.Result result = Plugins.RouteLoadingPlugins[pluginIndices[0]].Api.LoadRoute(origin.Path, origin.Encoding, data, out route);
						return result;
					} else if (pluginCount != 0) {
						Array.Sort<General.Priority, int>(pluginPriorities, pluginIndices, 0, pluginCount);
						int i = pluginIndices[pluginCount - 1];
						General.Result result = Plugins.RouteLoadingPlugins[pluginIndices[i]].Api.LoadRoute(origin.Path, origin.Encoding, data, out route);
						return result;
					} else {
						route = null;
						return General.Result.PluginNotFound;
					}
				}
			}

			
			
			// --- sound (runtime) ---
			
			/// <summary>Plays the specified sound buffer and returns a handle by which the generated sound source can be identified.</summary>
			/// <param name="bufferHandle">A handle to the sound buffer.</param>
			/// <param name="position">The absolute position at which to play the sound.</param>
			/// <param name="velocity">The velocity vector at which the sound travels.</param>
			/// <param name="pitch">The pitch of the sound, where 1 represents nominal pitch.</param>
			/// <param name="volume">The volume of the sound, where 1 represents nominal volume.</param>
			/// <param name="looped">A boolean indicating whether to play the sound in an endless loop.</param>
			/// <param name="sourceHandle">Receives a handle to the sound source.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result PlaySound(Sound.SoundBufferHandle bufferHandle, OpenBveApi.Math.Vector3 position, OpenBveApi.Math.Vector3 velocity, double pitch, double volume, bool looped, out Sound.SoundSourceHandle sourceHandle) {
				// TODO: Implement this.
				sourceHandle = null;
				return General.Result.NotSupported;
			}
			
			/// <summary>Updates an already playing sound.</summary>
			/// <param name="handle">A handle to the sound source.</param>
			/// <param name="position">The absolute position at which to play the sound.</param>
			/// <param name="velocity">The velocity vector at which the sound travels.</param>
			/// <param name="pitch">The pitch of the sound, where 1 represents nominal pitch.</param>
			/// <param name="volume">The volume of the sound, where 1 represents nominal volume.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result UpdateSound(Sound.SoundSourceHandle handle, OpenBveApi.Math.Vector3 position, OpenBveApi.Math.Vector3 velocity, double pitch, double volume) {
				// TODO: Implement this.
				return General.Result.NotSupported;
			}
			
			/// <summary>Stops playing the specified sound source.</summary>
			/// <param name="handle">The sound source to stop playing.</param>
			public General.Result StopSound(ref Sound.SoundSourceHandle handle) {
				// TODO: Implement this.
				return General.Result.NotSupported;
			}
			
			
			// --- plugins (runtime) ---

			/// <summary>Queries data from another runtime plugin.</summary>
			/// <param name="pluginType">The plugin to query data from.</param>
			/// <param name="contentType">The type of content to query.</param>
			/// <param name="contentData">Receives the queried content.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result QueryPluginData(int pluginType, int contentType, out object contentData) {
				// TODO: Implement this.
				contentData = null;
				return General.Result.NotSupported;
			}
			
			/// <summary>Submits data to another runtime plugin.</summary>
			/// <param name="pluginType">The plugin to submit data to.</param>
			/// <param name="contentType">The type of content to submit.</param>
			/// <param name="contentData">The data to submit.</param>
			/// <returns>The success of the operation.</returns>
			public General.Result SubmitPluginData(int pluginType, int contentType, object contentData) {
				// TODO: Implement this.
				return General.Result.NotSupported;
			}

		}
		
		// members
		internal static HostInterface Host10 = new HostInterface();
		
	}
}