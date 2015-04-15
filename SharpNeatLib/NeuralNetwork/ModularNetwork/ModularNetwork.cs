using System;
using SharpNeatLib.Experiments;
using System.Collections.Generic;

namespace SharpNeatLib.NeuralNetwork
{
    public class ModularNetwork : INetwork
    {

        public delegate void UpdateNetworkDelegate(ModularNetwork network);
        public event UpdateNetworkDelegate UpdateNetworkEvent;

        //Store connection list for visualization purposes
        public NeatGenome.NeatGenome genome;

        #region Class Variables

        //protected float A, B, C, D, learningRate, pre, post;

        //public bool adaptable, modulatory;

        // For the following array, neurons are ordered with bias nodes at the head of the list,
        // then input nodes, then output nodes, and then hidden nodes in the array's tail.
        public float[] neuronSignals;

        public float[] modSignals;

        // This array is a parallel of neuronSignals, and only has values during SingleStepInternal().
        // It is declared here to avoid having to reallocate it for every network activation.
        public float[] neuronSignalsBeingProcessed;

        // must be in the same order as neuronSignals. Has null entries for neurons that are inputs or outputs of a module.
        protected IActivationFunction[] activationFunctions;

        // The modules and connections are in no particular order; only the order of the neuronSignals is used for input and output methods.
        protected ModulePacket[] modules;
        public FloatFastConnection[] connections;

        /// <summary>
        /// The number of input neurons.
        /// </summary>
        protected int inputNeuronCount;

        /// <summary>
        /// The number of input neurons including any bias neurons. This is also the index of the first output neuron in the neuron signals.
        /// </summary>
        protected int totalInputNeuronCount;

        /// <summary>
        /// The number of output neurons.
        /// </summary>
        protected int outputNeuronCount;

        /// <summary>
        /// The number of bias neurons, usually one but sometimes zero. This is also the index of the first input neuron in the neuron signals.
        /// </summary>
        protected int biasNeuronCount;

        protected float[] biasList;

        /// <summary>
        /// [neuronid,dependencies]: a list of connections that this neuron is dependent on for accumulating its signal
        /// </summary>
        protected List<int>[] dependencies;

        /// <summary>
        /// Indexes to neurons that do not have any dependent neurons (generally output neurons... or disconnected neurons). These serve as a start point for recursion
        /// </summary>
        protected List<int> endNeurons;

        /// <summary>
        /// Whether or not each neuron has had its activation calculated yet
        /// </summary>
        protected bool[] calculated;

        #endregion


        #region Constructor

        public ModularNetwork(int biasNeuronCount,
                                int inputNeuronCount,
                                int outputNeuronCount,
                                int totalNeuronCount,
                                FloatFastConnection[] connections,
                                float[] biasList,
                                IActivationFunction[] activationFunctions,
                                ModulePacket[] modules)
        {
            this.biasNeuronCount = biasNeuronCount;
            this.inputNeuronCount = inputNeuronCount;
            this.totalInputNeuronCount = biasNeuronCount + inputNeuronCount;
            this.outputNeuronCount = outputNeuronCount;
            this.connections = connections;
            this.activationFunctions = activationFunctions;
            this.modules = modules;
            this.biasList = biasList;


            // Justin: Setup fields for recursive network activation
            calculated = new bool[totalNeuronCount];
            dependencies = new List<int>[totalNeuronCount];
            endNeurons = new List<int>();
            for (int i = 0; i < totalNeuronCount; i++)
            {
                dependencies[i] = new List<int>();
                calculated[i] = false;
            }
            // Calculate dependencies. We will temporarily use calculated[] to store whether or not neurons have dependent "children" (will re-initialize the array to false afterwards)
            for (int i = 0; i < connections.Length; i++)
            {
                calculated[connections[i].sourceNeuronIdx] = true; // Mark the source as having dependents
                dependencies[connections[i].targetNeuronIdx].Add(i); // Add the source as a dependency of the target
            }
            // Mark "ending neurons" by seeing which neurons have no dependents
            for (int i = 0; i < totalNeuronCount; i++)
            {
                if (calculated[i] == false) endNeurons.Add(i);
                calculated[i] = false; // Also, re-initialize calculated[] to false
            }
            //foreach (int i in endNeurons) Console.Write(i + " "); Console.WriteLine();
            //Console.WriteLine("# Outputs: " + endNeurons.Count + " guessed: " + outputNeuronCount + " total: " + totalNeuronCount + " bias: " + biasNeuronCount);
            /*int counter = 0;
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (dependencies[i].Count == 0) counter++;
                //foreach (int arr in dependencies[i]) Console.Write(arr + " ");
                //Console.WriteLine();
            }
            Console.WriteLine("# Inputs: " + counter + " guessed: " + totalInputNeuronCount + " total: " + totalNeuronCount + " bias: " + biasNeuronCount);
            //*/

            // Allocate the arrays that store the states at different points in the neural network.
            // The neuron signals are initialised to 0 by default. Only bias nodes need setting to 1.
            neuronSignals = new float[totalNeuronCount];
            modSignals = new float[totalNeuronCount];

            neuronSignalsBeingProcessed = new float[totalNeuronCount];
            for (int i = 0; i < biasNeuronCount; i++) {
                neuronSignals[i] = 1.0F;
            }
        }

