using System;
using Tao.OpenGl;

namespace OpenBve {
	internal class DisplayList {
		
		// -- members --
		
		/// <summary>Whether this display list has been assigned an OpenGL list.</summary>
		private bool Assigned;
		
		/// <summary>The OpenGL list index. This field may only be queried if the display list is assigned.</summary>
		private int OpenGlIndex;
		
		
		// -- constructors --
		
		/// <summary>Creates a new instance of this class.</summary>
		internal DisplayList() {
			this.Assigned = false;
			this.OpenGlIndex = 0;
		}
		
		
		// -- instance functions --
		
		/// <summary>Begins creating the display list.</summary>
		/// <param name="state">Receives the default OpenGL state.</param>
		/// <remarks>This method invokes calls to glGenLists and glNewList.</summary>
		internal void Begin(out Renderer.OpenGlState state) {
			if (!this.Assigned) {
				this.OpenGlIndex = Gl.glGenLists(1);
				this.Assigned = true;
			}
			Gl.glNewList(this.OpenGlIndex, Gl.GL_COMPILE);
			state = new Renderer.OpenGlState();
		}
		
		/// <summary>Ends creating the display list.</summary>
		/// <param name="state">The current OpenGL state.</param>
		/// <remarks>This method invokes a call to glEndList.</summary>
		internal void End(ref Renderer.OpenGlState state) {
			if (this.Assigned) {
				state.Reset();
				Gl.glEndList();
			}
		}
		
		/// <summary>Calls this display list.</summary>
		/// <remarks>This method invokes a call to glCallList.</summary>
		internal void Call() {
			if (this.Assigned) {
				Gl.glCallList(this.OpenGlIndex);
			}
		}
		
		/// <summary>Destroys the display list.</summary>
		/// <remarks>This method invokes a call to glDeleteLists.</summary>
		internal void Destroy() {
			if (this.Assigned) {
				Gl.glDeleteLists(this.OpenGlIndex, 1);
				this.Assigned = false;
			}
		}
		
		/// <summary>Checks whether this display list is available.</summary>
		/// <remarks>The display list is available after the Begin and End calls have been made.</remarks>
		/// <remarks>The display list is unavailable after the Destroy call has been made.</remarks>
		internal bool IsAvailable() {
			return this.Assigned;
		}
		
		/// <summary>Checks whether this display list is unavailable.</summary>
		/// <remarks>The display list is available after the Begin and End calls have been made.</remarks>
		/// <remarks>The display list is unavailable after the Destroy call has been made.</remarks>
		internal bool IsUnavailable() {
			return !this.Assigned;
		}
	}
	
}