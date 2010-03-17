using System;

namespace Plugin {
	internal static class IO {
		
		// --- functions ---
		
		/// <summary>Wrapper function for reporting general X object errors to host.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="description">String containing information about the encountered problem.</param>
		internal static void ReportError(string fileName, string description) {
			Interfaces.Host.Report(
				new OpenBveApi.General.ReportSourcePath(fileName),
				new OpenBveApi.General.ReportDescription(description)
			);
		}
		
		/// <summary>Wrapper function for reporting missing texture files referenced by X objects to host.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="materialIndex">String containing the material template index with a missing texture.</param>
		/// <param name="missingFile">String containing the texture path and filename.</param>
		internal static void ReportError(string fileName, string materialIndex, string missingFile) {
			Interfaces.Host.Report(
				new OpenBveApi.General.ReportSourcePath(fileName),
				new OpenBveApi.General.ReportAdditionalLocation("Material template index " + materialIndex),
				new OpenBveApi.General.ReportMissingElement("Could not load texture file " + missingFile)
			);
		}
		
		/// <summary>Wrapper function for reporting specifically binary or textual X object errors to host.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="isBinaryX">Whether or not this error pertains to a binary X object.</param>
		/// <param name="description">String containing information about the encountered problem.</param>
		internal static void ReportError(string fileName, bool isBinaryX, string description) {
			string formatType = "textual";
			if (isBinaryX) {
				formatType = "binary";
			}
			Interfaces.Host.Report(
				new OpenBveApi.General.ReportSourcePath(fileName),
				new OpenBveApi.General.ReportDescription(description + " in " + formatType + " x object file")
			);
		}
		
		/// <summary>Wrapper function for reporting generic X object errors to host.</summary>
		/// <param name="fileName">The platform-specific absolute file name of the object.</param>
		/// <param name="row">The row number on which the error was found.</param>
		/// <param name="description">String containing information about the encountered problem.</param>
		internal static void ReportError(string fileName, int row, string description) {
			Interfaces.Host.Report(
				new OpenBveApi.General.ReportSourcePath(fileName),
				new OpenBveApi.General.ReportTextRow(row),
				new OpenBveApi.General.ReportDescription(description)
			);
		}
	}
}