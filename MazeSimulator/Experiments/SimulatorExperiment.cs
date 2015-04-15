using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.CPPNs;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.NeatGenome.Xml;
using SharpNeatLib.Experiments;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MazeSimulator
{
    /// <summary>
    /// Encapsulation of a simulator experiment instance.
    /// </summary>
    public class instance_pack
    {
        public bool recordTrajectories;
        public bool recordEndpoints;
        public AgentBrain agentBrain;
        public CollisionManager collisionManager;
        public List<Robot> robots;
        public int num_rbts;
        public IFitnessFunction ff;
        public IBehaviorCharacterization bc;
        public Environment env;
        public int eval;
        public double elapsed;
        public int timeSteps;
        public double timestep;
        public instance_pack()
        {
            timeSteps = 0;
            timestep = 1.0;
        }
    }

    /// <summary>
    /// Base class for all simulator experiments. 
    /// </summary>
    public class SimulatorExperiment : SimulatorExperimentInterface
    {
        #region Instance variables
        // Legacy naming convention used to maintain compatibility with existing experiments. Do not modify.

        [XmlIgnore]
        public AgentBrain agentBrain;
        [XmlIgnore]
        public SubstrateDescription substrateDescription;
        [XmlIgnore]
        public CollisionManager collisionManager;
        public bool gridCollision = false;
        public bool neatBrain = false;

        public bool homogeneousTeam = true;

        public bool multibrain = false;

        public string substrateDescriptionFilename;

        public bool multiobjective = false;

        public bool evolveSubstrate;

        public bool adaptableANN = false;
        public bool modulatoryANN = false;
        public bool normalizeWeights = true;

        public int populationSize = 150;

        public float headingNoise = 0.0f;
        public float effectorNoise = 0.0f;
        public float sensorNoise = 0.0f;

        public int timesToRunEnvironments = 1;
        public int evaluationTime;

        [XmlIgnore]
        public bool initialized;

        public double timestep;
        public string explanatoryText;
        public string scriptFile;
        public String robotModelName;
        public string fitnessFunctionName;
        public string behaviorCharacterizationName;

        [XmlIgnore]
        public IFitnessFunction fitnessFunction;

        [XmlIgnore]
        public IBehaviorCharacterization behaviorCharacterization;

        [XmlIgnore]
        public Environment environment;
        [XmlIgnore]
        public List<Environment> environmentList = new List<Environment>();
        [XmlIgnore]
        public int timeSteps = 0;

        public string environmentName;

        [XmlIgnore]
        public NeatGenome bestGenomeSoFar;

        [XmlIgnore]
        public List<Robot> robots = new List<Robot>();

        public bool overrideDefaultSensorDensity = false;
        public int sensorDensity;

        public bool overrideTeamFormation = false;
        public float group_orientation;
        public float group_spacing;
        public int robot_heading;

        public bool useScript = false;
        [XmlIgnore]
        public bool running;

        [XmlIgnore]
        public NeatGenome genome;

        [XmlIgnore]
        public bool recordTrajectories = false;
        [XmlIgnore]
        public bool recordEndpoints = false;

        #endregion

        #region Methods

        /// <summary>
        /// Loads a genome from the specified XML file.
        /// </summary>
        public void loadGenome(String filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            genome = XmlNeatGenomeReaderStatic.Read(doc);
            bestGenomeSoFar = genome;
        }

        /// <summary>
        /// Sets the behavior characterization to the specified class name. See Behavior Characterizations folder for list of valid class names.
        /// </summary>
        public void setBehavioralCharacterization(string behaviorName)
        {
            if (behaviorName == null || behaviorName.Equals("null")) return;
            behaviorCharacterization = BehaviorCharacterizationFactory.getBehaviorCharacterization(behaviorName);
            behaviorCharacterization.reset();
        }

        /// <summary>
        /// Sets the fitness function to the specified class name. See the Fitness Functions folder for the list of valid class names.
        /// </summary>
        /// <param name="fitnessFunctionName"></param>
        public void setFitnessFunction(string fitnessFunctionName)
        {
            this.fitnessFunctionName = fitnessFunctionName;
            fitnessFunction = FitnessFunctionFactory.getFitnessFunction(fitnessFunctionName);
            fitnessFunction.reset();
        }

        /// <summary>
        /// Loads the specified CurrentEnvironment.
        /// </summary>
        public void loadEnvironment(String environment_name)
        {
            environmentName = environment_name;
            environment = Environment.loadEnvironment(environment_name);
        }

        /// <summary>
        /// Unloads the CurrentEnvironment.
        /// </summary>
        public void clearEnvironment()
        {
            environmentName = "Unnammed";
            environment.reset();
        }

        /// <summary>
        /// Virtual method that implements junk logic. Actual logic is in derived class's evaluateNetwork functions.
        /// </summary>
        internal virtual double evaluateNetwork(INetwork network, out SharpNeatLib.BehaviorType behavior, System.Threading.Semaphore sem) { behavior = null; return 0.0; }

        /// <summary>
        /// Resets the CurrentEnvironment without completely unloading it.
        /// </summary>
        public void resetEnvironment()
        {
            environment.reset();
        }

        /// <summary>
        /// Unimplemented in this class. See derived classes.
        /// </summary>
        public virtual String defaultSeedGenome()
        {
            return null;
        }

        /// <summary>
        /// Returns the number of CPPN / NEAT brain inputs, which varies with brain Type.
        /// </summary>
        public int getNumCPPNInputs()
        {
            int inputs;

            if (neatBrain) inputs = 10;
            else if (homogeneousTeam) inputs = 4;
            else inputs = 5;

            return inputs;
        }

        /// <summary>
        /// Returns the number of outputs for CPPNs and NEAT brains.
        /// </summary>
        /// <returns></returns>
        public int getNumCPPNOutputs()
        {
            int outputs;
            if (neatBrain)
                outputs = 2;
            else if (substrateDescription.useMultiPlaneSubstrate)
            {
                outputs = (substrateDescription.planesConnected.Count + substrateDescription.planes.Count);
                Console.WriteLine("Using multi-plane! # CPPN outputs: {0}, # Plane Connections: {1}, # Planes: {2}", outputs, substrateDescription.planesConnected.Count, substrateDescription.planes.Count);
                return outputs;
            }
            else
            {
                if (adaptableANN)
                {
                    if (modulatoryANN) outputs = 8;
                    else outputs = 7;
                }
                else
                    outputs = 2;
            }
            return outputs;
        }

        #endregion

        #region Virtual Methods

        public virtual void run() { }

        public virtual void draw(Graphics g, CoordinateFrame scale) { }

        public virtual void initialize() { }

        public virtual void initializeRobots(AgentBrain agentBrain, Environment e, float headingNoise, float sensorNoise, float effectorNoise, instance_pack x) { }
        
        #endregion

        #region XML Serialization

        /// <summary>
        /// Saves the CurrentEnvironment to the specified XML file.
        /// </summary>
        public virtual void save(string name)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.GetType());
            TextWriter outfile = new StreamWriter(name);
            x.Serialize(outfile, this);
            outfile.Close();
        }

        /// <summary>
        /// Loads the environments specified in the experiment file.
        /// </summary>
        public static void loadEnvironments(SimulatorExperiment experiment)
        {
            experiment.environmentList.Clear();
            Environment scene = Environment.loadEnvironment(experiment.environmentName);
            experiment.environmentList.Add(scene);

            experiment.environment = scene;

            Console.Write("Looking for additional environments [" + scene.name + "] ... ");
            String filenamePrefix = scene.name.Substring(0, scene.name.Length - 4);
            int num = 1;
            String filename2 = filenamePrefix + num + ".xml";
            while (File.Exists(filename2))
            {
                Console.WriteLine("Found CurrentEnvironment: " + filename2 + "\n");
                experiment.environmentList.Add(Environment.loadEnvironment(filename2));
                num++;
                filename2 = filenamePrefix + num + ".xml";
            }
            Console.WriteLine("Done");
            Console.WriteLine(experiment.environmentList.Count.ToString() + " CurrentEnvironment(s) found.");
        }

       

        #endregion
    }
}
