using System;

namespace OpenBveApi {
	/// <summary>Provides structures to store route data.</summary>
	public static class Route {
		
		// TODO: This is a stub. Expand as necessary.
		
		/// <summary>Represents the route.</summary>
		public class RouteData {
			// members
			/// <summary>The default position of the camera.</summary>
			public Math.Vector3 Position;
			/// <summary>The default orientation of the camera.</summary>
			public Math.Orientation3 Orientation;
			/// <summary>The lighting model used by this route.</summary>
			public LightingModel LightingModel;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="position">The default position of the camera.</param>
			/// <param name="orientation">The default orientation of the camera.</param>
			/// <param name="lightingModel">The lighting model used by this route.</param>
			public RouteData(Math.Vector3 position, Math.Orientation3 orientation, LightingModel lightingModel) {
				this.Position = position;
				this.Orientation = orientation;
				this.LightingModel = lightingModel;
			}
		}
		/// <summary>Represents an abstract lighting model.</summary>
		public abstract class LightingModel { }
		
		/// <summary>Represents a lighting model with a static, directional light source.</summary>
		public class DirectionalLight : LightingModel {
			// members
			/// <summary>The ambient light color.</summary>
			public Color.ColorRGB AmbientLight;
			/// <summary>The diffuse light color.</summary>
			public Color.ColorRGB DiffuseLight;
			/// <summary>The specular light color.</summary>
			public Color.ColorRGB SpecularLight;
			/// <summary>The direction the light shines at.</summary>
			public Math.Vector3 LightDirection;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="ambientLight">The ambient light color.</param>
			/// <param name="diffuseLight">The diffuse light color.</param>
			/// <param name="specularLight">The specular light color.</param>
			/// <param name="lightDirection">The direction the light shines at.</param>
			public DirectionalLight(Color.ColorRGB ambientLight, Color.ColorRGB diffuseLight, Color.ColorRGB specularLight, Math.Vector3 lightDirection) {
				this.AmbientLight = ambientLight;
				this.DiffuseLight = diffuseLight;
				this.SpecularLight = specularLight;
				this.LightDirection = lightDirection;
			}
		}
		
	}
}