using System;

namespace OpenBveApi {
	/// <summary>Provides structures, enumerations and functions for cross-platform file access.</summary>
	public static class Path {
		
		// path base
		/// <summary>Represents the base folder for a file or folder reference.</summary>
		public enum PathBase {
			/// <summary>The path is absolute.</summary>
			AbsolutePath = 0,
			/// <summary>The path is relative to the folder in which common plugins are stored.</summary>
			PluginFolder = 1,
			/// <summary>The path is relative to the BVE-style railway folder.</summary>
			BveRailwayFolder = 2,
			/// <summary>The path is relative to the BVE-style route folder.</summary>
			BveRouteFolder = 3,
			/// <summary>The path is relative to the BVE-style object folder.</summary>
			BveObjectFolder = 4,
			/// <summary>The path is relative to the BVE-style sound folder.</summary>
			BveSoundFolder = 5,
			/// <summary>The path is relative to the BVE-style train folder.</summary>
			BveTrainFolder = 6,
		}
		
		// path type
		/// <summary>Represents the type of a path, i.e. file or folder.</summary>
		public enum PathType {
			/// <summary>Represents a void path.</summary>
			None = 0,
			/// <summary>Represents a file.</summary>
			File = 1,
			/// <summary>Represents a folder.</summary>
			Folder = 2
		}
		
		// reference
		/// <summary>Represents a reference to a file or folder.</summary>
		public class PathReference {
			// members
			/// <summary>The base folder.</summary>
			public PathBase Base;
			/// <summary>The type of the relative path.</summary>
			public PathType Type;
			/// <summary>The path relative to the base folder.</summary>
			public string Path;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="pathBase">The base folder.</param>
			/// <param name="pathType">The type of the relative path.</param>
			/// <param name="path">The path relative to the base folder.</param>
			public PathReference(PathBase pathBase, PathType pathType, string path) {
				this.Base = pathBase;
				this.Type = pathType;
				this.Path = path;
			}
			// functions
			/// <summary>Checks whether two path references are equal.</summary>
			/// <param name="host">A reference to the host interface.</param>
			/// <param name="a">The first path reference.</param>
			/// <param name="b">The second path reference.</param>
			/// <returns>A boolean indicating whether the two path references are equal.</returns>
			/// <remarks>Two path references are considered equal whenever they point to the same file or folder.</remarks>
			public static bool Equals(IHost host, PathReference a, PathReference b) {
				if (a == null & b == null) {
					return true;
				} else if (a == null | b == null) {
					return false;
				} else {
					if (a.Type == PathType.File & b.Type == PathType.File) {
						string fileA = host.Resolve(a);
						string fileB = host.Resolve(b);
						return fileA == fileB;
					} else if (a.Type == PathType.Folder & b.Type == PathType.Folder) {
						string folderA = host.Resolve(a);
						string folderB = host.Resolve(b);
						return folderA == folderB;
					} else {
						return false;
					}
				}
			}
		}
		
		// members
		private static bool PlatformIsCaseSensitive = (int)System.Environment.OSVersion.Platform == 4 | (int)System.Environment.OSVersion.Platform == 6 | (int)System.Environment.OSVersion.Platform == 128;
		
		// contains invalid path character
		/// <summary>Checks if an expression contains invalid characters to form a path. Both the slash and backslash characters are considered valid.</summary>
		/// <param name="expression">The expression to check.</param>
		/// <returns>A boolean indicating whether the expression contains invalid characters.</returns>
		public static bool ContainsInvalidPathChars(string expression) {
			char[] chars = new char[] {
				'\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
				'\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F',
				'\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17',
				'\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F',
				'\x22', '\x2A', '\x3A', '\x3C', '\x3E', '\x3F', '\x7C'
			};
			return expression.IndexOfAny(chars) != -1;
		}
		
