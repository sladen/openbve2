using System;

namespace Plugin {
	internal static partial class Parser {
		
		private enum RailStatus {
			/// <summary>Indicates that the rail does not exist in the current block.</summary>
			NotAvailable = 0,
			/// <summary>Indicates that the rail exists in the current block and connects to the same rail in the next block.</summary>
			Continuous = 1,
			/// <summary>Indicates that the rail exists in the current block but does not connect to the same rail in the next block.</summary>
			Discontinuous = 2
		}
		
		/// <summary>Represents an object to be placed somewhere in the block, e.g. for Track.FreeObj, Track.Beacon, etc.</summary>
		private class BlockObject {
			// members
			/// <summary>The structure index.</summary>
			internal int StructureIndex;
			/// <summary>The x-offset relative to the beginning of the block.</summary>
			internal double X;
			/// <summary>The y-offset relative to the beginning of the block.</summary>
			internal double Y;
			/// <summary>The z-offset relative to the beginning of the block.</summary>
			internal double Z;
			/// <summary>The yaw angle.</summary>
			internal double Yaw;
			/// <summary>The pitch angle.</summary>
			internal double Pitch;
			/// <summary>The roll angle.</summary>
			internal double Roll;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="structureIndex"></param>
			/// <param name="x">The x-offset relative to the beginning of the block.</param>
			/// <param name="y">The y-offset relative to the beginning of the block.</param>
			/// <param name="z">The z-offset relative to the beginning of the block.</param>
			/// <param name="yaw">The yaw angle.</param>
			/// <param name="pitch">The pitch angle.</param>
			/// <param name="roll">The roll angle.</param>
			internal BlockObject(int structureIndex, double x, double y, double z, double yaw, double pitch, double roll) {
				this.StructureIndex = structureIndex;
				this.X = x;
				this.Y = y;
				this.Z = z;
				this.Yaw = yaw;
				this.Pitch = pitch;
				this.Roll = roll;
			}
		}
		
		/// <summary>Represents the layout of the form.</summary>
		private enum FormLayout {
			/// <summary>Represents a layout that has not been initialized.</summary>
			Invalid = 0,
			/// <summary>The FormL, RoofL, FormR and RoofR structures are placed unmodified on this rail.</summary>
			Stub = 1,
			/// <summary>The FormL, FormCL, RoofL and RoofCL structures are placed unmodified on this rail.</summary>
			Left = 2,
			/// <summary>The FormR, FormCR, RoofR and RoofCR structures are placed unmodified on this rail.</summary>
			Right = 3,
			/// <summary>The form structures are placed between this rail and a secondary rail in modified form.</summary>
			SecondaryRail = 4
		}
		
		private class Form {
			// members
			/// <summary>The form structure index.</summary>
			internal int FormType;
			/// <summary>The roof structure index.</summary>
			internal int RoofType;
			/// <summary>The form layout.</summary>
			internal FormLayout Layout;
			/// <summary>The secondary rail for FormLayout.SecondaryRail.</summary>
			internal int SecondaryRail;
			// constructor
			internal Form(int formType, int roofType, FormLayout layout, int secondaryRail) {
				this.FormType = formType;
				this.RoofType = roofType;
				this.Layout = layout;
				this.SecondaryRail = secondaryRail;
			}
		}
		
		private class Crack {
			// members
			/// <summary>The crack structure index.</summary>
			internal int Type;
			/// <summary>The secondary rail.</summary>
			internal int SecondaryRail;
			// constructors
			internal Crack(int type, int secondaryRail) {
				this.Type = type;
				this.SecondaryRail = secondaryRail;
			}
		}
		
		private struct Pole {
			// members
			/// <summary>The first structure index, or -1 is no pole is used in this block.</summary>
			internal int StructureIndex1;
			/// <summary>The second structure index.</summary>
			internal int StructureIndex2;
			/// <summary>The location. If the second structure index is 0, a non-positive number places the structure as-is, while a positive number mirrors the structure on its x-axis. If the second structure index is positive, this gives the inverted x-offset.</summary>
			internal double Location;
			/// <summary>The spacing of poles, expressed as multiples of blocks. A spacing of 1 will place the pole every block, a spacing of two every second block, and so on.</summary>
			internal int Spacing;
			// constructors
			internal Pole(int structureIndex1, int structureIndex2, double location, int spacing) {
				this.StructureIndex1 = structureIndex1;
				this.StructureIndex2 = structureIndex2;
				this.Location = location;
				this.Spacing = spacing;
			}
		}
		
