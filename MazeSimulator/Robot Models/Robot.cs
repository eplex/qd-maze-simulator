using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib;
using System.Drawing;

namespace MazeSimulator
{
    /// <summary>
    /// Basic robot class. 
    /// </summary>
    public abstract class Robot : SimulatorObject
    {
        #region Instance variables

        // Basic info
        public int ID;
        public int TicksAlive = 0;
        protected float Timestep;
        public Environment CurrentEnvironment;
        
        // Networks
        public AgentBrain Brain;
        public float ZStack;
        public float[] NetworkOutputCopy = new float[0];
        public List<float> InputDelta = new List<float>();
        public List<float> HiddenDelta = new List<float>();
        public List<float> OutputDelta = new List<float>();
        public List<float> InputValues = new List<float>();
        public List<float> OutputValues = new List<float>();
        
        // Collision handling
        public int NumCollisions = 0;
        public bool CollisionPenalty = false;
        public bool HasCollided;
        
        // Visualization
        public bool DisplayDebug = true;
        public bool DrawSensors;

        // Sensors and effectors
        public List<ISensor> CompassSensors = new List<ISensor>(); // Used for sensing the north-pointing compass
        public List<ISensor> GoalSensors = new List<ISensor>();
        public List<ISensor> WallSensors = new List<ISensor>(); 
        public List<ISensor> RobotSensors = new List<ISensor>(); 
        public List<ISensor> CommSensors = new List<ISensor>(); // Used for robot-to-robot communication
        public List<ISensor> CommDistanceSensors = new List<ISensor>(); // Records the Distance to the speaker who is activating the corresponding commSensor
        public double CurrentVoiceLevel = 0.0; // Used for robot-to-robot communication (this is what is visualized)
        public double[] CurrentVoiceLevels = { 0.0, 0.0, 0.0, 0.0, 0.0 }; // For talking to each of 5 robots
        public List<ISensor> CurrentDirectedVoiceLevels = new List<ISensor>(); // There are 10 speaking cones that start at 270 degrees (just like GoalSensors)
        
        // Sensor and effector Noise
        protected float HeadingNoise;
        protected float EffectorNoise;
        protected float SensorNoise;

        // Movement
        public bool Autopilot;
        public bool Stopped = false;
        public bool Disabled = false;
        public double Velocity, Heading, Radius = 5; // Heading is in radians
        public Circle2D AreaOfImpact;
        public Point2D OldLocation = new Point2D();
        public float DistanceTraveled = 0.0f;
        public float TempDistance = 0.0f;
        
        // Behavioral Trajectory
        public List<int> Trajectory;
        public int TrajectoryUpdateCounter = 0;

