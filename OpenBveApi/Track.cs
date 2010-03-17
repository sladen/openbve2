using System;

namespace OpenBveApi {
	/// <summary>Provides structures to represent the track.</summary>
	public static class Track {
		
		
		// TODO: This is a stub. Expand as necessary.
		
		
		// --- structures and enumerations ---
		
		/// <summary>Represents the endpoint of a track segment.</summary>
		public enum SegmentEndpoint {
			/// <summary>An invalid endpoint.</summary>
			Invalid = 0,
			/// <summary>The beginning of the track segment.</summary>
			Beginning = 1,
			/// <summary>The end of the track segment.</summary>
			End = 2,
			/// <summary>A segment-specific special endpoint.</summary>
			Special = 3
		}
		
		/// <summary>Represents a connection to a track segment.</summary>
		public struct SegmentConnection {
			// members
			/// <summary>The segment this connection points to, or a null reference.</summary>
			public Segment Segment;
			/// <summary>The endpoint of the segment this connection points to.</summary>
			public SegmentEndpoint Endpoint;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="segment">The segment this connection points to, or a null reference.</param>
			/// <param name="endpoint">The endpoint of the segment this connection points to, or SegmentEndpoint.Invalid.</param>
			public SegmentConnection(Segment segment, SegmentEndpoint endpoint) {
				this.Segment = segment;
				this.Endpoint = endpoint;
			}
			// read-only fields
			/// <summary>Represents a connection that does not point anywhere. Can be used at the end of the track, or to represent an uninitialized connection.</summary>
			public static readonly SegmentConnection Empty = new SegmentConnection(null, SegmentEndpoint.Invalid);
		}
		
		/// <summary>Represents a point on a track segment.</summary>
		public struct SegmentPoint {
			// members
			/// <summary>The physical track segment the point lies on.</summary>
			public PhysicalSegment Segment;
			/// <summary>The position at the point.</summary>
			public Math.Vector3 Position;
			/// <summary>The orientation at the point without factoring in the Roll parameter.</summary>
			/// <summary>If the OrientationWithoutRoll is rotated around its z-axis by the Roll parameter, it is equivalent to OrientationWithRoll.</summary>
			public Math.Orientation3 OrientationWithoutRoll;
			/// <summary>The orientation at the point with factoring in the Roll parameter.</summary>
			/// <summary>If the OrientationWithoutRoll is rotated around its z-axis by the Roll parameter, it is equivalent to OrientationWithRoll.</summary>
			public Math.Orientation3 OrientationWithRoll;
			/// <summary>The roll expressed as an angle.</summary>
			/// <summary>If the OrientationWithoutRoll is rotated around its z-axis by the Roll parameter, it is equivalent to OrientationWithRoll.</summary>
			public double Roll;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="segment">The physical track segment the point lies on.</param>
			/// <param name="position">The position.</param>
			/// <param name="orientation">The orientation, either with or without factoring in the Roll parameter.</param>
			/// <param name="orientationIncludesRoll">Whether the Orientation has the Roll parameter factored in.</param>
			/// <param name="roll">The roll expressed as an angle.</param>
			public SegmentPoint(PhysicalSegment segment, Math.Vector3 position, Math.Orientation3 orientation, bool orientationIncludesRoll, double roll) {
				this.Segment = segment;
				this.Position = position;
				if (orientationIncludesRoll) {
					this.OrientationWithRoll = orientation;
					this.OrientationWithoutRoll = Math.Orientation3.RotateAroundZAxis(orientation, System.Math.Cos(roll), -System.Math.Sin(roll));
				} else {
					this.OrientationWithoutRoll = orientation;
					this.OrientationWithRoll = Math.Orientation3.RotateAroundZAxis(orientation, System.Math.Cos(roll), System.Math.Sin(roll));
				}
				this.Roll = roll;
			}
			// read-only fields
			/// <summary>Represents an invalid or uninitialized point.</summary>
			public static readonly SegmentPoint Invalid = new SegmentPoint(null, Math.Vector3.Null, Math.Orientation3.Null, false, 0.0);
		}
		
		
		// --- abstract segment ---
		
