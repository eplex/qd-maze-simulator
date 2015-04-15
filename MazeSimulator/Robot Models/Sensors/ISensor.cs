using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace MazeSimulator
{
    /// <summary>
    /// Interface for all sensor classes.
    /// </summary>
    public interface ISensor
    {
        #region Methods

        double getActivation();
		double getRawActivation();
		void update(Environment env, List<Robot> robots, CollisionManager cm);
		void draw(Graphics g, CoordinateFrame frame);

        #endregion
    }
}
