using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace MazeSimulator
{
    /// <summary>
    /// Class for managing instances of behavior characterizations.
    /// </summary>
    public static class BehaviorCharacterizationFactory
    {
        #region Instance variables

        public static System.Collections.Generic.Dictionary<string, IBehaviorCharacterization> behaviorCharacterizationTable = new Dictionary<string, IBehaviorCharacterization>();

        #endregion

        #region Methods

        /// <summary>
        /// Returns an instance of the specified behavior characterization.
        /// </summary>
        public static IBehaviorCharacterization getBehaviorCharacterization(string functionName)
        {
            IBehaviorCharacterization behaviorCharacterization;
            behaviorCharacterization = createBehaviorCharacterization(functionName);

            if (behaviorCharacterization == null)
                return null;

            return behaviorCharacterization;
        }

        /// <summary>
        /// Creates an instance of the specified behavior characterization. 
        /// </summary>
        private static IBehaviorCharacterization createBehaviorCharacterization(string functionName)
        {
            string className = typeof(BehaviorCharacterizationFactory).Namespace + '.' + functionName;
            return (IBehaviorCharacterization)Assembly.GetExecutingAssembly().CreateInstance(className);
        }

        #endregion
    }
}