		/// <summary>Represents a track segment.</summary>
		public abstract class Segment { }

		
		// --- physical segments ---
		
		/// <summary>Represents a physical track segment, i.e. a track segment of non-zero length.</summary>
		/// <remarks>Physical track segments include straight and curved pieces of track.</remarks>
		public abstract class PhysicalSegment : Segment {
			// members
			/// <summary>The connection pointing away from the beginning of this track segment.</summary>
			public SegmentConnection Previous;
			/// <summary>The connection pointing away from the end of this track segment.</summary>
			public SegmentConnection Next;
			/// <summary>The positive length of this track segment.</summary>
			/// <remarks>The length is measured on the track, i.e. for curves, this is the arc length on the curve.</remarks>
			public double Length;
			/// <summary>The roll at the beginning of this track segment, expressed as an angle in radians.</summary>
			/// <remarks>This angle can be used to represent cant or an otherwise banked track.</remarks>
			public double StartingRoll;
			/// <summary>The roll at the end of this track segment, expressed as an angle in radians.</summary>
			/// <remarks>This angle can be used to represent cant or an otherwise banked track.</remarks>
			public double EndingRoll;
			// instance functions
			/// <summary>Takes an offset relative to the beginning of this track segment and returns the point on the track corresponding to this offset in an output parameter.</summary>
			/// <param name="offset">The offset relative to the beginning of this track segment. A value of zero corresponds to the beginning of this track segment, while a value of the underlying Length field corresponds to the end of this track segment.</param>
			/// <param name="point">Receives point on the track, including its position, orientation and roll.</param>
			/// <returns>The success of the operation. This operation fails if the specified offset points to a point outside of the available track.</returns>
			public bool GetPoint(double offset, out SegmentPoint point) {
				if (offset < 0.0) {
					/*
					 * The offset is negative and thus outside the bounds of this segment.
					 * We need to continue processing with the previous segment.
					 * */
					if (this.Previous.Segment is PhysicalSegment) {
						PhysicalSegment previous = (PhysicalSegment)this.Previous.Segment;
						SegmentEndpoint endpoint = this.Previous.Endpoint;
						if (endpoint == SegmentEndpoint.Beginning) {
							return GetPoint(-offset, out point);
						} else if (endpoint == SegmentEndpoint.End) {
							return GetPoint(previous.Length + offset, out point);
						} else {
							throw new InvalidOperationException();
						}
					} else if (this.Previous.Segment is VirtualSegment) {
						// TODO: Virtual segments can have various endpoints.
						//       Each virtual segment needs to be handled specially.
						throw new NotImplementedException();
					} else if (this.Previous.Segment == null) {
						point = SegmentPoint.Invalid;
						return false;
					} else {
						throw new InvalidOperationException();
					}
				} else if (offset > this.Length) {
					/*
					 * The offset exceeds the length of this segment and is thus outside the bounds of this segment.
					 * We need to continue processing with the next segment.
					 * */
					if (this.Next.Segment is PhysicalSegment) {
						PhysicalSegment next = (PhysicalSegment)this.Next.Segment;
						SegmentEndpoint endpoint = this.Next.Endpoint;
						if (endpoint == SegmentEndpoint.Beginning) {
							return GetPoint(offset - this.Length, out point);
						} else if (endpoint == SegmentEndpoint.End) {
							return GetPoint(this.Length + next.Length - offset, out point);
						} else {
							throw new InvalidOperationException();
						}
					} else if (this.Next.Segment is VirtualSegment) {
						// TODO: Virtual segments can have various endpoints.
						//       Each virtual segment needs to be handled specially.
						throw new NotImplementedException();
					} else if (this.Next.Segment == null) {
						point = SegmentPoint.Invalid;
						return false;
					} else {
						throw new InvalidOperationException();
					}
				} else {
					/*
					 * The offset is within the bounds of this segment.
					 * We can calculate the track point data now.
					 * */
					if (this is StraightSegment) {
						StraightSegment straight = (StraightSegment)this;
						Math.Vector3 position = straight.Position + offset * straight.Orientation.Z;
						double rollFactor = offset / straight.Length;
						double roll = (1.0 - rollFactor) * straight.StartingRoll + rollFactor * straight.EndingRoll;
						point = new SegmentPoint(this, position, straight.Orientation, false, roll);
						return true;
					} else if (this is CurvedSegment) {
						CurvedSegment curve = (CurvedSegment)this;
						double angle = offset / curve.Radius;
						double cosineOfAngle = System.Math.Cos(angle);
						double sineOfAngle = System.Math.Sin(angle);
						double radiusX = curve.Radius * cosineOfAngle;
						double radiusZ = curve.Radius * sineOfAngle;
						Math.Vector3 position = curve.Center - radiusX * curve.Orientation.X + radiusZ * curve.Orientation.Z;
						Math.Orientation3 orientation = Math.Orientation3.RotateAroundYAxis(curve.Orientation, cosineOfAngle, sineOfAngle);
						double rollFactor = offset / curve.Length;
						double roll = (1.0 - rollFactor) * curve.StartingRoll + rollFactor * curve.EndingRoll;
						point = new SegmentPoint(this, position, orientation, false, roll);
						return true;
					} else {
						throw new InvalidOperationException();
					}
				}
			}
		}
		
