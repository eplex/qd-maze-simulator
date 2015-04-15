using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.Experiments;
using SharpNeatLib.Evolution;

namespace MazeSimulator
{
    /// <summary>
    /// Base class for all experiment domains.
    /// </summary>
    public class Experiment : IExperiment
    {
        #region Instance variables

        public SimulatorExperiment simExp = null;

        private IPopulationEvaluator _populationEvaluator = null;
        public SharpNeatLib.Evolution.IPopulationEvaluator PopulationEvaluator
        {
            get
            {
                if (_populationEvaluator == null)
                    _populationEvaluator = new MultiThreadedPopulationEvaluator(new NetworkEvaluator(simExp), null);
                return _populationEvaluator;
            }
        }

        NeatParameters EvolutionParameters = null;
        public SharpNeatLib.Evolution.NeatParameters DefaultNeatParameters
        {
            get
            {
                if (EvolutionParameters == null)
                {
                    NeatParameters np = new NeatParameters();
                    if (simExp.neatBrain)
                    {
                        // NEAT brain usese the settings from Joel Lehman's original published hard maze experiments
                        np.pMutateAddConnection = 0.1;
                        np.pMutateAddNode = 0.005;
                        np.pInterspeciesMating = 0.001;
                        np.pMutateConnectionWeights = 0.6;
                        np.compatibilityThreshold = 4;
                        np.activationProbabilities = new double[1];
                        np.activationProbabilities[0] = 1.0;
                        np.populationSize = 250;
                    }
                    else
                    {
                        np.connectionWeightRange = 3;
                        np.pMutateAddConnection = .03; 
                        np.pMutateAddNode = 0.01; 
                        np.pMutateAddModule = 0;
                        np.pMutateConnectionWeights = .96; 
                        np.pMutateDeleteConnection = 0.00; 
                        np.pMutateDeleteSimpleNeuron = 0.00; 
                        np.activationProbabilities = new double[4];
                        np.activationProbabilities[0] = .25;
                        np.activationProbabilities[1] = .25;
                        np.activationProbabilities[2] = .25;
                        np.activationProbabilities[3] = .25;
                        np.populationSize = 100;
                        np.pruningPhaseBeginComplexityThreshold = float.MaxValue;
                        np.pruningPhaseBeginFitnessStagnationThreshold = int.MaxValue;
                        np.pruningPhaseEndComplexityStagnationThreshold = int.MinValue;
                        np.pInitialPopulationInterconnections = .1f; 
                        np.elitismProportion = .2;
                        np.targetSpeciesCountMax = np.populationSize / 10;
                        np.targetSpeciesCountMin = np.populationSize / 10 - 2;
                        np.selectionProportion = .8;
                        np.multiobjective = simExp.multiobjective;
                        np.feedForwardOnly = true; 
                    }
                    EvolutionParameters = np;
                }
                return EvolutionParameters;
            }
        }

        public SharpNeatLib.NeuralNetwork.IActivationFunction SuggestedActivationFunction
        {
            get { return HyperNEATParameters.substrateActivationFunction; }
        }

        int _inputNeuronCount, _outputNeuronCount;
        public int InputNeuronCount
        {
            get { return _inputNeuronCount; }
        }
        public int OutputNeuronCount
        {
            get { return _outputNeuronCount; }
        }

        /// <summary>
        /// Returns a string describing this class.
        /// </summary>
        public string ExplanatoryText
        {
            get { return "Experiment to interface with the simulator code"; }
        }
        
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new Experiment object by copying an existing SimulatorExperiment.
        /// </summary>
        public Experiment(SimulatorExperiment experiment)
        {
            simExp = experiment;
            _inputNeuronCount = experiment.getNumCPPNInputs();
            _outputNeuronCount = experiment.getNumCPPNOutputs();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Currently not implemented.
        /// </summary>
        public AbstractExperimentView createExperimentView()
        {
            return null;
        }

        #endregion
    }
}