        #endregion


        #region INetwork Members

        /// <summary>
        /// This function carries out a single network activation.
        /// It is called by all those methods that require network activations.
        /// </summary>
        /// <param name="maxAllowedSignalDelta">
        /// The network is not relaxed as long as the absolute value of the change in signals at any given point is greater than this value.
        /// Only positive values are used. If the value is less than or equal to 0, the method will return true without checking for relaxation.
        /// </param>
        /// <returns>True if the network is relaxed, or false if not.</returns>
        public virtual bool SingleStepInternal(double maxAllowedSignalDelta,bool hive)
        {
            bool isRelaxed = true;	// Assume true.

            // Calculate each connection's output signal, and add the signals to the target neurons.
            for (int i = 0; i < connections.Length; i++) {
                //if (connectionArray[i].modConnection == 0.0f)       //normal connection
                //    connectionArray[i].signal = neuronSignalArray[connectionArray[i].sourceNeuronIdx] * connectionArray[i].weight;
                //else
                //    connectionArray[i].modSignal = neuronSignalArray[connectionArray[i].sourceNeuronIdx] * connectionArray[i].weight;
                
                neuronSignalsBeingProcessed[connections[i].targetNeuronIdx] += neuronSignals[connections[i].sourceNeuronIdx] * connections[i].weight;

             }

            // Pass the signals through the single-valued activation functions. 
            // Do not change the values of input neurons or neurons that have no activation function because they are part of a module.
            for (int i = totalInputNeuronCount; i < neuronSignalsBeingProcessed.Length; i++) {
                neuronSignalsBeingProcessed[i] = activationFunctions[i].Calculate(neuronSignalsBeingProcessed[i]+biasList[i]);
            }
            //TODO Sebastian CHECK IF BIAS NEURON IS WORKING CORRECTLY

            // Pass the signals through each module (activation function with more than one input or output).
            foreach (ModulePacket module in modules) {
                float[] inputs = new float[module.inputLocations.Length];
                for (int i = inputs.Length - 1; i >= 0; i--) {
                    inputs[i] = neuronSignalsBeingProcessed[module.inputLocations[i]];
                }

                float[] outputs = module.function.Calculate(inputs);
                for (int i = outputs.Length - 1; i >= 0; i--) {
                    neuronSignalsBeingProcessed[module.outputLocations[i]] = outputs[i];
                }
            }

            /*foreach (float f in neuronSignals)
                HyperNEATParameters.distOutput.Write(f.ToString("R") + " ");
            HyperNEATParameters.distOutput.WriteLine();
            HyperNEATParameters.distOutput.Flush();*/

            // Move all the neuron signals we changed while processing this network activation into storage.
            if (maxAllowedSignalDelta > 0) {
                for (int i = totalInputNeuronCount; i < neuronSignalsBeingProcessed.Length; i++) {

                    // First check whether any location in the network has changed by more than a small amount.
                    isRelaxed &= (Math.Abs(neuronSignals[i] - neuronSignalsBeingProcessed[i]) > maxAllowedSignalDelta);

                    neuronSignals[i] = neuronSignalsBeingProcessed[i];
                    neuronSignalsBeingProcessed[i] = 0.0F;
                }
            } else {
                //neuronSignalsBeingProcessed.CopyTo(neuronSignals, totalInputNeuronCount,);
                //neuronSignalsBeingProcessed = new float[neuronSignalsBeingProcessed.Length];
                for (int i = totalInputNeuronCount; i < neuronSignalsBeingProcessed.Length; i++) {
                    neuronSignals[i] = neuronSignalsBeingProcessed[i];
                    neuronSignalsBeingProcessed[i] = 0.0F;
                }
            }

           // Console.WriteLine(inputNeuronCount);

            return isRelaxed;
        }

