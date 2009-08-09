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
			/// <param name="a">The vector.</param>
			/// <returns>The negation of the vector.</returns>
			public static Vector2 operator -(Vector2 a) {
				return new Vector2(-a.X, -a.Y);
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
			/// <exception cref="DivideByZeroException">Raised when any member of the second vector is zero.</exception>
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
			/// <exception cref="DivideByZeroException">Raised when any member of the vector is zero.</exception>
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
			// functions
			/// <summary>Gives the dot product of two vectors.</summary>
			/// <param name="a">The first vector.</param>
			/// <param name="b">The second vector.</param>
			/// <returns>The dot product of the two vectors.</returns>
			public static double Dot(Vector2 a, Vector2 b) {
				return a.X * b.X + a.Y * b.Y;
			}
			/// <summary>Gives the cross product of a vector.</summary>
			/// <param name="a">The vector.</param>
			/// <returns>The cross product of the vector.</returns>
			public static Vector2 Cross(Vector2 a) {
				return new Vector2(-a.Y, a.X);
			}
			/// <summary>Normalizes a vector.</summary>
			/// <param name="a">The vector.</param>
			/// <returns>The normalized vector.</returns>
			/// <exception cref="DivideByZeroException">Raised when the vector is a null vector.</exception>
			public static Vector2 Normalize(Vector2 a) {
				double norm = a.X * a.X + a.Y * a.Y;
				if (norm == 0.0) {
					throw new DivideByZeroException();
				} else {
					double factor = 1.0 / System.Math.Sqrt(norm);
					return new Vector2(a.X * factor, a.Y * factor);
				}
			}
			/// <summary>Rotates a vector.</summary>
			/// <param name="vector">The vector to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle by which to rotate.</param>
			/// <param name="sineOfAngle">The sine of the angle by which to rotate.</param>
			/// <returns>The rotated vector.</returns>
			public static Vector2 Rotate(Vector2 vector, double cosineOfAngle, double sineOfAngle) {
				double x = vector.X * cosineOfAngle - vector.Y * sineOfAngle;
				double y = vector.X * sineOfAngle + vector.Y * cosineOfAngle;
				return new Vector2(x, y);
			}
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
			/// <param name="a">The vector.</param>
			/// <returns>The negation of the vector.</returns>
			public static Vector3 operator -(Vector3 a) {
				return new Vector3(-a.X, -a.Y, -a.Z);
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
			/// <exception cref="DivideByZeroException">Raised when any member of the second vector is zero.</exception>
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
			/// <exception cref="DivideByZeroException">Raised when any member of the vector is zero.</exception>
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
			/// <exception cref="DivideByZeroException">Raised when any member of the vector is zero.</exception>
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
			// functions
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
			/// <param name="a">The vector.</param>
			/// <returns>The normalized vector.</returns>
			/// <exception cref="DivideByZeroException">Raised when the vector is a null vector.</exception>
			public static Vector3 Normalize(Vector3 a) {
				double norm = a.X * a.X + a.Y * a.Y + a.Z * a.Z;
				if (norm == 0.0) {
					throw new DivideByZeroException();
				} else {
					double factor = 1.0 / System.Math.Sqrt(norm);
					return new Vector3(a.X * factor, a.Y * factor, a.Z * factor);
				}
			}
			/// <summary>Normalizes a vector.</summary>
			/// <param name="a">The vector.</param>
			/// <exception cref="DivideByZeroException">Raised when the vector is a null vector.</exception>
			public static void Normalize(ref Vector3 a) {
				double norm = a.X * a.X + a.Y * a.Y + a.Z * a.Z;
				if (norm == 0.0) {
					throw new DivideByZeroException();
				} else {
					double factor = 1.0 / System.Math.Sqrt(norm);
					a.X *= factor;
					a.Y *= factor;
					a.Z *= factor;
				}
			}
			/// <summary>Rotates a vector on the plane perpendicular to a specified direction.</summary>
			/// <param name="a">The vector to rotate.</param>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle by which to rotate.</param>
			/// <param name="sineOfAngle">The sine of the angle by which to rotate.</param>
			/// <returns>The rotated vector.</returns>
			public static Vector3 Rotate(Vector3 a, Vector3 direction, double cosineOfAngle, double sineOfAngle) {
				double cosineComplement = 1.0 - cosineOfAngle;
				double x = (cosineOfAngle + cosineComplement * direction.X * direction.X) * a.X + (cosineComplement * direction.X * direction.Y - sineOfAngle * direction.Z) * a.Y + (cosineComplement * direction.X * direction.Z + sineOfAngle * direction.Y) * a.Z;
				double y = (cosineOfAngle + cosineComplement * direction.Y * direction.Y) * a.Y + (cosineComplement * direction.X * direction.Y + sineOfAngle * direction.Z) * a.X + (cosineComplement * direction.Y * direction.Z - sineOfAngle * direction.X) * a.Z;
				double z = (cosineOfAngle + cosineComplement * direction.Z * direction.Z) * a.Z + (cosineComplement * direction.X * direction.Z - sineOfAngle * direction.Y) * a.X + (cosineComplement * direction.Y * direction.Z + sineOfAngle * direction.X) * a.Y;
				return new Vector3(x, y, z);
			}
			/// <summary>Rotates a vector on the plane perpendicular to a specified direction.</summary>
			/// <param name="a">The vector to rotate.</param>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle by which to rotate.</param>
			/// <param name="sineOfAngle">The sine of the angle by which to rotate.</param>
			public static void Rotate(ref Vector3 a, Vector3 direction, double cosineOfAngle, double sineOfAngle) {
				double cosineComplement = 1.0 - cosineOfAngle;
				double x = (cosineOfAngle + cosineComplement * direction.X * direction.X) * a.X + (cosineComplement * direction.X * direction.Y - sineOfAngle * direction.Z) * a.Y + (cosineComplement * direction.X * direction.Z + sineOfAngle * direction.Y) * a.Z;
				double y = (cosineOfAngle + cosineComplement * direction.Y * direction.Y) * a.Y + (cosineComplement * direction.X * direction.Y + sineOfAngle * direction.Z) * a.X + (cosineComplement * direction.Y * direction.Z - sineOfAngle * direction.X) * a.Z;
				double z = (cosineOfAngle + cosineComplement * direction.Z * direction.Z) * a.Z + (cosineComplement * direction.X * direction.Z - sineOfAngle * direction.Y) * a.X + (cosineComplement * direction.Y * direction.Z + sineOfAngle * direction.X) * a.Y;
				a.X = x;
				a.Y = y;
				a.Z = z;
			}
			/// <summary>Creates a vertor perpendicular to the plane described by three specified vectors, suitable for being a surface normal.</summary>
			/// <param name="a">The first vector to describe a plane.</param>
			/// <param name="b">The second vector to describe a plane.</param>
			/// <param name="c">The third vector to describe a plane.</param>
			/// <returns>The vector perpendicular to the described plane.</returns>
			/// <exception cref="DivideByZeroException">Raised when the specified vertices do not describe a plane.</exception>
			public static Vector3 CreateNormal(Vector3 a, Vector3 b, Vector3 c) {
				Vector3 ab = b - a;
				Vector3 ac = c - a;
				Vector3 n = Vector3.Cross(ab, ac);
				return Vector3.Normalize(n);
			}
			/// <summary>Checks whether this instance of a vector is a null vector.</summary>
			/// <returns>A boolean indicating whether this instance of a vector is a null vector.</returns>
			public bool IsNullVector() {
				return this.X == 0.0 & this.Y == 0.0 & this.Z == 0.0;
			}
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
			// arithmetic operators
			/// <summary>Negates an orientation.</summary>
			/// <param name="a">The orientation.</param>
			/// <returns>The negation of the orientation.</returns>
			public static Orientation3 operator -(Orientation3 a) {
				return new Orientation3(-a.X, -a.Y, -a.Z);
			}
			// comparisons
			
			// functions
			/// <summary>Rotates an orientation on the plane perpendicular to a specified direction.</summary>
			/// <param name="a">The orientation to rotate.</param>
			/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
			/// <param name="cosineOfAngle">The cosine of the angle by which to rotate.</param>
			/// <param name="sineOfAngle">The sine of the angle by which to rotate.</param>
			/// <returns>The rotated vector.</returns>
			public static Orientation3 Rotate(Orientation3 a, Vector3 direction, double cosineOfAngle, double sineOfAngle) {
				Vector3 x = Vector3.Rotate(a.X, direction, cosineOfAngle, sineOfAngle);
				Vector3 y = Vector3.Rotate(a.Y, direction, cosineOfAngle, sineOfAngle);
				Vector3 z = Vector3.Rotate(a.Z, direction, cosineOfAngle, sineOfAngle);
				return new Orientation3(x, y, z);
			}
			// static fields
			/// <summary>The default orientation with X = {1, 0, 0}, Y = {0, 1, 0} and Z = {0, 0, 1}.</summary>
			public static readonly Orientation3 Default = new Orientation3(new Vector3(1.0, 0.0, 0.0), new Vector3(0.0, 1.0, 0.0), new Vector3(0.0, 0.0, 1.0));
		}
		
	}
}