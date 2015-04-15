using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using SharpNeatLib;
using System.Xml.Serialization;

namespace MazeSimulator
{
    public class Environment
    {
        #region Instance variables

        // Legacy naming convention used to maintain compatibility with old experiment/CurrentEnvironment files. Do not modify.

        public List<Wall> walls;    
        public String name;  
		[XmlIgnore]
		public SharpNeatLib.Maths.FastRandom rng;

        public Rectangle AOIRectangle { get; set; }
        public List<Point> POIPosition { get; set; }
        public float maxDistance; // Distance from corner to corner. Determined by AOI.

        public Point2D start_point; //Start location of the agent
        public Point2D goal_point; 

        public int group_orientation;
        public int robot_spacing;
        public int robot_heading;
		public int seed;
		public float view_x;
		public float view_y;
		public float view_scale;

        public float stagger;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a small, empty CurrentEnvironment.
        /// </summary>
        public Environment()
        {
            reset();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a clone of the current CurrentEnvironment.
        /// </summary>
        public Environment copy()
        {
            Environment newenv = new Environment();
            newenv.seed = seed;
            newenv.AOIRectangle = new Rectangle(AOIRectangle.X, AOIRectangle.Y, AOIRectangle.Width, AOIRectangle.Height);
            newenv.group_orientation = group_orientation;
            newenv.goal_point = new Point2D(goal_point);
            newenv.start_point = new Point2D(start_point);
            newenv.robot_heading = robot_heading;
            newenv.robot_spacing = robot_spacing;
            foreach (Wall w in walls)
            {
                newenv.walls.Add(new Wall(w));
            }
            foreach (Point p in POIPosition)
            {
                newenv.POIPosition.Add(new Point(p.X, p.Y));
            }
            newenv.rng = new SharpNeatLib.Maths.FastRandom(seed);
            newenv.stagger = stagger;
            return newenv;
        }

        /// <summary>
        /// Save the CurrentEnvironment to the specified XML file.
        /// </summary>
        public void save(string name)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.GetType());
            TextWriter outfile = new StreamWriter(name);
            x.Serialize(outfile, this);
            outfile.Close();
        }

        /// <summary>
        /// Loads a CurrentEnvironment from the specified XML file and initializes it.
        /// </summary>
        public static Environment loadEnvironment(string name)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(Environment));
            TextReader infile = new StreamReader(name);
            Environment k = (Environment)x.Deserialize(infile);
            k.maxDistance = (float)Math.Sqrt(k.AOIRectangle.Width * k.AOIRectangle.Width + k.AOIRectangle.Height * k.AOIRectangle.Height);

            k.name = name;

            Console.WriteLine("# walls: " + k.walls.Count);
			k.rng = new SharpNeatLib.Maths.FastRandom(k.seed);
            return k;
        }

        /// <summary>
        /// Finds the wall with the specified name. Pretty straightforward.
        /// </summary>
        public Wall findWallByName(string name)
        {
            foreach (Wall wall in walls)
            {
                if (wall.Name == name)
                    return wall;
            }
            return null;
        }

        /// <summary>
        /// Resets the CurrentEnvironment to a small, blank square.
        /// </summary>
        public void reset()
        {

            seed = 0;
            view_x = 0.0f;
            view_y = 0.0f;
            view_scale = 5.0f;
            AOIRectangle = new Rectangle(30, 60, 640, 500);

            group_orientation = 0;
            robot_spacing = 30;
            robot_heading = 0;
            rng = new SharpNeatLib.Maths.FastRandom();
            walls = new List<Wall>();

            POIPosition = new List<Point>();

            start_point = new Point2D(0, 0);
            goal_point = new Point2D(100, 100);
        }


        /// <summary>
        /// Draws the CurrentEnvironment to the screen.
        /// </summary>
        public void draw(Graphics g, CoordinateFrame frame)
        {
            float sx, sy;
            float gx, gy;

            frame.convertToDisplay((float)start_point.X, (float)start_point.Y, out sx, out sy);
            frame.convertToDisplay((float)goal_point.X, (float)goal_point.Y, out gx, out gy);
            Rectangle startrect = new Rectangle((int)sx - 3, (int)sy - 3, 6, 6);
            Rectangle goalrect = new Rectangle((int)gx - 3, (int)gy - 3, 6, 6);

            float rx, ry, rsx, rsy;
            frame.convertToDisplay((float)AOIRectangle.X, (float)AOIRectangle.Y, out rx, out ry);
            frame.convertToDisplayOffset((float)AOIRectangle.Width, (float)AOIRectangle.Height, out rsx, out rsy);
            Rectangle AOIDisplay = new Rectangle((int)rx, (int)ry, (int)rsx, (int)rsy);

            //Display Area of Interest rectangle
            g.DrawRectangle(EngineUtilities.DashedPen, AOIDisplay);

            g.DrawEllipse(EngineUtilities.BluePen, startrect);
            g.DrawEllipse(EngineUtilities.RedPen, goalrect);

            //Display Point Of Interests
            int index = 0;
            foreach (Point p in POIPosition)
            {
                Point p2 = frame.convertToDisplay(p);
                g.DrawEllipse(EngineUtilities.GreendPen, new Rectangle((int)p2.X - 3, (int)p2.Y - 3, 6, 6));
                g.DrawString(index.ToString(), new Font("Verdana", 8), new SolidBrush(Color.Black), p2.X, p2.Y);
                index++;
            }

            foreach (Wall wall in walls)
            {
                wall.draw(g, frame);
            }
        }

        #endregion
    }
}
