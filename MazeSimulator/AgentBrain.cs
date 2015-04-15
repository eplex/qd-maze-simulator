using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;
using System.Xml;
using SharpNeatLib.CPPNs;
using System.IO;

namespace MazeSimulator
{
    /// <summary>
    /// Controller class for brains controlling both homogeneous and heterogeneous multiagent teams, in addition to single agents.
    /// </summary>
    public class AgentBrain
    {
        #region Constants

        private static int NET_ACTIVATION_STEPS = 3; //How many times should the network get activated. TODO make configurable

        #endregion

        #region Instance variables

        public INetwork Brain; // The "big" brain for heterogenous teams
        public List<INetwork> Brains; // Brains for homogenous teams
        public bool UseCTRNNs = false;
        public bool Hive = true; 
        public bool AllowHive = true;
        public bool Homogeneous; // All robots have identical copies of the same brain
        public int NumRobots;
        private List<Robot> RobotListeners;

        private float[] TeamInputOld;
        private float[] TeamInput;
        private float[] RobotInput;
        private float[] RobotInputOld;
        private float[] TeamOutput;
        private float[] TeamOutputOld;
        private float[] TeamHiddenOld;
        private float[] TeamHidden;
        private bool[] Activated;

        private SubstrateDescription SubstrateDescription;
        public INetwork Genome;
        public NeatGenome ANN;

        private bool NormalizeANNWeights;
        private bool AdaptableANN;
        private bool ModulatoryANN;
        private bool First = true;
        public bool MultipleBrains = false; // Multiple brains with situational policies
        public bool EvolveSubstrate = false;
        public bool NeatBrain = false;

        public static bool OutputComms = false;
        public static bool OutputsInitialized = false;
        public static List<StreamWriter> OutStreams;
        public static List<StreamWriter> InStreams;

