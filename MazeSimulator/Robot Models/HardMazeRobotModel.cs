using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MazeSimulator
{
    /// <summary>
    /// Robot model based on Joel Lehman's hard maze experiments.
    /// </summary>
    class HardMazeRobotModel : Robot
    {
        #region Instance variables

        int RangefinderRange = 100;
        int PiesliceRange = 2000;
        double AngularVelocity = 0;
        float[] Inputs;

        #endregion

        #region Methods

        /// <summary>
        /// Returns the default robot size.
        /// </summary>
        public override float defaultRobotSize()
        {
            return 10;
        }

        /// <summary>
        /// Returns the default number of GoalSensors.
        /// </summary>
        /// <returns></returns>
        public override int defaultSensorDensity()
        {
            return 5;
        }

        /// <summary>
        /// Populates the agent's GoalSensors using the default sensor density.
        /// </summary>
        public override void populateSensors()
        {
            populateSensors(defaultSensorDensity());
        }

        /// <summary>
        /// Initializes the robot's GoalSensors and positions them on the robot's body.
        /// </summary>
        public override void populateSensors(int numWallSensors)
        {
            // Set up the 5 front-facing rangefinders
            WallSensors = new List<ISensor>();
            double delta = 180.0 / 4; // in degrees
            delta /= 57.29578f; // convert degrees to radians because that is what RangeFinders take
            double startAngle = 4.71239f; // start the first rangefinder facing due left
            for (int j = 0; j < numWallSensors; j++)
            {
                WallSensors.Add(new RangeFinder(startAngle, this, RangefinderRange, 0.0));
                startAngle += delta;
            }

            // Set up the single rear-facing rangefinder
            startAngle -= delta; // Set the StartAngle to facing due right
            startAngle += (90 / 57.29578f); // (convert 90 degrees to radians)
            WallSensors.Add(new RangeFinder(startAngle, this, RangefinderRange, 0.0));

            // Set up the POI radars
            GoalSensors = new List<ISensor>();
            GoalSensors.Add(new Radar(45, 135, this, "goal", PiesliceRange)); // front
            GoalSensors.Add(new Radar(135, 225, this, "goal", PiesliceRange)); // right
            GoalSensors.Add(new Radar(225, 315, this, "goal", PiesliceRange)); // rear
            GoalSensors.Add(new Radar(315, 45, this, "goal", PiesliceRange)); // left

            // Set up the Northstar GoalSensors
            CompassSensors = new List<ISensor>();
            CompassSensors.Add(new Radar(45, 135, this, "northstar", PiesliceRange * 2)); // front   // Note: maxRange extended 2x purely for visual purposes.
            CompassSensors.Add(new Radar(135, 225, this, "northstar", PiesliceRange * 2)); // right
            CompassSensors.Add(new Radar(225, 315, this, "northstar", PiesliceRange * 2)); // rear
            CompassSensors.Add(new Radar(315, 45, this, "northstar", PiesliceRange * 2)); // left

            // Initialize a persistent inputs array so we don'type have to keep re-allocating memory
            Inputs = new float[WallSensors.Count + GoalSensors.Count];
        }

        /// <summary>
        /// Updates all of the robot's GoalSensors. Called on each simulator tick.
        /// </summary>
        /// <param name="env">The simulator CurrentEnvironment.</param>
        /// <param name="robots">List of other robots in the CurrentEnvironment. Not actually used in this function, only included for inheritance reasons.</param>
        /// <param name="cm">The CurrentEnvironment's collision manager.</param>
        public override void updateSensors(Environment env, List<Robot> robots, CollisionManager cm)
        {
            // Clear out GoalSensors from last time
            InputValues.Clear();
            foreach (Radar r in GoalSensors)
            {
                r.Activation = 0;
            }
            foreach (Radar r in CompassSensors)
            {
                r.Activation = 0;
            }

            // Update regular (target) GoalSensors
            double angle = 0;
            Point2D temp;
            temp = new Point2D(env.goal_point.X, env.goal_point.Y);
            temp.X -= (float)AreaOfImpact.Position.X;
            temp.Y -= (float)AreaOfImpact.Position.Y;

            angle = (float)temp.angle();
            angle -= Heading;
            angle *= 57.297f; // convert radians to degrees

            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;

            foreach (Radar r in GoalSensors)
            {
                // First, check if the Angle is in the wonky pie slice
                if ((angle < 45 || angle >= 315) && r.StartAngle == 315)
                {
                    r.Activation = 1;
                    break;
                }

                // Then check the other pie slices
                else if (angle >= r.StartAngle && angle < r.EndAngle)
                {
                    r.Activation = 1;
                    break;
                }
            }

            // Update the compass/northstar GoalSensors
            // Note: This is trivial compared to rangefinder updates, which check against all walls for collision. No need to gate it to save CPU.
            double northstarangle = Heading; 
            northstarangle *= 57.297f; // convert radians to degrees

            while (northstarangle > 360)
                northstarangle -= 360;
            while (northstarangle < 0)
                northstarangle += 360;

            foreach (Radar r in CompassSensors)
            {
                // First, check if the Angle is in the wonky pie slice
                if ((northstarangle < 45 || northstarangle >= 315) && r.StartAngle == 315)
                {
                    r.Activation = 1;
                    break;
                }

                // Then check the other pie slices
                else if (northstarangle >= r.StartAngle && northstarangle < r.EndAngle)
                {
                    r.Activation = 1;
                    break;
                }
            }

            // Update the rangefinders
            foreach (RangeFinder r in WallSensors)
            {
                r.update(env, robots, cm);
            }

            // Update wall sensor inputs
            for (int j = 0; j < WallSensors.Count; j++)
            {
                Inputs[j] = (float)(1 - (WallSensors[j].getActivation()));
                if (Inputs[j] > 1.0) Inputs[j] = 1.0f;
            }

            // Update pie slice sensor inputs
            for (int j = WallSensors.Count; j < WallSensors.Count + GoalSensors.Count; j++)
            {
                Inputs[j] = (float)GoalSensors[j-WallSensors.Count].getActivation();
                if (Inputs[j] > 1.0) Inputs[j] = 1.0f;
            }

            // Update NN inputs based on GoalSensors
            Brain.setInputSignals(ID, Inputs);
            
            // Make the sensor values accessible to the behavior characterization
            for (int i = 0; i < Inputs.Length; i++)
                InputValues.Add(Inputs[i]);
        }

        /// <summary>
        /// Wrapper function that passes the neural network outputs to the decideAction function, which forces robot behavior.
        /// </summary>
        /// <param name="outputs">Neural network outputs</param>
        public override void networkResults(float[] outputs)
        {
            decideAction(outputs, Timestep);
        }

        /// <summary>
        /// Enacts agent behavior based on neural network outputs. Movement uses instant/reactive turning and acceleration-based movement (hybrid approach) to encourage robots to move at the same (maximum) Velocity.
        /// </summary>
        /// <param name="outputs">Neural network outputs.</param>
        /// <param name="Timestep">The current timestep.</param>
        public void decideAction(float[] outputs, double timeStep)
        {
            Velocity += (outputs[1] - 0.5) * 2.0;
            if (Velocity > 6.0) Velocity = 6.0;
            if (Velocity < -6.0) Velocity = (-6.0); 
            Heading += (outputs[0] - 0.5) * 0.2094395104 * 2;

            // Position updating code below this point ---------------
            TempDistance = (float)OldLocation.squaredDistance(Location);
            DistanceTraveled += TempDistance;

            OldLocation.X = Location.X;
            OldLocation.Y = Location.Y;

            // Update current coordinates (may be revoked if new position forces collision)
            if (!Stopped)
            {
                double dx = Math.Cos(Heading) * Velocity * timeStep;
                double dy = Math.Sin(Heading) * Velocity * timeStep;
                Location.X += dx;
                Location.Y += dy;
            }
        }

        #endregion
    }
}
