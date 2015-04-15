using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeSimulator.Behavior_Characterizations
{
    /// <summary>
    /// Behavior characterization that samples the robot's Trajectory at regularly spaced intervals. This characterization is intended for single agent use only and is not currently configured for multiagent teams.
    /// </summary>
    class HalfTrajectoryBC : IBehaviorCharacterization
    {
        #region Instance variables

        private int VectorLength = 6; // MUST BE AN EVEN NUMBER. Length 6 = 3 x,y samples
        private bool Initialized = false;
        private List<int> Trajectory;
        private List<double> BehaviorVector;

        private int chunkSize;
        private int endTick = 0;

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
            get { return "Robot's position sampled at evenly spaced intervals across the run."; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty behavior characterization instance.
        /// </summary>
        public IBehaviorCharacterization copy()
        {
            return new HalfTrajectoryBC();
        }

        #endregion

        #region IBehaviorCharacterization Methods

        /// <summary>
        /// Constructs the behavior vector at the end of an individual evaluation.
        /// </summary>
        List<double> IBehaviorCharacterization.calculate(SimulatorExperiment exp, instance_pack ip)
        {
            // initialize the BC vector
            BehaviorVector = new List<double>();

            // If the robot never Stopped, set the max evaluation time as the end tick
            if (endTick == 0)
                endTick = exp.evaluationTime;

            // Adjust end tick by the fraction we are sampling
            endTick = (int)(endTick * 0.5);

            // Calculate when to perform an update
            int numBehaviorChunks = VectorLength / 2;
            chunkSize = Convert.ToInt32(Math.Floor((double)endTick / (double)numBehaviorChunks)) * 2;

            float x, y;

            for (int chunkNum = 1; chunkNum < numBehaviorChunks + 1; chunkNum++)
            {
                // Take bc samples from the internal Trajectory store
                x = Trajectory[chunkNum * chunkSize - 2];
                x = (x - ip.env.AOIRectangle.Left) / ip.env.AOIRectangle.Width;
                BehaviorVector.Add(x);

                y = Trajectory[chunkNum * chunkSize - 1];
                y = (y - ip.env.AOIRectangle.Top) / ip.env.AOIRectangle.Height;
                BehaviorVector.Add(y);
            }

            return BehaviorVector;
        }

        /// <summary>
        /// Records the individual's location at each tick of the simulation. 
        /// </summary>
        void IBehaviorCharacterization.update(SimulatorExperiment exp, instance_pack ip)
        {
            if (ip.robots[0].Stopped)
            {
                // If this is the first update after the robot has Stopped, 
                // send the endpoint to be the current simulation tick
                if (endTick == 0)
                    endTick = Convert.ToInt32(ip.timeSteps * exp.timestep);
            }

            // initialize the Trajectory list
            if (!Initialized)
            {
                // initialize the sensor value sampling/storage components
                Trajectory = new List<int>(exp.evaluationTime * 2);
                Initialized = true;
            }

            // update the Trajectory at every tick
            Trajectory.Add(Convert.ToInt16(ip.robots[0].Location.X));
            Trajectory.Add(Convert.ToInt16(ip.robots[0].Location.Y));
        }

        /// <summary>
        /// Resets the behavior characterization. Currently not implemented.
        /// </summary>
        void IBehaviorCharacterization.reset()
        {
        }
        #endregion
    }
}
