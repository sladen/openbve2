using System;
using Tao.OpenGl;

namespace OpenBve {

	internal static class Camera {

		// members
		internal static OpenBveApi.Math.Vector3 Position = new OpenBveApi.Math.Vector3(0.0, 0.0, 0.0);
		internal static OpenBveApi.Math.Orientation3 Orientation = OpenBveApi.Math.Orientation3.Default;
		
		// viewport
		/// <summary>Stores options required to set up the viewport.</summary>
		internal struct ViewportOptions {
			/// <summary>The horizontal viewing angle in radians.</summary>
			internal double HorizontalViewingAngle;
			/// <summary>The vertical viewing angle in radians.</summary>
			internal double VerticalViewingAngle;
			internal double NearClippingPlane;
			internal double FarClippingPlane;
			/// <summary>Creates a new instance of the ViewportOptions structure.</summary>
			/// <param name="HorizontalViewingAngle">The horizontal viewing angle in radians.</param>
			/// <param name="VerticalViewingAngle">The vertical viewing angle in radians.</param>
			/// <param name="NearClippingPlane">The distance to the near clipping plane in meters.</param>
			/// <param name="FarClippingPlane">The distance to the far clipping plane in meters.</param>
			internal ViewportOptions(double HorizontalViewingAngle, double VerticalViewingAngle, double NearClippingPlane, double FarClippingPlane) {
				this.HorizontalViewingAngle = HorizontalViewingAngle;
				this.VerticalViewingAngle = VerticalViewingAngle;
				this.NearClippingPlane = NearClippingPlane;
				this.FarClippingPlane = FarClippingPlane;
			}
			/// <summary>Creates a new instance of the ViewportOptions structure.</summary>
			/// <param name="VerticalViewingAngle">The vertical viewing angle in radians.</param>
			/// <param name="NearClippingPlane">The distance to the near clipping plane in meters.</param>
			/// <param name="FarClippingPlane">The distance to the far clipping plane in meters.</param>
			/// <remarks>The horizontal viewing angle is determined automatically from the vertical viewing angle and the window's aspect ratio.</remarks>
			internal ViewportOptions(double VerticalViewingAngle, double NearClippingPlane, double FarClippingPlane) {
				this.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * VerticalViewingAngle) * Window.Properties.AspectRatio);
				this.VerticalViewingAngle = VerticalViewingAngle;
				this.NearClippingPlane = NearClippingPlane;
				this.FarClippingPlane = FarClippingPlane;
			}
		}
		/// <summary>Sets the viewport.</summary>
		/// <param name="Options">The options to set up the viewport.</param>
		internal static void SetViewport(ViewportOptions Options) {
			Gl.glViewport(0, 0, Window.Properties.Width, Window.Properties.Height);
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();
			const double inverseDegrees = 57.295779513082320877;
			double aspectRatio = Math.Tan(0.5 * Options.HorizontalViewingAngle) / Math.Tan(0.5 * Options.VerticalViewingAngle);
			Glu.gluPerspective(Options.VerticalViewingAngle * inverseDegrees, -aspectRatio, Options.NearClippingPlane, Options.FarClippingPlane);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glLoadIdentity();
		}
		
	}
	
}