        public List<INetwork> MultiBrains = new List<INetwork>();
        public List<float> ZCoordinates;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new AgentBrain object from the specified parameters. This brain is designed to be used as a multiagent hivemind, but can also be used to control a single agent by setting numAgents equal to 1. 
        /// </summary>
        /// <param name="homogeneous">Is the team homogeneous? Agents in a homogeneous team all have identical copies of the same brain, while those in a heterogeneous team do not, instead each contributing to one central, distributed hive brain.</param>
        /// <param name="numAgents">Set to 1 for an individual agent, or >1 for a multiagent team.</param>
        /// <param name="substrateDescription">HyperNEAT substrate</param>
        /// <param name="genome">If using NEAT, this should be the already-decoded neural network. Otherwise, if using HyperNEAT, this should be the CPPN that encodes the neural network.</param>
        /// <param name="normalizeANNWeights"></param>
        /// <param name="adaptableANN"></param>
        /// <param name="modulatoryANN"></param>
        /// <param name="multi">Multiple brains with situational policies?</param>
        /// <param name="evolveSubstrate">Set to true to enable ES-HyperNEAT.</param>
        /// <param name="useNeatBrain">If false, the system will use HyperNEAT as the EA.</param>
        /// <param name="useCTRNNs">Set to true to use continuous time recurrent neural networks.</param>
        public AgentBrain(bool homogeneous, int numAgents, SubstrateDescription substrateDescription, INetwork genome,
            bool normalizeANNWeights, bool adaptableANN, bool modulatoryANN, bool multi, bool evolveSubstrate, bool useNeatBrain, bool useCTRNNs = false)
        {
            // Set instance variables
            EvolveSubstrate = evolveSubstrate;
            NormalizeANNWeights = normalizeANNWeights;
            AdaptableANN = adaptableANN;
            ModulatoryANN = modulatoryANN;
            Genome = genome;
            SubstrateDescription = substrateDescription;
            NumRobots = numAgents;
            Homogeneous = homogeneous;
            MultipleBrains = multi;
            NeatBrain = useNeatBrain;
            UseCTRNNs = useCTRNNs;

            // Initialiaze team arrays
            TeamInput = new float[numAgents * substrateDescription.InputCount];
            TeamInputOld = new float[numAgents * substrateDescription.InputCount];
            TeamOutput = new float[numAgents * substrateDescription.OutputCount];
            TeamOutputOld = new float[numAgents * substrateDescription.OutputCount];
            TeamHidden = new float[numAgents * 5];
            TeamHiddenOld = new float[numAgents * 5];
			RobotInput = new float[substrateDescription.InputCount];
			RobotInputOld = new float[substrateDescription.InputCount];
            Activated = new bool[numAgents];

            // Initialize the agents' brains
            createBrains();
            
            // Register listeners for the robots' neural network outputs
            RobotListeners = new List<Robot>();

            if (OutputComms)
            {
                // If the robots' input and output streams have already been initialized, reset them
                if (OutputsInitialized)
                {
                    for (int j = 0; j < numAgents; j++)
                    {
                        OutStreams[j].Close();
                        InStreams[j].Close();
                        OutStreams[j] = null;
                        InStreams[j] = null;
                    }
                }

                // Initialize the robots' input and output streams
                OutStreams = new List<StreamWriter>(numAgents);
                InStreams = new List<StreamWriter>(numAgents);
                for (int j = 0; j < numAgents; j++)
                {
                    OutStreams.Add(new StreamWriter("agentO" + j + ".txt"));
                    InStreams.Add(new StreamWriter("agentI" + j + ".txt"));
                }
                OutputsInitialized = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the average difference between items in two vector arrays of equal length.
        /// </summary>
        float calculateDifference(float[] vector1, float[] vector2, int startingIndex, int comparisonLength)
        {
            float dif = 0.0f;
            for (int j = startingIndex; j < startingIndex + comparisonLength; j++)
            {
                dif += Math.Abs(vector1[j] - vector2[j]);
            }
            return dif / comparisonLength;
        }

        /// <summary>
        /// Sets AllowHive to false.
        /// </summary>
        public void disableHive()
        {
            if (Hive)
            {
                AllowHive = !AllowHive;
            }

        }

        /// <summary>
        /// Switches from multiple brains to just 1, setting the agent brain to the first brain in the list of multiple brains.
        /// </summary>
        public void collapseBrains()
        {
            if (MultipleBrains)
                Brain = MultiBrains[1];
        }

        /// <summary>
        /// If using NEAT, this function simply sets the AgentBrain genome variable as the brain. Otherwise, the genome is actually used as a genome and is decoded into the appropriate brain structure.
        /// </summary>
        private void createBrains()
        {
            if (Genome != null)
            {
                if (NeatBrain)
                {
                    Brain = Genome;
                }
                else
                    if (Homogeneous)
                    {
                        ANN = SubstrateDescription.generateHomogeneousGenome(Genome, NormalizeANNWeights, this.AdaptableANN, this.ModulatoryANN, EvolveSubstrate);
                        Brains = new List<INetwork>();
                        for (int i = 0; i < NumRobots; i++)
                        {
                            INetwork b = ANN.Decode(null);
                            Brains.Add(b);
                        }
                    }
                    else
                    {
                        if (MultipleBrains) 
                        {
                            List<NeatGenome> genes = SubstrateDescription.generateGenomeStackSituationalPolicy(Genome, Convert.ToUInt32(NumRobots), NormalizeANNWeights, AdaptableANN, ModulatoryANN, 2, out ZCoordinates);

                            for (int j = 0; j < genes.Count; j++)
                                MultiBrains.Add(genes[j].Decode(null));

                            Brain = MultiBrains[0];
                        }
                        else if (Hive)
                        {
                            ANN = SubstrateDescription.generateHiveBrainGenomeStack(Genome, Convert.ToUInt32(NumRobots), NormalizeANNWeights, AdaptableANN,
                                                                            ModulatoryANN, out ZCoordinates, EvolveSubstrate, UseCTRNNs);

                            Brain = UseCTRNNs ? ANN.DecodeToCTRNN() : ANN.Decode(null);
                        }
                        else
                        {
                            ANN = SubstrateDescription.generateMultiGenomeStack(Genome, Convert.ToUInt32(NumRobots), NormalizeANNWeights, AdaptableANN,
                                                                            ModulatoryANN, out ZCoordinates, EvolveSubstrate);
                            Brain = ANN.Decode(null);
                        }
                    }
            }
        }

        /// <summary>
        /// Reconfigures agent brains if the number of GoalSensors changes.
        /// </summary>
        public void updateInputDensity()
        {
            TeamInput = new float[NumRobots * SubstrateDescription.InputCount];

            //Check if the number of GoalSensors changed so we have to regenerate the ANNs
            if (Homogeneous)
            {
                if (Brains != null && Brains[0] != null && Brains[0].InputNeuronCount != SubstrateDescription.InputCount)
                {
                    Console.WriteLine("Recreating ANNs");
                    createBrains();
                }
            }
            else
            {
                if (Brain != null && (Brain.InputNeuronCount / NumRobots) != SubstrateDescription.InputCount)
                {
                    Console.WriteLine("Recreating ANNs");
                    createBrains();
                }
            }
        }

        /// <summary>
        /// Adds the specified robot to the robot list. Robots need to be registered before they can receive ANN results.
        /// </summary>
        public void registerRobot(Robot robot)
        {
            if (RobotListeners.Contains(robot))
            {
                Console.WriteLine("Robot " + robot.ID + " already registered");
                return;
            }

            RobotListeners.Add(robot);

            if (RobotListeners.Count > NumRobots)
            {
                Console.WriteLine("Number of registered agents [" + RobotListeners.Count + "] and number of agents [" + NumRobots + "] does not match");
            }
        }

        /// <summary>
        /// Retrieves the robot brain at the specified index in the brains list.
        /// </summary>
        public INetwork getBrain(int number)
        {
            if (Homogeneous)
            {
                return Brains[number];
            }
            else
                return Brain;   //only one brain for heterogenous teams
        }

        /// <summary>
        /// Activates the neural network(s). Call once all agents have received their inputs.
        /// </summary>
        public void execute(System.Threading.Semaphore sem)
        {
            // Homogeneous brains
            if (Homogeneous && !NeatBrain)
            {
                if (Brains == null) return;
                    
                float[] inputs = new float[SubstrateDescription.InputCount];

                for (int agentNumber=0; agentNumber<NumRobots; agentNumber++) 
                {
                    for (int i = 0; i < SubstrateDescription.InputCount; i++)
                    {
                        inputs[i] = TeamInput[(i + agentNumber * SubstrateDescription.InputCount)];
                    }

                    Brains[agentNumber].SetInputSignals(inputs);
                    (Brains[agentNumber] as ModularNetwork).MultipleSteps(NET_ACTIVATION_STEPS,AllowHive);

                    float[] outputs = new float[Brains[agentNumber].OutputNeuronCount];
                    for (int j = 0; j < outputs.Length; j++)
                    {
                        outputs[j] = Brains[agentNumber].GetOutputSignal(j);
                    }

                    RobotListeners[agentNumber].networkResults(outputs);              
                }
                return;
            }

            // Heterogeneous brains
            if (Brain == null)
                return;
			Brain.SetInputSignals(TeamInput); 
            Brain.MultipleSteps(NET_ACTIVATION_STEPS);

            int out_count = 0;
            int numOutputAgent = Brain.OutputNeuronCount / NumRobots;
            float[] outp = new float[numOutputAgent];
			int rob_count = 0;

			foreach (Robot robot in RobotListeners)
            {
                for (int y = 0; y < numOutputAgent; y++)
                {
                    outp[out_count % numOutputAgent] = Brain.GetOutputSignal(out_count);
                    out_count++;
                }
                
                robot.networkResults(outp);
				rob_count++;
            }
			
			if(First) First=false;
        }

        /// <summary>
        /// Resets ANN signals to zero. Only valid if robot has z-values.
        /// </summary>
        public void clearANNSignals(float ZStack)
        {
			if (NeatBrain) return;
			int index = 0;
            ModularNetwork net = ((ModularNetwork)Brain);
            foreach (ConnectionGene gene in net.genome.ConnectionGeneList)
            {
                if (gene.coordinates.Length > 4 && !gene.coordinates[4].Equals(float.NaN))
                {
                    if (gene.coordinates[4] != ZStack)
                    {
                        index++;
                        continue;
                    }
                }

                ((ModularNetwork)Brain).neuronSignals[net.connections[index].targetNeuronIdx] = 0.0f;
                ((ModularNetwork)Brain).neuronSignals[net.connections[index].sourceNeuronIdx] = 0.0f;  
            }
        }

        /// <summary>
        /// Resets all ANN signals to zero.
        /// </summary>
        public void clearAllSignals()
        {
            Brain.ClearSignals();
        }

        /// <summary>
        /// Pass the given inputs to an individual agents' network.
        /// </summary>
        public void setInputSignals(int agentNumber, float[] inputs)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                TeamInput[(i + (agentNumber * inputs.Length))] = inputs[i];
            } 
        }

        #endregion
    }
}