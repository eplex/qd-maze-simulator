using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome.Xml;
using System.Xml;
using SharpNeatLib.CPPNs;
using SharpNeatLib.Experiments;
using System.Drawing;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;

namespace MazeSimulator
{
    /// <summary>
    /// Base class for all multiagent simulator experiments. Can be used for single agent experiments as well by setting the number of robots to 1.
    /// </summary>
    public class MultiAgentExperiment : SimulatorExperiment
    {
        #region Instance variables

        // Legacy naming convention used to maintain compatibility with existing experiments. Do not modify.

        public SharpNeatLib.Maths.FastRandom globalRng = new SharpNeatLib.Maths.FastRandom();
        public bool collisionPenalty;
        public bool multipleEnvironments;
        public bool modulatory;
        public bool benchmark;

        [XmlIgnore]
        public double elapsedTime;

        public List<String> robotValues = null;
        public int numberRobots;
        public bool agentsVisible;          // Can the agents see each other?
        public bool agentsCollide;          //Do the agents collide with each other?
        public bool noiseDeterministic;

        public bool useCTRNNS = false;

        [XmlIgnore]
        public int trajectoryUpdateInterval = 20;
        [XmlIgnore]
        int trajectoryUpdateCounter = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MultiAgentExperiment()
        {
            benchmark = false;
            multibrain = false;
            evolveSubstrate = false;
            noiseDeterministic = false;
            collisionPenalty = false;
            homogeneousTeam = false;

            scriptFile = "";
            numberRobots = 1;
            sensorDensity = 5;

            evaluationTime = 100;

            explanatoryText = "Multi-Agent Experiment";

            //Create an empty CurrentEnvironment
            environment = new Environment();
            robots = new List<Robot>();

            fitnessFunctionName = "SingleGoalPoint";
            behaviorCharacterizationName = "EndPointBC";
            robotModelName = "PackBotModel";
            robotValues = new List<string>();

            initialized = false;
            running = false;
        }

