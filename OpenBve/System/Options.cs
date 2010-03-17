using System;
using System.Globalization;
using System.Text;

namespace OpenBve {
	internal class Options {
		
		// --- enumerations ---
		
		internal enum TextureInterpolationMode {
			NearestNeighbor = 0,
			NearestNeighborMipmapped = 1,
			Bilinear = 2,
			BilinearMipmapped = 3,
			TrilinearMipmapped = 4,
			AnisotropicFiltering = 5
		}
		
		// --- members ---
		
		/// <summary>The width of the output window in pixels.</summary>
		internal int Width;
		/// <summary>The height of the output window in pixels.</summary>
		internal int Height;
		/// <summary>Whether to enable fullscreen mode.</summary>
		internal bool Fullscreen;
		/// <summary>Whether to enable vertical synchronization.</summary>
		internal bool VSync;
		/// <summary>The viewing distance in meters.</summary>
		internal double ViewingDistance;
		/// <summary>The texture interpolation mode.</summary>
		internal TextureInterpolationMode InterpolationMode;
		/// <summary>Whether to enable smooth text rendering.</summary>
		internal bool SmoothTextRendering;
		/// <summary>The bits per pixel for the red channel.</summary>
		internal int RedSize;
		/// <summary>The bits per pixel for the green channel.</summary>
		internal int GreenSize;
		/// <summary>The bits per pixel for the blue channel.</summary>
		internal int BlueSize;
		/// <summary>The bits per pixel for the alpha channel.</summary>
		internal int AlphaSize;
		/// <summary>The bits per pixel for the depth buffer.</summary>
		internal int DepthSize;
		
		internal int ObjectOptimization;
		internal bool BlockClipping;
		internal int FacesPerDisplayList;
		internal double SortInterval;
		internal double GridSize;
		internal bool ShowGrid;
		
		internal string ContentType;
		internal string ContentFile;
		internal int ContentCount;
		
		
		// --- constructors ---
		
		internal Options() {
			this.Width = 640;
			this.Height = 400;
			this.Fullscreen = false;
			this.VSync = true;
			this.ViewingDistance = 600.0;
			this.InterpolationMode = TextureInterpolationMode.BilinearMipmapped;
			this.SmoothTextRendering = true;
			this.RedSize = 8;
			this.GreenSize = 8;
			this.BlueSize = 8;
			this.AlphaSize = 0;
			this.DepthSize = 16;
			this.ObjectOptimization = 1;
			this.BlockClipping = true;
			this.FacesPerDisplayList = 100;
			this.SortInterval = 0.1;
			this.GridSize = 100.0;
			this.ShowGrid = false;
			this.ContentType = null;
			this.ContentFile = null;
			this.ContentCount = 1;
		}
		
		
		// --- static functions ---
		
		internal static Options LoadFromFile(string file) {
			Options options = new Options();
			CultureInfo culture = CultureInfo.InvariantCulture;
			string[] lines = OpenBveApi.Text.GetLinesFromFile(file, Encoding.UTF8);
			for (int i = 0; i < lines.Length; i++) {
				int semicolon = lines[i].IndexOf(';');
				if (semicolon >= 0) {
					lines[i] = lines[i].Substring(0, semicolon).Trim();
				} else {
					lines[i] = lines[i].Trim();
				}
				if (lines[i].Length != 0) {
					int equals = lines[i].IndexOf('=');
					if (equals >= 0) {
						string key = lines[i].Substring(0, equals).TrimEnd();
						string value = lines[i].Substring(equals + 1).TrimStart();
						switch (key.ToLowerInvariant()) {
							case "width":
								options.Width = int.Parse(value, culture);
								break;
							case "height":
								options.Height = int.Parse(value, culture);
								break;
							case "fullscreen":
								options.Fullscreen = value.Equals("true", StringComparison.OrdinalIgnoreCase);
								break;
							case "vsync":
								options.VSync = value.Equals("true", StringComparison.OrdinalIgnoreCase);
								break;
							case "viewingdistance":
								options.ViewingDistance = double.Parse(value, culture);
								break;
							case "redsize":
								options.RedSize = int.Parse(value, culture);
								break;
							case "greensize":
								options.GreenSize = int.Parse(value, culture);
								break;
							case "bluesize":
								options.BlueSize = int.Parse(value, culture);
								break;
							case "alphasize":
								options.AlphaSize = int.Parse(value, culture);
								break;
							case "depthsize":
								options.DepthSize = int.Parse(value, culture);
								break;
							case "interpolationmode":
								options.InterpolationMode = (TextureInterpolationMode)int.Parse(value, culture);
								break;
							case "objectoptimization":
								options.ObjectOptimization = int.Parse(value, culture);
								break;
							case "blockclipping":
								options.BlockClipping = value.Equals("true", StringComparison.OrdinalIgnoreCase);
								break;
							case "facesperdisplaylist":
								options.FacesPerDisplayList = int.Parse(value, culture);
								break;
							case "sortinterval":
								options.SortInterval = double.Parse(value, culture);
								break;
							case "gridsize":
								options.GridSize = double.Parse(value, culture);
								break;
							case "showgrid":
								options.ShowGrid = value.Equals("true", StringComparison.OrdinalIgnoreCase);
								break;
							case "contenttype":
								options.ContentType = value;
								break;
							case "contentfile":
								options.ContentFile = value;
								break;
							case "contentcount":
								options.ContentCount = int.Parse(value, culture);
								break;
						}
					}
				}
			}
			return options;
		}
		
	}
}