using System;
using System.Collections.Generic;
using System.Text;

namespace MazeSimulator
{
    public interface IBehaviorCharacterization
    {
        IBehaviorCharacterization copy();
        /// <summary>
        /// Name of the function, should be the same as the class name.  Use this.GetType().Name;
        /// </summary>
        string name
        {
            get;
        }

        /// <summary>
        /// Plain text description of what the fitness function does.
        /// </summary>
        string description
        {
            get;
        }

        /// <summary>
        /// Should BC-to-BC Distance calculations treat the 0-1 maxRange as if it wrapped around? (experiment.g. 0.0001 and 0.99999 are negligibly distant and 0.0 is max dist from 0.5)
        /// </summary>
        bool wraparoundDistCalc
        {
            get;
        }

        /// <summary>
        /// Calculates the behavioral characterization.
        /// </summary>
        /// <param name="MazeSimulator">The MazeSimulator currently running the simulation.</param>
        /// <param name="CurrentEnvironment">The CurrentEnvironment the agents are current in.</param>
        /// <returns>The current fitness value.</returns>
        List<double> calculate(SimulatorExperiment experiment, instance_pack i);

        /// <summary>
        /// Update the behavioral characterization on each frame.
        /// </summary>
        /// <param name="MazeSimulator">
        /// A <see cref="MazeSimulator"/>
        /// </param>
        /// <param name="CurrentEnvironment">
        /// A <see cref="CurrentEnvironment"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Double"/>
        /// </returns>
        void update(SimulatorExperiment MazeSimulator, instance_pack i);

        void reset();
    }
}