		/// <summary>Represents a straight track segment.</summary>
		public class StraightSegment : PhysicalSegment {
			/// <summary>The position at the beginning of this track segment.</summary>
			public Math.Vector3 Position;
			/// <summary>The orientation at the beginning of this track segment without factoring in Roll, i.e. as if the track was unbanked.</summary>
			/// <remarks>The X-component points right, the Y-component up. The Z-component points from the beginning of this track piece to the end.</remarks>
			public Math.Orientation3 Orientation;
		}
		
		/// <summary>Represents a curved track segment based on a circular arc.</summary>
		public class CurvedSegment : PhysicalSegment {
			/// <summary>The position of the center of the circle.</summary>
			public Math.Vector3 Center;
			/// <summary>The orientation at the beginning of this track segment without factoring in Roll, i.e. as if the track was unbanked.</summary>
			/// <remarks>The X-component points to the center of the circle. The Z-component points tangentially from the beginning of this track piece into the direction of travel.</remarks>
			public Math.Orientation3 Orientation;
			/// <summary>The positive curve radius.</summary>
			public double Radius;
		}
		
		
		// --- virtual segments ---
		
		/// <summary>Represents a virtual track segment, i.e. a track segment of zero length.</summary>
		/// <remarks>Virtual track segments are used to convey point-based information in-between physical track segments.</remarks>
		public abstract class VirtualSegment : Segment { }
		
		/// <summary>Represents an event occuring between physical track segments.</summary>
		public class EventSegment : VirtualSegment {
			/// <summary>The connection pointing away from the beginning of this track segment. The corresponding endpoint is SegmentEndpoint.Beginning.</summary>
			public SegmentConnection Previous;
			/// <summary>The connection pointing away from the end of this track segment. The corresponding endpoint is SegmentEndpoint.End.</summary>
			public SegmentConnection Next;
			/// <summary>The event occuring at this point.</summary>
			public Event Event;
		}
		
