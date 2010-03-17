using System;
using System.Text;

namespace Plugin {
	internal static partial class Parser {
		
		/*
		 * This file is about "structures", a term used in CSV routes to mean
		 * objects that are associated to a particular index.
		 
		 * This file contains helper functions to manage those structures and
		 * to load their objects in a delayed fashion: Only once the object of
		 * a structure is actually needed will it be loaded via the host. If
		 * it is never used, it will never be loaded.
		 * */
		
		
		// --- structure ---
		
		/// <summary>Represents the current status of a structure.</summary>
		private enum StructureStatus {
			/// <summary>Indicates that no attempts have yet been made to load this structure.</summary>
			Pending = 0,
			/// <summary>Indicates that a previous attempt to load this structure has failed.</summary>
			CannotLoad = 1,
			/// <summary>Indicates that the structure has been loaded and that the associated object data is populated. No attempt has yet been made to register the object.</summary>
			LoadedPendingRegistration = 2,
			/// <summary>Indicates that the structure has been loaded and that the associated object data is populated. A previous attempt to register the object has failed.</summary>
			LoadedCannotRegister = 3,
			/// <summary>Indicates that the structure has been loaded and that the associated object data is populated. The object has also been registered and the associated handle is populated.</summary>
			LoadedAndRegistered = 4
		}
		
