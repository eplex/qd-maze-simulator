using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SharpNeatLib;
using SharpNeatLib.Maths;

namespace MazeSimulator
{
    /// <summary>
    /// A class containing methods for converting between simulator space and graphic display space.
    /// </summary>
	public class CoordinateFrame
    {
        #region Instance variables

        public float X,Y;
		public float Scale;
		public float Rotation;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new CoordinateFrame object from the specified parameters.
        /// </summary>
        public CoordinateFrame(float x, float y, float scale, float rotation) 
        {
			X=x;
			Y=y;
			Scale=scale;
			Rotation=rotation;
		}

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Aligns the graphical display space with the simulator space.
        /// </summary>
        public void syncFromEnvironment(Environment environment) 
        {
			X=environment.view_x;
			Y=environment.view_y;
			Scale=environment.view_scale;
		}

        /// <summary>
        /// Aligns the simulator space with the graphical display space.
        /// </summary>
		public void syncToEnvironment(Environment environment) 
        {
			environment.view_x=X;
			environment.view_y=Y;
			environment.view_scale=Scale;
		}

        /// <summary>
        /// Converts a, (x,y) coordinate from simulator space to display space.
        /// </summary>
		public Point convertToDisplay(float x, float y) 
        {
			float ox,oy;
			convertToDisplay(x,y,out ox,out oy);
			return new Point((int)ox,(int)oy);
		}

		/// <summary>
        /// Converts a Point coordinate from simulator space to display space.
		/// </summary>
		public Point convertToDisplay(Point point) 
        {
			return convertToDisplay(point.X,point.Y);
		}
		
        /// <summary>
        /// Converts a Point2D coordinate from simulator space to display space
        /// </summary>
		public Point2D convertToDisplay(Point2D point) 
        {
			float ox,oy;
			convertToDisplay((float)point.X,(float)point.Y,out ox, out oy);
			return new Point2D((double)ox,(double)oy);
		}
		
		/// <summary>
        /// Converts an (x,y) coordinate from simulator space to display space and returns an (x,y) coordinate in raw float format. Input is a delta, not an absolute point in terms of screen coordinates. Output is a delta in terms of simulator coordinates.
		/// </summary>
		public void convertToDisplay(float ix,float iy,out float ox, out float oy) 
		{
			ox = (ix-X)/Scale;
			oy = (iy-Y)/Scale;
		}
		
		/// <summary>
        /// Converts an (x,y) coordinate from display space to simulator space and returns an (x,y) coordinate in raw float format. Input is a delta, not an absolute point in terms of screen coordinates. Output is a delta in terms of simulator coordinates.
		/// </summary>
		public void convertFromDisplay(float ix,float iy,out float ox, out float oy) 
		{
			ox = ix*Scale+X;
			oy = iy*Scale+Y;
		}
		
	    /// <summary>
        /// Converts an (x,y) coordinate from simulator space to display space and returns an (x,y) coordinate in raw float format, leaving offsets intact. Input is a delta, not an absolute point in terms of screen coordinates. Output is a delta in terms of simulator coordinates.
	    /// </summary>
		public void convertToDisplayOffset(float ix, float iy, out float ox, out float oy)
		{
			ox = ix/Scale;
			oy = iy/Scale;
		}

        /// <summary>
        /// Converts an (x,y) coordinate from display space to simulator space and returns an (x,y) coordinate in raw float format, leaving offsets intact. Input is a delta, not an absolute point in terms of screen coordinates. Output is a delta in terms of simulator coordinates.
        /// </summary>
		public void convertFromDisplayOffset(float ix, float iy, out float ox, out float oy)
		{
			ox = ix*Scale;
			oy = iy*Scale;
        }

        #endregion
    }
	
    /// <summary>
    /// Base class for all collision managers. 
    /// </summary>
	public abstract class CollisionManager
    {
        #region Instance variables

        public bool AgentVisible;
        public bool AgentCollide;

        #endregion

        #region Methods

        public abstract CollisionManager copy();
		public abstract void initialize(Environment environment, SimulatorExperiment experiment, List<Robot> robots);
		public virtual void simulationStepCallback() { }
		public abstract bool robotCollide(Robot robot);
		public abstract double raycast(double angle, double maxRange, Point2D point, Robot owner, out SimulatorObject hit, bool absolute = false);

        #endregion
    }
	
    /// <summary>
    /// A class containing various math utilities and methods supporting sensing and collision detection.
    /// </summary>
    public class EngineUtilities
    {
        #region Instance variables

        public static Random RNG = new Random();

        #region Pen Colors
        public static Pen RedPen = new Pen(System.Drawing.Brushes.Red, 2.0f);
        public static Pen BluePen = new Pen(System.Drawing.Brushes.Blue, 2.0f);
        public static Pen GreendPen = new Pen(System.Drawing.Brushes.Green, 2.0f);
        public static Pen YellowPen = new Pen(System.Drawing.Brushes.Yellow, 2.0f);
        public static Pen DashedPen = new Pen(Brushes.Black, 1.0f);
        public static Pen TransGrayPen = new Pen(Color.FromArgb(64, Color.Black));

        public static SolidBrush voiceColorBrush = new SolidBrush(Color.FromArgb(64, Color.Black));
        public static SolidBrush backGroundColorBrush = new SolidBrush( Color.White ); 
        #endregion

        #endregion

        #region Methods

