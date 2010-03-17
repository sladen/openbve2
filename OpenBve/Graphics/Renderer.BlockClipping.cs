using System;
using System.Collections;
using Tao.OpenGl;

namespace OpenBve {
	internal static partial class Renderer {
		
		private class Triangle {
			// members
			/// <summary>The first point of the triangle.</summary>
			internal OpenBveApi.Math.Vector2 PointA;
			/// <summary>The second point of the triangle.</summary>
			internal OpenBveApi.Math.Vector2 PointB;
			/// <summary>The third point of the triangle.</summary>
			internal OpenBveApi.Math.Vector2 PointC;
			/// <summary>The vector perpendicular to the line from the first to the second point.</summary>
			internal OpenBveApi.Math.Vector2 CrossAB;
			/// <summary>The vector perpendicular to the line from the second to the third point.</summary>
			internal OpenBveApi.Math.Vector2 CrossBC;
			/// <summary>The vector perpendicular to the line from the third to the first point.</summary>
			internal OpenBveApi.Math.Vector2 CrossCA;
			/// <summary>The bounding rectangle of the triangle.</summary>
			internal ObjectGrid.GridBounds BoundingRectangle;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="points">An array of three points that describe the triangle.</param>
			internal Triangle(OpenBveApi.Math.Vector2[] points) {
				this.PointA = points[0];
				this.PointB = points[1];
				this.PointC = points[2];
				this.BoundingRectangle = ObjectGrid.GridBounds.Uninitialized;
				for (int i = 0; i < 3; i++) {
					if (points[i].X < this.BoundingRectangle.Left) {
						this.BoundingRectangle.Left = points[i].X;
					}
					if (points[i].X > this.BoundingRectangle.Right) {
						this.BoundingRectangle.Right = points[i].X;
					}
					if (points[i].Y < this.BoundingRectangle.Near) {
						this.BoundingRectangle.Near = points[i].Y;
					}
					if (points[i].Y > this.BoundingRectangle.Far) {
						this.BoundingRectangle.Far = points[i].Y;
					}
				}
				this.CrossAB = OpenBveApi.Math.Vector2.Cross(this.PointB - this.PointA);
				this.CrossBC = OpenBveApi.Math.Vector2.Cross(this.PointC - this.PointB);
				this.CrossCA = OpenBveApi.Math.Vector2.Cross(this.PointA - this.PointC);
			}
		}
		
		private static void InitializeBlockClipping(out Triangle[] triangles) {
			/*
			 * Set up the four corners of the screen in screen coordinates.
			 * */
			OpenBveApi.Math.Vector2[] corners = new OpenBveApi.Math.Vector2[] {
				new OpenBveApi.Math.Vector2(-1.0, -1.0),
				new OpenBveApi.Math.Vector2(1.0, -1.0),
				new OpenBveApi.Math.Vector2(1.0, 1.0),
				new OpenBveApi.Math.Vector2(-1.0, 1.0)
			};
			/*
			 * Project the corners into world coordinates by
			 * extending them by the viewing distance.
			 * Incorporate the position of the camera as
			 * a fifth projected point.
			 * */
			double width = Math.Tan(0.5 * Camera.Viewport.HorizontalViewingAngle);
			double height = Math.Tan(0.5 * Camera.Viewport.VerticalViewingAngle);
			OpenBveApi.Math.Vector2[] projections = new OpenBveApi.Math.Vector2[5];
			for (int i = 0; i < 4; i++) {
				OpenBveApi.Math.Vector3 direction = width * corners[i].X * Camera.Orientation.X + height * corners[i].Y * Camera.Orientation.Y + Camera.Orientation.Z;
				OpenBveApi.Math.Vector3 vector = Camera.Position + Camera.Viewport.FarClippingPlane * direction;
				projections[i] = new OpenBveApi.Math.Vector2(vector.X, vector.Z);
			}
			projections[4] = new OpenBveApi.Math.Vector2(Camera.Position.X, Camera.Position.Z);
			/*
			 * Determine the convex hull of the five points.
			 * The algorithm used is Graham Scan, but with
			 * practical problems such as floating-point
			 * imprecisions and coindicing points taken into
			 * account.
			 * */
			int first = 0;
			for (int i = 1; i < projections.Length; i++) {
				const double threshold = 0.00000001;
				double deltaY = projections[i].Y - projections[first].Y;
				if (deltaY < -threshold | deltaY >= -threshold & deltaY <= threshold & projections[i].X < projections[first].X) {
					first = i;
				}
			}
			double[] angles = new double[projections.Length];
			for (int i = 0; i < projections.Length; i++) {
				if (i == first) {
					angles[i] = double.MinValue;
				} else {
					double dx = projections[i].X - projections[first].X;
					double dy = projections[i].Y - projections[first].Y;
					if (dy < 0.0) {
						dy = -dy;
					}
					if (dy <= 0.00000001) {
						angles[i] = -1e200 + (1e200 * Math.Atan(dx / 4096.0));
					} else {
						angles[i] = -dx / dy;
					}
				}
			}
			Array.Sort<double, OpenBveApi.Math.Vector2>(angles, projections);
			OpenBveApi.Math.Vector2[] hull = new OpenBveApi.Math.Vector2[projections.Length];
			hull[0] = projections[0];
			hull[1] = projections[1];
			int count = 2;
			for (int i = 2; i < projections.Length; i++) {
				while (count >= 2) {
					const double c = 0.000000001;
					double d = (hull[count - 1].X - hull[count - 2].X) * (projections[i].Y - hull[count - 2].Y) - (hull[count - 1].Y - hull[count - 2].Y) * (projections[i].X - hull[count - 2].X);
					if (d <= c) {
						count--;
					} else {
						break;
					}
				}
				hull[count] = projections[i];
				count++;
			}
			/*
			 * Divide the convex hull into triangles.
			 * */
			if (count == 3) {
				triangles = new Triangle[] {
					new Triangle(new OpenBveApi.Math.Vector2[] { hull[0], hull[1], hull[2] })
				};
			} else if (count == 4) {
				triangles = new Triangle[] {
					new Triangle(new OpenBveApi.Math.Vector2[] { hull[0], hull[1], hull[2] }),
					new Triangle(new OpenBveApi.Math.Vector2[] { hull[0], hull[2], hull[3] })
				};
			} else if (count == 5) {
				triangles = new Triangle[] {
					new Triangle(new OpenBveApi.Math.Vector2[] { hull[0], hull[1], hull[2] }),
					new Triangle(new OpenBveApi.Math.Vector2[] { hull[0], hull[2], hull[3] }),
					new Triangle(new OpenBveApi.Math.Vector2[] { hull[0], hull[3], hull[4] })
				};
			} else {
				triangles = new Triangle[] { };
			}
		}
		
