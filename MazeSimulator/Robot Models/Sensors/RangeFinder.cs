using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace MazeSimulator
{
	/// <summary>
	/// Basic pie slice sensor.
	/// </summary>
	public class Radar : ISensor
    {
        #region Instance variables

        public Robot Owner;
        string Type = "goal";
        public double StartAngle;  // Both angles are in radians
        public double EndAngle;
        public double Distance;
        public double MaxRange;
        public double Noise;
        public double Activation = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Radar(double startAngle, double endAngle, Robot owner, string type = "goal", double maxRange = 150)
        {
			Owner = owner;
            StartAngle = startAngle;
            EndAngle = endAngle;
            MaxRange = maxRange;
            Distance = (-1);
            Noise = 0.0;
            Type = type;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the current sensor activation.
        /// </summary>
        public double getActivation() 
        {
            return Activation;
		}

        /// <summary>
        /// Returns the raw sensor activation.
        /// </summary>
		public double getRawActivation() 
        {
            return Activation;
		}
      
        /// <summary>
        /// Updates the sensor based on the current state of the environment.
        /// </summary>
		public void update(Environment env, List<Robot> robots, CollisionManager cm) 
		{
            Activation = 0;
            Point2D temp;
            double dist;
            double angle;
            if (Type == "poi")
            {
                foreach (Point p in env.POIPosition)
                {
                    temp = new Point2D(p.X, p.Y);
                    dist = EngineUtilities.euclideanDistance(temp, new Point2D(Owner.AreaOfImpact.Position.X, Owner.AreaOfImpact.Position.Y));
                    temp.X -= (float)Owner.AreaOfImpact.Position.X;
                    temp.Y -= (float)Owner.AreaOfImpact.Position.Y;

                    angle = (float)temp.angle();

                    angle -= Owner.Heading;
                    angle *= 57.297f;

                    while (angle > 360)
                        angle -= 360;
                    while (angle < 0)
                        angle += 360;

                    if (StartAngle < 0 && EndAngle > 0) // sensor spans the 0 line
                    {
                        if ((angle >= StartAngle + 360.0 && angle <= 360) || (angle >= 0 && angle <= EndAngle))
                        {
                            Activation = Math.Max(1.0 - (dist > MaxRange ? 1.0 : dist / MaxRange), Activation);
                        }
                    }
                    else
                    {

                        if (angle >= StartAngle && angle < EndAngle)
                        {
                            Activation = Math.Max(1.0 - (dist > MaxRange ? 1.0 : dist / MaxRange), Activation);
                        }

                        else if (angle + 360.0 >= StartAngle && angle + 360.0 < EndAngle)
                        {
                            Activation = Math.Max(1.0 - (dist > MaxRange ? 1.0 : dist / MaxRange), Activation);
                        }
                    }
                }
                return;
            }

            
            if (Type == "goal")
                temp = new Point2D(env.goal_point.X, env.goal_point.Y);
            else
                temp = new Point2D(env.start_point.X, env.start_point.Y);

            dist = EngineUtilities.euclideanDistance(temp, new Point2D(Owner.AreaOfImpact.Position.X, Owner.AreaOfImpact.Position.Y));

            //translate with respect to location of navigator
            temp.X -= (float)Owner.AreaOfImpact.Position.X;
            temp.Y -= (float)Owner.AreaOfImpact.Position.Y;

            angle = (float)temp.angle();

            angle *= 57.297f;
            angle -= (float)Owner.Heading * 57.297f;

            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;

            if (angle >= StartAngle && angle < EndAngle)
            {
                if (Type == "goal")
                    Activation = 1.0 - (dist > MaxRange ? 1.0 : dist / MaxRange);
                else
                    Activation = 1.0;
            }

            else if (angle + 360.0 >= StartAngle && angle + 360.0 < EndAngle)
            {
                if (Type == "goal")
                    Activation = 1.0 - (dist > MaxRange ? 1.0 : dist / MaxRange);
                else
                    Activation = 1.0;
            }
		}

        /// <summary>
        /// Draws the radar to the screen.
        /// </summary>
		public void draw(Graphics g, CoordinateFrame frame)
        {
            Brush b = new SolidBrush(Color.Green);
            Size s = new Size((int)((MaxRange) / frame.Scale), (int)((MaxRange) / frame.Scale));

            if (Type == "directedVoiceNOTASENSOR") // Special drawing rules for this Type of sensor (used for showing communication)
            {   
                if (Activation < 0.1)
                {
                    s = new Size((int)((MaxRange * 1.0) / frame.Scale), (int)((MaxRange * 1.0) / frame.Scale));
                    b = new SolidBrush(Color.FromArgb((int)(0), 128, 128, 128));
                }
                else
                {
                    s = new Size((int)((MaxRange * Activation) / frame.Scale), (int)((MaxRange * Activation) / frame.Scale));
                    b = new SolidBrush(Color.FromArgb((int)(80), 200, 0, 200));
                }
                g.FillPie(b, new Rectangle(frame.convertToDisplay((float)(Owner.AreaOfImpact.Position.X), (float)(Owner.AreaOfImpact.Position.Y)) - new Size(s.Width / 2, s.Height / 2), s), (float)StartAngle + (float)(Owner.Heading * 57.2957795), (float)(EndAngle - StartAngle));
                return;
            }

            Rectangle r = new Rectangle(frame.convertToDisplay((float)(Owner.AreaOfImpact.Position.X), (float)(Owner.AreaOfImpact.Position.Y)),s);
            if (Activation == 0)
            {
                if(Type == "goal")
                    g.DrawPie(EngineUtilities.GreendPen, new Rectangle(frame.convertToDisplay((float)(Owner.AreaOfImpact.Position.X), (float)(Owner.AreaOfImpact.Position.Y)) - new Size(s.Width / 2, s.Height / 2), s), (float)StartAngle + (float)(Owner.Heading * 57.2957795), (float)(EndAngle - StartAngle));
                else
                    g.DrawPie(EngineUtilities.RedPen, new Rectangle(frame.convertToDisplay((float)(Owner.AreaOfImpact.Position.X), (float)(Owner.AreaOfImpact.Position.Y)) - new Size(s.Width / 2, s.Height / 2), s), (float)StartAngle + (float)(Owner.Heading * 57.2957795), (float)(EndAngle - StartAngle));
                
            }
            else
            {
                if (Type == "goal")
                {
                    b = new SolidBrush(Color.FromArgb((int)(Activation * 255), 0, 255, 0));
                    g.FillPie(b, new Rectangle(frame.convertToDisplay((float)(Owner.AreaOfImpact.Position.X), (float)(Owner.AreaOfImpact.Position.Y)) - new Size(s.Width / 2, s.Height / 2), s), (float)StartAngle + (float)(Owner.Heading * 57.2957795), (float)(EndAngle - StartAngle));
                }
                else
                {
                    b = new SolidBrush(Color.FromArgb((int)(Activation * 255), 255, 0, 0));
                    
                    g.FillPie(b, new Rectangle(frame.convertToDisplay((float)(Owner.AreaOfImpact.Position.X), (float)(Owner.AreaOfImpact.Position.Y)) - new Size(s.Width / 2, s.Height / 2), s), (float)StartAngle + (float)(Owner.Heading * 57.2957795), (float)(EndAngle - StartAngle));
                }
            }
		}

        #endregion  
	}
	
    /// <summary>
    /// Rangefinder sensor - returns the DistanceToClosestObject to the closest sensed object within its range.
    /// </summary>
    public class RangeFinder : ISensor
    {
        #region Instance variables

        public Robot Owner;
        public double Angle;
        public double DistanceToClosestObject;
        public double MaxRange;

        #endregion

        #region Constructors

        public RangeFinder(double a, Robot o, double _max_range, double _noise)
        {
 			Owner =o;
            Angle = a;
            MaxRange = _max_range;
            DistanceToClosestObject = (-1);
        }

        #endregion

        #region Methods

        public double getActivation() {
			return DistanceToClosestObject/MaxRange;
		}
		public double getRawActivation() {
			return DistanceToClosestObject;
		}
      
      
		public virtual void update(Environment env, List<Robot> robots,CollisionManager cm)
		{
			Point2D location = new Point2D(Owner.Location.X, Owner.Location.Y);
			SimulatorObject hit;			
			DistanceToClosestObject = cm.raycast(Angle,MaxRange,location,Owner,out hit);
            Debug.Assert(!Double.IsNaN(DistanceToClosestObject), "NaN in inputs");
		}

		public virtual void draw(Graphics g, CoordinateFrame frame)
        {
			Point a = frame.convertToDisplay((float)(Owner.Location.X),(float)(Owner.Location.Y));
	        Point b = frame.convertToDisplay((float)(Owner.Location.X+Math.Cos(Angle+Owner.Heading)*DistanceToClosestObject), (float)(Owner.Location.Y+Math.Sin(Angle+Owner.Heading)*DistanceToClosestObject));
			g.DrawLine(EngineUtilities.GreendPen, a, b);
		}
		
        #endregion
    }
}