		// combine file
		/// <summary>Combines a platform-specific base folder with a platform-independent relative file to form a platform-specific absolute path.</summary>
		/// <param name="baseFolder">The platform-specific absolute base folder.</param>
		/// <param name="relativeFile">The platform-independent relative file.</param>
		/// <returns>The platform-specific absolute path.</returns>
		/// <remarks>The relative file is searched for case-insensitively and may contain both the slash and backslash characters as directory separators.</remarks>
		public static string CombineFile(string baseFolder, string relativeFile) {
			relativeFile = relativeFile.Replace('/', System.IO.Path.DirectorySeparatorChar);
			relativeFile = relativeFile.Replace('\\', System.IO.Path.DirectorySeparatorChar);
			if (PlatformIsCaseSensitive) {
				while (true) {
					int index = relativeFile.IndexOf(System.IO.Path.DirectorySeparatorChar);
					if (index >= 0) {
						string parent = relativeFile.Substring(0, index);
						string file = relativeFile.Substring(index + 1);
						string combinedFolder = System.IO.Path.Combine(baseFolder, parent);
						if (System.IO.Directory.Exists(combinedFolder)) {
							baseFolder = combinedFolder;
							relativeFile = file;
						} else {
							try {
								string[] folders = System.IO.Directory.GetDirectories(baseFolder);
								bool found = false;
								for (int i = 0; i < folders.Length; i++) {
									if (string.Equals(folders[i], parent, StringComparison.InvariantCultureIgnoreCase)) {
										baseFolder = System.IO.Path.Combine(baseFolder, folders[i]);
										relativeFile = file;
										found = true;
										break;
									}
								}
								if (!found) {
									return System.IO.Path.Combine(baseFolder, relativeFile);
								}
							} catch {
								return System.IO.Path.Combine(baseFolder, relativeFile);
							}
						}
					} else {
						string combinedFile = System.IO.Path.Combine(baseFolder, relativeFile);
						if (System.IO.File.Exists(combinedFile)) {
							return combinedFile;
						} else {
							try {
								string[] files = System.IO.Directory.GetFiles(baseFolder);
								for (int i = 0; i < files.Length; i++) {
									if (string.Equals(files[i], relativeFile, StringComparison.InvariantCultureIgnoreCase)) {
										return System.IO.Path.Combine(baseFolder, files[i]);
									}
								}
								return System.IO.Path.Combine(baseFolder, relativeFile);
							} catch {
								return System.IO.Path.Combine(baseFolder, relativeFile);
							}
						}
					}
				}
			} else {
				return System.IO.Path.Combine(baseFolder, relativeFile);
			}
		}
		
		// combine folder
		/// <summary>Combines a platform-specific base folder with a platform-independent relative folder to form a platform-specific absolute path.</summary>
		/// <param name="baseFolder">The platform-specific absolute base folder.</param>
		/// <param name="relativeFolder">The platform-independent relative folder.</param>
		/// <returns>The platform-specific absolute path.</returns>
		/// <remarks>The relative folder is searched for case-insensitively and may contain both the slash and backslash characters as directory separators.</remarks>
		public static string CombineFolder(string baseFolder, string relativeFolder) {
			relativeFolder = relativeFolder.Replace('/', System.IO.Path.DirectorySeparatorChar);
			relativeFolder = relativeFolder.Replace('\\', System.IO.Path.DirectorySeparatorChar);
			if (PlatformIsCaseSensitive) {
				while (true) {
					int index = relativeFolder.IndexOf(System.IO.Path.DirectorySeparatorChar);
					if (index >= 0) {
						string parent = relativeFolder.Substring(0, index);
						string folder = relativeFolder.Substring(index + 1);
						string combinedFolder = System.IO.Path.Combine(baseFolder, parent);
						if (System.IO.Directory.Exists(combinedFolder)) {
							baseFolder = combinedFolder;
							relativeFolder = folder;
						} else {
							try {
								string[] folders = System.IO.Directory.GetDirectories(baseFolder);
								bool found = false;
								for (int i = 0; i < folders.Length; i++) {
									if (string.Equals(folders[i], parent, StringComparison.InvariantCultureIgnoreCase)) {
										baseFolder = System.IO.Path.Combine(baseFolder, folders[i]);
										relativeFolder = folder;
										found = true;
										break;
									}
								}
								if (!found) {
									return System.IO.Path.Combine(baseFolder, relativeFolder);
								}
							} catch {
								return System.IO.Path.Combine(baseFolder, relativeFolder);
							}
						}
					} else {
						string combinedFolder = System.IO.Path.Combine(baseFolder, relativeFolder);
						if (System.IO.File.Exists(combinedFolder)) {
							return combinedFolder;
						} else {
							try {
								string[] folders = System.IO.Directory.GetDirectories(baseFolder);
								for (int i = 0; i < folders.Length; i++) {
									if (string.Equals(folders[i], relativeFolder, StringComparison.InvariantCultureIgnoreCase)) {
										return System.IO.Path.Combine(baseFolder, folders[i]);
									}
								}
								return System.IO.Path.Combine(baseFolder, relativeFolder);
							} catch {
								return System.IO.Path.Combine(baseFolder, relativeFolder);
							}
						}
					}
				}
			} else {
				return System.IO.Path.Combine(baseFolder, relativeFolder);
			}
		}

		
	}
}