        #region Collision Handling

        public static float addNoise(float val,float percentage,SharpNeatLib.Maths.FastRandom rng)
		{
			percentage/=100.0f;
			float p1 = 1.0f -percentage;
			float p2 = percentage;
			float rval = (float)rng.NextDouble();
			return p1*val+p2*rval;
		}

        //wall-wall (should never collide)
        public static bool collide(Wall a, Wall b)
        {
            return false;
        }

        public static bool collide(Wall wall, Robot robot)
        {
            Point2D a1 = new Point2D(wall.Line.Endpoint1);
            Point2D a2 = new Point2D(wall.Line.Endpoint2);
            Point2D b = new Point2D(robot.Location.X, robot.Location.Y);
            if (!wall.Visible)
                return false;
            double rad = robot.Radius;
            double r = ((b.X - a1.X) * (a2.X - a1.X) + (b.Y - a1.Y) * (a2.Y - a1.Y)) / wall.Line.squaredLength();
            double px = a1.X + r * (a2.X - a1.X);
            double py = a1.Y + r * (a2.Y - a1.Y);
            Point2D np = new Point2D(px, py);
            double rad_sq = rad * rad;

            if (r >= 0.0f && r <= 1.0f)
            {
                if (np.squaredDistance(b) < rad_sq)
                    return true;
                else
                    return false;
            }

            double d1 = b.squaredDistance(a1);
            double d2 = b.squaredDistance(a2);
            if (d1 < rad_sq || d2 < rad_sq)
                return true;
            else
                return false;
        }

        public static bool collide(Robot a, Wall b)
        {
            return EngineUtilities.collide(b, a);
        }

        public static bool collide(Robot a, Robot b)
        {
            return a.AreaOfImpact.collide(b.AreaOfImpact);
        }

        #endregion

        /// <summary>
        /// Checks the conic area defined by a radar sensor for detectable objects.
        /// </summary>
        /// <returns>Distance to the closest object.</returns>
        public static double scanCone(Radar radar, List<SimulatorObject> objList)
        {
            double distance = radar.MaxRange;
            double new_distance;
			double heading=radar.Owner.Heading;
			Point2D point=new Point2D(radar.Owner.Location.X,radar.Owner.Location.Y);
			
            double startAngle = radar.StartAngle + heading;
            double endAngle = radar.EndAngle + heading;
            double twoPi = 2 * Math.PI;

            if (startAngle < 0)
            {
                startAngle += twoPi;
            }
            else if (startAngle > twoPi)
            {
                startAngle -= twoPi;
            }
            if (endAngle < 0)
            {
                endAngle += twoPi;
            }
            else if (endAngle > twoPi)
            {
                endAngle -= twoPi;
            }

            foreach (SimulatorObject obj in objList)
            {
                bool found = false;

                if (obj == radar.Owner)
                    continue;

                new_distance = point.distance(obj.Location);
                     
                double angle = Math.Atan2(obj.Location.Y - point.Y, obj.Location.X - point.X);
                       
                if (angle < 0)
                {
                    angle += Utilities.twoPi;
                }

                if (endAngle < startAngle)
                {
                    if ((angle >= startAngle && angle <= Math.PI * 2) || (angle >= 0 && angle <= endAngle))
                    {
                        found = true;
                    }
                }

                else if ((angle >= startAngle && angle <= endAngle))
                    found = true;

                if (found)
                {
                    if (new_distance < distance)
                    {
                        distance = new_distance;
                    }
                }
            }

            return distance;
        }

        /// <summary>
        /// Calculates the Euclidean Distance between two Point objects.
        /// </summary>
        public static double euclideanDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /// <summary>
        /// Calculates the Euclidean Distance between two Point2D objects.
        /// </summary>
        public static double euclideanDistance(Point2D p1, Point2D p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /// <summary>
        /// Calculates the Euclidean Distance between two Robot objects.
        /// </summary>
        public static double euclideanDistance(Robot r1, Robot r2)
        {
            return Math.Sqrt(Math.Pow(r1.Location.X - r2.Location.X, 2) + Math.Pow(r1.Location.Y - r2.Location.Y, 2));
        }

        /// <summary>
        /// Calculates the squared Distance between two Robot objects.
        /// </summary>
        public static double squaredDistance(Robot r1, Robot r2)
        {
            return Math.Pow(r1.Location.X - r2.Location.X, 2) + Math.Pow(r1.Location.Y - r2.Location.Y, 2);
        }

        /// <summary>
        /// Calculates the squared Distance between two Point2D objects.
        /// </summary>
        public static double squaredDistance(Point2D p1, Point2D p2)
        {
            return Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2);
        }

        /// <summary>
        /// Calculates the Euclidean Distance between a Point2D object and a Point object.
        /// </summary>
        public static double euclideanDistance(Point2D p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /// <summary>
        /// Calculates the Euclidean Distance between a Point object and a Point2D object.
        /// </summary>
        public static double euclideanDistance(Point p1, Point2D p2)
        {
            return euclideanDistance(p2, p1);
        }

        /// <summary>
        /// Ensures that a given value does not exceed a specified max and min, but not scaling the value if it already fits within the specified bounds.
        /// </summary>
        public static double clamp(double val, double min, double max)
        {
            if (val > max)
                val = max;
            else if (val < min)
                val = min;
            
            return val;
        }

        #endregion
    }
}