		/// <summary>Represents a structure prototype. The structure is associated a source file and is only loaded when actually needed.</summary>
		private class Structure {
			// members
			/// <summary>The first index associated to this structure.</summary>
			internal int Index1;
			/// <summary>The second index associated to this structure.</summary>
			internal int Index2;
			/// <summary>The file that stores the object.</summary>
			internal string File;
			/// <summary>Whether to mirror the structure on its x-axis.</summary>
			internal bool Mirrored;
			/// <summary>The current status of this structure.</summary>
			internal StructureStatus Status;
			/// <summary>Stores the object if Status is set to StructureStatus.LoadedPendingRegistration, StructureStatus.LoadedCannotRegister or StructureStatus.LoadedAndRegistered, otherwise a null reference.</summary>
			internal OpenBveApi.Geometry.GenericObject Object;
			/// <summary>Stores a handle to the object if Status is set to StructureStatus.LoadedAndRegistered, otherwise a null reference.</summary>
			internal OpenBveApi.Geometry.ObjectHandle Handle;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="index1">The first index associated to this structure.</param>
			/// <param name="index2">The second index associated to this structure.</param>
			/// <param name="file">The file that stores the object.</param>
			/// <param name="mirrored">Whether to mirror the structure on its x-axis.</param>
			/// <remarks>The Status of the created structure is set to StructureStatus.Pending.</remarks>
			internal Structure(int index1, int index2, string file, bool mirrored) {
				this.Index1 = index1;
				this.Index2 = index2;
				this.File = file;
				this.Mirrored = mirrored;
				this.Status = StructureStatus.Pending;
				this.Object = null;
				this.Handle = null;
			}
			// instance functions
			/// <summary>Gets the object associated to this structure.</summary>
			/// <param name="encoding">The fallback encoding for loading objects.</param>
			/// <param name="obj">Receives the object associated to this structure.</param>
			/// <returns>The success of this operation. The operation fails if the object cannot be loaded.</returns>
			internal bool Get(Encoding encoding, out OpenBveApi.Geometry.GenericObject obj) {
				if (this.Status == StructureStatus.Pending) {
					OpenBveApi.Path.FileReference file = new OpenBveApi.Path.FileReference(this.File);
					OpenBveApi.General.Origin origin = new OpenBveApi.General.Origin(file, null, encoding);
					OpenBveApi.General.Result result = Interfaces.Host.LoadObject(origin, out this.Object);
					if (result == OpenBveApi.General.Result.Successful) {
						this.Status = StructureStatus.LoadedPendingRegistration;
						if (this.Mirrored) {
							this.Object.Scale(new OpenBveApi.Math.Vector3(-1.0, 1.0, 1.0));
						}
						obj = this.Object;
						return true;
					} else {
						this.Status = StructureStatus.CannotLoad;
						obj = null;
						return false;
					}
				} else if (
					this.Status == StructureStatus.LoadedPendingRegistration |
					this.Status == StructureStatus.LoadedCannotRegister |
					this.Status == StructureStatus.LoadedAndRegistered
				) {
					obj = this.Object;
					return true;
				} else {
					obj = null;
					return false;
				}
			}
			/// <summary>Gets the handle to the object associated to this structure.</summary>
			/// <param name="encoding">The fallback encoding for loading objects.</param>
			/// <param name="obj">Receives the handle to the object associated to this structure.</param>
			/// <returns>The success of this operation. The operation fails if the object cannot be loaded or registered.</returns>
			internal bool Get(Encoding encoding, out OpenBveApi.Geometry.ObjectHandle handle) {
				if (this.Status == StructureStatus.Pending) {
					OpenBveApi.Path.FileReference file = new OpenBveApi.Path.FileReference(this.File);
					OpenBveApi.General.Origin origin = new OpenBveApi.General.Origin(file, null, encoding);
					OpenBveApi.General.Result result = Interfaces.Host.LoadObject(origin, out this.Object);
					if (result == OpenBveApi.General.Result.Successful) {
						if (this.Mirrored) {
							this.Object.Scale(new OpenBveApi.Math.Vector3(-1.0, 1.0, 1.0));
						}
						result = Interfaces.Host.RegisterObject(this.Object, out this.Handle);
						if (result == OpenBveApi.General.Result.Successful) {
							this.Status = StructureStatus.LoadedAndRegistered;
							handle = this.Handle;
							return true;
						} else {
							this.Status = StructureStatus.LoadedCannotRegister;
							handle = null;
							return false;
						}
					} else {
						this.Status = StructureStatus.CannotLoad;
						handle = null;
						return false;
					}
				} else if (this.Status == StructureStatus.LoadedPendingRegistration) {
					OpenBveApi.General.Result result = Interfaces.Host.RegisterObject(this.Object, out this.Handle);
					if (result == OpenBveApi.General.Result.Successful) {
						this.Status = StructureStatus.LoadedAndRegistered;
						handle = this.Handle;
						return true;
					} else {
						this.Status = StructureStatus.LoadedCannotRegister;
						handle = null;
						return false;
					}
				} else if (this.Status == StructureStatus.LoadedAndRegistered) {
					handle = this.Handle;
					return true;
				} else {
					handle = null;
					return false;
				}
			}
		}
		