		private class Rail {
			// members
			/// <summary>The index associated to this rail.</summary>
			internal int Index;
			/// <summary>The status of this rail.</summary>
			internal RailStatus Status;
			/// <summary>Indicates whether this rail was updated by a Track.RailStart or Track.Rail command in the current block.</summary>
			internal bool Updated;
			/// <summary>The X-offset at the beginning of the block.</summary>
			internal double StartX;
			/// <summary>The Y-offset at the beginning of the block.</summary>
			internal double StartY;
			/// <summary>The X-offset at the end of the block.</summary>
			internal double EndX;
			/// <summary>The Y-offset at the end of the block.</summary>
			internal double EndY;
			/// <summary>The type associated to the rail.</summary>
			internal int Type;
			/// <summary>The index of the wall structure used in this block, or -1 if no wall is used.</summary>
			internal int WallType;
			/// <summary>The side on which the wall is placed. Can be -1 (left), 0 (both), or 1 (right).</summary>
			internal int WallSide;
			/// <summary>The index of the dike structure used in this block, or -1 if no dike is used.</summary>
			internal int DikeType;
			/// <summary>The side on which the dike is placed. Can be -1 (left), 0 (both), or 1 (right).</summary>
			internal int DikeSide;
			/// <summary>The pole used in this block.</summary>
			internal Pole Pole;
			/// <summary>An array of forms associated to this rail in this block, or a null reference.</summary>
			internal Form[] Forms;
			/// <summary>The number of forms associated to this rail in this block.</summary>
			internal int FormCount;
			/// <summary>An array of cracks associated to this rail in this block, or a null reference.</summary>
			internal Crack[] Cracks;
			/// <summary>The number of cracks associated to this rail in this block.</summary>
			internal int CrackCount;
			/// <summary>An array of free objects associated to this rail in this block, or a null reference.</summary>
			internal BlockObject[] FreeObjs;
			/// <summary>The number of free objects associated to this rail in this block.</summary>
			internal int FreeObjCount;
			/// <summary>An array of beacons associated to this rail in this block, or a null reference.</summary>
			internal BlockObject[] Beacons;
			/// <summary>The number of beacons associated to this rail in this block.</summary>
			internal int BeaconCount;
			/// <summary>An array of stop posts used in this block, or a null reference.</summary>
			internal BlockObject[] Stops;
			/// <summary>The number of stop posts used in this block, or a null reference.</summary>
			internal int StopCount;
			/// <summary>An array of limit posts used in this block, or a null reference.</summary>
			internal BlockObject[] Limits;
			/// <summary>The number of limit posts used in this block, or a null reference.</summary>
			internal int LimitCount;
			// constructors
			internal Rail(int index) {
				this.Index = index;
				this.Status = RailStatus.NotAvailable;
				this.Updated = false;
				this.StartX = 0.0;
				this.StartY = 0.0;
				this.EndX = 0.0;
				this.EndY = 0.0;
				this.Type = 0;
				this.WallType = -1;
				this.WallSide = 0;
				this.DikeType = -1;
				this.DikeSide = 0;
				this.Pole = new Pole(-1, 0, 0.0, 1);
				this.Forms = null;
				this.FormCount = 0;
				this.Cracks = null;
				this.CrackCount = 0;
				this.FreeObjs = null;
				this.FreeObjCount = 0;
				this.Beacons = null;
				this.BeaconCount = 0;
				this.Stops = null;
				this.StopCount = 0;
				this.Limits = null;
				this.LimitCount = 0;
			}
			internal Rail(Rail previous) {
				this.Index = previous.Index;
				this.Status = previous.Status;
				this.Updated = false;
				this.StartX = previous.EndX;
				this.StartY = previous.EndY;
				this.EndX = previous.EndX;
				this.EndY = previous.EndY;
				this.Type = previous.Type;
				this.WallType = previous.WallType;
				this.WallSide = previous.WallSide;
				this.DikeType = previous.DikeType;
				this.DikeSide = previous.DikeSide;
				this.Pole = previous.Pole;
				this.Forms = null;
				this.FormCount = 0;
				this.Cracks = null;
				this.CrackCount = 0;
				this.FreeObjs = null;
				this.FreeObjCount = 0;
				this.Beacons = null;
				this.BeaconCount = 0;
				this.Stops = null;
				this.StopCount = 0;
				this.Limits = null;
				this.LimitCount = 0;
			}
		}
		