        /// <summary>
        /// Copy constructor. 
        /// </summary>
        public MultiAgentExperiment(MultiAgentExperiment exp)
        {
            gridCollision = exp.gridCollision;

            substrateDescriptionFilename = exp.substrateDescriptionFilename;
            adaptableANN = exp.adaptableANN;
            modulatoryANN = exp.modulatoryANN;
            normalizeWeights = exp.normalizeWeights;
            headingNoise = exp.headingNoise;
            effectorNoise = exp.effectorNoise;
            sensorNoise = exp.sensorNoise;

            timesToRunEnvironments = exp.timesToRunEnvironments;
            evaluationTime = exp.evaluationTime;
            initialized = false;

            timestep = exp.timestep;
            explanatoryText = exp.explanatoryText;

            scriptFile = exp.scriptFile;

            robotModelName = exp.robotModelName;

            fitnessFunctionName = exp.fitnessFunctionName;
            behaviorCharacterizationName = exp.behaviorCharacterizationName;
            environmentName = exp.environmentName;


            bestGenomeSoFar = exp.bestGenomeSoFar;

            numberRobots = exp.numberRobots;

            homogeneousTeam = exp.homogeneousTeam;

            overrideDefaultSensorDensity = exp.overrideDefaultSensorDensity;
            sensorDensity = exp.sensorDensity;

            useScript = exp.useScript;

            running = exp.running;

            agentsVisible = exp.agentsVisible;
            agentsCollide = exp.agentsCollide;

            genome = exp.genome;

            multipleEnvironments = exp.multipleEnvironments;

            evolveSubstrate = exp.evolveSubstrate;

            elapsedTime = exp.elapsedTime;

            populationSize = exp.populationSize;

            overrideTeamFormation = exp.overrideTeamFormation;
            group_orientation = exp.group_orientation;
            group_spacing = exp.group_spacing;
            robot_heading = exp.robot_heading;

            multibrain = exp.multibrain;
            multiobjective = exp.multiobjective;
            robotValues = new List<string>();
            foreach (String s in exp.robotValues)
                robotValues.Add(s);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets up the instance variables and initializes the experiment.
        /// </summary>
        public override void initialize()
        {
            setupVariables();
            initialized = true;
        }

        /// <summary>
        /// Performs the actual initialization logic. 
        /// </summary>
        protected void setupVariables()
        {
            substrateDescription = new SubstrateDescription(substrateDescriptionFilename);
            agentBrain = new AgentBrain(homogeneousTeam, numberRobots, substrateDescription, genome != null ? genome.Decode(null) : null, normalizeWeights, adaptableANN, modulatoryANN, multibrain, evolveSubstrate, neatBrain, useCTRNNS);

            loadEnvironments(this);

            initializeRobots(agentBrain, environment, headingNoise, sensorNoise, effectorNoise, null);

            setFitnessFunction(fitnessFunctionName);
            setBehavioralCharacterization(behaviorCharacterizationName);

            collisionManager = new StandardCollision();

            collisionManager.initialize(environment, this, this.robots);
            timeSteps = 0;
            elapsedTime = 0;
        }

        /// <summary>
        /// Loads the CurrentEnvironment from the specified XML file.
        /// </summary>
        public void loadEnvironment(String filename)
        {
            Console.WriteLine("loading CurrentEnvironment...");
            environment = Environment.loadEnvironment(filename);
        }

        /// <summary>
        /// Draws the maze, robots, etc. to the screen.
        /// </summary>
        public override void draw(Graphics g, CoordinateFrame scale)
        {
            foreach (Robot robot in robots)
            {
                robot.draw(g, scale);
            }
            double[] obj = null;

            if (environment != null)
            {
                environment.draw(g, scale);
            }

            instance_pack ip = new instance_pack();

            ip.robots = robots;
            ip.env = environment;
            ip.timeSteps = this.timeSteps;
            ip.agentBrain = agentBrain;
            ip.collisionManager = collisionManager;
            ip.elapsed = this.elapsedTime;
            ip.ff = this.fitnessFunction;
            ip.bc = this.behaviorCharacterization;
            ip.timestep = timestep;

            g.DrawString("Fitness: " + this.fitnessFunction.calculate(this, this.environment, ip, out obj), new Font("Tahoma", 12), Brushes.Black, 10, 90);
            g.DrawString("Elapsed time: " + this.elapsedTime, new Font("Tahoma", 12), Brushes.Black, 10, 60);
        }

        /// <summary>
        /// Wrapper for the runEnvironment function
        /// </summary>
        public override void run()
        {
            runEnvironment(environment, null, null);
        }

        /// <summary>
        /// Initializes robot brains and positions them in the CurrentEnvironment.
        /// </summary>
        public override void initializeRobots(AgentBrain agentBrain, Environment e, float headingNoise, float sensorNoise, float effectorNoise, instance_pack ip)
        {
            double spacing = 0;
            int num_bots;
            double dx, dy; // Deltas for spacing the robots
            if (overrideTeamFormation)
            {
                dx = Math.Cos(group_orientation / 180.0 * 3.14) * group_spacing;
                dy = Math.Sin(group_orientation / 180.0 * 3.14) * group_spacing;
                spacing = group_spacing;
            }
            else
            {
                dx = Math.Cos(e.group_orientation / 180.0 * 3.14) * e.robot_spacing;
                dy = Math.Sin(e.group_orientation / 180.0 * 3.14) * e.robot_spacing;
                spacing = e.robot_spacing;
            }

            AgentBrain ab;
            List<Robot> rbts;
            double _timestep = 0.0;
            if (ip == null)
            {
                ab = agentBrain;
                rbts = robots;
                num_bots = numberRobots;
                _timestep = this.timestep;
            }
            else
            {
                ab = ip.agentBrain;
                rbts = new List<Robot>();
                ip.robots = rbts;
                num_bots = ip.num_rbts;
                _timestep = ip.timestep;
            }

            rbts.Clear();

            bool circle = false;
            bool staggered = true;

            double radius = (spacing) / (2.0 * Math.PI * (1.0 / num_bots));
            double angleDelta = (2 * Math.PI) / num_bots;

            // Add robots in their formation according to CurrentEnvironment
            double nx, ny;
            for (int num = 0; num < num_bots; num++)
            {

                double heading = overrideTeamFormation ? robot_heading : e.robot_heading;
                Robot r;
                if (robotValues != null && robotValues.Count > 0 && robotValues.Count >= num)
                {
                    r = RobotModelFactory.getRobotModel(robotValues[num]);
                }
                else
                    r = RobotModelFactory.getRobotModel(robotModelName);

                if (circle)
                {
                    nx = e.start_point.X + radius * Math.Cos(num * angleDelta);
                    ny = e.start_point.Y + radius * Math.Sin(num * angleDelta);
                    heading = num * angleDelta - Math.PI;
                }
                else
                {
                    if (staggered && num % 2 == 1 && e.stagger != 0)
                        nx = e.start_point.X + num * dx + (staggered ? e.stagger * num : 0);
                    else
                        nx = e.start_point.X + num * dx - (staggered ? e.stagger * num : 0);
                    ny = e.start_point.Y + num * dy;
                    heading = (heading / 180.0) * 3.14;
                }

                r.init(num, nx, ny,
                    heading, ab, e, sensorNoise, effectorNoise, headingNoise, (float)_timestep);
                r.CollisionPenalty = collisionPenalty;
                ab.registerRobot(r);

                if (overrideDefaultSensorDensity)
                    r.populateSensors(sensorDensity);
                else
                    r.populateSensors();
                if (agentBrain.ZCoordinates == null) r.ZStack = 0;
                else
                    r.ZStack = ab.ZCoordinates[num];
                rbts.Add(r);
            }

            ab.updateInputDensity();
        }

        /// <summary>
        /// Performs one tick of the experiment.
        /// </summary>
        protected internal virtual bool runEnvironment(Environment e, instance_pack ip, System.Threading.Semaphore sem)
        {
            bool collide;

            if (ip == null)
            {
                elapsedTime += timestep;
                timeSteps++;
            }
            else
            {
                ip.timeSteps++;
                ip.elapsed += ip.timestep;
                ip.env = e;
            }

            AgentBrain ab;
            CollisionManager cm;
            List<Robot> rbts;
            IFitnessFunction fit_fun;
            IBehaviorCharacterization beh_char;

            if (ip == null)
            {
                ip = new instance_pack();
                ab = agentBrain;
                cm = collisionManager;
                rbts = robots;
                fit_fun = fitnessFunction;
                beh_char = behaviorCharacterization;
                ip.agentBrain = agentBrain;
                ip.collisionManager = cm;
                ip.robots = rbts;
                ip.ff = fit_fun;
                ip.env = environment;
                ip.bc = beh_char;
                ip.timeSteps = timeSteps;
                ip.timestep = timestep;
            }
            else
            {
                ab = ip.agentBrain;
                cm = ip.collisionManager;
                rbts = ip.robots;
                fit_fun = ip.ff;
                beh_char = ip.bc;
            }

            cm.simulationStepCallback();

            for (int x = 0; x < rbts.Count; x++)
            {
                rbts[x].updateSensors(e, rbts, cm);
                if (!rbts[x].Autopilot)
                    rbts[x].doAction();
                else
                    ab.clearANNSignals(rbts[x].ZStack);
            }

            ab.execute(sem);

            for (int x = 0; x < rbts.Count; x++)
            {
                collide = cm.robotCollide(rbts[x]);
                rbts[x].HasCollided = collide;
                if (collide)
                    rbts[x].onCollision();
            }

            if (ip != null)
            {
                if (beh_char != null)
                    beh_char.update(this, ip);
                fit_fun.update(this, e, ip);

                // Track the Trajectory independent of the BC in case the BC has nothing to do with position
                if (recordTrajectories)
                {
                    if (ip.robots[0].TrajectoryUpdateCounter == trajectoryUpdateInterval)
                    {
                        ip.robots[0].Trajectory.Add(Convert.ToInt16(rbts[0].Location.X));
                        ip.robots[0].Trajectory.Add(Convert.ToInt16(rbts[0].Location.Y));
                        ip.robots[0].TrajectoryUpdateCounter = 0;
                    }
                    else
                        ip.robots[0].TrajectoryUpdateCounter++;
                }
            }
            return false;
        }

        /// <summary>
        /// Runs a single individual (or single multiagent team) through the CurrentEnvironment(s) specified in the experiment file.
        /// </summary>
        internal override double evaluateNetwork(INetwork network, out SharpNeatLib.BehaviorType behavior, System.Threading.Semaphore sem)
        {
            double fitness = 0;
            List<double> fitList = new List<double>();
            behavior = new SharpNeatLib.BehaviorType();
            List<Double> origBehavior = new List<Double>();
            double[] accumObjectives = new double[6];
            for (int i = 0; i < 6; i++) accumObjectives[i] = 0.0;

            IFitnessFunction fit_copy;
            IBehaviorCharacterization bc_copy;
            instance_pack inst = new instance_pack();
            inst.timestep = timestep;

            foreach (Environment env2 in environmentList)
            {
                fit_copy = fitnessFunction.copy();
                if (behaviorCharacterization != null)
                {
                    bc_copy = behaviorCharacterization.copy();
                    inst.bc = bc_copy;
                }
                inst.ff = fit_copy;

                double tempFit = 0;
                double[] fitnesses = new double[timesToRunEnvironments];

                SharpNeatLib.Maths.FastRandom evalRand;
                if (noiseDeterministic) evalRand = new SharpNeatLib.Maths.FastRandom(100);
                else
                {
                    evalRand = new SharpNeatLib.Maths.FastRandom();
                }

                for (int evals = 0; evals < timesToRunEnvironments; evals++)
                {
                    inst.num_rbts = this.numberRobots;

                    Environment env = env2.copy();

                    double evalTime = evaluationTime;

                    inst.eval = evals;
                    env.seed = evals;
                    if (!benchmark && noiseDeterministic)
                        env.rng = new SharpNeatLib.Maths.FastRandom(env.seed + 1);
                    else
                    {
                        env.rng = new SharpNeatLib.Maths.FastRandom();
                    }

                    float new_sn = this.sensorNoise;
                    float new_ef = this.effectorNoise;
                    float new_headingnoise = this.headingNoise;

                    inst.agentBrain = new AgentBrain(homogeneousTeam, inst.num_rbts, substrateDescription, network, normalizeWeights, adaptableANN, modulatoryANN, multibrain, evolveSubstrate, neatBrain, useCTRNNS);
                    inst.timestep = timestep;
                    initializeRobots(agentBrain, env, new_headingnoise, new_sn, new_ef, inst);
                    inst.elapsed = 0;
                    inst.timeSteps = 0;
                    inst.collisionManager = collisionManager.copy();
                    inst.collisionManager.initialize(env, this, inst.robots);

                    while (inst.elapsed < evalTime)
                    {
                        runEnvironment(env, inst, sem);
                    }

                    double thisFit = inst.ff.calculate(this, env, inst, out behavior.objectives);
                    fitnesses[evals] = thisFit;
                    tempFit += thisFit;

                    if (behavior != null && behavior.objectives != null && inst.bc != null)
                    {
                        for (int i = 0; i < behavior.objectives.Length; i++)
                            accumObjectives[i] += behavior.objectives[i];

                        if (behavior.behaviorList == null)
                        {
                            behavior.behaviorList = new List<double>();
                        }
                        behavior.behaviorList.AddRange(inst.bc.calculate(this, inst));

                        inst.bc.reset();
                    }
                    else if (behavior != null && inst.bc != null)
                    {
                        if (behavior.behaviorList == null)
                        {
                            behavior.behaviorList = new List<double>();
                        }
                        behavior.behaviorList.AddRange(inst.bc.calculate(this, inst));

                        inst.bc.reset();
                    }
                    inst.ff.reset();
                }

                fitness += tempFit / timesToRunEnvironments;
                fitList.Add(tempFit / timesToRunEnvironments);
            }
            behavior.objectives = accumObjectives;
            if (behaviorCharacterization != null)
            {

                behavior.wraparoundRange = behaviorCharacterization.wraparoundDistCalc;
            }

            double t = 0;

            if (recordEndpoints)
            {
                behavior.finalLocation = new List<double>(2);
                behavior.finalLocation.Add(inst.robots[0].Location.X);
                behavior.finalLocation.Add(inst.robots[0].Location.Y);
            }                                                                   

            if (recordTrajectories)
            {
                behavior.trajectory = new List<int>();
                behavior.trajectory.AddRange(inst.robots[0].Trajectory);
            }

            return (fitness - t) / environmentList.Count;

        }

        /// <summary>
        /// Returns the filename of the default seed.
        /// </summary>
        public override String defaultSeedGenome()
        {
            if (homogeneousTeam) return
                "seedGenome2x.xml";
            else return
                "seedGenomeHomo.xml";
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
