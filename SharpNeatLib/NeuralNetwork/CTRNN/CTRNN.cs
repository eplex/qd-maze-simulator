using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNeatLib.NeuralNetwork
{
    class CTRNN : ModularNetwork
    {
        public float[] timeConstantArray;
        private float[] activeSumsArray;
        private float[] oldActivation;

        public CTRNN(int biasNeuronCount,
                                int inputNeuronCount,
                                int outputNeuronCount,
                                int totalNeuronCount,
                                FloatFastConnection[] connections,
                                float[] biasList,
                                IActivationFunction[] activationFunctions,
                                ModulePacket[] modules,
                                float[] timeConstArray) : base(biasNeuronCount,inputNeuronCount,outputNeuronCount,totalNeuronCount,connections, biasList,activationFunctions, modules)
        {
            timeConstantArray = timeConstArray;
            activeSumsArray = new float[timeConstantArray.Length];
            oldActivation = new float[timeConstantArray.Length];
        }



        public override bool SingleStepInternal(double maxAllowedSignalDelta, bool hive)
        {
            float dt = .02f;
            bool isRelaxed = true;	// Assume true.
            //Console.WriteLine("CTRNN");
            // Calculate each connection's output signal, and add the signals to the target neurons.
            for (int i = 0; i < connections.Length; i++)
            {
                //if (connectionArray[i].modConnection == 0.0f)       //normal connection
                //    connectionArray[i].signal = neuronSignalArray[connectionArray[i].sourceNeuronIdx] * connectionArray[i].weight;
                //else
                //    connectionArray[i].modSignal = neuronSignalArray[connectionArray[i].sourceNeuronIdx] * connectionArray[i].weight;
                {
                    //neuronSignalsBeingProcessed[connections[i].targetNeuronIdx] += neuronSignals[connections[i].sourceNeuronIdx] * connections[i].weight;
                    activeSumsArray[connections[i].targetNeuronIdx] +=neuronSignals[connections[i].sourceNeuronIdx] * connections[i].weight;
                }
            }


            // Pass the signals through the single-valued activation functions. 
            // Do not change the values of input neurons or neurons that have no activation function because they are part of a module.
            for (int i = totalInputNeuronCount; i < neuronSignalsBeingProcessed.Length; i++)
            {
                // Added by Skyler for CTRNN 1/24/2012
                // CTRNN activation (same as before)
                neuronSignalsBeingProcessed[i] = oldActivation[i] + (-oldActivation[i] + activeSumsArray[i]) * dt / timeConstantArray[i];
                // Store old pure activation for next iteration
                oldActivation[i] = neuronSignalsBeingProcessed[i];
                // Add the activation function to make the final modified activation
                neuronSignalsBeingProcessed[i] = activationFunctions[i].Calculate(neuronSignalsBeingProcessed[i] + biasList[i]);

                /*neuronSignalsBeingProcessed[i] = neuronSignals[i];
                neuronSignalsBeingProcessed[i]+= dt / timeConstantArray[i] * (-neuronSignalsBeingProcessed[i] + activeSumsArray[i]);
                neuronSignalsBeingProcessed[i] = activationFunctions[i].Calculate(neuronSignalsBeingProcessed[i] + biasList[i]);*/
            }
            //TODO Sebastian CHECK IF BIAS NEURON IS WORKING CORRECTLY

            // Pass the signals through each module (activation function with more than one input or output).
            foreach (ModulePacket module in modules)
            {
                float[] inputs = new float[module.inputLocations.Length];
                for (int i = inputs.Length - 1; i >= 0; i--)
                {
                    inputs[i] = neuronSignalsBeingProcessed[module.inputLocations[i]];
                }

                float[] outputs = module.function.Calculate(inputs);
                for (int i = outputs.Length - 1; i >= 0; i--)
                {
                    neuronSignalsBeingProcessed[module.outputLocations[i]] = outputs[i];
                }
            }

            /*foreach (float f in neuronSignals)
                HyperNEATParameters.distOutput.Write(f.ToString("R") + " ");
            HyperNEATParameters.distOutput.WriteLine();
            HyperNEATParameters.distOutput.Flush();*/

            // Move all the neuron signals we changed while processing this network activation into storage.
            if (maxAllowedSignalDelta > 0)
            {
                for (int i = totalInputNeuronCount; i < neuronSignalsBeingProcessed.Length; i++)
                {

                    // First check whether any location in the network has changed by more than a small amount.
                    isRelaxed &= (Math.Abs(neuronSignals[i] - neuronSignalsBeingProcessed[i]) > maxAllowedSignalDelta);

                    neuronSignals[i] = neuronSignalsBeingProcessed[i];
                    neuronSignalsBeingProcessed[i] = 0.0F;
                }
            }
            else
            {
                for (int i = totalInputNeuronCount; i < neuronSignalsBeingProcessed.Length; i++)
                {
                    neuronSignals[i] = neuronSignalsBeingProcessed[i];
                    neuronSignalsBeingProcessed[i] = 0.0F;
                }
            }

            // Console.WriteLine(inputNeuronCount);

            for (int i = totalInputNeuronCount; i < neuronSignalsBeingProcessed.Length; i++)
            {
                modSignals[i] = 0.0F;
                activeSumsArray[i] = 0;
            }

            return isRelaxed;
        }
    }
}
