using System;

namespace OpenBveApi {
	/// <summary>Provides structures, enumerations and functions for cross-platform file access.</summary>
	public static class Path {
		
		
		// --- enumerations and classes ---
		
		/// <summary>Represents a reference to a file or folder.</summary>
		public abstract class PathReference {
			// static functions
			/// <summary>Determines whether two path references are equal.</summary>
			/// <param name="a">The first path reference.</param>
			/// <param name="b">The second path reference.</param>
			/// <returns>A boolean indicating whether the path references are equal.</returns>
			public static bool Equals(PathReference a, PathReference b) {
				if (a == null & b == null) {
					return true;
				} else if (a == null | b == null) {
					return false;
				} if (a is FileReference & b is FileReference) {
					FileReference x = (FileReference)a;
					FileReference y = (FileReference)b;
					return x.Path == y.Path;
				} else if (a is FolderReference & b is FolderReference) {
					FolderReference x = (FolderReference)a;
					FolderReference y = (FolderReference)b;
					return x.Path == y.Path;
				} else {
					return false;
				}
			}
		}

		/// <summary>Represents a reference to a file.</summary>
		public class FileReference : PathReference {
			// members
			/// <summary>The platform-specific absolute path to the file.</summary>
			public string Path;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="path">The platform-specific absolute path to the file.</param>
			public FileReference(string path) {
				this.Path = path;
			}
		}

		/// <summary>Represents a reference to a folder.</summary>
		public class FolderReference : PathReference {
			// members
			/// <summary>The platform-specific absolute path to the folder.</summary>
			public string Path;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="path">The platform-specific absolute path to the folder.</param>
			public FolderReference(string path) {
				this.Path = path;
			}
		}

		
		// --- static constructor ---
		
		/// <summary>The static constructor for this class.</summary>
		static Path () {
			try {
				int platform = (int)System.Environment.OSVersion.Platform;
				CaseSensitiveFileSystem = platform < 0 | platform > 3;
			} catch {
				CaseSensitiveFileSystem = true;
			}
		}


		// --- members ---
		
		/// <summary>Indicates whether the current file system is case-sensitive.</summary>
		private static bool CaseSensitiveFileSystem;
		
		/// <summary>A list of characters that may not be used in paths.</summary>
		private static char[] InvalidPathCharacters = new char[] {
			'\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
			'\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F',
			'\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17',
			'\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F',
			'\x22', '\x2A', '\x3A', '\x3C', '\x3E', '\x3F', '\x7C'
		};
		
		
		// --- static functions ---
		
		/// <summary>Checks if an expression contains invalid characters to form a path. Both the slash and backslash characters are considered valid.</summary>
		/// <param name="expression">The expression to check.</param>
		/// <returns>A boolean indicating whether the expression contains invalid characters.</returns>
		/// <exception cref="System.NullReferenceException">Raised when the submitted argument is a null reference.</exception>
		public static bool ContainsInvalidPathCharacters(string expression) {
			return expression.IndexOfAny(InvalidPathCharacters) != -1;
		}
		
		/// <summary>Combines a platform-specific base folder with a platform-independent relative file to form a platform-specific absolute path.</summary>
		/// <param name="baseFolder">The platform-specific absolute base folder.</param>
		/// <param name="relativeFile">The platform-independent relative file.</param>
		/// <returns>The platform-specific absolute path.</returns>
		/// <remarks>The relative file is searched for case-insensitively and may contain both the slash and backslash characters as directory separators.</remarks>
		/// <exception cref="System.NullReferenceException">Raised when any of the submitted arguments are null references.</exception>
		public static string CombineFile(string baseFolder, string relativeFile) {
			relativeFile = relativeFile.Replace('/', System.IO.Path.DirectorySeparatorChar);
			relativeFile = relativeFile.Replace('\\', System.IO.Path.DirectorySeparatorChar);
			if (relativeFile.Length != 0 && relativeFile[0] == System.IO.Path.DirectorySeparatorChar) {
				relativeFile = relativeFile.Substring(1);
			}
			if (CaseSensitiveFileSystem) {
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
									if (string.Equals(folders[i], parent, StringComparison.OrdinalIgnoreCase)) {
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
									if (string.Equals(files[i], relativeFile, StringComparison.OrdinalIgnoreCase)) {
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
		
		/// <summary>Combines a platform-specific base folder with a platform-independent relative folder to form a platform-specific absolute path.</summary>
		/// <param name="baseFolder">The platform-specific absolute base folder.</param>
		/// <param name="relativeFolder">The platform-independent relative folder.</param>
		/// <returns>The platform-specific absolute path.</returns>
		/// <remarks>The relative folder is searched for case-insensitively and may contain both the slash and backslash characters as directory separators.</remarks>
		/// <exception cref="System.NullReferenceException">Raised when any of the submitted arguments are null references.</exception>
		public static string CombineFolder(string baseFolder, string relativeFolder) {
			relativeFolder = relativeFolder.Replace('/', System.IO.Path.DirectorySeparatorChar);
			relativeFolder = relativeFolder.Replace('\\', System.IO.Path.DirectorySeparatorChar);
			if (relativeFolder.Length != 0 && relativeFolder[0] == System.IO.Path.DirectorySeparatorChar) {
				relativeFolder = relativeFolder.Substring(1);
			}
			if (CaseSensitiveFileSystem) {
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
									if (string.Equals(folders[i], parent, StringComparison.OrdinalIgnoreCase)) {
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
									if (string.Equals(folders[i], relativeFolder, StringComparison.OrdinalIgnoreCase)) {
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