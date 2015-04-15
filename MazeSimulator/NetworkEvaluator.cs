using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.Experiments;
using SharpNeatLib;

namespace MazeSimulator
{
    /// <summary>
    /// A simple NetworkEvaluator that passes the CPPN to an experimental domain to be evaluated
    /// </summary>
    public class NetworkEvaluator : INetworkEvaluator
    {
        #region Instance variables

        public SimulatorExperiment Experiment;

        /// <summary>
        /// Returns a message about the evaluator's current state. Currently unimplemented.
        /// </summary>
        public string evaluatorStateMessage
        {
            get { return ""; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new NetworkEvaluator object from the specified parameters.
        /// </summary>
        /// <param name="experiment">An experimental domain in which networks will be evaluated</param>
        public NetworkEvaluator(SimulatorExperiment experiment)
        {
            Experiment = experiment;
        }

        #endregion

        #region INetworkEvaluator Members

        /// <summary>
        /// Wrapper function for the experiment class's evaluteNetwork function.
        /// </summary>
        /// <param name="network">If using NEAT (with direct encoding), the network is the controller itself. Otherwise, if using HyperNEAT, the network is a CPPN that indirectly encodes the controller.</param>
        /// <param name="behavior">** Output parameter ** Returns a vector representation of the agent's behavior inside the experimental domain.</param>
        public double evaluateNetwork(SharpNeatLib.NeuralNetwork.INetwork network, out SharpNeatLib.BehaviorType behavior)
        {
            return Experiment.evaluateNetwork(network, out behavior,null);
        }

        /// <summary>
        /// Threadsafe wrapper function for the experiment class's evaluteNetwork function.
        /// </summary>
        /// <param name="network">If using NEAT (with direct encoding), the network is the controller itself. Otherwise, if using HyperNEAT, the network is a CPPN that indirectly encodes the controller.</param>
        /// <param name="sem">Semaphore for managing parallel processes.</param>
        /// <param name="behavior">** Output parameter ** Returns a vector representation of the agent's behavior inside the experimental domain.</param>
        public double threadSafeEvaluateNetwork(SharpNeatLib.NeuralNetwork.INetwork network, System.Threading.Semaphore sem, out SharpNeatLib.BehaviorType behavior, int thread)
        {
            //TODO: deal with threading
            return Experiment.evaluateNetwork(network, out behavior,sem);
        }

        #endregion
    }
}
