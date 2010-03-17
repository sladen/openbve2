using System;
using System.Globalization;

namespace OpenBveApi {
	/// <summary>Provides general structures and functions.</summary>
	public static class General {
		
		
		// --- priority ---
		
		/// <summary>Represents the priority at which a plugin reports to be capable of supporting an operation.</summary>
		public enum Priority {
			/// <summary>The plugin is not capable of performing the operation.</summary>
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
		
		
		// --- result ---
		
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
			NotAllowed = 7,
			/// <summary>The plugin encountered an internal error, indicating a bug in the plugin, or an unexpected behavior in an external component.</summary>
			InternalError = 8
		}
		
		
		// --- report ---
		
		/// <summary>Represents abstract data for a report.</summary>
		public abstract class ReportData { }
		
		/// <summary>Represents the file or folder that triggered the report.</summary>
		public class ReportSourcePath : ReportData {
			// members
			/// <summary>The path to the file or folder that triggered the report.</summary>
			public string Path;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="path">The path to the file or folder that triggered the report.</param>
			public ReportSourcePath(string path) {
				this.Path = path;
			}
			// instance functions
			/// <summary>Converts this instance to its equivalent string representation.</summary>
			public override string ToString() {
				return this.Path;
			}
		}
		
		/// <summary>Represents the row in the file that triggered the report.</summary>
		public class ReportTextRow : ReportData {
			// members
			/// <summary>The row in the file that triggered the report.</summary>
			public int Row;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="row">The row in the file that triggered the report.</param>
			public ReportTextRow(int row) {
				this.Row = row;
			}
			// instance functions
			/// <summary>Converts this instance to its equivalent string representation.</summary>
			public override string ToString() {
				return this.Row.ToString(CultureInfo.InvariantCulture);
			}
		}
		
		/// <summary>Represents the row in the file that triggered the report.</summary>
		public class ReportTextColumn : ReportData {
			// members
			/// <summary>The column in the file that triggered the report.</summary>
			public int Column;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="column">The column in the file that triggered the report.</param>
			public ReportTextColumn(int column) {
				this.Column = column;
			}
			// instance functions
			/// <summary>Converts this instance to its equivalent string representation.</summary>
			public override string ToString() {
				return this.Column.ToString(CultureInfo.InvariantCulture);
			}
		}
		
		/// <summary>Represents the byte position in the file that triggered the report.</summary>
		public class ReportBytePosition : ReportData {
			// members
			/// <summary>The byte position in the file that triggered the report.</summary>
			public int Position;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="position">The byte position in the file that triggered the report.</param>
			public ReportBytePosition(int position) {
				this.Position = position;
			}
			// instance functions
			/// <summary>Converts this instance to its equivalent string representation.</summary>
			public override string ToString() {
				return this.Position.ToString(CultureInfo.InvariantCulture);
			}
		}
		
		/// <summary>Represents additional information about the location in the file that triggered the report.</summary>
		public class ReportAdditionalLocation : ReportData {
			// members
			/// <summary>Additional information about the location in the file that triggered the report.</summary>
			public string Location;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="location">Additional information about the location in the file that triggered the report.</param>
			public ReportAdditionalLocation(string location) {
				this.Location = location;
			}
			// instance functions
			/// <summary>Converts this instance to its equivalent string representation.</summary>
			public override string ToString() {
				return this.Location;
			}
		}
		
		/// <summary>Represents a description of the problem.</summary>
		public class ReportDescription : ReportData {
			// members
			/// <summary>A description of the problem.</summary>
			public string Description;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="description">A description of the problem.</param>
			public ReportDescription(string description) {
				this.Description = description;
			}
			// instance functions
			/// <summary>Converts this instance to its equivalent string representation.</summary>
			public override string ToString() {
				return this.Description;
			}
		}
		
		/// <summary>Represents a missing element, e.g. a file or folder.</summary>
		public class ReportMissingElement : ReportData {
			// members
			/// <summary>The element that is missing, e.g. a file or folder.</summary>
			public string Element;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="element">The element that is missing, e.g. a file or folder.</param>
			public ReportMissingElement(string element) {
				this.Element = element;
			}
			// instance functions
			/// <summary>Converts this instance to its equivalent string representation.</summary>
			public override string ToString() {
				return this.Element;
			}
		}

		[Obsolete]
		public enum ReportType {
			Miscellaneous = 0,
			FileNotFound = 1,
			InvalidData = 2
		}
		
		[Obsolete]
		public enum ReportKey {
			SourcePath = 1,
			TargetPath = 2,
			Row = 3,
			Column = 4,
			BytePosition = 6,
			Location = 7,
			Description = 8
		}
		
		[Obsolete]
		public struct ReportKeyValuePair {
			public ReportKey Key;
			public object Value;
			public ReportKeyValuePair(ReportKey key, object value) {
				this.Key = key;
				this.Value = value;
			}
		}


		
		// --- plugin reference ---
		
		/// <summary>Represents a reference to a plugin along with associated data.</summary>
		public class PluginReference {
			// members
			/// <summary>The path to the file that represents the plugin.</summary>
			public string File;
			/// <summary>Additional data for the plugin, or a null reference.</summary>
			public object Data;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="file">The path to the file that represents the plugin.</param>
			/// <param name="data">Additional data for the plugin, or a null reference.</param>
			public PluginReference(string file, object data) {
				this.File = file;
				this.Data = data;
			}
			// functions
			/// <summary>Checks whether two plugin references are equal.</summary>
			/// <param name="a">The first plugin reference.</param>
			/// <param name="b">The second plugin reference.</param>
			/// <returns>A boolean indicating whether the two plugin references are equal.</returns>
			public static bool Equals(PluginReference a, PluginReference b) {
				if (a == null & b == null) {
					return true;
				} else if (a == null | b == null) {
					return false;
				} else if (a.File != b.File) {
					return false;
				} else if (a.Data != b.Data) {
					return false;
				} else {
					return true;
				}
			}
		}
		
		
		// --- origin ---
		
		/// <summary>Represents where data originates from.</summary>
		public struct Origin {
			// members
			/// <summary>A reference to the file or folder where the data is stored.</summary>
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
			/// <param name="a">The first origin.</param>
			/// <param name="b">The second origin.</param>
			/// <returns>A boolean indicating whether the origins are equal.</returns>
			public static bool Equals(Origin a, Origin b) {
				if (OpenBveApi.Path.PathReference.Equals(a.Path, b.Path)) {
					if (PluginReference.Equals(a.Plugin, b.Plugin)) {
						if (a.Encoding == b.Encoding) {
							return true;
						}
					}
				}
				return false;
			}
			// static fields
			/// <summary>Represents an invalid or uninitialized origin.</summary>
			public static readonly Origin Empty = new Origin(null, null, null);
		}
		
	}
}