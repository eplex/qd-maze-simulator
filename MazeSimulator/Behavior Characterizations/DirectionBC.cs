using System;
using System.Collections.Generic;
using System.Text;

namespace MazeSimulator
{
    /// <summary>
    /// Behavior characterization that records which N/E/S/W direction a robot is facing most often over regularly spaced intervals. This class is intended to work with a single agent in the hard maze and currently does not support multiagent teams.
    /// </summary>
    class DirectionBC : IBehaviorCharacterization
    {
        #region Instance variables

        private int NumIntervals = 5; // Configure and recompile before using this BC
        private bool Initialized = false;
        private List<double> HeadingValues;
        private List<double> BehaviorVector;

        private int endTick = 0;
        private int chunkSize;

        bool IBehaviorCharacterization.wraparoundDistCalc
        {
            get { return true; }
        }

        string IBehaviorCharacterization.name
        {
            get { return this.GetType().Name; }
        }

        string IBehaviorCharacterization.description
        {
            get { return "Robot Heading is discretized into 4 bins. BC is a vector of discrete/constant values, one per time slice, each value corresponding to which Heading-bin was most often visited over the time slice."; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty behavior characterization instance.
        /// </summary>
        public IBehaviorCharacterization copy()
        {
            return new DirectionBC();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs the behavior vector at the end of an individual evaluation.
        /// </summary>
        List<double> IBehaviorCharacterization.calculate(SimulatorExperiment exp, instance_pack ip)
        {
            // Initialize the BC vector
            BehaviorVector = new List<double>();
            // If the robot never Stopped, set the max evaluation time as the end tick
            if (endTick == 0)
                endTick = exp.evaluationTime - 1;
            // Calculate when to perform an update
            chunkSize = Convert.ToInt32(Math.Floor((double)endTick / (double)NumIntervals));

            int[] headingBins;
            for (int chunkNum = 0; chunkNum < NumIntervals; chunkNum++)
            {
                // Reset the accumulators / Heading bins for this time slice.
                headingBins = new int[] { 0, 0, 0, 0 };
                double temp;
                // Fill the Heading bins for this time slice.
                for (int j = 0; j < chunkSize; j++)
                {
                    if ((chunkNum * chunkSize + j) >= HeadingValues.Count)
                    {
                        continue;
                    }
                    temp = HeadingValues[chunkNum * chunkSize + j];
                    temp *= 57.297f; // convert radians to degrees

                    // Convert the Heading to the maxRange 0-360
                    while (temp > 360)
                        temp -= 360;
                    while (temp < 0)
                        temp += 360;

                    if ((temp < 45 || temp >= 315))
                    {
                        headingBins[0] += 1;
                    }
                    else if (temp >= 45 && temp < 135)
                    {
                        headingBins[1] += 1;
                    }
                    else if (temp >= 135 && temp < 225)
                    {
                        headingBins[2] += 1;
                    }
                    else if (temp >= 225 && temp < 315)
                    {
                        headingBins[3] += 1;
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Unrecognized Heading! Something wrong (DirectionBC). What happen.");
                    }
                }

                // Now figure out which bin had the majority and assign the corresponding discrete value to the BC
                int majorityIndex = 0;
                int majorityCount = -1;
                for (int i = 0; i < headingBins.Length; i++)
                {
                    if (headingBins[i] > majorityCount)
                    {
                        majorityIndex = i;
                        majorityCount = headingBins[i];
                    }
                }

                BehaviorVector.Add(0.125 + (majorityIndex * 0.250));
            }

            return BehaviorVector;
        }

        /// <summary>
        /// Records the individual's Heading at each tick of the simulation. 
        /// </summary>
        void IBehaviorCharacterization.update(SimulatorExperiment exp, instance_pack ip)
        {
            // If this is the first update, initialize the sensor value accumulator
            if (!Initialized)
            {
                HeadingValues = new List<double>(exp.evaluationTime + 1);
                Initialized = true;
            }

            if (ip.robots[0].Stopped)
            {
                // If this is the first update after the robot has Stopped, 
                // set the endpoint to be the current simulation tick
                if (endTick == 0)
                {
                    HeadingValues.Add(ip.robots[0].Heading); // JUSTIN: Sample on the last tick (first tick of being Stopped) to fix an array overflow error
                    endTick = Convert.ToInt32(ip.timeSteps * exp.timestep);
                }
            }
            else
            {
                // Sample the robot's Heading on this tick.
                HeadingValues.Add(ip.robots[0].Heading);
            }
        }

        /// <summary>
        /// Resets the behavior characterization.
        /// </summary>
        void IBehaviorCharacterization.reset()
        {
            endTick = 0;
            Initialized = false;
        }

        #endregion
    }
}