		/// <summary>Represents a collection of indexes structures. Functions allow structures to only load or register their objects when actually needed.</summary>
		private class StructureCollection {
			// members
			/// <summary>An array of structures stored in this collection.</summary>
			internal Structure[] Structures;
			/// <summary>The amount of structures stored in this collection.</summary>
			internal int StructureCount;
			/// <summary>Creates a new instance of this class.</summary>
			internal StructureCollection() {
				this.Structures = new Structure[16];
				this.StructureCount = 0;
			}
			// instance functions
			/// <summary>Sets the structure of the specified index to point to the specified file.</summary>
			/// <param name="index1">The first index.</param>
			/// <param name="index2">The second index.</param>
			/// <param name="file">The file to the object. A leading slash or backslash character is ignored.</param>
			/// <param name="mirrored">Whether to mirror the structure on its x-axis.</param>
			/// <remarks>If the structure of the specified indices already exists, its content is overwritten. Otherwise, it will be created.</remarks>
			internal void Set(int index1, int index2, string file, bool mirrored) {
				for (int i = 0; i < this.StructureCount; i++) {
					if (this.Structures[i].Index1 == index1 && this.Structures[i].Index2 == index2) {
						this.Structures[i] = new Structure(index1, index2, file, mirrored);
						return;
					}
				}
				if (this.Structures.Length == this.StructureCount) {
					Array.Resize<Structure>(ref this.Structures, this.Structures.Length << 1);
				}
				this.Structures[this.StructureCount] = new Structure(index1, index2, file, mirrored);
				this.StructureCount++;
			}
			/// <summary>Gets the object associated to the specified structure.</summary>
			/// <param name="index1">The first index.</param>
			/// <param name="index2">The second index.</param>
			/// <param name="encoding">The fallback encoding for loading objects.</param>
			/// <param name="obj">Receives the object associated to the structure.</param>
			/// <returns>The success of this operation. The operation fails if the object cannot be loaded.</returns>
			internal bool Get(int index1, int index2, Encoding encoding, out OpenBveApi.Geometry.GenericObject obj) {
				for (int i = 0; i < this.StructureCount; i++) {
					if (this.Structures[i].Index1 == index1 && this.Structures[i].Index2 == index2) {
						return this.Structures[i].Get(encoding, out obj);
					}
				}
				obj = null;
				return false;
			}
			/// <summary>Gets the handle to the object that is associated to the specified structure.</summary>
			/// <param name="index1">The first index.</param>
			/// <param name="index2">The second index.</param>
			/// <param name="encoding">The fallback encoding for loading objects.</param>
			/// <param name="obj">Receives the handle to the object that is associated to the structure.</param>
			/// <returns>The success of this operation. The operation fails if the object cannot be loaded or registered.</returns>
			internal bool Get(int index1, int index2, Encoding encoding, out OpenBveApi.Geometry.ObjectHandle handle) {
				for (int i = 0; i < this.StructureCount; i++) {
					if (this.Structures[i].Index1 == index1 && this.Structures[i].Index2 == index2) {
						return this.Structures[i].Get(encoding, out handle);
					}
				}
				handle = null;
				return false;
			}
			/// <summary>Checks if the structure with the specified index exists.</summary>
			/// <param name="index1">The first index.</param>
			/// <param name="index2">The second index.</param>
			/// <returns>Whether the structure of the specified index exists.</returns>
			internal bool Exists(int index1, int index2) {
				for (int i = 0; i < this.StructureCount; i++) {
					if (this.Structures[i].Index1 == index1 && this.Structures[i].Index2 == index2) {
						return true;
					}
				}
				return false;
			}
		}
		
		
		// --- cycle ---
		
		private class Cycle {
			// members
			internal int Index;
			internal int[] GroundStructures;
			// constructors
			internal Cycle(int index, int[] groundStructures) {
				this.Index = index;
				this.GroundStructures = groundStructures;
			}
		}
		
		private class CycleCollection {
			// members
			internal Cycle[] Cycles;
			internal int CycleCount;
			// constructors
			internal CycleCollection() {
				this.Cycles = new Cycle[16];
				this.CycleCount = 0;
			}
			// instance functions
			internal bool Get(int index, out int[] groundStructures) {
				for (int i = 0; i < this.CycleCount; i++) {
					if (this.Cycles[i].Index == index) {
						groundStructures = this.Cycles[i].GroundStructures;
						return true;
					}
				}
				groundStructures = null;
				return false;
			}
			internal void Set(int index, int[] groundStructures) {
				for (int i = 0; i < this.CycleCount; i++) {
					if (this.Cycles[i].Index == index) {
						this.Cycles[i].GroundStructures = groundStructures;
						return;
					}
				}
				if (this.CycleCount == this.Cycles.Length) {
					Array.Resize<Cycle>(ref this.Cycles, this.Cycles.Length << 1);
				}
				this.Cycles[this.CycleCount] = new Cycle(index, groundStructures);
				this.CycleCount++;
			}
		}
		
		
		// --- structures ---
		
