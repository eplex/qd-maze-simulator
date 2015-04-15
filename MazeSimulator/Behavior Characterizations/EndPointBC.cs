using System;
using System.Collections.Generic;
using System.Text;

namespace MazeSimulator
{
    /// <summary>
    /// Behavior characterization that records the robot's final position. If used with a multiagent team, the characterization will be a vector composed of *all* robots' endpoints.
    /// </summary>
    class EndPointBC : IBehaviorCharacterization
    {
        #region Instance variables

        bool IBehaviorCharacterization.wraparoundDistCalc
        {
            get { return false; }
        }
        string IBehaviorCharacterization.name
        {
            get { return this.GetType().Name; }
        }

        string IBehaviorCharacterization.description
        {
            get { return "Vector of robot's ending locations appended together"; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty behavior characterization instance.
        /// </summary>
        public IBehaviorCharacterization copy()
        {
            return new EndPointBC();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs the behavior vector at the end of an individual evaluation.
        /// </summary>
        List<double> IBehaviorCharacterization.calculate(SimulatorExperiment exp, instance_pack ip)
        {
            List<double> bc = new List<double>();

            for (int agentIndex = 0; agentIndex < ip.robots.Count; agentIndex++)
            {
                double x;
                double y;

                x = ip.robots[agentIndex].Location.X;
                y = ip.robots[agentIndex].Location.Y;
                x = (x - ip.env.AOIRectangle.Left) / ip.env.AOIRectangle.Width;
                y = (y - ip.env.AOIRectangle.Top) / ip.env.AOIRectangle.Height;
                bc.Insert(agentIndex * 2, x);
                bc.Insert(agentIndex * 2 + 1, y);

            }
            return bc;
        }

        void IBehaviorCharacterization.update(SimulatorExperiment exp, instance_pack ip) { }
        void IBehaviorCharacterization.reset() { }

        #endregion
    }
}