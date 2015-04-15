using System;
using System.Collections.Generic;
using System.Text;

namespace MazeSimulator
{
    /// <summary>
    /// Base clase for simulated objects
    /// </summary>
    public abstract class SimulatorObject
    {
        #region Instance variables

        public string Name;
        public Point2D Location; 
        public bool Dynamic;
        public bool Visible; 
		public bool Selected;

        #endregion

        #region Methods

        public abstract void update();
        public abstract void undo();

        #endregion
    }
}
