using System;

namespace MazeSimulator
{
	/// <summary>
	/// Circle class. Performs basic trig operations.
	/// </summary>
	public class Circle2D
    {
        #region Instance variables

        public Point2D Position;
		public double Radius;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty Circle2D object.
        /// </summary>
		public Circle2D() { }

        /// <summary>
        /// Creates a new Circle2D object by copying another AreaOfImpact.
        /// </summary>
		public Circle2D(Circle2D otherCircle2D)
		{
			Position=otherCircle2D.Position;
			Radius=otherCircle2D.Radius;
		}

        /// <summary>
        /// Creates a new Circle2D object from the speficied parameters.
        /// </summary>
		public Circle2D(Point2D position, double radius)
		{
			Radius=radius;
			Position=position;
		}

        #endregion

        #region Methods

        /// <summary>
        /// Tests if this AreaOfImpact collides with another cirle. Used for robot collision detection.
        /// </summary>
        /// <returns>Returns true if the circles collide; false otherwise.</returns>
		public bool collide(Circle2D otherCircle2D)
		{
			double dx = otherCircle2D.Position.X - Position.X;
			double dy = otherCircle2D.Position.Y - Position.Y;
			dx*=dx;
			dy*=dy;
			double rad_sum = otherCircle2D.Radius+Radius;
			rad_sum*=rad_sum;
			if ((dx+dy)<rad_sum)
				return true;
			return false;
        }

        #endregion
    }
	
	/// <summary>
	/// Point class. Performs basic trig operations.
	/// </summary>
	public class Point2D
    {
        #region Instance variables

        public double X,Y;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty Point2D object.
        /// </summary>
        public Point2D() { }

        /// <summary>
        /// Creates a new Point2D object from the specified parameters.
        /// </summary>
		public Point2D(double x,double y)
		{
			X=x;
			Y=y;
		}
		
        /// <summary>
        /// Creates a new Point2D object by copying another Point2D
        /// </summary>
        /// <param name="otherPoint2D"></param>
		public Point2D(Point2D otherPoint2D)
		{
			X=otherPoint2D.X;
			Y=otherPoint2D.Y;
		}

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the Angle of the line from the origin to this point.
        /// </summary>
        /// <returns>Angle in radians</returns>
		public double angle()
		{
			if(X==0.0)
			{
				if(Y>0)
					return 3.14/2.0;
				else
					return 3.14*3.0/2.0;
			}
			double ang = Math.Atan(Y/X);
			if(X>0)
				return ang;
			
			return ang+3.14;
		}

        /// <summary>
        /// Rotates this point about another point.
        /// </summary>
        /// <param name="Angle">Angle of rotation, in radians.</param>
        /// <param name="point">Second point to be rotated about.</param>
		public void rotate(double angle, Point2D point)
		{
			X-=point.X;
			Y-=point.Y;
			
			double ox = X;
			double oy = Y;

			X=Math.Cos(angle)*ox - Math.Sin(angle)*oy;
			Y=Math.Sin(angle)*ox + Math.Cos(angle)*oy;
			
			X+=point.X;
			Y+=point.Y;
		}
		
		/// <summary>
		/// Calculates the squared Distance to another point.
		/// </summary>
		public double squaredDistance(Point2D point)
		{
			double dx=point.X-X;
			double dy=point.Y-Y;
			return dx*dx+dy*dy;
		}
		
        /// <summary>
        /// Calculates the Manhattan Distance (according to a Cartesian grid coordinate system) to a second point.
        /// </summary>
		public double manhattanDistance(Point2D point) 
		{
			double dx=Math.Abs(point.X-X);
			double dy=Math.Abs(point.Y-Y);
			return dx+dy;
		}

		/// <summary>
		/// Calculates the Distance to another point.
		/// </summary>
		public double distance(Point2D point)
		{
			return Math.Sqrt(squaredDistance(point));
        }

        #endregion
    }
	
	/// <summary>
	/// Line segment class. Performs basic trig operations.
	/// </summary>
	public class Line2D
    {
        #region Instance variables

        public Point2D Endpoint1,Endpoint2;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty Line2D object.
        /// </summary>
        public Line2D() { }

        /// <summary>
        /// Creates a new Line2D object from the specified parameters.
        /// </summary>
		public Line2D(Point2D endpoint1, Point2D endpoint2)
		{
			Endpoint1=endpoint1;
			Endpoint2=endpoint2;
		}

        /// <summary>
        /// Creates a new Line2D object by copying another Line2D.
        /// </summary>
		public Line2D(Line2D otherLine2D)
		{
			Endpoint1=otherLine2D.Endpoint1;
			Endpoint2=otherLine2D.Endpoint2;
		}

        #endregion

        #region Methods

        /// <summary>
        /// Scales the line segment by the specified factor.
        /// </summary>
        public void scale(double factor)
		{
			Point2D mid = midpoint();
			Endpoint1.X = mid.X + (Endpoint1.X-mid.X)*factor;
			Endpoint1.Y = mid.Y + (Endpoint1.Y-mid.Y)*factor;
			Endpoint2.X = mid.X + (Endpoint2.X-mid.X)*factor;
			Endpoint2.Y = mid.Y + (Endpoint2.Y-mid.Y)*factor;
		}
		