		/// <summary>Represents a switch.</summary>
		public class SwitchSegment : VirtualSegment {
			/// <summary>The connection pointing away from the beginning of this track segment. The corresponding endpoint is SegmentEndpoint.Beginning.</summary>
			public SegmentConnection Previous;
			/// <summary>The first connection pointing away from the end of this track segment. The corresponding endpoint is SegmentEndpoint.End.</summary>
			public SegmentConnection Next;
			/// <summary>The second connection pointing away from the end of this track segment. The corresponding endpoint is SegmentEndpoint.Special.</summary>
			public SegmentConnection Branch;
		}
		
		
		// --- events ---
		
		/// <summary>Represents an event occuring in-between physical track segments.</summary>
		public abstract class Event { }
		
		
		
		
		
		
		
		// TODO: Helper functions
		
		/// <summary>Creates a straight track segment from two specified points.</summary>
		/// <param name="pointA">The first point.</param>
		/// <param name="pointB">The second point.</param>
		/// <param name="orientation">The orientation at the first point without factoring in roll. The Z component of this parameter must point from A toward B.</param>
		/// <param name="rollA">The roll at the first point.</param>
		/// <param name="rollB">The roll at the second point.</param>
		/// <param name="segment">Receives the segment on success. The start and end of the segment may be connected to other track segments.</param>
		/// <returns>The success of this operation. This operation fails if the two specified points coincide.</returns>
		public static bool CreateStraightSegmentFromPoints(Math.Vector3 pointA, Math.Vector3 pointB, Math.Orientation3 orientation, double rollA, double rollB, out PhysicalSegment segment) {
			if (pointA == pointB) {
				segment = null;
				return false;
			} else {
				double length = OpenBveApi.Math.Vector3.Norm(pointB - pointA);
				StraightSegment straight = new StraightSegment();
				straight.Previous = SegmentConnection.Empty;
				straight.Next = SegmentConnection.Empty;
				straight.Length = length;
				straight.StartingRoll = rollA;
				straight.EndingRoll = rollB;
				straight.Position = pointA;
				straight.Orientation = orientation;
				segment = straight;
				return true;
			}
		}
		
		/// <summary>Creates a track segment from two specified points.</summary>
		/// <param name="pointA">The first point.</param>
		/// <param name="pointB">The second point.</param>
		/// <param name="orientationA">The orientation at the first point.</param>
		/// <param name="orientationB">Receives the orientation at the second point.</param>
		/// <param name="rollA">The roll at the first point.</param>
		/// <param name="rollB">The roll at the second point.</param>
		/// <param name="segment">Receives the segment on success. The start and end of the segment may be connected to other track segments.</param>
		/// <returns>The success of this operation. This operation fails if the two specified points coincide.</returns>
		public static bool CreateCurvedSegmentFromPoints(Math.Vector3 pointA, Math.Vector3 pointB, Math.Orientation3 orientationA, out Math.Orientation3 orientationB, double rollA, double rollB, out PhysicalSegment segment) {
			throw new NotImplementedException();
		}
		
		/// <summary>Creates track segments from two specified points.</summary>
		/// <param name="pointA">The first point.</param>
		/// <param name="pointB">The second point.</param>
		/// <param name="orientationA">The orientation at the first point.</param>
		/// <param name="orientationB">The orientation at the second point.</param>
		/// <param name="rollA">The roll at the first point.</param>
		/// <param name="rollB">The roll at the second point.</param>
		/// <param name="segments">Receives the segments on success. The start of the first segment and end of the last segment may be connected to other track segments.</param>
		/// <returns>The success of this operation. This operation fails if the two specified points coincide.</returns>
		/// <remarks>This method usually creates two circular segments of equal radius but opposing direction in order to connect the two points while respecting their specified orientations. In special cases, this method will create a single curved segment or a single straight segment.</remarks>
		public static bool CreateCurvedSegmentFromPoints(Math.Vector3 pointA, Math.Vector3 pointB, Math.Orientation3 orientationA, Math.Orientation3 orientationB, double rollA, double rollB, out PhysicalSegment[] segments) {
			throw new NotImplementedException();
		}
		
	}
}