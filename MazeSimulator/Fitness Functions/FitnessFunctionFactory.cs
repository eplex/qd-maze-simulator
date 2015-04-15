using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace MazeSimulator
{
    /// <summary>
    /// Generates instances of fitness functions.
    /// </summary>
    public static class FitnessFunctionFactory
    {
        #region Instance variables

        public static System.Collections.Generic.Dictionary<string, IFitnessFunction> fitnessFunctionTable = new Dictionary<string, IFitnessFunction>();

        #endregion

        #region Methods

        /// <summary>
        /// Returns an instance of the specified fitness function. See the Fitness Function folder for a list of valid function class names.
        /// </summary>
        public static IFitnessFunction getFitnessFunction(string functionName)
        {
            IFitnessFunction fitnessFunction;
            fitnessFunction = createFitnessFuction(functionName);

            if (fitnessFunction == null)
                return null;

            return fitnessFunction;
        }

        /// <summary>
        /// Creates an instance of the specified fitness function. See the Fitness Function folder for a list of valid function class names.
        /// </summary>
        private static IFitnessFunction createFitnessFuction(string functionName)
        {
            string className = typeof(FitnessFunctionFactory).Namespace + '.' + functionName;
            return (IFitnessFunction)Assembly.GetExecutingAssembly().CreateInstance(className);
        }

        #endregion
    }
}