		/// <summary>
		/// Calculates the midpoint of the line segment.
		/// </summary>
		public Point2D midpoint()
		{
			double x = (Endpoint1.X+Endpoint2.X)/2.0;
			double y = (Endpoint1.Y+Endpoint2.Y)/2.0;
			return new Point2D(x,y);
		}

        /// <summary>
        /// Calculates the nearest intersection of the line segment with a given AreaOfImpact. (The line segment is interpreted as a ray going from its first endpoint to its second one.
        /// </summary>
        /// <param name="C">Circle that is supposed to intersect with the line segment.</param>
        /// <param name="found">**Output parameter**, will return false if no intersection exists.</param>
		public double nearestIntersection(Circle2D circle,out bool found)
		{
			double dx,dy;
			
			dx=Endpoint2.X-Endpoint1.X;
			dy=Endpoint2.Y-Endpoint1.Y;

			double px=Endpoint1.X-circle.Position.X;
			double py=Endpoint1.Y-circle.Position.Y;
			
			double a= dx*dx + dy*dy;
			double b= 2*px*dx+2*py*dy;
			double c= px*px + py*py - circle.Radius*circle.Radius;
			
			double det = b*b-4.0*a*c;
			
			if(det<0.0)
			{
				found=false;
				return -1.0;
			}
				
			double sqrt_det = Math.Sqrt(det);
			double t1 = (-b+sqrt_det)/(2*a);
			double t2 = (-b-sqrt_det)/(2*a);
			
			found=false;
			double t=0.0;
			if(t2<0)
			{
				if(t1>0)
				{
					found=true;
					t=t1;
				}
			}
			else
			{
				found=true;
				t=t2;
			}
			if(!found)
				return -1.0;
				
			return t*Math.Sqrt(dx*dx+dy*dy);
			
		}
		
        /// <summary>
        /// Calculates the point of intersection between two line segments
        /// </summary>
        /// <param name="L">Second line segment that is supposed to intersect with this line segment.</param>
        /// <param name="found">**Output parameter**, will return false if no intersection exists.</param>
		public Point2D intersection(Line2D line,out bool found)
		{
			Point2D pt = new Point2D(0.0,0.0);
			Point2D A = Endpoint1;
			Point2D B = Endpoint2;
			Point2D C = line.Endpoint1;
			Point2D D = line.Endpoint2;
			
			double rTop = (A.Y-C.Y)*(D.X-C.X)-(A.X-C.X)*(D.Y-C.Y);
			double rBot = (B.X-A.X)*(D.Y-C.Y)-(B.Y-A.Y)*(D.X-C.X);
			
			double sTop = (A.Y-C.Y)*(B.X-A.X)-(A.X-C.X)*(B.Y-A.Y);
			double sBot = (B.X-A.X)*(D.Y-C.Y)-(B.Y-A.Y)*(D.X-C.X);
			
			if ((rBot == 0 || sBot == 0))
			{
				found = false;
				return pt;
			}
			double r = rTop/rBot;
			double s = sTop/sBot;
			if( (r>0) && (r<1) && (s>0) && (s<1))
			{
				pt.X = A.X + r * (B.X-A.X);
				pt.Y = A.Y + r * (B.Y - A.Y);
				found=true;
				return pt;
			}
			else
			{
				found = false;
				return pt;
			}
		}
		
		/// <summary>
        /// Calculate the squared Distance from this line to a point.
		/// </summary>
		public double squaredDistance(Point2D point)
		{
			double utop = (point.X-Endpoint1.X)*(Endpoint2.X-Endpoint1.X)+(point.Y-Endpoint1.Y)*(Endpoint2.Y-Endpoint1.Y);
			double ubot = Endpoint1.squaredDistance(Endpoint2);
			double u = utop/ubot;
			
			if(u<0 || u> 1)
			{
				double d1 = Endpoint1.squaredDistance(point);
				double d2 = Endpoint2.squaredDistance(point);
				if(d1<d2) return d1;
				return d2;
			}
			Point2D p=new Point2D(0.0,0.0);
			p.X=Endpoint1.X+u*(Endpoint2.X-Endpoint1.X);
			p.Y=Endpoint1.Y+u*(Endpoint2.Y-Endpoint1.Y);
			return p.squaredDistance(point);
		}
	
        /// <summary>
        /// Calculate the Distance from this line to a point
        /// </summary>
		public double distance(Point2D point)
		{
			return Math.Sqrt(squaredDistance(point));
		}
		
        /// <summary>
        /// Calculates the squared magnitude of this line segment
        /// </summary>
		public double squaredLength()
		{
			return Endpoint1.squaredDistance(Endpoint2);
		}

		/// <summary>
        /// Calculates the length of this line segment
		/// </summary>
		public double length()
		{
			return Endpoint1.distance(Endpoint2);
        }

        #endregion
    }
}