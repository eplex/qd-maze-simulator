using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace MazeSimulator
{
    /// <summary>
    /// A simple wall class.
    /// </summary>
    public class Wall : SimulatorObject
    {
        #region Instance variables

        public Line2D Line;
        public bool Colored;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an empty Wall object
        /// </summary>
        public Wall()
        {
        }

        /// <summary>
        ///  Constructs a new Wall object by copying an existing Wall.
        /// </summary>
        public Wall(Wall otherWall)
        {
            Name = otherWall.Name;
            Colored = false;

            Line = new Line2D(otherWall.Line);

            //set center point
            Location = Line.midpoint();

            Dynamic = false;
            Visible = otherWall.Visible;
        }

        /// <summary>
        /// Constructs a new Wall object from scratch
        /// </summary>
        /// <param name="nx1">X component of endpoint 1</param>
        /// <param name="ny1">Y component of endpoint 1</param>
        /// <param name="nx2">X component of endpoint 2</param>
        /// <param name="ny2">Y component of endpoint 2</param>
        /// <param name="vis">Is the wall visible?</param>
        /// <param name="n">Wall name</param>
        public Wall(double nx1, double ny1, double nx2, double ny2, bool vis, string n)
        {
            Name = n;
            Colored = false;
            Point2D p1 = new Point2D(nx1, ny1);
            Point2D p2 = new Point2D(nx2, ny2);
            Line = new Line2D(p1, p2);

            //set center point
            Location = Line.midpoint();

            Dynamic = false;
            Visible = vis;
        }

        #endregion

        #region Methods

        public override void update() { }
        public override void undo() { }

        /// <summary>
        /// Draws the wall to the screen.
        /// </summary>
        public void draw(Graphics graphics, CoordinateFrame frame)
        {
			float ax,ay,bx,by;
			frame.convertToDisplay((float)Line.Endpoint1.X,(float)Line.Endpoint1.Y,out ax, out ay);
			frame.convertToDisplay((float)Line.Endpoint2.X,(float)Line.Endpoint2.Y,out bx, out by);

            if (Visible)
                graphics.DrawLine(EngineUtilities.BluePen, ax,ay, bx,by);
            else
                graphics.DrawLine(EngineUtilities.GreendPen, ax,ay,bx, by);
        }

        #endregion
    }
}