		private class Block {
			// members
			/// <summary>The position at the beginning of the block.</summary>
			internal OpenBveApi.Math.Vector3 Position;
			/// <summary>The orientation at the beginning of the block.</summary>
			internal OpenBveApi.Math.Orientation3 Orientation;
			/// <summary>The track position at the beginning of the block.</summary>
			internal double Location;
			/// <summary>The signed turn ratio.</summary>
			internal double TurnRatio;
			/// <summary>The signed curve radius, or 0 to indicate straight track.</summary>
			internal double CurveRadius;
			/// <summary>The signed cant, expressed as an angle.</summary>
			internal double CurveCant;
			/// <summary>The signed pitch at the beginning of this block, expressed as a ratio.</summary>
			internal double Pitch;
			/// <summary>The height above ground.</summary>
			internal double Height;
			/// <summary>Whether the height is defined for this block. If not, it will later be interpolated.</summary>
			internal bool HeightDefined;
			/// <summary>An array of rails used in this block.</summary>
			internal Rail[] Rails;
			/// <summary>The number of rails used in this block.</summary>
			internal int RailCount;
			/// <summary>The index of the ground cycle structure used in this block, or -1 if no ground is used.</summary>
			internal int GroundCycle;
			/// <summary>An array of free objects associated to the ground in this block, or a null reference.</summary>
			internal BlockObject[] FreeObjs;
			/// <summary>The number of free objects associated to the ground in this block.</summary>
			internal int FreeObjCount;
			// constructors
			internal Block(double location) {
				this.Position = OpenBveApi.Math.Vector3.Null;
				this.Orientation = OpenBveApi.Math.Orientation3.Default;
				this.Location = location;
				this.TurnRatio = 0.0;
				this.CurveRadius = 0.0;
				this.CurveCant = 0.0;
				this.Pitch = 0.0;
				this.Height = 0.0;
				this.HeightDefined = false;
				this.Rails = new Rail[1];
				this.RailCount = 0;
				this.GroundCycle = 0;
				this.FreeObjs = null;
				this.FreeObjCount = 0;
			}
			internal Block(double location, Block previous) {
				this.Position = previous.Position;
				this.Orientation = previous.Orientation;
				this.Location = location;
				this.TurnRatio = 0.0;
				this.CurveRadius = previous.CurveRadius;
				this.CurveCant = previous.CurveCant;
				this.Pitch = previous.Pitch;
				this.Height = 0.0;
				this.HeightDefined = false;
				this.Rails = new Rail[previous.Rails.Length];
				for (int i = 0; i < previous.Rails.Length; i++) {
					if (previous.Rails[i] != null) {
						this.Rails[i] = new Rail(previous.Rails[i]);
					} else {
						this.Rails[i] = null;
					}
				}
				this.RailCount = previous.RailCount;
				this.GroundCycle = previous.GroundCycle;
				this.FreeObjs = null;
				this.FreeObjCount = 0;
			}
			// instance functions
			internal Rail GetRail(int index) {
				for (int i = 0; i < this.RailCount; i++) {
					if (this.Rails[i].Index == index) {
						return this.Rails[i];
					}
				}
				if (this.Rails.Length == this.RailCount) {
					Array.Resize<Rail>(ref this.Rails, this.Rails.Length << 1);
				}
				this.Rails[this.RailCount] = new Rail(index);
				this.RailCount++;
				return this.Rails[this.RailCount - 1];
			}
		}
		
		private class BlockCollection {
			// members
			internal Block[] Blocks;
			internal int BlockCount;
			internal double BlockLength;
			// constructors
			internal BlockCollection(double blockLength) {
				this.Blocks = new Block[16];
				this.BlockCount = 0;
				this.BlockLength = blockLength;
			}
			// instance functions
			/// <summary>Gets the block that precedes the block at the specified location. If it does not exist, all blocks from the last existing block until the specified one are created automatically.</summary>
			/// <param name="location">The location.</param>
			/// <param name="block">Receives the block.</param>
			/// <returns>The success of the operation. The operation fails if the previous block is at a negative location.</returns>
			internal bool GetPreviousBlock(double location, out Block block) {
				int index = (int)Math.Floor(location / this.BlockLength);
				if (index >= 1) {
					block = GetBlock(index - 1);
					return true;
				} else {
					block = null;
					return false;
				}
			}
			/// <summary>Gets the block at the specified location. If it does not exist, all blocks from the last existing block until the specified one are created automatically.</summary>
			/// <param name="location">The non-negative location.</param>
			/// <returns>The block at the specified location.</returns>
			/// <exception cref="System.IndexOutOfRangeException">Raised when the location is negative.</exception>
			internal Block GetCurrentBlock(double location) {
				int index = (int)Math.Floor(location / this.BlockLength);
				return GetBlock(index);
			}
			/// <summary>Gets the block that follows the block at the specified location. If it does not exist, all blocks from the last existing block until the specified one are created automatically.</summary>
			/// <param name="location">The non-negative location.</param>
			/// <returns>The block that follows the block at the specified location.</returns>
			/// <exception cref="System.IndexOutOfRangeException">Raised when the location is negative.</exception>
			internal Block GetNextBlock(double location) {
				int index = (int)Math.Floor(location / this.BlockLength);
				return GetBlock(index + 1);
			}
			/// <summary>Gets the specified block. If it does not exist, all blocks from the last existing block until the specified one are created automatically.</summary>
			/// <param name="index">The block index.</param>
			/// <returns>The block.</returns>
			/// <exception cref="System.IndexOutOfRangeException">Raised when the block index is negative.</exception>
			internal Block GetBlock(int index) {
				if (index < this.BlockCount) {
					return this.Blocks[index];
				} else {
					while (this.Blocks.Length <= index) {
						Array.Resize<Block>(ref this.Blocks, this.Blocks.Length << 1);
					}
					for (int i = this.BlockCount; i <= index; i++) {
						if (i == 0) {
							this.Blocks[i] = new Block(0.0);
						} else {
							this.Blocks[i] = new Block(this.Blocks[i - 1].Location + this.BlockLength, this.Blocks[i - 1]);
						}
					}
					this.BlockCount = index + 1;
					return this.Blocks[index];
				}
			}
		}
		
	}
}