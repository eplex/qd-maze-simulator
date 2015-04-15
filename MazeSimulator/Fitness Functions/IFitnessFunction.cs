using System;
using System.Collections.Generic;
using System.Text;

namespace MazeSimulator
{
    /// <summary>
    /// Interface for all fitness function classes.
    /// </summary>
    public interface IFitnessFunction
    {
        IFitnessFunction copy();

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
        /// Calculates the fitness function.
        /// </summary>
        /// <param name="MazeSimulator">The MazeSimulator currently running the simulation.</param>
        /// <param name="CurrentEnvironment">The CurrentEnvironment the agents are current in.</param>
        /// <returns>The current fitness value.</returns>
        double calculate(SimulatorExperiment MazeSimulator, Environment e, instance_pack i, out double[] objectives);

        /// <summary>
        /// Update the fitness function on each frame.
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
        void update(SimulatorExperiment MazeSimulator, Environment e, instance_pack i);

        void reset();
    }
}