        // Random number generator utility
        public SharpNeatLib.Maths.FastRandom rng;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Robot()
        {
            Dynamic = true;
            DrawSensors = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the robot.
        /// </summary>
        public void init(int id, double locationX, double locationY, double heading, AgentBrain agentBrain, Environment environment, float sensorNoise, float effectorNoise, float headingNoise, float timeStep)
        {
            this.ID = id;
            Radius = defaultRobotSize();
            Location = new Point2D(locationX, locationY);
            AreaOfImpact = new Circle2D(Location, Radius);
            OldLocation = new Point2D(Location);

            Heading = heading;
            Velocity = 0.0;
            HasCollided = false;
            this.Timestep = timeStep;
            this.CurrentEnvironment = environment;
            this.Brain = agentBrain;
            this.SensorNoise = sensorNoise;
            this.EffectorNoise = effectorNoise;
            this.HeadingNoise = headingNoise;
            populateSensors();
            if (environment.seed != -1)
                rng = environment.rng;
            else
                rng = environment.rng;
            this.Trajectory = new List<int>();
        }

        /// <summary>
        /// Updates the robot at every tick. Not implemented for this class (see derived classes).
        /// </summary>
        public override void update()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when robot collides. Applies any collision penalties and adds to the number of NumCollisions. 
        /// </summary>
        public virtual void onCollision()
        {
            DistanceTraveled -= TempDistance;
            NumCollisions++;
            undo();

            if (CollisionPenalty && NumCollisions > 0)
            {
                Disabled = true;
                Stopped = true;
            }
        }

        /// <summary>
        /// Applies Noise to the Heading per the HeadingNoise property.
        /// </summary>
        public double noisyHeading()
        {
            return Heading + 0.1 * (rng.NextBool() ? 1 : -1) * rng.Next(0, (int)HeadingNoise) / 100.0;
        }

        /// <summary>
        /// Updates all of the robot's GoalSensors. 
        /// </summary>
        public virtual void updateSensors(Environment env, List<Robot> robots, CollisionManager cm)
        {
            foreach (ISensor sensor in GoalSensors)
            {
                sensor.update(env, robots, cm);
            }
        }

        /// <summary>
        /// Finds the robot's new position given its Heading and Velocity.
        /// </summary>
        public void updatePosition()
        {
            //record old coordinates
            TempDistance = (float)OldLocation.squaredDistance(Location);
            DistanceTraveled += TempDistance;

            OldLocation.X = Location.X;
            OldLocation.Y = Location.Y;

            //update current coordinates (may be revoked if new position forces collision)
            if (!Stopped)
            {
                double tempHeading = noisyHeading();
                Heading = tempHeading;
                double dx = Math.Cos(tempHeading) * Velocity * Timestep;
                double dy = Math.Sin(tempHeading) * Velocity * Timestep;
                Location.X += dx;
                Location.Y += dy;
            }
        }

        /// <summary>
        /// Reverts the robot's position to the last recorded value.
        /// </summary>
        public override void undo()
        {
            Location.X = OldLocation.X;
            Location.Y = OldLocation.Y;
        }

        /// <summary>
        /// Draws the robot to the screen.
        /// </summary>
        public virtual void draw(Graphics g, CoordinateFrame frame)
        {
            Point upperleft = frame.convertToDisplay((float)(AreaOfImpact.Position.X - Radius), (float)(AreaOfImpact.Position.Y - Radius));
            int size = (int)((Radius * 2) / frame.Scale);
            Rectangle r = new Rectangle(upperleft.X, upperleft.Y, size, size);

            double voiceRadius = Radius + (CurrentVoiceLevel * 8.5 * Radius);
            Point upperleftVoice = frame.convertToDisplay((float)(AreaOfImpact.Position.X - voiceRadius), (float)(AreaOfImpact.Position.Y - voiceRadius));
            int sizeVoice = (int)((voiceRadius * 2) / frame.Scale);
            Rectangle rv = new Rectangle(upperleftVoice.X, upperleftVoice.Y, sizeVoice, sizeVoice);

            double maxVoiceRadius = Radius + (8.5 * Radius);
            Point upperleftMaxVoice = frame.convertToDisplay((float)(AreaOfImpact.Position.X - maxVoiceRadius), (float)(AreaOfImpact.Position.Y - maxVoiceRadius));
            int sizeMaxVoice = (int)((maxVoiceRadius * 2) / frame.Scale);
            Rectangle rmv = new Rectangle(upperleftMaxVoice.X, upperleftMaxVoice.Y, sizeMaxVoice, sizeMaxVoice);

            if (Disabled)
                g.DrawEllipse(EngineUtilities.YellowPen, r);
            else if (HasCollided)
                g.DrawEllipse(EngineUtilities.RedPen, r);
            else if (Stopped)
                g.DrawEllipse(EngineUtilities.RedPen, r);
            else
            {
                g.DrawEllipse(EngineUtilities.BluePen, r);
            }

            int sensCount = 0;

            if (DisplayDebug)
            {
                // Print "other GoalSensors" not in the main GoalSensors list
                foreach (RangeFinder rf in WallSensors)
                {
                    rf.draw(g, frame);
                }

                foreach (float f in NetworkOutputCopy)
                {
                    float f1 = (f + 1) / 2.0f;
                    Color col = Color.FromArgb(0, 0, (int)(f1 * 255));
                    SolidBrush newpen = new SolidBrush(col);
                    g.DrawString(f.ToString(), SystemFonts.DefaultFont, Brushes.Black, sensCount * 60 + 400, 500 + 10 * ID);
                    sensCount += 1;
                }
                sensCount = 0;

                // Color in the voice level for this robot
                g.FillEllipse(EngineUtilities.voiceColorBrush, rv);
            }

            float rad = (float)Radius * frame.Scale;
            g.DrawLine(EngineUtilities.BluePen, frame.convertToDisplay((float)AreaOfImpact.Position.X, (float)AreaOfImpact.Position.Y), frame.convertToDisplay((float)AreaOfImpact.Position.X + (float)Math.Cos(Heading) * 2 * (float)(Radius), (float)AreaOfImpact.Position.Y + (float)Math.Sin(Heading) * 2 * (float)(Radius)));
        }

        #region Virtual methods

        public abstract float defaultRobotSize();

        public abstract int defaultSensorDensity();

        public abstract void populateSensors();

        public abstract void populateSensors(int size);

        public abstract void networkResults(float[] outputs);

        public virtual void robotSettingsChanged() { }

        public virtual void doAction() { }

        #endregion

        #endregion
    }
}
