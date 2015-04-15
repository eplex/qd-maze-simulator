using System;
using System.Collections.Generic;

namespace MazeSimulator
{
    /// <summary>
    /// Collision manager for continuous (i.experiment. non-grid-based) domains.
    /// </summary>
	public class StandardCollision : CollisionManager
    {
        #region Instance variables

        public Environment Domain;
		public SimulatorExperiment Exp;
		public List<Robot> Robots;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor. Creates a new StandardCollision object.
        /// </summary>
        public StandardCollision() { }

        /// <summary>
        /// Creates a new StandardCollision object cast as a CollisionManager.
        /// </summary>
        /// <returns></returns>
		public override CollisionManager copy() { return new StandardCollision(); }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the collision manager.
        /// </summary>
        /// <param name="env">Environment for simulator experiments</param>
        /// <param name="exp">Simulator experiment</param>
        /// <param name="robots">Set of robots that should be tested for collision</param>
        public override void initialize (Environment domain, SimulatorExperiment exp, List<Robot> robots)
		{
			Robots = robots;
			Exp=exp;
            if (exp is MultiAgentExperiment)
            {
                AgentCollide = ((MultiAgentExperiment)Exp).agentsCollide;
                AgentVisible = ((MultiAgentExperiment)Exp).agentsVisible;
            }
            Domain = domain;
		}
		
        /// <summary>
        /// Tests collision between the specified robot and all walls and other robots in the CurrentEnvironment.
        /// </summary>
		public override bool robotCollide (Robot robot)
		{
			foreach (Wall wall in Domain.walls)
            {
                if (EngineUtilities.collide(robot, wall))
                {
                    return true;
                }
            }
			if(!AgentCollide) return false;
			
			foreach (Robot otherRobot in Robots)
            {
                 if (robot == otherRobot)
                        continue;
                 if (EngineUtilities.collide(robot, otherRobot))
                 {
                       return true;
                 }
            }
			
			return false;
		}
		
        /// <summary>
        /// Casts a ray from the robot's center point according to the specified Angle and returns the Distance to the closest object.
        /// </summary>
        /// <param name="Angle">Angle of sensor, in radians. Also see the "absolute" parameter.</param>
        /// <param name="maxRange">Max Distance that the robot can see in front of itself.</param>
        /// <param name="point">2D location of the robot's center.</param>
        /// <param name="Owner">The currently active robot.</param>
        /// <param name="hit">** Output variable ** - true if another object intersects the cast ray, false otherwise.</param>
        /// <param name="absolute">If false, the Angle parameter is relative to the agent's Heading. Otherwise it follows the standard unit AreaOfImpact.</param>
        /// <returns></returns>
		public override double raycast (double angle, double maxRange, Point2D point, Robot owner, out SimulatorObject hit, bool absolute = false)
		{
			hit=null;
	        Point2D casted = new Point2D(point);
            double distance = maxRange;

            // Cast point casted out from the robot's center point along the sensor direction
            if (!absolute)
            {
                casted.X += Math.Cos(angle + owner.Heading) * distance;
                casted.Y += Math.Sin(angle + owner.Heading) * distance;
            }
            else
            {
                casted.X += Math.Cos(angle) * distance;
                casted.Y += Math.Sin(angle) * distance;
            }

            // Create line segment from robot's center to casted point
            Line2D cast = new Line2D(point, casted);

            // Now do naive detection of collision of casted rays with objects
            // First for all walls
            foreach (Wall wall in Domain.walls)
            {
                if (!wall.Visible)
                    continue;
                bool found = false;
                Point2D intersection = wall.Line.intersection(cast, out found);
                if (found)
                {
                    double new_distance = intersection.distance(point);
                    if (new_distance < distance) {
                        distance = new_distance;
						hit=wall;
					}
                }
            }

            // Then for all robots
			if(!AgentVisible)
            	return distance;

            foreach (Robot robot2 in Robots)
            {
                bool found = false;

                if (robot2 == owner)
                    continue;

                double new_distance = cast.nearestIntersection(robot2.AreaOfImpact, out found);

                if (found)
                {
                    if (new_distance < distance)
                    {
                        distance = new_distance;
						hit=robot2;
                    }
                }
            }
            return distance;
        }

        #endregion
    }
}