		private class Structures {
			// members
			internal StructureCollection Ground;
			internal StructureCollection Rail;
			internal StructureCollection WallL;
			internal StructureCollection WallR;
			internal StructureCollection DikeL;
			internal StructureCollection DikeR;
			internal StructureCollection Pole;
			internal StructureCollection PoleMirrored;
			internal StructureCollection FormL;
			internal StructureCollection FormR;
			internal StructureCollection FormCL;
			internal StructureCollection FormCR;
			internal StructureCollection RoofL;
			internal StructureCollection RoofR;
			internal StructureCollection RoofCL;
			internal StructureCollection RoofCR;
			internal StructureCollection CrackL;
			internal StructureCollection CrackR;
			internal StructureCollection FreeObj;
			internal StructureCollection Beacon;
			internal CycleCollection Cycle;
			internal Structure Stop;
			internal StructureCollection Limits;
			// constructors
			internal Structures(string pluginDataFolder, Encoding encoding) {
				this.Ground = new StructureCollection();
				this.Rail = new StructureCollection();
				this.WallL = new StructureCollection();
				this.WallR = new StructureCollection();
				this.DikeL = new StructureCollection();
				this.DikeR = new StructureCollection();
				this.Pole = new StructureCollection();
				this.PoleMirrored = new StructureCollection();
				this.FormL = new StructureCollection();
				this.FormR = new StructureCollection();
				this.FormCL = new StructureCollection();
				this.FormCR = new StructureCollection();
				this.RoofL = new StructureCollection();
				this.RoofR = new StructureCollection();
				this.RoofCL = new StructureCollection();
				this.RoofCR = new StructureCollection();
				this.CrackL = new StructureCollection();
				this.CrackR = new StructureCollection();
				this.FreeObj = new StructureCollection();
				this.Beacon = new Parser.StructureCollection();
				this.Cycle = new CycleCollection();
				this.Limits = new Parser.StructureCollection();
				/*
				 * Set up the default objects.
				 * */
				string poleFolder = OpenBveApi.Path.CombineFolder(pluginDataFolder, "poles");
				this.PoleMirrored.Set(0, 0, OpenBveApi.Path.CombineFile(poleFolder, "pole_1.csv"), true);
				this.Pole.Set(0, 0, OpenBveApi.Path.CombineFile(poleFolder, "pole_1.csv"), false);
				this.Pole.Set(0, 1, OpenBveApi.Path.CombineFile(poleFolder, "pole_2.csv"), false);
				this.Pole.Set(0, 2, OpenBveApi.Path.CombineFile(poleFolder, "pole_3.csv"), false);
				this.Pole.Set(0, 3, OpenBveApi.Path.CombineFile(poleFolder, "pole_4.csv"), false);
				string transponderFolder = OpenBveApi.Path.CombineFolder(pluginDataFolder, "transponders");
				this.Beacon.Set(-1, 0, OpenBveApi.Path.CombineFile(transponderFolder, "s.csv"), false);
				this.Beacon.Set(-2, 0, OpenBveApi.Path.CombineFile(transponderFolder, "sn.csv"), false);
				this.Beacon.Set(-3, 0, OpenBveApi.Path.CombineFile(transponderFolder, "falsestart.csv"), false);
				this.Beacon.Set(-4, 0, OpenBveApi.Path.CombineFile(transponderFolder, "porigin.csv"), false);
				this.Beacon.Set(-5, 0, OpenBveApi.Path.CombineFile(transponderFolder, "pstop.csv"), false);
				string stopFolder = OpenBveApi.Path.CombineFolder(pluginDataFolder, "stops");
				this.Stop = new Parser.Structure(0, 0, OpenBveApi.Path.CombineFile(stopFolder, "stop.csv"), false);
				string limitFolder = OpenBveApi.Path.CombineFolder(pluginDataFolder, "limits");
				this.Limits.Set(0, 0, OpenBveApi.Path.CombineFile(limitFolder, "digit_0.csv"), false);
				this.Limits.Set(1, 0, OpenBveApi.Path.CombineFile(limitFolder, "digit_1.csv"), false);
				this.Limits.Set(2, 0, OpenBveApi.Path.CombineFile(limitFolder, "digit_2.csv"), false);
				this.Limits.Set(3, 0, OpenBveApi.Path.CombineFile(limitFolder, "digit_3.csv"), false);
				this.Limits.Set(4, 0, OpenBveApi.Path.CombineFile(limitFolder, "digit_4.csv"), false);
				this.Limits.Set(5, 0, OpenBveApi.Path.CombineFile(limitFolder, "digit_5.csv"), false);
				this.Limits.Set(6, 0, OpenBveApi.Path.CombineFile(limitFolder, "digit_6.csv"), false);
				this.Limits.Set(7, 0, OpenBveApi.Path.CombineFile(limitFolder, "digit_7.csv"), false);
				this.Limits.Set(8, 0, OpenBveApi.Path.CombineFile(limitFolder, "digit_8.csv"), false);
				this.Limits.Set(9, 0, OpenBveApi.Path.CombineFile(limitFolder, "digit_9.csv"), false);
				this.Limits.Set(10, 0, OpenBveApi.Path.CombineFile(limitFolder, "unlimited.csv"), false);
				this.Limits.Set(11, 0, OpenBveApi.Path.CombineFile(limitFolder, "limit_left.csv"), false);
				this.Limits.Set(12, 0, OpenBveApi.Path.CombineFile(limitFolder, "limit_straight.csv"), false);
				this.Limits.Set(13, 0, OpenBveApi.Path.CombineFile(limitFolder, "limit_right.csv"), false);
			}
		}
		
		
		// --- static functions ---

