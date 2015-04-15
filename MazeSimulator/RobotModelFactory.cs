using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace MazeSimulator
{
    /// <summary>
    /// Wrapper class that generates Robot objects.
    /// </summary>
    public static class RobotModelFactory
    {
        #region Methods

        /// <summary>
        /// Wrapper function that retrieves an instance of the specified robot model.
        /// </summary>
        public static Robot getRobotModel(string robotModelname)
        {
            Robot robotModel;

            robotModel = createRobotModel(robotModelname);

            return robotModel;
        }

        /// <summary>
        /// Constructs an isntance of the specified robot model.
        /// </summary>
        private static Robot createRobotModel(string robotModelname)
        {
            string className = typeof(RobotModelFactory).Namespace + '.' + robotModelname;
            return (Robot)Assembly.GetExecutingAssembly().CreateInstance(className);
        }

        #endregion
    }
}
