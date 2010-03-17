using System;

namespace OpenBve {
	/// <summary>Provides information about the current platform.</summary>
	internal static class Platform {
		

		// --- enumerations ---
		
		/// <summary>Represents an operating system platform.</summary>
		internal enum PlatformTypes {
			/// <summary>The platform is unknown.</summary>
			Unknown,
			/// <summary>The platform is Microsoft Windows.</summary>
			Windows,
			/// <summary>The platform is Unix.</summary>
			/// <remarks>This field is only used if Linux or MacOsX are not detected explicitly.</remarks>
			Unix,
			/// <summary>The platform is Linux.</summary>
			Linux,
			/// <summary>The platform is Mac OS X.</summary>
			MacOsX
		}
		
		/// <summary>Represents a CLI type.</summary>
		internal enum CliTypes {
			/// <summary>The CLI type is unknown.</summary>
			Unknown,
			/// <summary>The CLI is Microsoft.NET.</summary>
			MicrosoftDotNet,
			/// <summary>The CLI is Mono.</summary>
			Mono
		}
		
		
		// --- members ---
		
		/// <summary>The platform this applications runs on.</summary>
		internal static PlatformTypes PlatformType;
		
		/// <summary>The CLI this application runs on.</summary>
		internal static CliTypes CliType;
		
		/// <summary>The number of processors on this machine.</summary>
		internal static int Processors;
		
		/// <summary>The preferred newline style on the current platform.</summary>
		internal static string NewLine;
		
		/// <summary>Whether the file system is case-sensitive.</summary>
		internal static bool CaseSensitiveFileSystem;
		
		
		// --- functions ---
		
		/// <summary>Initializes information about the current platform.</summary>
		internal static void Initialize() {
			// platform
			try {
				int platform = (int)Environment.OSVersion.Platform;
				/*
				 * 0	Win32S
				 * 1	Win32Windows
				 * 2	Win32NT
				 * 3	WinCE
				 * 4	Unix
				 * 5	Xbox
				 * 6	MacOSX
				 * 128	Unix
				 * */
				if (platform >= 0 & platform <= 3) {
					PlatformType = PlatformTypes.Windows;
				} else if (platform == 4 | platform == 128) {
					PlatformType = PlatformTypes.Unix;
				} else if (platform == 6) {
					PlatformType = PlatformTypes.MacOsX;
				} else {
					PlatformType = PlatformTypes.Unknown;
				}
			} catch {
				PlatformType = PlatformTypes.Unknown;
			}
			// cli
			try {
				if (Type.GetType("Mono.Runtime") != null) {
					CliType = CliTypes.Mono;
				} else {
					CliType = CliTypes.Unknown;
				}
			} catch {
				CliType = CliTypes.Unknown;
			}
			// others
			Processors = Environment.ProcessorCount;
			NewLine = Environment.NewLine;
			CaseSensitiveFileSystem = PlatformType != PlatformTypes.Windows;
		}

	}
}