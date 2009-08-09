using System;

namespace OpenBveApi {
	/// <summary>Provides general structures and functions.</summary>
	public static class General {
		
		// priority
		/// <summary>Represents the priority at which a plugin reports to be capable of supporting an operation.</summary>
		public enum Priority {
			/// <summary>The plugin is not capable of the operation.</summary>
			NotCapable = 0,
			/// <summary>A very low priority.</summary>
			VeryLow = 1,
			/// <summary>A low priority.</summary>
			Low = 2,
			/// <summary>A normal priority.</summary>
			Normal = 3,
			/// <summary>A high priority.</summary>
			High = 4,
			/// <summary>A very high priority.</summary>
			VeryHigh = 5
		}
		
		// result
		/// <summary>Represents the result of an operation.</summary>
		public enum Result {
			/// <summary>The operation was successful.</summary>
			Successful = 0,
			/// <summary>The requested file could not be found.</summary>
			FileNotFound = 1,
			/// <summary>The requested folder could not be found.</summary>
			FolderNotFound = 2,
			/// <summary>A plugin to handle the operation could not be found.</summary>
			PluginNotFound = 3,
			/// <summary>An argument given to the operation was invalid.</summary>
			InvalidArgument = 4,
			/// <summary>The operation encountered invalid data that could not be recovered from.</summary>
			InvalidData = 5,
			/// <summary>The component does not support the operation.</summary>
			NotSupported = 6,
			/// <summary>The component does not allow the operation at the moment.</summary>
			NotAllowed = 7
		}
		
		// report type
		/// <summary>Represents the type of report to be submitted to the host application.</summary>
		public enum ReportType {
			/// <summary>Anything else. </summary>
			Miscellaneous = 0,
			/// <summary>A requested file could not be found.</summary>
			FileNotFound = 1,
			/// <summary>Invalid data was encountered.</summary>
			InvalidData = 2
		}
		
		// key value pair
		/// <summary>Represents a key-value pair.</summary>
		public struct KeyValuePair {
			// members
			/// <summary>The key.</summary>
			public string Key;
			/// <summary>The value.</summary>
			public object Value;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="key">The key.</param>
			/// <param name="value">The value.</param>
			public KeyValuePair(string key, object value) {
				this.Key = key;
				this.Value = value;
			}
		}
		
		// plugin reference
		/// <summary>Represents a reference to a plugin along with associated data.</summary>
		public class PluginReference {
			// members
			/// <summary>The file that represents the plugin.</summary>
			public Path.PathReference Path;
			/// <summary>Additional data for the plugin, or a null reference.</summary>
			public object Data;
			// constructors
			
			// functions
			/// <summary>Checks whether two plugin references are equal.</summary>
			/// <param name="host">A reference to the host interface.</param>
			/// <param name="a">The first plugin reference.</param>
			/// <param name="b">The second plugin reference.</param>
			/// <returns>A boolean indicating whether the two plugin references are equal.</returns>
			public static bool Equals(IHost host, PluginReference a, PluginReference b) {
				if (a == null & b == null) {
					return true;
				} else if (a == null | b == null) {
					return false;
				} else if (!OpenBveApi.Path.PathReference.Equals(a.Path, b.Path)) {
					return false;
				} else if (a.Data != b.Data) {
					return false;
				} else {
					return true;
				}
			}
		}
		
		// origin
		/// <summary>Represents where data originates from.</summary>
		public struct Origin {
			// members
			/// <summary>A reference to the file or folder where the data is stored, or a null reference.</summary>
			public Path.PathReference Path;
			/// <summary>A reference to a plugin that should be used to load the data, or a null reference.</summary>
			/// <remarks>If no plugin is specified, the host application may choose any compatible plugin at its own discretion.</remarks>
			public General.PluginReference Plugin;
			/// <summary>The suggested encoding, or a null reference.</summary>
			public System.Text.Encoding Encoding;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="path">A reference to the file or folder where the data is stored, or a null reference.</param>
			/// <param name="plugin">A reference to a plugin that should be used to load the data, or a null reference.</param>
			/// <param name="encoding">The suggested encoding, or a null reference.</param>
			public Origin(Path.PathReference path, General.PluginReference plugin, System.Text.Encoding encoding) {
				this.Path = path;
				this.Plugin = plugin;
				this.Encoding = encoding;
			}
			// functions
			/// <summary>Checks whether two origins are equal.</summary>
			/// <param name="host">A reference to the host interface.</param>
			/// <param name="a">The first origin.</param>
			/// <param name="b">The second origin.</param>
			/// <returns>A boolean indicating whether the origins are equal.</returns>
			public static bool Equals(IHost host, Origin a, Origin b) {
				if (!OpenBveApi.Path.PathReference.Equals(host, a.Path, b.Path)) return false;
				if (!PluginReference.Equals(host, a.Plugin, b.Plugin)) return false;
				if (a.Encoding != b.Encoding) return false;
				return true;
			}
		}
		
	}
}