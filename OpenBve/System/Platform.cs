using System;

namespace OpenBve {
	/// <summary>Stores information about the current platform.</summary>
	internal static class Platform {
		
		// enumerations
		internal enum PlatformTypes {
			Unknown,
			Windows,
			Linux,
			MacOsX
		}
		internal enum CliTypes {
			Unknown,
			Mono
		}
		
		// members
		internal static PlatformTypes PlatformType;
		internal static CliTypes CliType;
		
		// initialize
		internal static void Initialize() {
			// platform
			int platform = (int)Environment.OSVersion.Platform;
			if (platform == 4 | platform == 128) {
				PlatformType = PlatformTypes.Linux;
			} else if (platform == 6) {
				PlatformType = PlatformTypes.MacOsX;
			} else {
				PlatformType = PlatformTypes.Windows;
			}
			// cli
			if (Type.GetType("Mono.Runtime") != null) {
				CliType = CliTypes.Mono;
			} else {
				CliType = CliTypes.Unknown;
			}
		}

	}
	
}