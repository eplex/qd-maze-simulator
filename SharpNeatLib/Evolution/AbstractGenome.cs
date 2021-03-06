using System;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.Evolution
{
	abstract public class AbstractGenome : IGenome
	{
		// See comments on individual properties for more information on these fields.
		protected uint genomeId;
		long genomeAge=0;
		double fitness = 0;
		double realfitness =0;
		double _competition=0;
		double _locality=0;
        double _localGenomeNovelty = 0; //JUSTIN (added at Paul's suggestion)
        int _nearestNeighbors = 0; //JUSTIN (added at Paul's suggestion)
        double _geneticDiversity = 0.0; // JUSTIN (added this because of stupid errors trying to access objectives[] inside noveltyfixed.measureNovelty())
		double[] _objectives;
	    BehaviorType behavior;
		long evaluationCount = 0;
		double totalFitness = 0;
		int speciesId = -1;
		int parentSpeciesId1 = -1;
		int parentSpeciesId2 = -1;
		Population owningPopulation;

		// Stores the decoded network. Storing this prevents the need to re-decode genomes during
		// experiments where the same genome may be evaluated multiple times, e.g. re-evaluation 
		// per generation because of a non-deterministic evaluation function, or a deterministic
		// function that is changing as the search progresses.
		// If it can be cast to AbstractNetwork then this can also form the basis of constructing a
		// NetworkModel for network visualization.
		public INetwork network=null;

		/// <summary>
		/// A tag object that can be used by evaluators to store evaluation state information. This isn't
		/// normally used. An example usage is the ParetoCoEv Tic-Tac-Toe evaluator which uses this to store 
		/// an integer which gives the index of the last entry in the pareto chain to have been evaluated against.
		/// Thus we only have to evaluate against later entries which elimintates a large number of redundant evaluations.
		/// </summary>
		object tag;

		#region Public Methods [Implemented]

		/// <summary>
		/// Implemented in contravention of the .net documentation. ArrayList.Sort() will sort into descending order.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int CompareTo(Object obj)
		{
			if(((IGenome)obj).Fitness > fitness)
				return 1;

			if(((IGenome)obj).Fitness < fitness)
				return -1;

			return 0;
		}

		#endregion

		#region Public Methods [Abstract]

		/// <summary>
		/// Some(most) types of network have fixed numbers of input and output nodes and will not work correctly or
		/// throw an exception if we try and use inputs/outputs that do not exist. This method allows us to check
		/// compatibility before we begin.
		/// </summary>
		/// <param name="inputCount"></param>
		/// <param name="outputCount"></param>
		/// <returns></returns>
		abstract public bool IsCompatibleWithNetwork(int inputCount, int outputCount);

		/// <summary>
		/// Asexual reproduction with built in mutation.
		/// </summary>
		/// <returns></returns>
		abstract public IGenome CreateOffspring_Asexual(EvolutionAlgorithm ea);

		/// <summary>
		/// Sexual reproduction. No mutation performed.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		abstract public IGenome CreateOffspring_Sexual(EvolutionAlgorithm ea, IGenome parent);

		/// <summary>
		/// Decode the genome's 'DNA' into a working network.
		/// </summary>
		/// <returns></returns>
		abstract public INetwork Decode(IActivationFunction activationFn);

		/// <summary>
		/// Clone this genome.
		/// </summary>
		/// <returns></returns>
		abstract public IGenome Clone(EvolutionAlgorithm ea);

		/// <summary>
		/// Compare this IGenome with the provided one. They are compatibile if their calculated difference 
		/// is below the current threshold specified by NeatParameters.compatibilityThreshold
		/// </summary>
		/// <param name="comparisonGenome"></param>
		/// <param name="neatParameters"></param>
		/// <returns></returns>
		abstract public bool IsCompatibleWithGenome(IGenome comparisonGenome, NeatParameters neatParameters);

		/// <summary>
		/// Persist to XML.
		/// </summary>
		/// <param name="parentNode"></param>
		abstract public void Write(XmlNode parentNode);

		/// <summary>
		/// For debug purposes only.
		/// </summary>
		/// <returns>Returns true if genome integrity checks out OK.</returns>
		abstract public bool PerformIntegrityCheck();

		#endregion

		#region Public Properties [Implemented]

		public object Tag
		{
			get
			{
				return tag;
			}
			set
			{
				tag = value;
			}
		}

		public uint GenomeId
		{
			get
			{
				return genomeId;
			}
			set
			{
				genomeId = value;
			}
		}

        // JUSTIN: Adding some info that is only used for dumping extra info to file when we dump the MapElites finalGrid.xml
        public int[] GridCoords
        {
            get;
            set;
        }

		public long GenomeAge
		{
			get
			{
				return genomeAge;
			}
			set
			{
				genomeAge = value;
			}
		}

        /// <summary>
        /// This genome's behavior as calculated by the evaluation environment.
        ///</summary>
        public BehaviorType Behavior
        {
            get
            {
                return behavior;
		    }
		    set
		    {
		        behavior = value;
		    }
		}
		
		public double[] objectives
		{
			get
			{
				return _objectives;
			}
			set
			{
				_objectives= value;
			}
		}
		
		public double locality
		{
			get
			{
				return _locality;
			}
			set
			{
				_locality=value;
			}
		}

        public double geneticDiversity
        {
            get
            { 
                return _geneticDiversity;
            }
            set 
            { 
                _geneticDiversity = value; 
            }
        }

        public double localGenomeNovelty
        {
            get
            {
                return _localGenomeNovelty;
            }
            set
            {
                _localGenomeNovelty = value;
            }
        }

        public int nearestNeighbors
        {
            get
            {
                return _nearestNeighbors;
            }
            set
            {
                _nearestNeighbors = value;
            }
        }

		public double competition
		{
			get
			{
				return _competition;
			}
			set
			{
				_competition=value;
			}
		}
		
		/// <summary>
		/// This genome's real fitness (i.e. not novelty score)
		///</summary>
		public double RealFitness
		{
		  get
		  {
		     return realfitness;
		  }
		  set
		  {
		     realfitness = value;
          }
		}
		
		/// <summary>
		/// This genome's fitness as calculated by the evaluation environment.
		/// </summary>
		public double Fitness
		{
			get
			{
				return fitness;
			}
			set
			{
				//Debug.Assert(value>=EvolutionAlgorithm.MIN_GENOME_FITNESS, "Genome fitness must be non-zero. Use EvolutionAlgorithm.MIN_GENOME_FITNESS");
				fitness = value;
			}
		}

		/// <summary>
		/// The number of times this genome has been evaluated.
		/// </summary>
		public long EvaluationCount
		{
			get
			{
				return evaluationCount;
			}
			set
			{
				evaluationCount = value;
			}
		}

		/// <summary>
		/// Returns the total of all fitness scores if this genome has been evaluated more than once.
		/// Average fitness is therefore this figure divided by GenomeAge.
		/// </summary>
		public double TotalFitness
		{
			get
			{
				return totalFitness;
			}
			set
			{
				totalFitness = value;
			}
		}

		/// <summary>
		/// The species this genome is within.
		/// </summary>
		public int SpeciesId
		{
			get
			{
				return speciesId;
			}

			set
			{
				speciesId = value;
			}
		}

		/// <summary>
		/// The ID of this genome's first parent.
		/// </summary>
		public int ParentSpeciesId1
		{
			get
			{
				return parentSpeciesId1;
			}

			set
			{
				parentSpeciesId1 = value;
			}
		}

		/// <summary>
		/// The ID of this genome's second parent. -1 if no second parent.
		/// </summary>
		public int ParentSpeciesId2
		{
			get
			{
				return parentSpeciesId2;
			}

			set
			{
				parentSpeciesId2 = value;
			}
		}

		public AbstractNetwork AbstractNetwork
		{
			get
			{	// The INetwork may not be a AbstractNetwork, return null if that is the case.
				return network as AbstractNetwork;
			}
		}

		/// <summary>
		/// Used primarily to give this IGenome a hook onto the Population it is within.
		/// </summary>
		public Population OwningPopulation
		{
			get
			{
				return owningPopulation;
			}
			set
			{
				owningPopulation = value;
			}
		}

		#endregion


        public double FitnessBeforeNormalization
        {
            get;
            set;
        }
    }
}