        public float getHiddenSignal(int index)
        {
            return neuronSignals[totalInputNeuronCount + OutputNeuronCount + index];
        }

        public void SingleStep()
        {
            SingleStep(true);
        }

        public void SingleStep(bool hive)
        {
            SingleStepInternal(0.0,hive); // we will ignore the value of this function, so the "allowedDelta" argument doesn't matter.
            if (UpdateNetworkEvent != null)
            {
                UpdateNetworkEvent(null);
            }
        }

        public void MultipleSteps(int numberOfSteps)
        {
            MultipleSteps(numberOfSteps, true);
        }


        public void MultipleSteps(int numberOfSteps, bool hive)
        {
            //JUSTIN: Replacing the crappy inefficient activation below with recursive activation. Yeah!
            /*RecursiveActivation();
            return;//*/


            for (int i = 0; i < numberOfSteps; i++) {
                SingleStep(hive);
            }
        }

        /// <summary>
        /// Activate the network recursively (thus wasting no activation ticks)
        /// </summary>
        public void RecursiveActivation()
        {
            // Clear out the workhorse variables for the next time the network is activated
            for (int i = 0; i < TotalNeuronCount; i++)
            {
                calculated[i] = false;
            }
            // Get the signals for all output neurons, which will recursively get all signals across the network
            foreach (int i in endNeurons)
            {
                neuronSignals[i] = GetValueRecursively(i);
            }
            
        }

        /// <summary>
        /// Gets the value for the specified neuron index
        /// </summary>
        /// <param name="idx">The index of the neuron whose value we should get</param>
        private float GetValueRecursively(int idx)
        {
            if (calculated[idx]) return neuronSignals[idx]; // Stop if we have already calculated this neuron's signal
            // Otherwise, we need to calculate this neuron's signal.
            foreach (int i in dependencies[idx]) // Accumulate the dependency signals
            {
                neuronSignals[idx] += (GetValueRecursively(connections[i].sourceNeuronIdx) * connections[i].weight);
            }
            // Add the bias and apply the activation function (unless there were no dependencies... which means this was an input
            if (dependencies[idx].Count != 0)
            {
                neuronSignals[idx] = activationFunctions[idx].Calculate(neuronSignals[idx] + biasList[idx]);
            }

            // Mark this index as calculated and return the value
            calculated[idx] = true;
            return neuronSignals[idx];
        }


        /// <summary>
        /// Using RelaxNetwork erodes some of the perofrmance gain of FastConcurrentNetwork because of the slightly 
        /// more complex implemementation of the third loop - whe compared to SingleStep().
        /// </summary>
        /// <param name="maxSteps"></param>
        /// <param name="maxAllowedSignalDelta"></param>
        /// <returns></returns>
        public bool RelaxNetwork(int maxSteps, double maxAllowedSignalDelta)
        {
            bool isRelaxed = false;
            for (int j = 0; j < maxSteps && !isRelaxed; j++) {
                isRelaxed = SingleStepInternal(maxAllowedSignalDelta, true);
            }
            return isRelaxed;
        }

        public void SetInputSignal(int index, float signalValue)
        {
            // For speed we don't bother with bounds checks.
            neuronSignals[biasNeuronCount + index] = signalValue;
        }


        public void SetInputSignals(float[] signalArray)
        {
            // For speed we don't bother with bounds checks.
            for (int i = 0; i < signalArray.Length; i++)
                neuronSignals[i + biasNeuronCount] = signalArray[i];
        }

        
        public float GetOutputSignal(int index)
        {
            // For speed we don't bother with bounds checks.
            return neuronSignals[totalInputNeuronCount + index];
        }

        
        public void ClearSignals()
        {
            // Clear signals for input, hidden and output nodes. Only the bias node is untouched.
            for (int i = biasNeuronCount; i < neuronSignals.Length; i++)
                neuronSignals[i] = 0.0F;
        }

        
        public int InputNeuronCount
        {
            get
            {
                return inputNeuronCount;
            }
        }

        
        public int OutputNeuronCount
        {
            get
            {
                return outputNeuronCount;
            }
        }

        
        public int TotalNeuronCount
        {
            get
            {
                return neuronSignals.Length;
            }
        }

        #endregion
    }
}
