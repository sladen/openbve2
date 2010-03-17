using System;

namespace OpenBve {
	internal static partial class Renderer {

		// --- members ---
		
		/// <summary>The current light position.</summary>
		private static OpenBveApi.Math.Vector3 LightPosition = OpenBveApi.Math.Vector3.Up;
		/// <summary>The current ambient color.</summary>
		private static OpenBveApi.Color.ColorRGB LightAmbientColor = new OpenBveApi.Color.ColorRGB(0.6f, 0.6f, 0.6f);
		/// <summary>The current diffuse color.</summary>
		private static OpenBveApi.Color.ColorRGB LightDiffuseColor = new OpenBveApi.Color.ColorRGB(0.6f, 0.6f, 0.6f);
		
		
		private static OpenBveApi.Math.Vector3 SunPosition;
		private static OpenBveApi.Math.Vector3 MoonPosition;
		private static bool RoundTheClockLighting = false;
		
		
		// --- functions ---

		internal static void InitializeLighting(OpenBveApi.Route.DirectionalLight light) {
			LightPosition = -light.LightDirection;
			LightAmbientColor = light.AmbientLight;
			LightDiffuseColor = light.DiffuseLight;
		}
		
		private static void UpdateLighting(double secondsSinceMidnight) {
			if (RoundTheClockLighting) {
				double hour = (secondsSinceMidnight % 86400.0) / 3600.0;
				LightAmbientColor = OpenBveApi.Color.ColorRGB.Black;
				LightDiffuseColor = OpenBveApi.Color.ColorRGB.Black;
				double sunIntensity;
				double moonIntensity;
				/* The sun */
				{
					double sunAngle = secondsSinceMidnight * 2.0 * Math.PI / 86400.0;
					SunPosition = new OpenBveApi.Math.Vector3(Math.Sin(sunAngle), -Math.Cos(sunAngle), 0.0);
					double r = 0.6 * Math.Max(0.0, Math.Min(1.0, SunPosition.Y + 0.18));
					double g = 0.6 * Math.Max(0.0, Math.Min(1.0, SunPosition.Y + 0.09));
					double b = 0.6 * Math.Max(0.0, Math.Min(1.0, SunPosition.Y + 0.03));
					sunIntensity = (r + g + b) / 3.0;
					LightAmbientColor += new OpenBveApi.Color.ColorRGB((float)r, (float)g, (float)b);
					LightDiffuseColor += new OpenBveApi.Color.ColorRGB((float)r, (float)g, (float)b);
				}
				/* The moon */
				{
					double moonAngle = secondsSinceMidnight * 2.0 * Math.PI / 86400.0 + 0.9 * Math.PI;
					MoonPosition = new OpenBveApi.Math.Vector3(0.0, -Math.Cos(moonAngle), Math.Sin(moonAngle));
					double r = Math.Max(0.0, Math.Min(1.0, 0.05 * MoonPosition.Y + 0.12));
					double g = Math.Max(0.0, Math.Min(1.0, 0.07 * MoonPosition.Y + 0.12));
					double b = Math.Max(0.0, Math.Min(1.0, 0.13 * MoonPosition.Y + 0.12));
					moonIntensity = (r + g + b) / 3.0;
					LightAmbientColor += new OpenBveApi.Color.ColorRGB((float)r, (float)g, (float)b);
					LightDiffuseColor += new OpenBveApi.Color.ColorRGB((float)r, (float)g, (float)b);
				}
				double sunFactor = sunIntensity / (sunIntensity + moonIntensity);
				double moonFactor = moonIntensity / (sunIntensity + moonIntensity);
				LightPosition = SunPosition * sunFactor + MoonPosition * moonFactor;
				LightPosition.Normalize();
			}
		}

		
	}
}