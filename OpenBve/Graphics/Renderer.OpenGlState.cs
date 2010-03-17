using System;
using Tao.OpenGl;

namespace OpenBve {
	internal static partial class Renderer {

		// --- classes ---
		
		/// <summary>Represents the current OpenGL state.</summary>
		/// <remarks>Instead of calling OpenGL functions directly, use the instance methods of this class.</remarks>
		/// <remarks>Call the Reset method at the end of rendering in order to reset the OpenGL state to default values.</remarks>
		/// <remarks>An instance of this class keeps track of the current OpenGL state, and only executes functions that actually invoke a state change.</remarks>
		internal class OpenGlState {
			// members
			private int AlphaFunctionComparison;
			private float AlphaFunctionValue;
			private bool AlphaTest;
			private bool Blend;
			private int BlendSourceFactor;
			private int BlendDestinationFactor;
			private OpenBveApi.Color.ColorRGB EmissiveColor;
			private bool DepthMask;
			private bool DepthTest;
			private int Texture;
			private bool Texturing;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <remarks>Make sure to call the Reset method once you are finished rendering with this state.</remarks>
			internal OpenGlState() {
				this.AlphaFunctionComparison = Gl.GL_ALWAYS;
				this.AlphaFunctionValue = 0.0f;
				this.AlphaTest = false;
				this.Blend = false;
				this.BlendSourceFactor = Gl.GL_SRC_ALPHA;
				this.BlendDestinationFactor = Gl.GL_ONE_MINUS_SRC_ALPHA;
				this.EmissiveColor = OpenBveApi.Color.ColorRGB.Black;
				this.DepthMask = true;
				this.DepthTest = true;
				this.Texture = 0;
				this.Texturing = false;
			}
			// instance functions
			internal void SetAlphaFunction(int function, float value) {
				if (this.AlphaTest) {
					if (this.AlphaFunctionComparison != function | this.AlphaFunctionValue != value) {
						Gl.glAlphaFunc(function, value);
						this.AlphaFunctionComparison = function;
						this.AlphaFunctionValue = value;
					}
				} else {
					Gl.glEnable(Gl.GL_ALPHA_TEST);
					Gl.glAlphaFunc(function, value);
					this.AlphaFunctionComparison = function;
					this.AlphaFunctionValue = value;
					this.AlphaTest = true;
				}
			}
			internal void UnsetAlphaFunction() {
				if (this.AlphaTest) {
					Gl.glDisable(Gl.GL_ALPHA_TEST);
					this.AlphaTest = false;
				}
			}
			internal void SetBlend(int sourceFactor, int destinationFactor) {
				if (!this.Blend) {
					Gl.glEnable(Gl.GL_BLEND);
					Gl.glBlendFunc(sourceFactor, destinationFactor);
					this.Blend = true;
					this.BlendSourceFactor = sourceFactor;
					this.BlendDestinationFactor = destinationFactor;
				} else if (this.BlendSourceFactor != sourceFactor | this.BlendDestinationFactor != destinationFactor) {
					Gl.glBlendFunc(sourceFactor, destinationFactor);
					this.BlendSourceFactor = sourceFactor;
					this.BlendDestinationFactor = destinationFactor;
				}
			}
			internal void UnsetBlend() {
				if (this.Blend) {
					Gl.glDisable(Gl.GL_BLEND);
					this.Blend = false;
				}
			}
			internal void SetEmissiveColor(OpenBveApi.Color.ColorRGB color) {
				if (color != this.EmissiveColor) {
					Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_EMISSION, new float[] { color.R, color.G, color.B, 1.0f });
					this.EmissiveColor = color;
				}
			}
			internal void SetDepthMask(bool value) {
				if (this.DepthMask & !value) {
					Gl.glDepthMask(Gl.GL_FALSE);
					this.DepthMask = false;
				} else if (!this.DepthMask & value) {
					Gl.glDepthMask(Gl.GL_TRUE);
					this.DepthMask = true;
				}
			}
			internal void SetDepthTest(bool value) {
				if (this.DepthTest & !value) {
					Gl.glDisable(Gl.GL_DEPTH_TEST);
					this.DepthTest = false;
				} else if (!this.DepthTest & value) {
					Gl.glEnable(Gl.GL_DEPTH_TEST);
					this.DepthTest = true;
				}
			}
			internal void BindTexture(int openGlTextureIndex) {
				if (this.Texturing) {
					if (this.Texture != openGlTextureIndex) {
						Gl.glBindTexture(Gl.GL_TEXTURE_2D, openGlTextureIndex);
						this.Texture = openGlTextureIndex;
					}
				} else {
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, openGlTextureIndex);
					this.Texture = openGlTextureIndex;
					this.Texturing = true;
				}
			}
			internal void UnbindTexture() {
				if (this.Texturing) {
					Gl.glDisable(Gl.GL_TEXTURE_2D);
					this.Texturing = false;
				}
			}
			/// <summary>Resets those OpenGL states that have changed from the default values.</summary>
			internal void Reset() {
				if (this.AlphaTest) {
					Gl.glDisable(Gl.GL_ALPHA_TEST);
					this.AlphaTest = false;
				}
				if (this.Blend) {
					Gl.glDisable(Gl.GL_BLEND);
					this.Blend = false;
				}
				if (this.EmissiveColor != OpenBveApi.Color.ColorRGB.Black) {
					Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_EMISSION, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
					this.EmissiveColor = OpenBveApi.Color.ColorRGB.Black;
				}
				if (!this.DepthMask) {
					Gl.glDepthMask(Gl.GL_TRUE);
					this.DepthMask = true;
				}
				if (!this.DepthTest) {
					Gl.glEnable(Gl.GL_DEPTH_TEST);
					this.DepthTest = true;
				}
				if (this.Texturing) {
					Gl.glDisable(Gl.GL_TEXTURE_2D);
					this.Texture = 0;
					this.Texturing = false;
				}
			}
			// static functions
			/// <summary>Resets all OpenGL states to default values.</summary>
			/// <remarks>This function should be called before entering the rendering loop.</remarks>
			/// <remarks>This function may also be called at the beginning of a frame in order to ensure that each frame begins with the same state.</remarks>
			internal static void Initialize() {
				Gl.glDisable(Gl.GL_ALPHA_TEST);
				Gl.glDisable(Gl.GL_BLEND);
				Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_EMISSION, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
				Gl.glDepthMask(Gl.GL_TRUE);
				Gl.glEnable(Gl.GL_DEPTH_TEST);
				Gl.glDisable(Gl.GL_TEXTURE_2D);
			}
		}
		
	}
}