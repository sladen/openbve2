using System;

namespace Plugin
{
	internal static partial class Parser {
		
		// structures
		/// <summary>Represents material properties.</summary>
		private struct Material {
			// members
			internal OpenBveApi.Color.ColorRGBA FaceColor;
			internal OpenBveApi.Color.ColorRGB SpecularColor;
			internal OpenBveApi.Color.ColorRGB EmissiveColor;
			internal string TextureFilename;
		}
		
		private struct BinaryCache {
			// members
			internal int[] Integers;
			internal int IntegersRemaining;
			internal double[] Floats;
			internal int FloatsRemaining;
		}
		
		// data
		/// <summary>Class for working with template data loaded from an X object file.</summary>
		private class Structure {
			// members
			internal string Name;
			internal object[] Data;
			// constructors
			/// <summary>Creates a new instance of a structure.</summary>
			/// <param name="Name">The name of the template.</param>
			/// <param name="Data">The data associated with the template.</param>
			internal Structure(string name, object[] data) {
				this.Name = name;
				this.Data = data;
			}
		}
		
		// template
		/// <summary>Class representing X object templates.</summary>
		private class Template {
			// members
			internal string Name;
			internal string[] Members;
			// constructors
			/// <summary>Creates a new instance of a template.</summary>
			/// <param name="Name">The name of the template.</param>
			/// <param name="Members">The members of the template.</param>
			internal Template(string name, string[] members) {
				this.Name = name;
				this.Members = members;
			}
		}
		// supported templates
		private static Template[] Templates = new Template[] {
			new Template("Mesh", new string[] { "DWORD", "Vector[0]", "DWORD", "MeshFace[2]", "[...]" }),
			new Template("Vector", new string[] { "float", "float", "float" }),
			new Template("MeshFace", new string[] { "DWORD", "DWORD[0]" }),
			new Template("MeshMaterialList", new string[] { "DWORD", "DWORD", "DWORD[1]", "[...]" }),
			new Template("Material", new string[] { "ColorRGBA", "float", "ColorRGB", "ColorRGB", "[...]" }),
			new Template("ColorRGBA", new string[] { "float", "float", "float", "float" }),
			new Template("ColorRGB", new string[] { "float", "float", "float" }),
			new Template("TextureFilename", new string[] { "string" }),
			new Template("MeshTextureCoords", new string[] { "DWORD", "Coords2d[0]" }),
			new Template("Coords2d", new string[] { "float", "float" }),
			new Template("MeshNormals", new string[] { "DWORD", "Vector[0]", "DWORD", "MeshFace[2]" })
		};
	}
}