		/// <summary>Gives the signed area of the triangle formed by three specified points.</summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <param name="c">The third point.</param>
		/// <returns>The signed area of the triangle.</returns>
		private static double GetSignedArea(OpenBveApi.Math.Vector2 a, OpenBveApi.Math.Vector2 b, OpenBveApi.Math.Vector2 c) {
			return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
		}
		
		/// <summary>Checks if a block specified by its bounding rectangle intersects any of the specified triangles.</summary>
		/// <param name="block">The bounding rectangle of the block.</param>
		/// <param name="triangles">An array of triangles that was obtained from a call to InitializeBlockClipping.</param>
		/// <returns>Whether the block intersects any of the triangles.</returns>
		private static bool BlockIntersectsTriangles(ObjectGrid.GridBounds block, Triangle[] triangles) {
			for (int i = 0; i < triangles.Length; i++) {
				if (BlockIntersectsTriangle(block, triangles[i])) {
					return true;
				}
			}
			return false;
		}
		
		/// <summary>Checks if a block specified by its bounding rectangle intersects a specified triangle.</summary>
		/// <param name="block">The bounding rectangle of the block.</param>
		/// <param name="triangles">The triangle.</param>
		/// <returns>Whether the block intersects the triangle.</returns>
		private static bool BlockIntersectsTriangle(ObjectGrid.GridBounds block, Triangle triangle) {
			/*
			 * First, let's check if the bounding rectangle of the
			 * block intersects the bounding rectangle of the
			 * triangle. If so, we need further checks to ensure
			 * that the actual triangle intersects the block,
			 * but if not, the triangle cannot intersect the block.
			 * */
			if (
				block.Left <= triangle.BoundingRectangle.Right &
				block.Right >= triangle.BoundingRectangle.Left &
				block.Near <= triangle.BoundingRectangle.Far &
				block.Far >= triangle.BoundingRectangle.Near
			) {
				/*
				 * The bounding rectangles intersect. This means that
				 * the triangle could intersect the block, but not
				 * necessarily. First, let's check if any point of
				 * the triangle lies inside the rectangle.
				 * */
				if (
					triangle.PointA.X >= block.Left && triangle.PointA.X <= block.Right &&
					triangle.PointA.Y >= block.Near && triangle.PointA.Y <= block.Far
				) {
					return true;
				} else if (
					triangle.PointB.X >= block.Left && triangle.PointB.X <= block.Right &&
					triangle.PointB.Y >= block.Near && triangle.PointB.Y <= block.Far
				) {
					return true;
				} else if (
					triangle.PointC.X >= block.Left && triangle.PointC.X <= block.Right &&
					triangle.PointC.Y >= block.Near && triangle.PointC.Y <= block.Far
				) {
					return true;
				} else {
					/*
					 * There is no point of the triangle that lies inside
					 * the rectangle. Now, let's check if any point of the
					 * rectangle lies inside the triangle.
					 * */
					if (IsPointInTriangle(new OpenBveApi.Math.Vector2(block.Left, block.Near), triangle)) {
						return true;
					} else if (IsPointInTriangle(new OpenBveApi.Math.Vector2(block.Left, block.Far), triangle)) {
						return true;
					} else if (IsPointInTriangle(new OpenBveApi.Math.Vector2(block.Right, block.Far), triangle)) {
						return true;
					} else if (IsPointInTriangle(new OpenBveApi.Math.Vector2(block.Right, block.Near), triangle)) {
						return true;
					} else {
						/* No point of the rectangle lies inside the triangle.
						 * Now, let's check each edge of the triangle against
						 * each edge of the rectangle for intersection.
						 * */
						for (int i = 0; i < 3; i++) {
							OpenBveApi.Math.Vector2 a;
							OpenBveApi.Math.Vector2 b;
							if (i == 0) {
								a = triangle.PointA;
								b = triangle.PointB;
							} else if (i == 1) {
								a = triangle.PointB;
								b = triangle.PointC;
							} else {
								a = triangle.PointC;
								b = triangle.PointA;
							}
							/*
							 * Compare the current triangle edge against the
							 * left and right vertical edges of the rectangle.
							 * */
							{
								double denominator = a.X - b.X;
								if (denominator == 0.0) {
									if (a.X >= block.Left & a.X <= block.Right) {
										if (a.Y <= block.Near & b.Y >= block.Far | a.Y >= block.Far & b.Y <= block.Near) {
											return true;
										}
									}
								} else {
									for (int j = 0; j < 2; j++) {
										double px = j == 0 ? block.Left : block.Right;
										double numerator = a.Y * (px - b.X) - b.Y * (px - a.X);
										double py = numerator / denominator;
										if (py >= block.Near & py <= block.Far) {
											double r = (px - a.X) / (b.X - a.X);
											if (r >= 0.0 & r <= 1.0) {
												return true;
											}
										}
									}
								}
							}
							/*
							 * Compare the current triangle edge against the
							 * near and far horizontal edges of the rectangle.
							 * */
							{
								double denominator = a.Y - b.Y;
								if (denominator == 0.0) {
									if (a.Y >= block.Near & a.Y <= block.Far) {
										if (a.X <= block.Left & b.X >= block.Right | a.X >= block.Right & b.X <= block.Left) {
											return true;
										}
									}
								} else {
									for (int j = 0; j < 2; j++) {
										double py = j == 0 ? block.Near : block.Far;
										double numerator = a.X * (py - b.Y) - b.X * (py - a.Y);
										double px = numerator / denominator;
										if (px >= block.Left & px <= block.Right) {
											double r = (py - a.Y) / (b.Y - a.Y);
											if (r >= 0.0 & r <= 1.0) {
												return true;
											}
										}
									}
								}
							}
						}
						/*
						 * No point of the triangle lies inside the rectangle,
						 * no point of the rectangle lies inside the triangle,
						 * and no triangle edge intersects with a rectangle
						 * edge. Thus, the triangle does not intersect the
						 * rectangle.
						 * */
						return false;
					}
				}
			} else {
				/*
				 * The bounding rectangles do not intersect. The
				 * triangle itself thus cannot intersect the block.
				 * */
				return false;
			}
		}
		
		/// <summary>Checks whether a specified point lies inside a specified triangle.</summary>
		/// <param name="point">The point.</param>
		/// <param name="triangle">The triangle.</param>
		/// <returns>Whether the point lies inside the triangle.</returns>
		private static bool IsPointInTriangle(OpenBveApi.Math.Vector2 point, Triangle triangle) {
			int signA = Math.Sign(OpenBveApi.Math.Vector2.Dot(triangle.PointA - point, triangle.CrossAB));
			int signB = Math.Sign(OpenBveApi.Math.Vector2.Dot(triangle.PointB - point, triangle.CrossBC));
			int signC = Math.Sign(OpenBveApi.Math.Vector2.Dot(triangle.PointC - point, triangle.CrossCA));
			bool negative = signA < 0 | signB < 0 | signC < 0;
			bool positive = signA > 0 | signB > 0 | signC > 0;
			return !(negative & positive);
		}
		
	}
}