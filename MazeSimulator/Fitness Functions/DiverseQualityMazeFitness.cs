using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace MazeSimulator
{
    /// <summary>
    /// Fitness function based on Distance to goal in the QD maze CurrentEnvironment. Not compatible with multiagent teams.
    /// </summary>
    class DiverseQualityMazeFitness : IFitnessFunction
    {
        #region Instance variables

        // Make sure to edit these variables and recompile before running experiments.

        bool[] ReachedPOI = new bool[7];
        bool ReachedGoal;
        double StoppingRange = 400;           // Individuals will be Stopped if their squared Distance to the goal point is below this number
        double MaxDistanceToGoal = 1142;      // Compute this value manually based on the furthest Distance from goal to one of the 4 corners of the AOI Rectangle
        const bool DEBUG = false;              // DEBUG = TRUE: Displays a "grid" of POI points which show how the space is divided up in the mapelites grid
        const bool DEBUG_CLEARONLY = false;    // DEBUG_CLEARONLY = TRUE: Clears all POI points without drawing new ones (useful when you want to save your finalized map)
        const int NumBinsPerDim = 6;          // <- (make sure this is the same as what mapelites thinks it is, or else the debug info won'type be much use. oh well)

        bool first = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Returns a new empty DiverseQualityMazeFitness instance.
        /// </summary>
        /// <returns></returns>
        public IFitnessFunction copy()
        {
            return new DiverseQualityMazeFitness();
        }

        /// <summary>
        /// Returns an empty DiverseQualityMazeFitnses instance.
        /// </summary>
        public DiverseQualityMazeFitness()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the name of the fitness function.
        /// </summary>
        string IFitnessFunction.name
        {
            get { return this.GetType().Name; }
        }

        /// <summary>
        /// Returns a string description of this fitness function.
        /// </summary>
        string IFitnessFunction.description
        {
            get { return "Diverse Quality Maze Single Goal Point Fitness"; }
        }

        /// <summary>
        /// Calculates the fitness of an individual based on Distance to the goal. Not compatible with multiagent teams.
        /// </summary>
        double IFitnessFunction.calculate(SimulatorExperiment engine, Environment environment, instance_pack ip, out double[] objectives)
        {
            objectives = null;
            return (MaxDistanceToGoal - ip.robots[0].Location.distance(environment.goal_point)) / MaxDistanceToGoal * 1000;
        }

        /// <summary>
        /// Called on every simulation tick. If this is the first tick, initialize some instance variables. On every other tick, check to see if the robot is within stopping Distance of the goal.
        /// </summary>
        void IFitnessFunction.update(SimulatorExperiment Experiment, Environment environment, instance_pack ip)
        {
            // Initialization routines
            if (first)
            {
                first = false;

                if (DEBUG)
                {
                    // If debugging: Add POI at thirds along the border of the AOI rectangle, so we can arrange the maze properly
                    // This functionality helps align the maze with the MAP-Elites grid.
                    environment.POIPosition.Clear();
                    if (!DEBUG_CLEARONLY)
                    {
                        double dwidth = environment.AOIRectangle.Width / NumBinsPerDim;
                        double dheight = environment.AOIRectangle.Height / NumBinsPerDim;
                        double cornerx = environment.AOIRectangle.Left;
                        double cornery = environment.AOIRectangle.Top;
                        for (int x = 0; x <= NumBinsPerDim; x++)
                        {
                            for (int y = 0; y <= NumBinsPerDim; y++)
                            {
                                environment.POIPosition.Add(new Point((int)(cornerx + dwidth * x), (int)(cornery + dheight * y)));
                            }
                        }
                    }
                }

                // Compute the max possible Distance a robot can achieve from the goal point while staying in the AOI bounds
                double maxSoFar = 0;

                // Top left
                Point2D cornerWeCareAboutRightNow = new Point2D(environment.AOIRectangle.Left, environment.AOIRectangle.Top);
                double tempDist = environment.goal_point.distance(cornerWeCareAboutRightNow);
                if (tempDist > maxSoFar) maxSoFar = tempDist;

                // Top right
                cornerWeCareAboutRightNow = new Point2D(environment.AOIRectangle.Right, environment.AOIRectangle.Top);
                tempDist = environment.goal_point.distance(cornerWeCareAboutRightNow);
                if (tempDist > maxSoFar) maxSoFar = tempDist;

                // Bottom right
                cornerWeCareAboutRightNow = new Point2D(environment.AOIRectangle.Right, environment.AOIRectangle.Bottom);
                tempDist = environment.goal_point.distance(cornerWeCareAboutRightNow);
                if (tempDist > maxSoFar) maxSoFar = tempDist;

                // Bottom left
                cornerWeCareAboutRightNow = new Point2D(environment.AOIRectangle.Left, environment.AOIRectangle.Bottom);
                tempDist = environment.goal_point.distance(cornerWeCareAboutRightNow);
                if (tempDist > maxSoFar) maxSoFar = tempDist;

                // Define the Distance (that will be used to calculate fitness values) (add a small value (10) to give a little breathing room)
                MaxDistanceToGoal = maxSoFar + 10;
            }

            if (ip.robots[0].Location.squaredDistance(environment.goal_point) < StoppingRange)
            {
                ip.robots[0].Stopped = true;
            }
            return;
        }

        /// <summary>
        /// Resets the flags for reaching the POIs and goal.
        /// </summary>
        void IFitnessFunction.reset()
        {
            for (int i = 0; i < ReachedPOI.Length; i++)
            {
                ReachedPOI[i] = false;
            }
            ReachedGoal = false;

            first = true;
        }

        #endregion
    }
}