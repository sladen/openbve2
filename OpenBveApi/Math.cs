using System;

namespace OpenBveApi {
	/// <summary>Provides mathematical structures.</summary>
	public static class Math {
		
		// vector2
		/// <summary>Represents a two-dimensional vector.</summary>
		public struct Vector2 {
			// members
			/// <summary>The x-coordinate.</summary>
			public double X;
			/// <summary>The y-coordinate.</summary>
			public double Y;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="x">The x-coordinate.</param>
			/// <param name="y">The y-coordinate.</param>
			public Vector2(double x, double y) {
				this.X = x;
				this.Y = y;
			}
			// arithmetic operators
			/// <summary>Adds two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The sum of the two vectors.</returns>
			public static Vector2 operator +(Vector2 a, Vector2 b) {
				return new Vector2(a.X + b.X, a.Y + b.Y);
			}
			/// <summary>Adds a vector and a scalar.</summary>
			/// <param name="a">The vector.</param>
			/// <param name="b">The scalar.</param>
			/// <returns>The sum of the vector and the scalar.</returns>
			public static Vector2 operator +(Vector2 a, double b) {
				return new Vector2(a.X + b, a.Y + b);
			}
			/// <summary>Adds a scalar and a vector.</summary>
			/// <param name="a">The scalar.</param>
			/// <param name="b">The vector.</param>
			/// <returns>The sum of the scalar and the vector.</returns>
			public static Vector2 operator +(double a, Vector2 b) {
				return new Vector2(a + b.X, a + b.Y);
			}
			/// <summary>Subtracts two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The difference of the two vectors.</returns>
			public static Vector2 operator -(Vector2 a, Vector2 b) {
				return new Vector2(a.X - b.X, a.Y - b.Y);
			}
			/// <summary>Subtracts a scalar from a vector.</summary>
			/// <param name="a">The vector.</param>
			/// <param name="b">The scalar.</param>
			/// <returns>The difference of the vector and the scalar.</returns>
			public static Vector2 operator -(Vector2 a, double b) {
				return new Vector2(a.X - b, a.Y - b);
			}
			/// <summary>Subtracts a vector from a scalar.</summary>
			/// <param name="a">The scalar.</param>
			/// <param name="b">The vector.</param>
			/// <returns>The difference of the scalar and the vector.</returns>
			public static Vector2 operator -(double a, Vector2 b) {
				return new Vector2(a - b.X, a - b.Y);
			}
			/// <summary>Negates a vector.</summary>
			/// <param name="vector">The vector.</param>
			/// <returns>The negation of the vector.</returns>
			public static Vector2 operator -(Vector2 vector) {
				return new Vector2(-vector.X, -vector.Y);
			}
			/// <summary>Multiplies two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The product of the two vectors.</returns>
			public static Vector2 operator *(Vector2 a, Vector2 b) {
				return new Vector2(a.X * b.X, a.Y * b.Y);
			}
			/// <summary>Multiplies a vector and a scalar.</summary>
			/// <param name="a">The vector.</param>
			/// <param name="b">The scalar.</param>
			/// <returns>The product of the vector and the scalar.</returns>
			public static Vector2 operator *(Vector2 a, double b) {
				return new Vector2(a.X * b, a.Y * b);
			}
			/// <summary>Multiplies a scalar and a vector.</summary>
			/// <param name="a">The scalar.</param>
			/// <param name="b">The vector.</param>
			/// <returns>The product of the scalar and the vector.</returns>
			public static Vector2 operator *(double a, Vector2 b) {
				return new Vector2(a * b.X, a * b.Y);
			}
			/// <summary>Divides two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The quotient of the two vectors.</returns>
			/// <exception cref="DivideByZeroException">Raised when a member of the second vector is zero.</exception>
			public static Vector2 operator /(Vector2 a, Vector2 b) {
				if (b.X == 0.0 | b.Y == 0.0) {
					throw new DivideByZeroException();
				} else {
					return new Vector2(a.X / b.X, a.Y / b.Y);
				}
			}
			/// <summary>Divides a vector by a scalar.</summary>
			/// <param name="a">The vector.</param>
			/// <param name="b">The scalar.</param>
			/// <returns>The quotient of the vector and the scalar.</returns>
			/// <exception cref="DivideByZeroException">Raised when a member of the vector is zero.</exception>
			public static Vector2 operator /(Vector2 a, double b) {
				if (b == 0.0) {
					throw new DivideByZeroException();
				} else {
					double factor = 1.0 / b;
					return new Vector2(a.X * factor, a.Y * factor);
				}
			}
			/// <summary>Divides a scalar by a vector.</summary>
			/// <param name="a">The scalar.</param>
			/// <param name="b">The vector.</param>
			/// <returns>The quotient of the scalar and the vector.</returns>
			/// <exception cref="DivideByZeroException">Raised when any member of the vector is zero.</exception>
			public static Vector2 operator /(double a, Vector2 b) {
				if (b.X == 0.0 | b.Y == 0.0) {
					throw new DivideByZeroException();
				} else {
					return new Vector2(a / b.X, a / b.Y);
				}
			}
			// comparisons
			/// <summary>Checks two vectors for equality.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>A boolean indicating whether the two vectors are equal.</returns>
			public static bool operator ==(Vector2 a, Vector2 b) {
				return a.X == b.X & a.Y == b.Y;
			}
			/// <summary>Checks two vectors for inequality.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>A boolean indicating whether the two vectors are unequal.</returns>
			public static bool operator !=(Vector2 a, Vector2 b) {
				return a.X != b.X | a.Y != b.Y;
			}
			/// <summary>Checks this instance and a specified object for equality.</summary>
			/// <param name="obj">The object to compare.</param>
			/// <returns>A boolean indicating whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (obj is Vector2) {
					Vector2 vector = (Vector2)obj;
					return this.X == vector.X & this.Y == vector.Y;
				} else {
					return base.Equals(obj);
				}
			}
			/// <summary>Gets the hash code for this instance.</summary>
			/// <returns>The hash code.</returns>
			public override int GetHashCode() {
				return base.GetHashCode();
			}
			// static functions
			/// <summary>Gives the dot product of two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The dot product of the two vectors.</returns>
			public static double Dot(Vector2 a, Vector2 b) {
				return a.X * b.X + a.Y * b.Y;
			}
			/// <summary>Gives the cross product of a vector.</summary>
			/// <param name="vector">The vector.</param>
			/// <returns>The cross product of the vector.</returns>
			public static Vector2 Cross(Vector2 vector) {
				return new Vector2(-vector.Y, vector.X);
			}
			/// <summary>Normalizes a vector.</summary>
			/// <param name="vector">The vector.</param>
			/// <returns>The normalized vector.</returns>
			/// <exception cref="DivideByZeroException">Raised when the vector is a null vector.</exception>
			public static Vector2 Normalize(Vector2 vector) {
				double norm = vector.X * vector.X + vector.Y * vector.Y;
				if (norm == 0.0) {
					throw new DivideByZeroException();
				} else {
					double factor = 1.0 / System.Math.Sqrt(norm);
					return new Vector2(vector.X * factor, vector.Y * factor);
				}
			}
			/// <summary>Translates a vector by a specified offset.</summary>
			/// <param name="vector">The vector.</param>
			/// <param name="offset">The offset.</param>
			/// <returns>The translated vector.</returns>
			public static Vector2 Translate(Vector2 vector, Vector2 offset) {
				double x = vector.X + offset.X;
				double y = vector.Y + offset.Y;
				return new Vector2(x, y);
			}
			/// <summary>Scales a vector by a specified factor.</summary>
			/// <param name="vector">The vector.</param>
			/// <param name="factor">The factor.</param>
			/// <returns>The scaled vector.</returns>
			public static Vector2 Scale(Vector2 vector, Vector2 factor) {
				double x = vector.X * factor.X;
				double y = vector.Y * factor.Y;
				return new Vector2(x, y);
			}
			/// <summary>Rotates a vector.</summary>
			/// <param name="vector">The vector.</param>
			/// <param name="cosineOfAngle">The cosine of the angle by which to rotate.</param>
			/// <param name="sineOfAngle">The sine of the angle by which to rotate.</param>
			/// <returns>The rotated vector.</returns>
			public static Vector2 Rotate(Vector2 vector, double cosineOfAngle, double sineOfAngle) {
				double x = vector.X * cosineOfAngle - vector.Y * sineOfAngle;
				double y = vector.X * sineOfAngle + vector.Y * cosineOfAngle;
				return new Vector2(x, y);
			}
			/// <summary>Checks whether a vector is a null vector.</summary>
			/// <returns>A boolean indicating whether the vector is a null vector.</returns>
			public static bool IsNullVector(Vector2 vector) {
				return vector.X == 0.0 & vector.Y == 0.0;
			}
			/// <summary>Gets the vector norm of a specified vector.</summary>
			/// <param name="vector">The vector.</param>
			/// <returns>The vector norm.</returns>
			public static double Norm(Vector2 vector) {
				return System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
			}
			// instance functions
			/// <summary>Normalizes this instance of a vector.</summary>
			/// <exception cref="DivideByZeroException">Raised when the vector is a null vector.</exception>
			public void Normalize() {
				double norm = this.X * this.X + this.Y * this.Y;
				if (norm == 0.0) {
					throw new DivideByZeroException();
				} else {
					double factor = 1.0 / System.Math.Sqrt(norm);
					this.X *= factor;
					this.Y *= factor;
				}
			}
			/// <summary>Translates this instance of a vector by a specified offset.</summary>
			/// <param name="offset">The offset.</param>
			public void Translate(Vector2 offset) {
				this.X += offset.X;
				this.Y += offset.Y;
			}
			/// <summary>Scales this instance of a vector by a specified factor.</summary>
			/// <param name="factor">The factor.</param>
			public void Scale(Vector2 factor) {
				this.X *= factor.X;
				this.Y *= factor.Y;
			}
			/// <summary>Rotates this instance of a vector by a specified angle.</summary>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			public void Rotate(double cosineOfAngle, double sineOfAngle) {
				double x = this.X * cosineOfAngle - this.Y * sineOfAngle;
				double y = this.X * sineOfAngle + this.Y * cosineOfAngle;
				this.X = x;
				this.Y = y;
			}
			/// <summary>Checks whether this instance of a vector is a null vector.</summary>
			/// <returns>A boolean indicating whether the vector is a null vector.</returns>
			public bool IsNullVector() {
				return this.X == 0.0 & this.Y == 0.0;
			}
			/// <summary>Gets the vector norm of this instance.</summary>
			/// <returns>The vector norm.</returns>
			public double Norm() {
				return System.Math.Sqrt(this.X * this.X + this.Y * this.Y);
			}
			// read-only fields
			/// <summary>The null vector, i.e. a vector with all coordinates set to zero.</summary>
			public static readonly Vector2 Null = new Vector2(0.0, 0.0);
			/// <summary>A vector of unit length pointing left, i.e. with its X-coordinate set to -1.</summary>
			public static readonly Vector2 Left = new Vector2(-1.0, 0.0);
			/// <summary>A vector of unit length pointing right, i.e. with its X-coordinate set to 1.</summary>
			public static readonly Vector2 Right = new Vector2(1.0, 0.0);
			/// <summary>A vector of unit length pointing down, i.e. with its Y-coordinate set to -1.</summary>
			public static readonly Vector2 Down = new Vector2(0.0, -1.0);
			/// <summary>A vector of unit length pointing up, i.e. with its Y-coordinate set to 1.</summary>
			public static readonly Vector2 Up = new Vector2(0.0, 1.0);
		}
		
		// vector3
		/// <summary>Represents a three-dimensional vector.</summary>
		public struct Vector3 {
			// members
			/// <summary>The x-coordinate.</summary>
			public double X;
			/// <summary>The y-coordinate.</summary>
			public double Y;
			/// <summary>The z-coordinate.</summary>
			public double Z;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="x">The x-coordinate.</param>
			/// <param name="y">The y-coordinate.</param>
			/// <param name="z">The z-coordinate.</param>
			public Vector3(double x, double y, double z) {
				this.X = x;
				this.Y = y;
				this.Z = z;
			}
			// arithmetic operators
			/// <summary>Adds two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The sum of the two vectors.</returns>
			public static Vector3 operator +(Vector3 a, Vector3 b) {
				return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
			}
			/// <summary>Adds a vector and a scalar.</summary>
			/// <param name="a">The vector.</param>
			/// <param name="b">The scalar.</param>
			/// <returns>The sum of the vector and the scalar.</returns>
			public static Vector3 operator +(Vector3 a, double b) {
				return new Vector3(a.X + b, a.Y + b, a.Z + b);
			}
			/// <summary>Adds a scalar and a vector.</summary>
			/// <param name="a">The scalar.</param>
			/// <param name="b">The vector.</param>
			/// <returns>The sum of the scalar and the vector.</returns>
			public static Vector3 operator +(double a, Vector3 b) {
				return new Vector3(a + b.X, a + b.Y, a + b.Z);
			}
			/// <summary>Subtracts two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The difference of the two vectors.</returns>
			public static Vector3 operator -(Vector3 a, Vector3 b) {
				return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
			}
			/// <summary>Subtracts a scalar from a vector.</summary>
			/// <param name="a">The vector.</param>
			/// <param name="b">The scalar.</param>
			/// <returns>The difference of the vector and the scalar.</returns>
			public static Vector3 operator -(Vector3 a, double b) {
				return new Vector3(a.X - b, a.Y - b, a.Z - b);
			}
			/// <summary>Subtracts a vector from a scalar.</summary>
			/// <param name="a">The scalar.</param>
			/// <param name="b">The vector.</param>
			/// <returns>The difference of the scalar and the vector.</returns>
			public static Vector3 operator -(double a, Vector3 b) {
				return new Vector3(a - b.X, a - b.Y, a - b.Z);
			}
			/// <summary>Negates a vector.</summary>
			/// <param name="vector">The vector.</param>
			/// <returns>The negation of the vector.</returns>
			public static Vector3 operator -(Vector3 vector) {
				return new Vector3(-vector.X, -vector.Y, -vector.Z);
			}
			/// <summary>Multiplies two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The product of the two vectors.</returns>
			public static Vector3 operator *(Vector3 a, Vector3 b) {
				return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
			}
			/// <summary>Multiplies a vector and a scalar.</summary>
			/// <param name="a">The vector.</param>
			/// <param name="b">The scalar.</param>
			/// <returns>The product of the vector and the scalar.</returns>
			public static Vector3 operator *(Vector3 a, double b) {
				return new Vector3(a.X * b, a.Y * b, a.Z * b);
			}
			/// <summary>Multiplies a scalar and a vector.</summary>
			/// <param name="a">The scalar.</param>
			/// <param name="b">The vector.</param>
			/// <returns>The product of the scalar and the vector.</returns>
			public static Vector3 operator *(double a, Vector3 b) {
				return new Vector3(a * b.X, a * b.Y, a * b.Z);
			}
			/// <summary>Divides two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The quotient of the two vectors.</returns>
			/// <exception cref="DivideByZeroException">Raised when a member of the second vector is zero.</exception>
			public static Vector3 operator /(Vector3 a, Vector3 b) {
				if (b.X == 0.0 | b.Y == 0.0 | b.Z == 0.0) {
					throw new DivideByZeroException();
				} else {
					return new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
				}
			}
			/// <summary>Divides a vector by a scalar.</summary>
			/// <param name="a">The vector.</param>
			/// <param name="b">The scalar.</param>
			/// <returns>The quotient of the vector and the scalar.</returns>
			/// <exception cref="DivideByZeroException">Raised when a member of the vector is zero.</exception>
			public static Vector3 operator /(Vector3 a, double b) {
				if (b == 0.0) {
					throw new DivideByZeroException();
				} else {
					double factor = 1.0 / b;
					return new Vector3(a.X * factor, a.Y * factor, a.Z * factor);
				}
			}
			/// <summary>Divides a scalar by a vector.</summary>
			/// <param name="a">The scalar.</param>
			/// <param name="b">The vector.</param>
			/// <returns>The quotient of the scalar and the vector.</returns>
			/// <exception cref="DivideByZeroException">Raised when a member of the vector is zero.</exception>
			public static Vector3 operator /(double a, Vector3 b) {
				if (b.X == 0.0 | b.Y == 0.0 | b.Z == 0.0) {
					throw new DivideByZeroException();
				} else {
					return new Vector3(a / b.X, a / b.Y, a / b.Z);
				}
			}
			// comparisons
			/// <summary>Checks two vectors for equality.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>A boolean indicating whether the two vectors are equal.</returns>
			public static bool operator ==(Vector3 a, Vector3 b) {
				return a.X == b.X & a.Y == b.Y & a.Z == b.Z;
			}
			/// <summary>Checks two vectors for inequality.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>A boolean indicating whether the two vectors are unequal.</returns>
			public static bool operator !=(Vector3 a, Vector3 b) {
				return a.X != b.X | a.Y != b.Y | a.Z != b.Z;
			}
			/// <summary>Checks this instance and a specified object for equality.</summary>
			/// <param name="obj">The object to compare.</param>
			/// <returns>A boolean indicating whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (obj is Vector3) {
					Vector3 vector = (Vector3)obj;
					return this.X == vector.X & this.Y == vector.Y & this.Z == vector.Z;
				} else {
					return base.Equals(obj);
				}
			}
			/// <summary>Gets the hash code for this instance.</summary>
			/// <returns>The hash code.</returns>
			public override int GetHashCode() {
				return base.GetHashCode();
			}
			// static functions
			/// <summary>Gives the dot product of two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The dot product of the two vectors.</returns>
			public static double Dot(Vector3 a, Vector3 b) {
				return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
			}
			/// <summary>Gives the cross product of two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The cross product of the two vectors.</returns>
			public static Vector3 Cross(Vector3 a, Vector3 b) {
				return new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
			}
			/// <summary>Normalizes a vector.</summary>
			/// <param name="vector">The vector.</param>
			/// <returns>The normalized vector.</returns>
			/// <exception cref="DivideByZeroException">Raised when the vector is a null vector.</exception>
			public static Vector3 Normalize(Vector3 vector) {
				double norm = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
				if (norm == 0.0) {
					throw new DivideByZeroException();
				} else {
					double factor = 1.0 / System.Math.Sqrt(norm);
					return new Vector3(vector.X * factor, vector.Y * factor, vector.Z * factor);
				}
			}
			/// <summary>Translates a vector by a specified offset.</summary>
			/// <param name="vector">The vector.</param>
			/// <param name="offset">The offset.</param>
			/// <returns>The translated vector.</returns>
			public static Vector3 Translate(Vector3 vector, Vector3 offset) {
				double x = vector.X + offset.X;
				double y = vector.Y + offset.Y;
				double z = vector.Z + offset.Z;
				return new Vector3(x, y, z);
			}
			/// <summary>Scales a vector by a specified factor.</summary>
			/// <param name="vector">The vector.</param>
			/// <param name="factor">The factor.</param>
			/// <returns>The scaled vector.</returns>
			public static Vector3 Scale(Vector3 vector, Vector3 factor) {
				double x = vector.X * factor.X;
				double y = vector.Y * factor.Y;
				double z = vector.Z * factor.Z;
				return new Vector3(x, y, z);
			}
			/// <summary>Rotates a vector on the plane perpendicular to a specified direction by a specified angle.</summary>
			/// <param name="vector">The vector.</param>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			/// <returns>The rotated vector.</returns>
			public static Vector3 Rotate(Vector3 vector, Vector3 direction, double cosineOfAngle, double sineOfAngle) {
				double cosineComplement = 1.0 - cosineOfAngle;
				double x = (cosineOfAngle + cosineComplement * direction.X * direction.X) * vector.X + (cosineComplement * direction.X * direction.Y - sineOfAngle * direction.Z) * vector.Y + (cosineComplement * direction.X * direction.Z + sineOfAngle * direction.Y) * vector.Z;
				double y = (cosineOfAngle + cosineComplement * direction.Y * direction.Y) * vector.Y + (cosineComplement * direction.X * direction.Y + sineOfAngle * direction.Z) * vector.X + (cosineComplement * direction.Y * direction.Z - sineOfAngle * direction.X) * vector.Z;
				double z = (cosineOfAngle + cosineComplement * direction.Z * direction.Z) * vector.Z + (cosineComplement * direction.X * direction.Z - sineOfAngle * direction.Y) * vector.X + (cosineComplement * direction.Y * direction.Z + sineOfAngle * direction.X) * vector.Y;
				return new Vector3(x, y, z);
			}
			/// <summary>Rotates a vector from the default orientation into a specified orientation.</summary>
			/// <param name="vector">The vector.</param>
			/// <param name="orientation">The orientation.</param>
			/// <returns>The rotated vector.</returns>
			/// <remarks>The default orientation is X = {1, 0, 0), Y = {0, 1, 0} and Z = {0, 0, 1}.</remarks>
			public static Vector3 Rotate(Vector3 vector, Orientation3 orientation) {
				double x = orientation.X.X * vector.X + orientation.Y.X * vector.Y + orientation.Z.X * vector.Z;
				double y = orientation.X.Y * vector.X + orientation.Y.Y * vector.Y + orientation.Z.Y * vector.Z;
				double z = orientation.X.Z * vector.X + orientation.Y.Z * vector.Y + orientation.Z.Z * vector.Z;
				return new Vector3(x, y, z);
			}
			/// <summary>Creates a unit vertor perpendicular to the plane described by three spatial coordinates, suitable for being a surface normal.</summary>
			/// <param name="a">The first spatial coordinate.</param>
			/// <param name="b">The second spatial coordinate.</param>
			/// <param name="c">The third spatial coordinate.</param>
			/// <param name="normal">On success, receives the vector perpendicular to the described plane. On failure, receives Vector3.Up.</param>
			/// <returns>The success of the operation. This operation fails if the specified three vectors are colinear.</returns>
			public static bool CreateNormal(Vector3 a, Vector3 b, Vector3 c, out Vector3 normal) {
				normal = Vector3.Cross(b - a, c - a);
				double norm = normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z;
				if (norm != 0.0) {
					normal *= 1.0 / System.Math.Sqrt(norm);
					return true;
				} else {
					normal = Vector3.Up;
					return false;
				}
			}
			/// <summary>Checks whether three spatial coordinates are colinear.</summary>
			/// <param name="a">The first spatial coordinate.</param>
			/// <param name="b">The second spatial coordinate.</param>
			/// <param name="c">The third spatial coordinate.</param>
			/// <returns>A boolean indicating whether the three spatial coordinates are colinear.</returns>
			public static bool AreColinear(Vector3 a, Vector3 b, Vector3 c) {
				Vector3 normal = Vector3.Cross(b - a, c - a);
				return IsNullVector(normal);
			}
			/// <summary>Checks whether a vector is a null vector.</summary>
			/// <returns>A boolean indicating whether the vector is a null vector.</returns>
			public static bool IsNullVector(Vector3 vector) {
				return vector.X == 0.0 & vector.Y == 0.0 & vector.Z == 0.0;
			}
			/// <summary>Gets the vector norm of a specified vector.</summary>
			/// <param name="vector">The vector.</param>
			/// <returns>The vector norm.</returns>
			public static double Norm(Vector3 vector) {
				return System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
			}
			// instance functions
			/// <summary>Normalizes this instance of a vector.</summary>
			/// <exception cref="DivideByZeroException">Raised when the vector is a null vector.</exception>
			public void Normalize() {
				double norm = this.X * this.X + this.Y * this.Y + this.Z * this.Z;
				if (norm == 0.0) {
					throw new DivideByZeroException();
				} else {
					double factor = 1.0 / System.Math.Sqrt(norm);
					this.X *= factor;
					this.Y *= factor;
					this.Z *= factor;
				}
			}
			/// <summary>Translates this instance of a vector by a specified offset.</summary>
			/// <param name="offset">The offset.</param>
			public void Translate(Vector3 offset) {
				this.X += offset.X;
				this.Y += offset.Y;
				this.Z += offset.Z;
			}
			/// <summary>Scales this instance of a vector by a specified factor.</summary>
			/// <param name="factor">The factor.</param>
			public void Scale(Vector3 factor) {
				this.X *= factor.X;
				this.Y *= factor.Y;
				this.Z *= factor.Z;
			}
			/// <summary>Rotates this instance of a vector on the plane perpendicular to a specified direction by a specified angle.</summary>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			public void Rotate(Vector3 direction, double cosineOfAngle, double sineOfAngle) {
				double cosineComplement = 1.0 - cosineOfAngle;
				double x = (cosineOfAngle + cosineComplement * direction.X * direction.X) * this.X + (cosineComplement * direction.X * direction.Y - sineOfAngle * direction.Z) * this.Y + (cosineComplement * direction.X * direction.Z + sineOfAngle * direction.Y) * this.Z;
				double y = (cosineOfAngle + cosineComplement * direction.Y * direction.Y) * this.Y + (cosineComplement * direction.X * direction.Y + sineOfAngle * direction.Z) * this.X + (cosineComplement * direction.Y * direction.Z - sineOfAngle * direction.X) * this.Z;
				double z = (cosineOfAngle + cosineComplement * direction.Z * direction.Z) * this.Z + (cosineComplement * direction.X * direction.Z - sineOfAngle * direction.Y) * this.X + (cosineComplement * direction.Y * direction.Z + sineOfAngle * direction.X) * this.Y;
				this = new Vector3(x, y, z);
			}
			/// <summary>Rotates this instance of a vector from the default orientation into a specified orientation.</summary>
			/// <param name="orientation">The orientation.</param>
			/// <remarks>The default orientation is X = {1, 0, 0), Y = {0, 1, 0} and Z = {0, 0, 1}.</remarks>
			public void Rotate(Orientation3 orientation) {
				double x = orientation.X.X * this.X + orientation.Y.X * this.Y + orientation.Z.X * this.Z;
				double y = orientation.X.Y * this.X + orientation.Y.Y * this.Y + orientation.Z.Y * this.Z;
				double z = orientation.X.Z * this.X + orientation.Y.Z * this.Y + orientation.Z.Z * this.Z;
				this = new Vector3(x, y, z);
			}
			/// <summary>Checks whether this instance of a vector is a null vector.</summary>
			/// <returns>A boolean indicating whether this instance of a vector is a null vector.</returns>
			public bool IsNullVector() {
				return this.X == 0.0 & this.Y == 0.0 & this.Z == 0.0;
			}
			/// <summary>Gets the vector norm of this instance.</summary>
			/// <returns>The vector norm.</returns>
			public double Norm() {
				return System.Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
			}
			// read-only fields
			/// <summary>The null vector, i.e. a vector with all coordinates set to zero.</summary>
			public static readonly Vector3 Null = new Vector3(0.0, 0.0, 0.0);
			/// <summary>A vector of unit length pointing left, i.e. with its X-coordinate set to -1.</summary>
			public static readonly Vector3 Left = new Vector3(-1.0, 0.0, 0.0);
			/// <summary>A vector of unit length pointing right, i.e. with its X-coordinate set to 1.</summary>
			public static readonly Vector3 Right = new Vector3(1.0, 0.0, 0.0);
			/// <summary>A vector of unit length pointing down, i.e. with its Y-coordinate set to -1.</summary>
			public static readonly Vector3 Down = new Vector3(0.0, -1.0, 0.0);
			/// <summary>A vector of unit length pointing up, i.e. with its Y-coordinate set to 1.</summary>
			public static readonly Vector3 Up = new Vector3(0.0, 1.0, 0.0);
			/// <summary>A vector of unit length pointing backward, i.e. with its Z-coordinate set to -1.</summary>
			public static readonly Vector3 Backward = new Vector3(0.0, 0.0, -1.0);
			/// <summary>A vector of unit length pointing forward, i.e. with its Z-coordinate set to 1.</summary>
			public static readonly Vector3 Forward = new Vector3(0.0, 0.0, 1.0);
		}
		
		// orientation2
		/// <summary>Represents a two-dimensional orientation, consisting of two perpendicular, normalized vectors.</summary>
		public struct Orientation2 {
			// members
			/// <summary>The vector pointing right.</summary>
			public Vector2 X;
			/// <summary>The vector pointing up.</summary>
			public Vector2 Y;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="x">The vector pointing right.</param>
			/// <param name="y">The vector pointing up.</param>
			public Orientation2(Vector2 x, Vector2 y) {
				this.X = x;
				this.Y = y;
			}
			// static functions
			/// <summary>Rotates an orientation by a specified angle.</summary>
			/// <param name="orientation">The orientation.</param>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			/// <returns>The rotated vector.</returns>
			public static Orientation2 Rotate(Orientation2 orientation, double cosineOfAngle, double sineOfAngle) {
				Vector2 x = Vector2.Rotate(orientation.X, cosineOfAngle, sineOfAngle);
				Vector2 y = Vector2.Rotate(orientation.Y, cosineOfAngle, sineOfAngle);
				return new Orientation2(x, y);
			}
			// instance functions
			/// <summary>Rotates this instance of an orientation by a specified angle.</summary>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			/// <returns>The rotated vector.</returns>
			public void Rotate(double cosineOfAngle, double sineOfAngle) {
				this.X.Rotate(cosineOfAngle, sineOfAngle);
				this.Y.Rotate(cosineOfAngle, sineOfAngle);
			}
			// read-only fields
			/// <summary>The null orientation with X = Vector2.Null and Y = Vector2.Null.</summary>
			/// <remarks>This field can be used to represent an invalid or uninitialized orientation.</remarks>
			public static readonly Orientation2 Null = new Orientation2(Vector2.Null, Vector2.Null);
			/// <summary>The default orientation with X = Vector2.Right and Y = Vector2.Up.</summary>
			public static readonly Orientation2 Default = new Orientation2(Vector2.Right, Vector2.Up);
		}
		
		// orientation3
		/// <summary>Represents a three-dimensional orientation, consisting of three perpendicular, normalized vectors.</summary>
		public struct Orientation3 {
			// members
			/// <summary>The vector pointing right.</summary>
			public Vector3 X;
			/// <summary>The vector pointing up.</summary>
			public Vector3 Y;
			/// <summary>The vector pointing forward.</summary>
			public Vector3 Z;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="x">The vector pointing right.</param>
			/// <param name="y">The vector pointing up.</param>
			/// <param name="z">The vector pointing forward.</param>
			public Orientation3(Vector3 x, Vector3 y, Vector3 z) {
				this.X = x;
				this.Y = y;
				this.Z = z;
			}
			// static functions
			/// <summary>Rotates an orientation on the plane perpendicular to a specified direction by a specified angle.</summary>
			/// <param name="orientation">The orientation to rotate.</param>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			/// <returns>The rotated orientation.</returns>
			public static Orientation3 Rotate(Orientation3 orientation, Vector3 direction, double cosineOfAngle, double sineOfAngle) {
				Vector3 x = Vector3.Rotate(orientation.X, direction, cosineOfAngle, sineOfAngle);
				Vector3 y = Vector3.Rotate(orientation.Y, direction, cosineOfAngle, sineOfAngle);
				Vector3 z = Vector3.Rotate(orientation.Z, direction, cosineOfAngle, sineOfAngle);
				return new Orientation3(x, y, z);
			}
			/// <summary>Rotates an orientation around its X-axis by a specified angle.</summary>
			/// <param name="orientation">The orientation to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			/// <returns>The rotated orientation.</returns>
			public static Orientation3 RotateAroundXAxis(Orientation3 orientation, double cosineOfAngle, double sineOfAngle) {
				Vector3 y = Vector3.Rotate(orientation.Y, orientation.X, cosineOfAngle, sineOfAngle);
				Vector3 z = Vector3.Rotate(orientation.Z, orientation.X, cosineOfAngle, sineOfAngle);
				return new Orientation3(orientation.X, y, z);
			}
			/// <summary>Rotates an orientation around its Y-axis by a specified angle.</summary>
			/// <param name="orientation">The orientation to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			/// <returns>The rotated orientation.</returns>
			public static Orientation3 RotateAroundYAxis(Orientation3 orientation, double cosineOfAngle, double sineOfAngle) {
				Vector3 x = Vector3.Rotate(orientation.X, orientation.Y, cosineOfAngle, sineOfAngle);
				Vector3 z = Vector3.Rotate(orientation.Z, orientation.Y, cosineOfAngle, sineOfAngle);
				return new Orientation3(x, orientation.Y, z);
			}
			/// <summary>Rotates an orientation around its Z-axis by a specified angle.</summary>
			/// <param name="orientation">The orientation to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			/// <returns>The rotated orientation.</returns>
			public static Orientation3 RotateAroundZAxis(Orientation3 orientation, double cosineOfAngle, double sineOfAngle) {
				Vector3 x = Vector3.Rotate(orientation.X, orientation.Z, cosineOfAngle, sineOfAngle);
				Vector3 y = Vector3.Rotate(orientation.Y, orientation.Z, cosineOfAngle, sineOfAngle);
				return new Orientation3(x, y, orientation.Z);
			}
			// instance functions
			/// <summary>Rotates this instance of an orientation on the plane perpendicular to a specified direction by a specified angle.</summary>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			public void Rotate(Vector3 direction, double cosineOfAngle, double sineOfAngle) {
				this.X.Rotate(direction, cosineOfAngle, sineOfAngle);
				this.Y.Rotate(direction, cosineOfAngle, sineOfAngle);
				this.Z.Rotate(direction, cosineOfAngle, sineOfAngle);
			}
			/// <summary>Rotate this instance of an orientation around its X-axis by a specified angle.</summary>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			public void RotateAroundXAxis(double cosineOfAngle, double sineOfAngle) {
				this.Y.Rotate(this.X, cosineOfAngle, sineOfAngle);
				this.Z.Rotate(this.X, cosineOfAngle, sineOfAngle);
			}
			/// <summary>Rotate this instance of an orientation around its Y-axis by a specified angle.</summary>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			public void RotateAroundYAxis(double cosineOfAngle, double sineOfAngle) {
				this.X.Rotate(this.Y, cosineOfAngle, sineOfAngle);
				this.Z.Rotate(this.Y, cosineOfAngle, sineOfAngle);
			}
			/// <summary>Rotate this instance of an orientation around its Z-axis by a specified angle.</summary>
			/// <param name="cosineOfAngle">The cosine of the angle.</param>
			/// <param name="sineOfAngle">The sine of the angle.</param>
			public void RotateAroundZAxis(double cosineOfAngle, double sineOfAngle) {
				this.X.Rotate(this.Z, cosineOfAngle, sineOfAngle);
				this.Y.Rotate(this.Z, cosineOfAngle, sineOfAngle);
			}
			// read-only fields
			/// <summary>The null orientation with X = Vector3.Null, Y = Vector3.Null and Z = Vector3.Null.</summary>
			/// <remarks>This field can be used to represent an invalid or uninitialized orientation.</remarks>
			public static readonly Orientation3 Null = new Orientation3(Vector3.Null, Vector3.Null, Vector3.Null);
			/// <summary>The default orientation with X = Vector3.Right, Y = Vector3.Up and Z = Vector3.Forward.</summary>
			public static readonly Orientation3 Default = new Orientation3(Vector3.Right, Vector3.Up, Vector3.Forward);
		}
		
	}
}