		/// <summary>Transforms a FormCL, FormCR, RoofCL, RoofCR or Crack object.</summary>
		/// <param name="obj">The object to transform.</param>
		/// <param name="nearDistance">The signed distance from the first rail to the second rail at the beginning of the block.</param>
		/// <param name="farDistance">The signed distance from the first rail to the second rail at the end of the block.</param>
		/// <returns>The transformed object.</returns>
		/// <remarks>Only objects of type OpenBveApi.Geometry.FaceVertexMesh are supported. If other objects are passed, they are returned unmodified.</remarks>
		private static OpenBveApi.Geometry.GenericObject GetTransformedObject(OpenBveApi.Geometry.GenericObject obj, double nearDistance, double farDistance) {
			if (obj is OpenBveApi.Geometry.FaceVertexMesh) {
				OpenBveApi.Geometry.FaceVertexMesh mesh = (OpenBveApi.Geometry.FaceVertexMesh)obj.Clone();
				double x2 = 0.0, x3 = 0.0, x6 = 0.0, x7 = 0.0;
				for (int i = 0; i < mesh.Vertices.Length; i++) {
					switch (mesh.Vertices[i].Tag) {
						case 2:
							x2 = mesh.Vertices[i].SpatialCoordinates.X;
							break;
						case 3:
							x3 = mesh.Vertices[i].SpatialCoordinates.X;
							break;
						case 6:
							x6 = mesh.Vertices[i].SpatialCoordinates.X;
							break;
						case 7:
							x7 = mesh.Vertices[i].SpatialCoordinates.X;
							break;
					}
				}
				for (int i = 0; i < mesh.Vertices.Length; i++) {
					switch (mesh.Vertices[i].Tag) {
						case 0:
							mesh.Vertices[i].SpatialCoordinates.X = nearDistance - x3;
							break;
						case 1:
							mesh.Vertices[i].SpatialCoordinates.X = farDistance - x2;
							break;
						case 4:
							mesh.Vertices[i].SpatialCoordinates.X = nearDistance - x7;
							break;
						case 5:
							mesh.Vertices[i].SpatialCoordinates.X = nearDistance - x6;
							break;
					}
				}
				return (OpenBveApi.Geometry.GenericObject)mesh;
			} else {
				return obj;
			}
		}
		
	}
}