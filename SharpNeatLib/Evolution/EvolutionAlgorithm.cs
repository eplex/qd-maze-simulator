using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;

//TODO: decouple from NeatGenome.
using SharpNeatLib.NeatGenome;
using SharpNeatLib.Novelty;
using SharpNeatLib.Multiobjective;
using SharpNeatLib.Evolution;

using System.Xml;
using SharpNeatLib.Evolution.Xml;
using SharpNeatLib.NeatGenome.Xml;
using System.IO;

namespace SharpNeatLib.Evolution
{
	public class EvolutionAlgorithm
	{
        public const bool SAVETHINGS = false;
        public const double FITNESS_SAVE_THRESHOLD = 1200.0;
        public const double SAVED_PER_GENERATION = 1.1; //JUSTIN: TODO: Implement the code to save high-fitness individuals each gen.
        // idea: ticker that goes up by 0.6 each generation and whenever it is >1 it saves random individuals, each saved decrements it until its under 1.
        GenomeList savedGenomeHopper = new GenomeList();
        public const double EMPTY_HOPPER_AT = 500;
        public int timesDumped = 0;
        public double saveTicker = 0.0; // incremented by SAVED_PER_GENERATION each generation, decremented by 1.0 when a genome is saved
        public int totalOverFitnessThreshold = 0;
        public string outputFolder = "";

        // MapElites stuff - JUSTIN
        Dictionary<UInt64, IGenome> meGrid = new Dictionary<UInt64, IGenome>();
        Dictionary<UInt64, IGenome> meGeneticDiversityGrid = new Dictionary<UInt64, IGenome>(); // A parallel grid where individuals have a second chance to be stored if they aren't better fitness than those in meGrid
        int meNumDimensions;
        int meNumBins;
        List<double> meMin;
        List<double> meMax;
        List<double> meRange;
        List<double> meBinSize; // meBinSize = meRange / meNumBins
        // MapElites statistics
        public int numFilledBins; // number of grid bins that have genomes in them
        public double averageFitness; // average fitness of the running algorithm (depends on type)
        public double gridAverageFitness; // average fitness of all members of the grid (if we are tracking a grid)
        public double generationBase500; // The generation that we would be in if there were 500 genomes evaluated per generation (useful for comparison with old algorithms)
        public uint numEvaluations = 0; // Counts the number of evaluations

        // Complexity statistics - JUSTIN
        public double minComplexity = 0;
        public double maxComplexity = 0;
        public double avgComplexity = 0;
        public double stdevComplexity = 0;

        // NS2 stuff - JUSTIN
        List<IGenome> ns2NoveltyArchive = new List<IGenome>();
        public int archiveSize = 0;
        public TimeSpan timeSpentOutsideFitnessEval; // time spent calculating novelty and making babies (we want this time to not be a significant percentage of time, otherwise we need to optimize some of the O(N^2) novelty archive calculations
        public TimeSpan timeSpentInFitnessEval; // time spent working in the simulator coal mine (we want this time to be much larger than the other time)
        public TimeSpan timeSpentInSpeciation;
        public List<int> speciesSizes = new List<int>();
        public int totalSpeciesSize = 0;
        public double averageNovelty = 0.0;
        public List<Species> ns2species = new List<Species>();
        public int ns2numSpecies;
        public bool ns2_areSpeciesInitializedYet = false;
        public KMeansSpeciation kmeans = new KMeansSpeciation();

        // NS1 stuff - JUSTIN
        public bool ns1 = false;
        List<IGenome> ns1NoveltyArchive = new List<IGenome>(); // In NS1, *this* is the real archive, while ns2NoveltyArchive is treated as the steady-state population
        public double ns1archiveAverageNovelty = 0.0;

        // JUSTIN - Tracking how the archive changes with each generation
        // Note: This is tracked only if recordEndpoints=TRUE and we are using an algorithm with an archive
        /* Currently implemented for: 
         * 
         * 
         */
        public List<IGenome> addedToArchive = new List<IGenome>();
        public List<IGenome> removedFromArchive = new List<IGenome>();

        // Initialize the variables pertaining to the grid bins (min, max, range, bin size, num bins..)
        // MAPELITES TODO: Define this in a better way. For now it is hardcoded.
        //      - a better way: specify in the behavior characterization what the bounds are for each dimension
        public void meGridInit(Population pop)
        {
            meNumDimensions = pop.GenomeList[0].Behavior.behaviorList.Count; // see how many dimensions we are working with.


            // HARDCODED - IMPORTANT: CHANGE THIS TO FIT THE DOMAIN. DO NOT USE TOO MANY BINS FOR THE DIMENSIONALITY!
            int HC_NUMBINS = 36;
            double HC_DEFAULTMIN = 0;
            double HC_DEFAULTMAX = 1;
            int HC_HOWMANYISTOOMANYBINS = 2000;
            int HC_HOWMANYISTOOMANYDIMENSIONS = 14;

            if (meNumDimensions >= HC_HOWMANYISTOOMANYDIMENSIONS)
            {
                Console.WriteLine("[!] Behavior characterization has TOO MANY VALUES for use with MapElites grid! (" + meNumDimensions + " vals, max " + HC_HOWMANYISTOOMANYDIMENSIONS + ")");
                throw new Exception("[!] Behavior characterization has TOO MANY VALUES for use with MapElites grid! (" + meNumDimensions + " vals, max " + HC_HOWMANYISTOOMANYDIMENSIONS + ")");
            }

            double totalNumberOfBins = Math.Pow(HC_NUMBINS, meNumDimensions);
            while (totalNumberOfBins > HC_HOWMANYISTOOMANYBINS)
            {
                if (HC_NUMBINS <= 1)
                {
                    Console.WriteLine("[!] Cannot resolve MapElites bins (this should never happen). Try using a BC with fewer values.");
                    throw new Exception("[!] Cannot resolve MapElites bins (this should never happen). Try using a BC with fewer values.");
                }

                // Decrement the number of bins per dimension and then recalculate the total.
                HC_NUMBINS--;
                totalNumberOfBins = Math.Pow(HC_NUMBINS, meNumDimensions);
                Console.WriteLine("[!] Too many bins per dimension for this BC, trying fewer bins: " + HC_NUMBINS + " per dimension");
            }

            meNumBins = HC_NUMBINS;
            meMin = new List<double>();
            meMax = new List<double>();
            meRange = new List<double>();
            meBinSize = new List<double>(); // meBinSize = meRange / meNumBins

            for (int i = 0; i < meNumDimensions; i++)
            {
                meMin.Add(HC_DEFAULTMIN);
                meMax.Add(HC_DEFAULTMAX);
                meRange.Add(HC_DEFAULTMAX - HC_DEFAULTMIN);
                meBinSize.Add((meRange[i])/meNumBins);
            }
        }

        // Generalized cantor pairing function for uniquely mapping a coordinate tuple to a single integer (recursive function)
        //   http://en.wikipedia.org/wiki/Cantor_pairing_function
        public UInt64 cantorPairing(List<int> coord)
        {  
            if (coord.Count == 2) // Base case
            {
                UInt64 first = Convert.ToUInt64(coord[0]);
                UInt64 second = Convert.ToUInt64(coord[1]);
                UInt64 rval = ((((first + second) * (first + second + 1)) / 2) + second);
                return rval;
            }
            else if (coord.Count == 1) // This should never happen unless we only have 1 dimension from the start. Otherwise the base case is dim=2.
            { 
                return Convert.ToUInt64(coord[0]); 
            } 
            else
            {
                UInt64 second = Convert.ToUInt64(coord[coord.Count - 1]); // take off the last element
                UInt64 first = cantorPairing(coord.GetRange(0, (coord.Count - 1))); // recurse everything else
                // Now perform the cantorPairing
                UInt64 rval = ((((first + second) * (first + second + 1)) / 2) + second);
                return rval;                
            }
        }

        // Converts a behavior characterization into a coordinate vector corresponding to the ME Grid bin it fits into
        public List<int> bcToBinCoordinates(List<double> bc)
        {
            List<int> rval = new List<int>();

            for (int i = 0; i < bc.Count; i++)
            {
                // DEBUG: Verify that we haven't violated bin boundaries
                // MAPELITES TODO: Perhaps grid boundaries could dynamically re-adjust as new bounds are found?
                //    for now, just keep track of violated bounds and adjust them by hand accordingly
                if (bc[i] < meMin[i])
                {
                    Console.WriteLine("Minimum grid bound " + meMin[i].ToString() + " violated: " + bc[i].ToString());
                    rval.Add(0);
                }
                else if (bc[i] > meMax[i])
                {
                    Console.WriteLine("Maximum grid bound " + meMax[i].ToString() + " violated: " + bc[i].ToString());
                    rval.Add(meNumBins-1);
                }
                else
                {
                    rval.Add((int)((bc[i] - meMin[i]) / meBinSize[i]));
                }
            }

            return rval;
        }


		#region Constants

		/// <summary>
		/// Genomes cannot have zero fitness because the fitness sharing logic requires there to be 
		/// a non-zero total fitness in the population. Therefore this figure should be substituted
		/// in where zero fitness occurs.
		/// </summary>
		public const double MIN_GENOME_FITNESS = 0.0000001;
 
		#endregion

		#region Class Variables

		Population pop;
		IPopulationEvaluator populationEvaluator;
		NeatParameters neatParameters;
		NeatParameters neatParameters_Normal;
		NeatParameters neatParameters_PrunePhase;

		public Multiobjective.Multiobjective multiobjective;
        public noveltyhistogram histogram;
        public noveltyfixed noveltyFixed;
		public bool noveltyInitialized=false;
        
		bool pruningModeEnabled=false;
		bool connectionWeightFixingEnabled=false;
		bool pruningMode=false;
		
		
		
		/// <summary>
		/// The last generation at which Population.AvgComplexity was reduced. We track this
		/// when simplifications have completed and that therefore the prune phase should end.
		/// </summary>
		long prunePhase_generationAtLastSimplification;
		float prunePhase_MinimumStructuresPerGenome;

		/// <summary>
		/// Population.AvgComplexity when AdjustSpeciationThreshold() was last called. If mean complexity
		/// moves away from this value by a certain amount then it's time to re-apply the speciation threshold
		/// to the whole population by calling pop.RedetermineSpeciation().
		/// </summary>
		double meanComplexityAtLastAdjustSpeciationThreshold;

		// All offspring are temporarily held here before being added to the population proper.
		GenomeList offspringList = new GenomeList();

		// Tables of new connections and neurons created during adiitive mutations. These tables
		// are available during the mutations and can be used to check for matching mutations so
		// that two mutations that create the same structure will be allocated the same ID. 
		// Currently this matching is only performed within the context of a generation, which
		// is how the original C++ NEAT code operated also.
		Hashtable newConnectionGeneTable = new Hashtable();
		Hashtable newNeuronGeneStructTable = new Hashtable();

		// Statistics
		uint generation=0;
		IGenome bestGenome;


        // JUSTIN
        public bool neatBrain; // This is used in Mutate_AddNode() to determine which type of activation function to use

		
		#endregion

		#region Constructors

		/// <summary>
		/// Default Constructor.
		/// </summary>
		public EvolutionAlgorithm(Population pop, IPopulationEvaluator populationEvaluator) : this(pop, populationEvaluator, new NeatParameters())
		{}

		/// <summary>
		/// Default Constructor.
		/// </summary>
		public EvolutionAlgorithm(Population pop, IPopulationEvaluator populationEvaluator, NeatParameters neatParameters)
		{
			this.pop = pop;
			this.populationEvaluator = populationEvaluator;
			this.neatParameters = neatParameters;
			neatParameters_Normal = neatParameters;

			neatParameters_PrunePhase = new NeatParameters(neatParameters);
			neatParameters_PrunePhase.pMutateAddConnection = 0.0;
            neatParameters_PrunePhase.pMutateAddNode = 0.0;
            neatParameters_PrunePhase.pMutateAddModule = 0.0;
            neatParameters_PrunePhase.pMutateConnectionWeights = 0.33;
			neatParameters_PrunePhase.pMutateDeleteConnection = 0.33;
			neatParameters_PrunePhase.pMutateDeleteSimpleNeuron = 0.33;

			// Disable all crossover as this has a tendency to increase complexity, which is precisely what
			// we don't want during a pruning phase.
			neatParameters_PrunePhase.pOffspringAsexual = 1.0;
			neatParameters_PrunePhase.pOffspringSexual = 0.0;

            if (neatParameters.mapelites)
            {
                meInitialisePopulation();
                meGridInit(pop);
                Console.WriteLine("Mapelites stuff has been initialized. Oh btw, we're doing mapelites.");
                if (neatParameters.me_simpleGeneticDiversity)
                {
                    Console.WriteLine("Mapelites reinforced by the power of 51MPLE gENET1C d1VER51TY!!!!1  *fireworks* *applause* *receive phd*");
                }
                if (neatParameters.me_noveltyPressure)
                {
                    Console.WriteLine("Mapelites now with NOVELTY PRESSURE! (>'')>");
                }
            } // Skip all that other stupid shit if we are doing MapElites
            else if (neatParameters.NS2)
            {
                if (neatParameters.NS1) ns1 = true;

                ns2InitializePopulation();
                if (neatParameters.track_me_grid)
                {
                    Console.WriteLine("Initializing mapelites-style-grid genome tracking..");
                    meGridInit(pop);
                }
                Console.WriteLine("Novelty Search 2.0 has been initialized.");
            } // Skip the code jungle below if we are doing Novelty Search 2.0
            else if (neatParameters.NSLC) // (Steady-State NSLC -- NEW!!)
            {
                // TODO: JUSTIN: SS-NSLC GOES HERE!
                ns1 = true;
                ns2InitializePopulation();
                if (neatParameters.track_me_grid)
                {
                    Console.WriteLine("Initializing mapelites-style-grid genome tracking..");
                    meGridInit(pop);
                }
                Console.WriteLine("Initializing STEADY STATE -- NSLC! NEW! This is a thing that is happening now. You cannot stop it. Relax.");
                // TODO: INITIALIZATION for SS-NSLC (is like NS1... but make it separate so we can stop being so intertwined. cleaner is better, yo)


            } // Skip the nasty quagmire of unverified bogus rotten banana sandwiches if doing Steady-State NSLC
            else
            {
                if (neatParameters.multiobjective)
                {
                    this.multiobjective = new Multiobjective.Multiobjective(neatParameters);
                    neatParameters.compatibilityThreshold = 100000000.0; //disable speciation w/ multiobjective
                }

                if (neatParameters.noveltySearch)
                {
                    if (neatParameters.noveltyHistogram)
                    {
                        this.noveltyFixed = new noveltyfixed(neatParameters.archiveThreshold);
                        this.histogram = new noveltyhistogram(neatParameters.histogramBins);
                        noveltyInitialized = true;
                        InitialisePopulation();
                    }

                    if (neatParameters.noveltyFixed || neatParameters.noveltyFloat)
                    {
                        this.noveltyFixed = new noveltyfixed(neatParameters.archiveThreshold);
                        InitialisePopulation();
                        noveltyFixed.initialize(this.pop);
                        noveltyInitialized = true;
                        populationEvaluator.EvaluatePopulation(pop, this);
                        UpdateFitnessStats();
                        DetermineSpeciesTargetSize();
                    }

                    if (neatParameters.track_me_grid)
                    {
                        Console.WriteLine("Initializing mapelites-style-grid genome tracking..");
                        meGridInit(pop); // JUSTIN: Trying to add grid-tracking to NS1
                    }

                }
                else
                {
                    InitialisePopulation();

                    if (neatParameters.track_me_grid)
                    {
                        Console.WriteLine("Initializing mapelites-style-grid genome tracking..");
                        meGridInit(pop); // JUSTIN: Trying to add grid-tracking to fitness-based search
                    }
                }
            }
		}

		#endregion

		#region Properties

		public Population Population
		{
			get
			{
				return pop;
			}
		}

		public uint NextGenomeId
		{
			get
			{
				return pop.IdGenerator.NextGenomeId;
			}
		}

		public uint NextInnovationId
		{
			get
			{
				return pop.IdGenerator.NextInnovationId;
			}
		}

		public NeatParameters NeatParameters
		{
			get
			{
				return neatParameters;
			}
		}

		public IPopulationEvaluator PopulationEvaluator
		{
			get
			{
				return populationEvaluator;
			}
		}

		public uint Generation
		{
			get
			{
				return generation;
			}
		}

		public IGenome BestGenome
		{
			get
			{
				return bestGenome;
			}
		}

		public Hashtable NewConnectionGeneTable
		{
			get
			{
				return newConnectionGeneTable;
			}
		}

		public Hashtable NewNeuronGeneStructTable
		{
			get
			{
				return newNeuronGeneStructTable;
			}
		}

		public bool IsInPruningMode
		{
			get
			{
				return pruningMode;
			}
		}

		/// <summary>
		/// Get/sets a boolean indicating if the search should use pruning mode.
		/// </summary>
		public bool IsPruningModeEnabled
		{
			get
			{
				return pruningModeEnabled;
			}
			set
			{
				pruningModeEnabled = value;
				if(value==false)
				{	// Weight fixing cannot (currently) occur with pruning mode disabled.
					connectionWeightFixingEnabled = false;
				}
			}
		}

		/// <summary>
		/// Get/sets a boolean indicating if connection weight fixing is enabled. Note that this technique
		/// is currently tied to pruning mode, therefore if pruning mode is disabled then weight fixing
		/// will automatically be disabled.
		/// </summary>
		public bool IsConnectionWeightFixingEnabled
		{
			get
			{
				return connectionWeightFixingEnabled;
			}
			set
			{	// Ensure disabled if pruningMode is disabled.
				connectionWeightFixingEnabled = pruningModeEnabled && value;
			}
		}

		#endregion

		#region Public Methods
        
        public void CalculateNovelty()
        {
            if (neatParameters.multiobjective)
            {
                //Console.WriteLine("Calculating genomic novelty");
                multiobjective.calculateGenomicNovelty(pop.GenomeList);
            }
            //Console.WriteLine("CalculateNovelty() in EvolutionAlgorithm.cs");
            if(neatParameters.noveltyHistogram)
            {
                Console.WriteLine("[!] This pathway is untested: noveltyHistogram=true [!]");
                    int count = pop.GenomeList.Count;
                    for (int i = 0; i < count; i++)
                    {
                    pop.GenomeList[i].Fitness =
                            histogram.query_point(pop.GenomeList[i].Behavior.behaviorList,true);
                    noveltyFixed.measureAgainstArchive((NeatGenome.NeatGenome)pop.GenomeList[i],true);
                    }
                    noveltyFixed.addPending();
                    histogram.update_histogram();
            }
            
            if(neatParameters.noveltyFixed || neatParameters.noveltyFloat)
            {
      
					int count = pop.GenomeList.Count;
                    for (int i = 0; i < count; i++) //JUSTIN: Before we do anything, reset the aux data for calculating NS+LC objectives
                    {
					  pop.GenomeList[i].locality=0.0;
					  pop.GenomeList[i].competition=0.0;
                      pop.GenomeList[i].localGenomeNovelty = 0.0;
                      pop.GenomeList[i].nearestNeighbors = 0;
                        // GeneticDiversity was assigned above and should not be reset
					}
					double max=0.0,min=100000000000.0;
					for (int i = 0; i< count; i++)
                    {
                    	double fit = noveltyFixed.measureNovelty((NeatGenome.NeatGenome)pop.GenomeList[i]);        

						pop.GenomeList[i].Fitness = fit;
                    	
                    	if(fit>max) max=fit;
                    	if(fit<min) min=fit;
                    				
					}
					//Console.WriteLine("fitscore: " + min + " " + max);
                    
					/*
                    double max=0.0;
                    
					for (int i =0; i < count;  i++)
                    {
                    	if(pop.GenomeList[i].locality>max)
                    		max=pop.GenomeList[i].locality;	
					}
				    
					for (int i = 0; i< count; i++)
					{
						double locality_score = max-pop.GenomeList[i].locality;
						double competition_score = pop.GenomeList[i].competition;	
						pop.GenomeList[i].Fitness=locality_score+competition_score+1.0;	
					}   
					*/ 
					
				}

        }

        // Special InitialisePopulation() for MapElites (i.e. super stripped down version, with no species)
        private void meInitialisePopulation()
        {
            MatchConnectionIds();
            populationEvaluator.EvaluatePopulation(pop, this);
            meUpdateFitnessStats();

            // Check integrity.
            Debug.Assert(pop.PerformIntegrityCheck(), "Population integrity check failed.");
            // uhh? I hope that doesn't happen.
        }

        private void ns2InitializePopulation() // (Can you spot the spelling inconsistency across parallel methods?) 
        {
            ns2numSpecies = (int)(Math.Floor(Math.Sqrt(neatParameters.NS2_archiveCap))); // Num species = sqrt of pop size (1000 => 32, 3000 => 55)

            MatchConnectionIds();
            populationEvaluator.EvaluatePopulation(pop, this);
            ns2UpdateFitnessStats();

            // Check integrity.
            Debug.Assert(pop.PerformIntegrityCheck(), "Population integrity check failed.");
            // uhh? I hope that doesn't happen.
        }

		/// <summary>
		/// Evaluate all genomes in the population, speciate them and then calculate adjusted fitness
		/// and related stats.
		/// </summary>
		/// <param name="p"></param>
		private void InitialisePopulation()
		{
			// The GenomeFactories normally won't bother to ensure that like connections have the same ID 
			// throughout the population (because it's not very easy to do in most cases). Therefore just
			// run this routine to search for like connections and ensure they have the same ID. 
			// Note. This could also be done periodically as part of the search, remember though that like
			// connections occuring within a generation are already fixed - using a more efficient scheme.
			MatchConnectionIds();

			// Evaluate the whole population. 
			populationEvaluator.EvaluatePopulation(pop, this);

			// Speciate the population.
			pop.BuildSpeciesTable(this);

			// Now we have fitness scores and a speciated population we can calculate fitness stats for the
			// population as a whole and per species.
			UpdateFitnessStats();

			// Set new threshold 110% of current level or 10 more if current complexity is very low.
			pop.PrunePhaseAvgComplexityThreshold = pop.AvgComplexity + neatParameters.pruningPhaseBeginComplexityThreshold;

			// Obtain an initial value for this variable that tracks when we should call pp.RedetermineSpeciation().
			meanComplexityAtLastAdjustSpeciationThreshold = pop.AvgComplexity;

			// Now we have stats we can determine the target size of each species as determined by the
			// fitness sharing logic.
			DetermineSpeciesTargetSize();

			// Check integrity.
			Debug.Assert(pop.PerformIntegrityCheck(), "Population integrity check failed.");
		}

        public void dumpGoodGenomes()
        {
            XmlDocument dumpme = new XmlDocument();
            XmlPopulationWriter.WriteGenomeList(dumpme, savedGenomeHopper);
            FileInfo oFileInfo = new FileInfo(outputFolder + timesDumped.ToString() + "ahfDump.xml");
            dumpme.Save(oFileInfo.FullName);

            savedGenomeHopper.Clear();
            timesDumped++;
        }

        // TODO: Add "simple genetic diversity" to this method :)
        public void mePerformOneGeneration()
        {
                        
            // 1. Insert population into ME Grid
            foreach (IGenome g in pop.GenomeList)
            {
                (g as AbstractGenome).GridCoords = bcToBinCoordinates(g.Behavior.behaviorList).ToArray();
                UInt64 coord = cantorPairing(bcToBinCoordinates(g.Behavior.behaviorList));
                if (!meGrid.ContainsKey(coord))
                { // If this grid slot is empty, just go ahead and add the genome
                    meGrid[coord] = g;
                    addedToArchive.Add(g); // Tracking changes to the archive (grid)
                }
                else
                { // If it's not empty, replace only if g has higher fitness
                    if (g.RealFitness > meGrid[coord].RealFitness)
                    {
                        removedFromArchive.Add(meGrid[coord]); // Tracking changes to the archive (grid)
                        meGrid[coord] = g;
                        addedToArchive.Add(g); // Tracking changes to the archive (grid)
                    }
                    else
                    {
                        // So sad

                        if (neatParameters.me_simpleGeneticDiversity)
                        {
                            // Lo, another chance at life!
                            if (!meGeneticDiversityGrid.ContainsKey(coord))
                            { // If this grid slot is empty, just go ahead and add the genome
                                meGeneticDiversityGrid[coord] = g;
                                addedToArchive.Add(g); // Tracking changes to the archive (grid)
                            }
                            else
                            { // If it's not empty, then replace at random (yeah, this is a really shoddy way of maintaining genetic diversity, but it's cheap!)
                                if (Utilities.NextDouble() < 0.3) // TODO: Should this be 50% chance? or lower?
                                {
                                    removedFromArchive.Add(meGeneticDiversityGrid[coord]); // Tracking changes to the archive (grid)
                                    meGeneticDiversityGrid[coord] = g;
                                    addedToArchive.Add(g); // Tracking changes to the archive (grid)
                                }
                            }
                        }
                    }
                }
            }

            // 2. Clear population
            int popsize = pop.GenomeList.Count; // (but remember how many there were)
            pop.GenomeList.Clear();

            

            // 3. Generate new offspring to fill population
            IGenome[] parents = new IGenome[meGrid.Count + meGeneticDiversityGrid.Count];
            meGrid.Values.CopyTo(parents, 0); // get an array of all parents
            meGeneticDiversityGrid.Values.CopyTo(parents, meGrid.Count); // affirmative action

            if (neatParameters.me_noveltyPressure)
            {
                // 3.1. Recalculate all novelty scores within the grid
                int totalArchiveCount = parents.Length;
                List<double> tempDistances = new List<double>(totalArchiveCount); // A list to measure distances to all of the other genomes in the archive (note: includes self, exclude later)
                for (int i = 0; i < totalArchiveCount; i++) { tempDistances.Add(0.0); } // Initialization

                int innerlimit = (totalArchiveCount < neatParameters.NS2_neighborhoodSize) ? totalArchiveCount : neatParameters.NS2_neighborhoodSize;
                for (int i = 0; i < totalArchiveCount; i++)
                {
                    // TODO: Calculate local competition objective.
                    //       - this requires keeping track of the genomes that each "tempDistances[j]" is attached to, so that we can check the nearest neighbors for fitness

                    for (int j = 0; j < totalArchiveCount; j++)
                    {
                        tempDistances[j] = BehaviorDistance.Distance(parents[i].Behavior, parents[j].Behavior);
                    }
                    tempDistances.Sort();
                    double tempSumOfNearestNeighborDistances = 0.0;

                    for (int j = 1; j < innerlimit; j++) // note: start at index 1 to skip the inevitable distance=0 from comparing against yourself. But you still count as one of your own neighbors as far as NN count goes.. if NN=5, then you only compare against 4 nearest neighbors. Arbitrary design decision that we will forget about in 3... 2...
                    {
                        tempSumOfNearestNeighborDistances += tempDistances[j];
                    }
                    parents[i].Fitness = tempSumOfNearestNeighborDistances; // Store novelty score in .Fitness
                    parents[i].FitnessBeforeNormalization = tempSumOfNearestNeighborDistances;
                    if (neatParameters.NS2_speciation && ns2_areSpeciesInitializedYet)
                    { // Divide fitness by the size of the species
                        ns2NoveltyArchive[i].Fitness /= ns2species[ns2NoveltyArchive[i].SpeciesId].Members.Count;
                    }
                }
            }

            // Actually generate the children to fill the pop
            
                if (neatParameters.me_noveltyPressure)
                {
                    // Selecting parents from the archive proportional to their novelty scores, create the next generation
                    List<IGenome> selectedParents = ns2RouletteSelectParents(new List<IGenome>(parents), popsize);
                    foreach (IGenome parent in selectedParents)
                    {
                        pop.GenomeList.Add(parent.CreateOffspring_Asexual(this));
                    }
                }
                else
                {
                    for (int i = 0; i < popsize; i++)
                    {
                        // TODO: Should there be an equal chance of selecting genetic diversity parents? Or should they have a reduced chance? Who knows!
                        IGenome parent = parents[Utilities.Next(parents.Length)]; // Select a parent at random,
                        IGenome child = parent.CreateOffspring_Asexual(this); // it produces an offspring,
                        pop.GenomeList.Add(child); // which goes to the next population (individuals to be evaluated)
                    }
                }
                
            

            // 4. Evaluate population
            populationEvaluator.EvaluatePopulation(pop, this); // hopefully this works? multi-threaded evaluation of the population (1 per thread)

            // 5. Update fitness stats and other generational bookkeeping
            meUpdateFitnessStats();
            generation++;
        }

        // we need a special method to do this because the regular one only updates the population. we need to update the grid.
        private void meIncrementGenomeAges()
        {
            ulong[] allkeys = new ulong[meGrid.Count];
            meGrid.Keys.CopyTo(allkeys, 0);
            foreach (ulong key in allkeys)
            {
                meGrid[key].GenomeAge = meGrid[key].GenomeAge++;
            }
        }

        // Dump the contents of the ME Grid to a GenomeList (e.g. for printing at the end of a run)
        //  (should be okay to call this even if grid is empty, e.g. not doing MapElites)
        // MAPELITES TODO: add a "grid location" property (some coordinates) to all genomes so that when we save the grid, it can be easily reconstructed, visually. this will be junk if not using MapElites. w/e
        public GenomeList meDumpGrid()
        {
            GenomeList rval = new GenomeList();
            IGenome[] allgenomes = new IGenome[meGrid.Count];
            meGrid.Values.CopyTo(allgenomes, 0);

            foreach (IGenome g in allgenomes)
            {
                rval.Add(g);
            }

            return rval;
        }

        /// <summary>
        /// Selects the specified number of parents from a population of potential parents (with replacement) using roulette wheel selection on .Fitness scores.
        /// Note: This uses a very naive/unoptimized roulette wheel. If 'numberToSelect' is high, it is probably worth using Vose's Aliasing Method (not implemented).
        /// </summary>
        /// <param name="potentialParents">The population of potential parents that are being selected from</param>
        /// <param name="numberToSelect">How many parents to select from the population (selections may very well contain duplicates)</param>
        /// <returns>A list of parents, which are randomly selected proportionally to their .Fitness scores.</returns>
        private List<IGenome> ns2RouletteSelectParents(List<IGenome> potentialParents, int numberToSelect)
        {
            List<IGenome> parents = new List<IGenome>();

            // First, we need to get the total fitness across the population, so we can calculate individual proportions.
            double totalFitness = 0.0;
            foreach (IGenome g in potentialParents)
            {
                totalFitness += g.Fitness;
            }

            // Then, we select %numberToSelect% parents at random using roulette wheel
            for (int i = 0; i < numberToSelect; i++)
            {
                double diceRoll = Utilities.NextDouble();
                double accumulator = 0.0;
                int selectedParentIndex = potentialParents.Count - 1; // (default to the last parent if the diceroll wheel that follows doesnt pick anyone. As it would have picked the last one anyways but didn't only because of rounding errors)
                for (int j = 0; j < potentialParents.Count; j++)
                {
                    accumulator += (potentialParents[j].Fitness / totalFitness);
                    if (diceRoll < accumulator)
                    {
                        selectedParentIndex = j;
                        break;
                    }
                }
                parents.Add(potentialParents[selectedParentIndex]);
            }
            
            return parents;
        }

        // ====================================================================================================================
        /*
         * 
         * NSLC Auxiliary methods for calculating the pareto front
         *      These methods are borrowed from Multiobjective.cs and stripped down so that they are no longer part of that code path at all
         * 
         */
        // ====================================================================================================================
        //if genome x dominates y, increment y's dominated count, add y to x's dominated list
        public void nslc_update_domination(NeatGenome.NeatGenome x, NeatGenome.NeatGenome y, RankInformation r1, RankInformation r2)
        {
            if (nslc_dominates(x, y))
            {
                r1.dominates.Add(r2);
                r2.domination_count++;
            }
        }

        //function to check whether genome x dominates genome y, usually defined as being no worse on all
        //objectives, and better at at least one
        public bool nslc_dominates(NeatGenome.NeatGenome x, NeatGenome.NeatGenome y)
        {
            bool better = false;
            double[] objx = x.objectives, objy = y.objectives;
            int sz = objx.Length;
            //if x is ever worse than y, it cannot dominate y
            //also check if x is better on at least one
            for (int i = 0; i < sz; i++)
            {
                if (objx[i] < objy[i]) return false;
                if (objx[i] > objy[i]) better = true;
            }

            return better;
        }

        // Assign NSLC objectives to the NeatGenome.NeatGenome .objectives list so the regular pareto-domination code will work :) // JUSTIN
        public void nslcAssignObjectives(List<IGenome> populationToRank)
        {
            // 0: Novelty (stored under .Fitness before it is replaced by Rank fitness)
            // 1: Local Competition (stored under .competition)
            for (int i = 0; i < populationToRank.Count; i++)
            {
                ((NeatGenome.NeatGenome)(populationToRank[i])).objectives = new double[2];
                ((NeatGenome.NeatGenome)(populationToRank[i])).objectives[0] = populationToRank[i].Fitness;
                ((NeatGenome.NeatGenome)(populationToRank[i])).objectives[1] = populationToRank[i].competition;
            }

        }

        public void nslcRankGenomes(List<IGenome> populationToRank)
        {
            // Make sure we assign the objective information properly, before anything else
            nslcAssignObjectives(populationToRank);

            int size = populationToRank.Count;

            // Reset rank information (just create new ones, whatever)
            List<RankInformation> ranks = new List<RankInformation>();
            for (int i = 0; i < size; i++)
            {
                ranks.Add(new RankInformation());
            }

            // Calculate domination by testing each genome against every other genome
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    nslc_update_domination((NeatGenome.NeatGenome)populationToRank[i], (NeatGenome.NeatGenome)populationToRank[j], ranks[i], ranks[j]);
                }
            }

            // Successively peel off non-dominated fronts (e.g. those genomes no longer dominated by any in the remaining population)
            List<int> front = new List<int>();
            int ranked_count = 0;
            int current_rank = 1;
            while (ranked_count < size)
            {
                //search for non-dominated front
                for (int i = 0; i < size; i++)
                {
                    //continue if already ranked
                    if (ranks[i].ranked) continue;
                    //if not dominated, add to front
                    if (ranks[i].domination_count == 0)
                    {
                        front.Add(i);
                        ranks[i].ranked = true;
                        ranks[i].rank = current_rank;
                    }
                }

                int front_size = front.Count;
                //Console.WriteLine("Front " + current_rank + " size: " + front_size);

                //now take all the non-dominated individuals, see who they dominated, and decrease
                //those genomes' domination counts, because we are removing this front from consideration
                //to find the next front of individuals non-dominated by the remaining individuals in
                //the population
                for (int i = 0; i < front_size; i++)
                {
                    RankInformation r = ranks[front[i]];
                    foreach (RankInformation dominated in r.dominates)
                    {
                        dominated.domination_count--;
                    }
                }

                ranked_count += front_size;
                front.Clear();
                current_rank++;
            }

            for (int i = 0; i < size; i++)
            {
                populationToRank[i].Fitness = (current_rank + 1) - ranks[i].rank; // Overwrite fitness with rank fitness
            }

            /*foreach (RankInformation r in ranks)
            {
                Console.WriteLine("After ranking: " + r.domination_count + " " + r.rank + " " + r.ranked);
            }//*/
        }
        // END: NSLC Auxiliary Methods ========================================================================================
        // ====================================================================================================================

        // TODO: JUSTIN: SS-NSLC GOES HERE! DO IT, MONKEY.
        private void nslcPerformOneGeneration()
        {
            DateTime before, after;
            timeSpentInFitnessEval = new TimeSpan(0L);
            timeSpentOutsideFitnessEval = new TimeSpan(0L);
            timeSpentInSpeciation = new TimeSpan(0L);

            // TODO: FINISH OFF THIS VIA COPYPASTA VIA NS2 (BELOW)
            // TODO: INITIALIZE NSLC* JUST LIKE NS1 IS INITIALIZED (OVERRIDE CERTAIN NS2_ PREFIXED PARAMETERS WITH NS1_ VALUES...)

            // If we are tracking a ME-style grid, we have to insert the freshly-evaluated population ("batch") here, before any further processing.
            // Note: This takes negligible time.
            if (neatParameters.track_me_grid)
            {
                // Insert population into ME Grid
                foreach (IGenome g in pop.GenomeList)
                {
                    (g as AbstractGenome).GridCoords = bcToBinCoordinates(g.Behavior.behaviorList).ToArray();
                    UInt64 coord = cantorPairing(bcToBinCoordinates(g.Behavior.behaviorList));
                    if (!meGrid.ContainsKey(coord))
                    { // If this grid slot is empty, just go ahead and add the genome
                        meGrid[coord] = g;
                    }
                    else
                    { // If it's not empty, replace only if g has higher fitness
                        if (g.RealFitness > meGrid[coord].RealFitness)
                        { // It's higher fitness, good. Replace with the higher fit individual.
                            meGrid[coord] = g;
                        }
                        else
                        { // So sad

                        }
                    }
                }
            }

            // Track changes to the archive this generation
            addedToArchive.AddRange(pop.GenomeList); // Not all of them will make it. This will be pared down later, to exclude everyone who didn't stay past the pruning phase

            before = DateTime.Now;
            // 1. Add population to archive (the population has already been evaluated at this point)
            ns2NoveltyArchive.AddRange(pop.GenomeList); // The breeding population of size ~500
            ns1NoveltyArchive.AddRange(pop.GenomeList); // The non-breeding novelty archive of size ~2500
            int popSizeWeAreUsing = pop.GenomeList.Count; // (remember this for later so we know how many children to create for the next generation)
            pop.GenomeList.Clear(); // Clear out the old population (we'll add to this later when we spawn children)
            after = DateTime.Now;
            timeSpentOutsideFitnessEval = timeSpentOutsideFitnessEval.Add(after.Subtract(before));

            before = DateTime.Now;
            // 2. Recalculate all novelty scores within the archive (use full ns1+ns2 = 2500+500 = 3000 to calculate for the NS2)
            int totalArchiveCount = ns2NoveltyArchive.Count + ns1NoveltyArchive.Count;
            List<double> tempDistances = new List<double>(totalArchiveCount); // A list to measure distances to all of the other genomes in the archive (note: includes self, exclude later)
            for (int i = 0; i < totalArchiveCount; i++) { tempDistances.Add(0.0); } // Initialization
            List<KeyValuePair<IGenome, double>> tempDistancesAndGenomesForNSLC = new List<KeyValuePair<IGenome, double>>(ns2NoveltyArchive.Count);

            int innerlimit = (totalArchiveCount < neatParameters.NS2_neighborhoodSize) ? totalArchiveCount : neatParameters.NS2_neighborhoodSize;
            for (int i = 0; i < ns2NoveltyArchive.Count; i++)
            {
                tempDistancesAndGenomesForNSLC.Clear();
                for (int j = 0; j < ns2NoveltyArchive.Count; j++)
                {
                    tempDistances[j] = BehaviorDistance.Distance(ns2NoveltyArchive[i].Behavior, ns2NoveltyArchive[j].Behavior);
                    tempDistancesAndGenomesForNSLC.Add(new KeyValuePair<IGenome, double>(ns2NoveltyArchive[j], tempDistances[j])); // NEEDED TO COMPUTE THE NEIGHBORHOOD FOR LC OBJECTIVE (DOES NOT INCLUDE THE NS1 GENOMES)
                }
                for (int j = 0; j < ns1NoveltyArchive.Count; j++) // Additionally, get distance to everyone in the NS1 auxiliary archive (if it exists)
                {
                    tempDistances[j + ns2NoveltyArchive.Count] = BehaviorDistance.Distance(ns2NoveltyArchive[i].Behavior, ns1NoveltyArchive[j].Behavior);
                }
                tempDistances.Sort();
                tempDistancesAndGenomesForNSLC.Sort((A, B) => A.Value.CompareTo(B.Value));
                if (tempDistancesAndGenomesForNSLC[0].Value > tempDistancesAndGenomesForNSLC[10].Value) { Console.WriteLine("BUG BUG! BUG! SORT ORDER IS BACKWARDS!"); }
                int innerlimit2 = (ns2NoveltyArchive.Count < neatParameters.NS2_neighborhoodSize) ? ns2NoveltyArchive.Count : neatParameters.NS2_neighborhoodSize;
                double tempSumOfNearestNeighborDistances = 0.0;
                double countForLCObjective = 0;

                for (int j = 1; j < innerlimit; j++) // note: start at index 1 to skip the inevitable distance=0 from comparing against yourself. But you still count as one of your own neighbors as far as NN count goes.. if NN=5, then you only compare against 4 nearest neighbors. Arbitrary design decision that we will forget about in 3... 2...
                {
                    tempSumOfNearestNeighborDistances += tempDistances[j];
                }
                for (int j = 1; j < innerlimit2; j++)
                {
                    if (ns2NoveltyArchive[i].RealFitness > tempDistancesAndGenomesForNSLC[j].Key.RealFitness) countForLCObjective++;
                }
                countForLCObjective /= (innerlimit2 * (1.0));
                ns2NoveltyArchive[i].Fitness = tempSumOfNearestNeighborDistances; // Store novelty score in .Fitness (to be replaced by rank fitness later, though)
                ns2NoveltyArchive[i].competition = countForLCObjective; // Store LC objective in .competition
                ns2NoveltyArchive[i].FitnessBeforeNormalization = tempSumOfNearestNeighborDistances;
            }


            // 2.1. Recalculate all novelty scores for the NS1 auxiliary archive (if there is one)
            // Note: This is very expensive and in reality would be done less often (or we would use a probabilistic archive)
            //       But it is being done every generation for the fairest possible comparison with NS2.
            if (ns1)
            {
                tempDistances = new List<double>(totalArchiveCount); // A list to measure distances to all of the other genomes in the archive (note: includes self, exclude later)
                for (int i = 0; i < totalArchiveCount; i++) { tempDistances.Add(0.0); } // Initialization

                innerlimit = (totalArchiveCount < neatParameters.NS2_neighborhoodSize) ? totalArchiveCount : neatParameters.NS2_neighborhoodSize;
                for (int i = 0; i < ns1NoveltyArchive.Count; i++)
                {
                    for (int j = 0; j < ns2NoveltyArchive.Count; j++)
                    {
                        tempDistances[j] = BehaviorDistance.Distance(ns1NoveltyArchive[i].Behavior, ns2NoveltyArchive[j].Behavior);
                    }
                    for (int j = 0; j < ns1NoveltyArchive.Count; j++) // Additionally, get distance to everyone in the NS1 auxiliary archive
                    {
                        tempDistances[j + ns2NoveltyArchive.Count] = BehaviorDistance.Distance(ns1NoveltyArchive[i].Behavior, ns1NoveltyArchive[j].Behavior);
                    }
                    tempDistances.Sort();
                    double tempSumOfNearestNeighborDistances = 0.0;

                    for (int j = 1; j < innerlimit; j++) // note: start at index 1 to skip the inevitable distance=0 from comparing against yourself. 
                    {
                        tempSumOfNearestNeighborDistances += tempDistances[j];
                    }
                    ns1NoveltyArchive[i].Fitness = tempSumOfNearestNeighborDistances; // Store novelty score in .Fitness
                    ns1NoveltyArchive[i].FitnessBeforeNormalization = tempSumOfNearestNeighborDistances;
                }
            }
            after = DateTime.Now;
            timeSpentOutsideFitnessEval = timeSpentOutsideFitnessEval.Add(after.Subtract(before));

            // 2.9. (lol) Calculate NSLC pareto ranks and assign rank fitness to .Fitness (novelty score still remembered in .FitnessBeforeNormalization)
            before = DateTime.Now;
            nslcRankGenomes(ns2NoveltyArchive);
            after = DateTime.Now;
            timeSpentInSpeciation = timeSpentInSpeciation.Add(after.Subtract(before));

            before = DateTime.Now;
            // 3. Trim down NS2 archive (breeding population) according to lowest rank fitness
            if (neatParameters.NS2_archiveCap > 0 && ns2NoveltyArchive.Count > neatParameters.NS2_archiveCap)
            {
                ns2NoveltyArchive.Sort((first, second) => first.Fitness.CompareTo(second.Fitness));
                int numToRemove = ns2NoveltyArchive.Count - neatParameters.NS2_archiveCap;
                ns2NoveltyArchive.RemoveRange(0, numToRemove);
            }
            // 3.1. Pare down the NS1 archive (no tournament deletion, just delete the lowest novelty scores)
            if (ns1)
            {
                if (neatParameters.NS1_archiveCap > 0 && ns1NoveltyArchive.Count > neatParameters.NS1_archiveCap)
                {
                    ns1NoveltyArchive.Sort((first, second) => first.Fitness.CompareTo(second.Fitness));
                    int numToRemove = ns1NoveltyArchive.Count - neatParameters.NS1_archiveCap;
                    removedFromArchive.AddRange(ns1NoveltyArchive.GetRange(0, numToRemove)); // Tracking changes to the archive
                    ns1NoveltyArchive.RemoveRange(0, numToRemove);
                }
            }

            foreach (IGenome g in removedFromArchive) addedToArchive.Remove(g); // Tracking changes to the archive: disregard any additions that didn't survive the pruning

            after = DateTime.Now;
            timeSpentOutsideFitnessEval = timeSpentOutsideFitnessEval.Add(after.Subtract(before));

            before = DateTime.Now;
            // 4. Selecting parents from the archive proportional to their rank fitness scores scores, create the next generation
            List<IGenome> selectedParents = ns2RouletteSelectParents(ns2NoveltyArchive, popSizeWeAreUsing);
            foreach (IGenome parent in selectedParents)
            {
                pop.GenomeList.Add(parent.CreateOffspring_Asexual(this));
            }
            after = DateTime.Now;
            timeSpentOutsideFitnessEval = timeSpentOutsideFitnessEval.Add(after.Subtract(before));

            before = DateTime.Now;
            // 5. Evaluate the next generation
            populationEvaluator.EvaluatePopulation(pop, this);
            after = DateTime.Now;
            timeSpentInFitnessEval = timeSpentInFitnessEval.Add(after.Subtract(before));

            before = DateTime.Now;
            // 6. Calculate statistics and other generational bookeeping. 
            ns2UpdateFitnessStats();
            generation++;
            after = DateTime.Now;
            timeSpentOutsideFitnessEval = timeSpentOutsideFitnessEval.Add(after.Subtract(before));





        }

        private void ns2PerformOneGeneration()
        {
            DateTime before, after;
            timeSpentInFitnessEval = new TimeSpan(0L);
            timeSpentOutsideFitnessEval = new TimeSpan(0L);
            timeSpentInSpeciation = new TimeSpan(0L);

            // If we are tracking a ME-style grid, we have to insert the freshly-evaluated population ("batch") here, before any further processing.
            // Note: This takes negligible time.
            if (neatParameters.track_me_grid)
            {
                // Insert population into ME Grid
                foreach (IGenome g in pop.GenomeList)
                {
                    (g as AbstractGenome).GridCoords = bcToBinCoordinates(g.Behavior.behaviorList).ToArray();
                    UInt64 coord = cantorPairing(bcToBinCoordinates(g.Behavior.behaviorList));
                    if (!meGrid.ContainsKey(coord))
                    { // If this grid slot is empty, just go ahead and add the genome
                        meGrid[coord] = g;
                    }
                    else
                    { // If it's not empty, replace only if g has higher fitness
                        if (g.RealFitness > meGrid[coord].RealFitness)
                        { // It's higher fitness, good. Replace with the higher fit individual.
                            meGrid[coord] = g;
                        }
                        else
                        { // So sad
                            
                        }
                    }
                }
            }

            // Track changes to the archive this generation
            addedToArchive.AddRange(pop.GenomeList); // Not all of them will make it. This will be pared down later, to exclude everyone who didn't stay past the pruning phase

            before = DateTime.Now;
            // 1. Add population to archive (the population has already been evaluated at this point)
            ns2NoveltyArchive.AddRange(pop.GenomeList);
            if (ns1) ns1NoveltyArchive.AddRange(pop.GenomeList);
            int popSizeWeAreUsing = pop.GenomeList.Count; // (remember this for later so we know how many children to create for the next generation)
            after = DateTime.Now;
            timeSpentOutsideFitnessEval = timeSpentOutsideFitnessEval.Add(after.Subtract(before));

            before = DateTime.Now;
            // 1.5. Speciate (if applicable)
            if (neatParameters.NS2_speciation)
            {
                if (!ns2_areSpeciesInitializedYet)
                {
                    if (ns2NoveltyArchive.Count >= ns2numSpecies) // We can only initialize species once we have at least SPECIESNUM members in the population.
                    {
                        ns2species = kmeans.InitializeSpeciation(ns2NoveltyArchive, ns2numSpecies);
                        ns2_areSpeciesInitializedYet = true;
                    }
                }
                else
                {
                    // Species have already been initialized, we just need to add this generation's individuals into the existing species
                    kmeans.SpeciateOffspring(pop.GenomeList, ns2species);
                }
                
            }
            after = DateTime.Now;
            timeSpentInSpeciation = timeSpentInSpeciation.Add(after.Subtract(before));

            pop.GenomeList.Clear(); // Clear out the old population (we'll add to this later when we spawn children)


            before = DateTime.Now;
            // 2. Recalculate all novelty scores within the archive
            int totalArchiveCount = ns2NoveltyArchive.Count + ns1NoveltyArchive.Count;
            List<double> tempDistances = new List<double>(totalArchiveCount); // A list to measure distances to all of the other genomes in the archive (note: includes self, exclude later)
            for (int i = 0; i < totalArchiveCount; i++) { tempDistances.Add(0.0); } // Initialization

            int innerlimit = (totalArchiveCount < neatParameters.NS2_neighborhoodSize) ? totalArchiveCount : neatParameters.NS2_neighborhoodSize;
            for (int i = 0; i < ns2NoveltyArchive.Count; i++)
            {
                // TODO: Calculate local competition objective.
                //       - this requires keeping track of the genomes that each "tempDistances[j]" is attached to, so that we can check the nearest neighbors for fitness

                for (int j = 0; j < ns2NoveltyArchive.Count; j++)
                {
                    tempDistances[j] = BehaviorDistance.Distance(ns2NoveltyArchive[i].Behavior, ns2NoveltyArchive[j].Behavior);
                }
                for (int j = 0; j < ns1NoveltyArchive.Count; j++) // Additionally, get distance to everyone in the NS1 auxiliary archive (if it exists)
                {
                    tempDistances[j + ns2NoveltyArchive.Count] = BehaviorDistance.Distance(ns2NoveltyArchive[i].Behavior, ns1NoveltyArchive[j].Behavior);
                }
                tempDistances.Sort();
                double tempSumOfNearestNeighborDistances = 0.0;
                
                for (int j = 1; j < innerlimit; j++) // note: start at index 1 to skip the inevitable distance=0 from comparing against yourself. But you still count as one of your own neighbors as far as NN count goes.. if NN=5, then you only compare against 4 nearest neighbors. Arbitrary design decision that we will forget about in 3... 2...
                {
                    tempSumOfNearestNeighborDistances += tempDistances[j];
                }
                ns2NoveltyArchive[i].Fitness = tempSumOfNearestNeighborDistances; // Store novelty score in .Fitness
                ns2NoveltyArchive[i].FitnessBeforeNormalization = tempSumOfNearestNeighborDistances;
                if (neatParameters.NS2_speciation && ns2_areSpeciesInitializedYet)
                { // Divide fitness by the size of the species
                    ns2NoveltyArchive[i].Fitness /= ns2species[ns2NoveltyArchive[i].SpeciesId].Members.Count;
                }
            }

            // 2.1. Recalculate all novelty scores for the NS1 auxiliary archive (if there is one)
            // Note: This is very expensive and in reality would be done less often (or we would use a probabilistic archive)
            //       But it is being done every generation for the fairest possible comparison with NS2.
            if (ns1)
            {
                tempDistances = new List<double>(totalArchiveCount); // A list to measure distances to all of the other genomes in the archive (note: includes self, exclude later)
                for (int i = 0; i < totalArchiveCount; i++) { tempDistances.Add(0.0); } // Initialization

                innerlimit = (totalArchiveCount < neatParameters.NS2_neighborhoodSize) ? totalArchiveCount : neatParameters.NS2_neighborhoodSize;
                for (int i = 0; i < ns1NoveltyArchive.Count; i++)
                {
                    for (int j = 0; j < ns2NoveltyArchive.Count; j++)
                    {
                        tempDistances[j] = BehaviorDistance.Distance(ns1NoveltyArchive[i].Behavior, ns2NoveltyArchive[j].Behavior);
                    }
                    for (int j = 0; j < ns1NoveltyArchive.Count; j++) // Additionally, get distance to everyone in the NS1 auxiliary archive
                    {
                        tempDistances[j + ns2NoveltyArchive.Count] = BehaviorDistance.Distance(ns1NoveltyArchive[i].Behavior, ns1NoveltyArchive[j].Behavior);
                    }
                    tempDistances.Sort();
                    double tempSumOfNearestNeighborDistances = 0.0;

                    for (int j = 1; j < innerlimit; j++) // note: start at index 1 to skip the inevitable distance=0 from comparing against yourself. 
                    {
                        tempSumOfNearestNeighborDistances += tempDistances[j];
                    }
                    ns1NoveltyArchive[i].Fitness = tempSumOfNearestNeighborDistances; // Store novelty score in .Fitness
                    ns1NoveltyArchive[i].FitnessBeforeNormalization = tempSumOfNearestNeighborDistances;
                }
            }
            after = DateTime.Now;
            timeSpentOutsideFitnessEval = timeSpentOutsideFitnessEval.Add(after.Subtract(before));

            before = DateTime.Now;
            // 3. Trim archive down to proper size via probabilistic deletion
            if (neatParameters.NS2_archiveCap > 0 && ns2NoveltyArchive.Count > neatParameters.NS2_archiveCap)
            {
                if (neatParameters.NS2_tournamentDeletion)
                {
                    while (ns2NoveltyArchive.Count > neatParameters.NS2_archiveCap)
                    {
                        // By the end of this loop, we will have deleted an individual from ns2NoveltyArchive.
                        double lowestScoreSoFar = double.MaxValue;
                        int indexToDelete = 0;
                        for (int i = 0; i < neatParameters.NS2_tournamentSize; i++) // We will make NS2_tournamentSize random draws to compete in the tournament (with replacement). The lowest score gets DELETED!
                        {
                            int tempIndex = Utilities.Next(ns2NoveltyArchive.Count);
                            if (ns2NoveltyArchive[tempIndex].Fitness < lowestScoreSoFar)
                            {
                                lowestScoreSoFar = ns2NoveltyArchive[tempIndex].Fitness;
                                indexToDelete = tempIndex;
                            }
                        }
                        // Goodbye, citizen #%indexToDelete%. Your service to the empire is important to us all.
                        if (neatParameters.NS2_speciation && ns2_areSpeciesInitializedYet)
                        { // Delete from species first
                            ns2species[ns2NoveltyArchive[indexToDelete].SpeciesId].Members.Remove(ns2NoveltyArchive[indexToDelete]);
                        }
                        if (!ns1) removedFromArchive.Add(ns2NoveltyArchive[indexToDelete]); // Tracking changes to the archive
                        ns2NoveltyArchive.RemoveAt(indexToDelete);
                    }
                }
                else
                { // No tournament delete, just delete the lowest diversity individuals
                    ns2NoveltyArchive.Sort((first, second) => first.Fitness.CompareTo(second.Fitness));
                    int numToRemove = ns2NoveltyArchive.Count - neatParameters.NS2_archiveCap;
                    if (neatParameters.NS2_speciation && ns2_areSpeciesInitializedYet)
                    { // Delete from species first
                        for (int i = 0; i < numToRemove; i++)
                        {
                            ns2species[ns2NoveltyArchive[i].SpeciesId].Members.Remove(ns2NoveltyArchive[i]);
                        }
                    }
                    if (!ns1) removedFromArchive.AddRange(ns2NoveltyArchive.GetRange(0, numToRemove)); // Tracking changes to the archive                    
                    ns2NoveltyArchive.RemoveRange(0, numToRemove);
                }
            }
            // 3.1. Pare down the NS1 archive (no tournament deletion, just delete the lowest novelty scores)
            if (ns1)
            {
                if (neatParameters.NS1_archiveCap > 0 && ns1NoveltyArchive.Count > neatParameters.NS1_archiveCap)
                {
                    ns1NoveltyArchive.Sort((first, second) => first.Fitness.CompareTo(second.Fitness));
                    int numToRemove = ns1NoveltyArchive.Count - neatParameters.NS1_archiveCap;
                    removedFromArchive.AddRange(ns1NoveltyArchive.GetRange(0, numToRemove)); // Tracking changes to the archive
                    ns1NoveltyArchive.RemoveRange(0, numToRemove);
                }
            }

            foreach (IGenome g in removedFromArchive) addedToArchive.Remove(g); // Tracking changes to the archive: disregard any additions that didn't survive the pruning
            
            after = DateTime.Now;
            timeSpentOutsideFitnessEval = timeSpentOutsideFitnessEval.Add(after.Subtract(before));

            before = DateTime.Now;
            // 4. Selecting parents from the archive proportional to their novelty scores, create the next generation
            List<IGenome> selectedParents = ns2RouletteSelectParents(ns2NoveltyArchive, popSizeWeAreUsing);
            foreach (IGenome parent in selectedParents)
            {
                pop.GenomeList.Add(parent.CreateOffspring_Asexual(this));
            }
            after = DateTime.Now;
            timeSpentOutsideFitnessEval = timeSpentOutsideFitnessEval.Add(after.Subtract(before));

            before = DateTime.Now;
            // 5. Evaluate the next generation
            populationEvaluator.EvaluatePopulation(pop, this);
            after = DateTime.Now;
            timeSpentInFitnessEval = timeSpentInFitnessEval.Add(after.Subtract(before));

            before = DateTime.Now;
            // 6. Calculate statistics and other generational bookeeping. 
            ns2UpdateFitnessStats();
            generation++;
            after = DateTime.Now;
            timeSpentOutsideFitnessEval = timeSpentOutsideFitnessEval.Add(after.Subtract(before));

        }
        

        public void PerformOneGeneration()
		{
            // JUSTIN: IF WE ARE DOING MAPELITES, BAIL AND DO THE MAPELITES CHAIN INSTEAD!
            if (neatParameters.mapelites)
            {
                mePerformOneGeneration();
                return;
            }

            // JUSTIN: IF WE ARE DOING NOVELTY SEARCH 2.0, BAIL AND DO THE NS2 CHAIN INSTEAD!
            if (neatParameters.NS2)
            {
                ns2PerformOneGeneration();
                return;
            }

            // JUSTIN: IF WE ARE DOING SS-NSLC, BAIL AND DO THE SS-NSLC CHAIN INSTEAD!
            if (neatParameters.NSLC)
            {
                nslcPerformOneGeneration(); // TODO: JUSTIN: IMPLEMENT THIS, OKAY.
                return;
            }


            // If we are tracking a ME-style grid, we have to insert the freshly-evaluated population ("batch") here, before any further processing.
            // Note: This takes negligible time.
            if (neatParameters.track_me_grid)
            {
                // Insert population into ME Grid
                
                foreach (IGenome g in pop.GenomeList)
                {
                    (g as AbstractGenome).GridCoords = bcToBinCoordinates(g.Behavior.behaviorList).ToArray();
                    UInt64 coord = cantorPairing(bcToBinCoordinates(g.Behavior.behaviorList));
                    if (!meGrid.ContainsKey(coord))
                    { // If this grid slot is empty, just go ahead and add the genome
                        meGrid[coord] = g;
                    }
                    else
                    { // If it's not empty, replace only if g has higher fitness
                        if (g.RealFitness > meGrid[coord].RealFitness)
                        { // It's higher fitness, good. Replace with the higher fit individual.
                            meGrid[coord] = g;
                        }
                        else
                        { // So sad

                        }
                    }
                }
            }






		//----- Elmininate any poor species before we do anything else. These are species with a zero target
		//		size for this generation and will therefore not have generate any offspring. Here we have to 
		//		explicitly eliminate these species, otherwise the species would persist because of elitism. 
		//		Also, the species object would persist without any genomes within it, so we have to clean it up.
		//		This code could be executed at the end of this method instead of the start, it doesn't really 
		//		matter. Except that If we do it here then the population size will be relatively constant
		//		between generations.

			/*if(pop.EliminateSpeciesWithZeroTargetSize())
			{	// If species were removed then we should recalculate population stats.
				UpdateFitnessStats();
				DetermineSpeciesTargetSize();
			}*/

		//----- Stage 1. Create offspring / cull old genomes / add offspring to population.
			bool regenerate = false;
			if(neatParameters.noveltySearch && neatParameters.noveltyFixed)
            {
                if((generation+1)%20==0)
                {
                    this.noveltyFixed.add_most_novel(pop);
                    this.noveltyFixed.update_measure(pop);
                    pop.ResetPopulation(noveltyFixed.measure_against,this);
                    pop.RedetermineSpeciation(this);
                    regenerate=true;
                }
            }

            /*for (int i = 0; i < pop.GenomeList.Count; i++)
            {
                Console.Write("!BehaviorList! ");
                foreach (double argh in pop.GenomeList[i].objectives)
                {
                    Console.Write(argh + " ");
                }
                Console.WriteLine();
            }//*/ // For some reason this crashes.. maybe there is no objectives list???

            /*Console.Write("localGenomeNovelty: ");
            foreach (IGenome g in pop.GenomeList) Console.Write(g.localGenomeNovelty + " ");
            Console.WriteLine();
            Console.Write("competition: ");
            foreach (IGenome g in pop.GenomeList) Console.Write(g.competition + " ");
            Console.WriteLine();
            Console.Write("nearestNeighbors: ");
            foreach (IGenome g in pop.GenomeList) Console.Write(g.nearestNeighbors + " ");
            Console.WriteLine();//*/

            
			if(neatParameters.multiobjective) {
                // JUSTIN: I hope this fixes it..
                if (neatParameters.noveltySearch)
                { // We are doing multiobjective novelty search, presumably NS+LC.
                    //Console.WriteLine("Adding in the NS+LC objectives"); // Add in the extra objectives (see comments below)
                    foreach (IGenome g in pop.GenomeList)
                    {
                        g.objectives = new double[6];
                        int len = g.objectives.Length;
                        //g.objectives[len - 5] = g.geneticDiversity;
                        g.objectives[len - 4] = g.RealFitness;
                        //g.objectives[len - 3] = g.competition / g.nearestNeighbors; // Local competition objective (based on real fitness)
                        g.objectives[len - 2] = g.Fitness + 0.001; // Novelty objective
                        //g.objectives[len - 1] = g.localGenomeNovelty / g.nearestNeighbors; // Local genetic diversity objective (does this work?)
                        //foreach (double d in g.objectives) Console.Write(d + " "); Console.WriteLine();
                    }
                    //Console.WriteLine("Completed adding NS+LC objectives with no problems. (" + pop.GenomeList.Count + " members)");
                }

                //Console.WriteLine("pop=" + pop.GenomeList.Count + "popMO=" + multiobjective.population.Count);
                /*foreach (IGenome g in multiobjective.population)
                {
                    Console.Write("MO_OldPop: ");
                    Console.Write(g.Fitness + " : [" + g.RealFitness + "] : ");
                    foreach (double d in g.objectives) Console.Write(d + " ");
                    Console.WriteLine();
                }//*/
                multiobjective.addPopulation(pop);
                //Console.WriteLine("pop2=" + pop.GenomeList.Count + "pop2MO=" + multiobjective.population.Count);
                /*foreach (IGenome g in multiobjective.population)
                {
                    Console.Write("MO_NewPop: ");
                    Console.Write(g.Fitness + " : [" + g.RealFitness + "] : ");
                    foreach (double d in g.objectives) Console.Write(d + " ");
                    Console.WriteLine();
                }//*/
                multiobjective.rankGenomes();
                //Console.WriteLine("pop3=" + pop.GenomeList.Count + "pop3MO=" + multiobjective.population.Count);
                /*foreach (IGenome g in multiobjective.population)
                {
                    Console.Write("MO_AfterRankingPop: ");
                    Console.Write(g.Fitness + " : [" + g.RealFitness + "] : ");
                    foreach (double d in g.objectives) Console.Write(d + " ");
                    Console.WriteLine();
                }//*/
                /*foreach (IGenome aye in pop.GenomeList)
                {
                    Console.Write("Before:"+ aye.Fitness + " : [" + aye.RealFitness + "] : ");
                    foreach (double d in aye.objectives) Console.Write(d + " ");
                    Console.WriteLine();
                }//*/
				pop.ResetPopulation(multiobjective.truncatePopulation(pop.GenomeList.Count),this);
                /*foreach (IGenome aye in pop.GenomeList)
                {
                    Console.Write("After:" + aye.Fitness + " : [" + aye.RealFitness + "] : ");
                    foreach (double d in aye.objectives) Console.Write(d + " ");
                    Console.WriteLine();
                }//*/
                //Console.WriteLine("pop4=" + pop.GenomeList.Count + "pop4MO=" + multiobjective.population.Count);
                /*foreach (IGenome g in multiobjective.population)
                {
                    Console.Write("MO_AfterTruncatePop: ");
                    Console.Write(g.Fitness + " : [" + g.RealFitness + "] : ");
                    foreach (double d in g.objectives) Console.Write(d + " ");
                    Console.WriteLine();
                }//*/
                /*foreach (IGenome g in pop.GenomeList)
                {
                    Console.Write("FinalPop: ");
                    foreach (double d in g.objectives) Console.Write(d + " ");
                    Console.WriteLine();
                }//*/
				pop.RedetermineSpeciation(this);
				UpdateFitnessStats();
				DetermineSpeciesTargetSize();
                //Console.WriteLine("DOES IT DO THIS? WHEN? SHOW ME!!");
			}
			
            if(!regenerate)
            {
			    CreateOffSpring();
			    pop.TrimAllSpeciesBackToElite();

                // JUSTIN: DEBUG
                /*Console.WriteLine("These are the newly created children. Are they zeroed out?");
                foreach (IGenome g in offspringList)
                {
                    Console.Write("RF:" + g.RealFitness + ", fit:" + g.Fitness + "misc:" + g.locality + " " + g.nearestNeighbors + " " + g.localGenomeNovelty + " " + g.geneticDiversity + " " + g.competition + ", objectives:");
                    foreach (double d in g.objectives) Console.Write(" " + d);
                    Console.WriteLine();
                }//*/ // They are.

			    // Add offspring to the population.
			    int genomeBound = offspringList.Count;
			    for(int genomeIdx=0; genomeIdx<genomeBound; genomeIdx++)
				    pop.AddGenomeToPopulation(this, offspringList[genomeIdx]);

                /*foreach (IGenome g in pop.GenomeList)
                {
                    Console.Write("RF:" + g.RealFitness + ", fit:" + g.Fitness + "misc:" + g.locality + " " + g.nearestNeighbors + " " + g.localGenomeNovelty + " " + g.geneticDiversity + " " + g.competition + ", objectives:");
                    //foreach (double d in g.objectives) Console.Write(" " + d);
                    Console.WriteLine();
                }//*/
            }
            
			// Adjust the speciation threshold to try and keep the number of species within defined limits.
            if (!neatParameters.multiobjective)
            {
                AdjustSpeciationThreshold();
            }

            //Console.WriteLine("pop5=" + pop.GenomeList.Count + "pop5MO=" + multiobjective.population.Count);
		//----- Stage 2. Evaluate genomes / Update stats.
            //Console.WriteLine("Before pop eval");
			populationEvaluator.EvaluatePopulation(pop, this);
            //Console.WriteLine("After pop eval");
            // JUSTIN: DEBUG
            /*Console.WriteLine("Here is the population rightafter being evaluated. Whats it look like?");
            foreach (IGenome g in pop.GenomeList)
            {
                Console.Write("RF:" + g.RealFitness + ", fit:" + g.Fitness + "misc:" + g.locality + " " + g.nearestNeighbors + " " + g.localGenomeNovelty + " " + g.geneticDiversity + " " + g.competition + ", objectives:");
                foreach (double d in g.objectives) Console.Write(" " + d);
                Console.WriteLine();
            }//*/ // It looked good. Objectives were all 0 0 0 0 0 0, everything else was defined.

            if (neatParameters.multiobjective && neatParameters.noveltySearch)
            {
                // Redefine all objectives in case they haven't been defined yet (new genomes) or the aux data changed (it probably did). This is especially important for genomes being added to the archive or the measure_against population.
                foreach (IGenome g in pop.GenomeList)
                {
                    g.objectives = new double[6];
                    int len = g.objectives.Length;
                    //g.objectives[len - 5] = g.geneticDiversity; // Global genetic diversity objective
                    g.objectives[len - 4] = g.RealFitness; // Global competition objective
                    //g.objectives[len - 3] = g.competition / g.nearestNeighbors; // Local competition objective
                    g.objectives[len - 2] = g.Fitness + 0.001; // Novelty objective
                    //g.objectives[len - 1] = g.localGenomeNovelty / g.nearestNeighbors; // Local genetic diversity objective
                    //foreach (double d in g.objectives) Console.Write(d + " "); Console.WriteLine();
                }
            }

			UpdateFitnessStats();
			DetermineSpeciesTargetSize();
			
			pop.IncrementGenomeAges();
			pop.IncrementSpeciesAges();
			generation++;


            // ------------------------------------------------------------------------------------------------------------------------
            // JUSTIN: 
            //   The following code saves some high fitness genomes at random, as long as they exceed a specified fitness threshold
            //   To use this functionality, edit the following parameters: 
            //
            //          SAVED_PER_GENERATION       FITNESS_SAVE_THRESHOLD       EMPTY_HOPPER_AT       
            //
            if (SAVETHINGS)
            {
                saveTicker += SAVED_PER_GENERATION;
                GenomeList tempOverThreshold = new GenomeList();
                foreach (IGenome g in pop.GenomeList)
                {
                    if (g.RealFitness > FITNESS_SAVE_THRESHOLD)
                    {
                        tempOverThreshold.Add(g);
                    }
                }
                totalOverFitnessThreshold += tempOverThreshold.Count;
                while (saveTicker >= 1)
                {
                    // Choose a random genome from tempOverThreshold to save... if possible.
                    if (tempOverThreshold.Count != 0)
                    {
                        //(int)Math.Floor(Utilities.NextDouble()*championSpecies.Count)
                        int pickMe = (int)(Math.Floor(Utilities.NextDouble() * tempOverThreshold.Count));
                        savedGenomeHopper.Add(tempOverThreshold[pickMe]);
                        tempOverThreshold.RemoveAt(pickMe);
                    }
                    saveTicker -= 1;
                } //NOTE: If no genomes are over the threshold, then no genomes are saved this tick. they are NOT made up later.
                // Potentially dump genomes to file (if there are enough)
                /*
                if (savedGenomeHopper.Count >= EMPTY_HOPPER_AT)
                { 
                    Console.WriteLine("Dumping high-fitness genomes. Total over threshold: " + totalOverFitnessThreshold);
                    dumpGoodGenomes();
                }
                 * */
            }
            // --------- Done dumping HF genomes. -------------------------------------------------- Done dumping HF genomes. ---------


			
            if(neatParameters.noveltySearch)
            {
                //Console.WriteLine("Archive size: " + this.noveltyFixed.archive.Count.ToString());
                //Console.WriteLine("MO Archive size: " + multiobjective.nov.archive.Count.ToString()); //JUSTIN: The MO archive doesn't grow... maybe call it below... or just don't use it at all...
            }
            
            if(neatParameters.noveltySearch && neatParameters.noveltyFloat)
            {
                //Console.WriteLine("I BET THIS HAPPENS: OPTION A.");
                this.noveltyFixed.initialize(pop);   
                this.noveltyFixed.addPending();
            }
            
            if(neatParameters.noveltySearch && neatParameters.noveltyFixed)
            {
                //Console.WriteLine("DOES THIS HAPPEN??????????????? NAH PROLLY NOT");
                this.noveltyFixed.addPending();
            }

            /*Console.WriteLine("Here's the measure_against. " + noveltyFixed.measure_against.Count);
            foreach (IGenome g in noveltyFixed.measure_against)
            {
                Console.Write("RF:" + g.RealFitness + ", fit:" + g.Fitness + "misc:" + g.locality + " " + g.nearestNeighbors + " " + g.localGenomeNovelty + " " + g.geneticDiversity + " " + g.competition + ", objectives:");
                foreach (double d in g.objectives) Console.Write(" " + d);
                Console.WriteLine();
            }
            Console.WriteLine("=================================== End measure_against");
            Console.WriteLine("Here's the archive. " + noveltyFixed.archive.Count);
            foreach (IGenome g in noveltyFixed.archive)
            {
                Console.Write("RF:" + g.RealFitness + ", fit:" + g.Fitness + "misc:" + g.locality + " " + g.nearestNeighbors + " " + g.localGenomeNovelty + " " + g.geneticDiversity + " " + g.competition + ", objectives:");
                foreach (double d in g.objectives) Console.Write(" " + d);
                Console.WriteLine();
            }
            Console.WriteLine("=================================== End archive");//*/ //JUSTIN: I THINK IT'S WORKING! FUUUUUUUUUUUU-N!

            
		//----- Stage 3. Pruning phase tracking / Pruning phase entry & exit.
			if(pruningModeEnabled)
			{
				if(pruningMode)
				{
					// Track the falling population complexity.
					if(pop.AvgComplexity < prunePhase_MinimumStructuresPerGenome)
					{
						prunePhase_MinimumStructuresPerGenome = pop.AvgComplexity;
						prunePhase_generationAtLastSimplification = generation;
					}

					if(TestForPruningPhaseEnd())
						EndPruningPhase();
				}
				else
				{
					if(TestForPruningPhaseBegin())
						BeginPruningPhase();
				}
			}
		}

        

		/// <summary>
		/// Indicates that the # of species is outside of the desired bounds and that AdjustSpeciationThreshold()
		/// is attempting to adjust the speciation threshold at each generation to remedy the situation.
		/// </summary>
		private bool speciationThresholdAdjustInProgress=false;

		/// <summary>
		/// If speciationThresholdAdjustInProgress is true then the amount by which we are adjustinf the speciation
		/// threshol dper generation. This value is modified in order to try and find the correct threshold as quickly
		/// as possibly.
		/// </summary>
		private double compatibilityThresholdDelta;

		private const double compatibilityThresholdDeltaAcceleration = 1.05;

		

		private void AdjustSpeciationThreshold()
		{
			bool redetermineSpeciationFlag = false;
			int speciesCount = pop.SpeciesTable.Count;

			if(speciesCount < neatParameters.targetSpeciesCountMin)
			{	
				// Too few species. Reduce the speciation threshold.
				if(speciationThresholdAdjustInProgress)
				{	// Adjustment is already in progress.
					if(compatibilityThresholdDelta<0.0)
					{	// Negative delta. Correct direction, so just increase the delta to try and find the correct value as quickly as possible.
						compatibilityThresholdDelta*=compatibilityThresholdDeltaAcceleration;
					}
					else
					{	// Positive delta. Incorrect direction. This means we have overshot the correct value.
						// Reduce the delta and flip its sign.
						compatibilityThresholdDelta*=-0.5;
					}
				}
				else
				{	// Start new adjustment 'phase'.
					speciationThresholdAdjustInProgress = true;
					compatibilityThresholdDelta = -Math.Max(0.1, neatParameters.compatibilityThreshold * 0.01);
				}

				// Adjust speciation threshold by compatibilityThresholdDelta.
				neatParameters.compatibilityThreshold += compatibilityThresholdDelta;
				neatParameters.compatibilityThreshold = Math.Max(0.01, neatParameters.compatibilityThreshold);

				redetermineSpeciationFlag = true;
			}
			else if(speciesCount > neatParameters.targetSpeciesCountMax)
			{	
				// Too many species. Increase the species threshold.
				if(speciationThresholdAdjustInProgress)
				{	// Adjustment is already in progress.
					if(compatibilityThresholdDelta<0.0)
					{	// Negative delta. Incorrect direction. This means we have overshot the correct value.
						// Reduce the delta and flip its sign.
						compatibilityThresholdDelta*=-0.5;
					}
					else
					{	// Positive delta. Correct direction, so just increase the delta to try and find the correct value as quickly as possible.
						compatibilityThresholdDelta*=compatibilityThresholdDeltaAcceleration;
					}
				}
				else
				{	// Start new adjustment 'phase'.
					speciationThresholdAdjustInProgress = true;
					compatibilityThresholdDelta = Math.Max(0.1, neatParameters.compatibilityThreshold * 0.01);
				}

				// Adjust speciation threshold by compatibilityThresholdDelta.
				neatParameters.compatibilityThreshold += compatibilityThresholdDelta;

				redetermineSpeciationFlag = true;
			}
			else
			{	// Correct # of species. Ensure flag is reset.
				speciationThresholdAdjustInProgress=false;
			}

			if(!redetermineSpeciationFlag)
			{
				double complexityDeltaProportion = Math.Abs(pop.AvgComplexity-meanComplexityAtLastAdjustSpeciationThreshold)/meanComplexityAtLastAdjustSpeciationThreshold; 

				if(complexityDeltaProportion>0.05)
				{	// If the population's complexity has changed by more than some proportion then force a 
					// call to RedetermineSpeciation().
					redetermineSpeciationFlag = true;

					// Update the tracking variable.
					meanComplexityAtLastAdjustSpeciationThreshold = pop.AvgComplexity;
				}
			}

			if(redetermineSpeciationFlag)
			{
				// If the speciation threshold was adjusted then we must disregard all previous speciation 
				// and rebuild the species table.
				pop.RedetermineSpeciation(this);

				// If we are in a pruning phase then we should reset the pruning phase tracking variables.
				// We are effectively re-starting the pruning phase.
				prunePhase_generationAtLastSimplification = generation;
				prunePhase_MinimumStructuresPerGenome = pop.AvgComplexity;

				Debug.WriteLine("ad hoc RedetermineSpeciation()");
			}
		}

//		/// <summary>
//		/// Returns true if the speciation threshold was adjusted.
//		/// </summary>
//		/// <returns></returns>
//		private bool AdjustSpeciationThreshold()
//		{
//			int speciesCount = pop.SpeciesTable.Count;
//
//			if(speciesCount < neatParameters.targetSpeciesCountMin)
//			{	
//				// Too few species. Reduce the speciation threshold.
//				if(speciationThresholdAdjustInProgress)
//				{	// Adjustment is already in progress.
//					if(compatibilityThresholdDelta<0.0)
//					{	// Negative delta. Correct direction, so just increase the delta to try and find the correct value as quickly as possible.
//						compatibilityThresholdDelta*=compatibilityThresholdDeltaAcceleration;
//					}
//					else
//					{	// Positive delta. Incorrect direction. This means we have overshot the correct value.
//						// Reduce the delta and flip its sign.
//						compatibilityThresholdDelta*=-0.5;
//					}
//				}
//				else
//				{	// Start new adjustment 'phase'.
//					speciationThresholdAdjustInProgress = true;
//					compatibilityThresholdDelta = -Math.Max(0.1, neatParameters.compatibilityThreshold * 0.01);
//				}
//
//				// Adjust speciation threshold by compatibilityThresholdDelta.
//				neatParameters.compatibilityThreshold += compatibilityThresholdDelta;
//				neatParameters.compatibilityThreshold = Math.Max(0.01, neatParameters.compatibilityThreshold);
//
//				Debug.WriteLine("delta=" + compatibilityThresholdDelta);
//
//				return true;
//			}
//			else if(speciesCount > neatParameters.targetSpeciesCountMax)
//			{	
//				// Too many species. Increase the species threshold.
//				if(speciationThresholdAdjustInProgress)
//				{	// Adjustment is already in progress.
//					if(compatibilityThresholdDelta<0.0)
//					{	// Negative delta. Incorrect direction. This means we have overshot the correct value.
//						// Reduce the delta and flip its sign.
//						compatibilityThresholdDelta*=-0.5;
//					}
//					else
//					{	// Positive delta. Correct direction, so just increase the delta to try and find the correct value as quickly as possible.
//						compatibilityThresholdDelta*=compatibilityThresholdDeltaAcceleration;
//					}
//				}
//				else
//				{	// Start new adjustment 'phase'.
//					speciationThresholdAdjustInProgress = true;
//					compatibilityThresholdDelta = Math.Max(0.1, neatParameters.compatibilityThreshold * 0.01);
//				}
//
//				// Adjust speciation threshold by compatibilityThresholdDelta.
//				neatParameters.compatibilityThreshold += compatibilityThresholdDelta;
//
//				Debug.WriteLine("delta=" + compatibilityThresholdDelta);
//
//				return true;
//			}
//			else
//			{	// Correct # of species. Ensure flag is reset.
//				speciationThresholdAdjustInProgress=false;
//				return false;
//			}
//		}

//		private const double compatibilityThresholdDeltaBaseline = 0.1;
//		private const double compatibilityThresholdDeltaAcceleration = 1.5;
//
//		private double compatibilityThresholdDelta = compatibilityThresholdDeltaBaseline;
//		private bool compatibilityThresholdDeltaDirection=true;
//		
//		/// <summary>
//		/// This routine adjusts the speciation threshold so that the number of species remains between the specified upper 
//		/// and lower limits. This routine implements a momentum approach so that the rate of change in the threshold increases
//		/// if the number of species remains incorrect for consecutive invocations.
//		/// </summary>
//		private void AdjustSpeciationThreshold()
//		{
//			double newThreshold;
//
//			if(pop.SpeciesTable.Count < neatParameters.targetSpeciesCountMin)
//			{
//				newThreshold = Math.Max(compatibilityThresholdDeltaBaseline, neatParameters.compatibilityThreshold - compatibilityThresholdDelta);
//
//				// Delta acceleration.
//				if(compatibilityThresholdDeltaDirection)
//				{	// Wrong direction - Direction change. Also reset compatibilityThresholdDelta.
//					compatibilityThresholdDelta = compatibilityThresholdDeltaBaseline;
//					compatibilityThresholdDeltaDirection=false;
//				}
//				else
//				{	// Already going in the right direction. 
//					compatibilityThresholdDelta *= compatibilityThresholdDeltaAcceleration;
//				}				
//			}
//			else if(pop.SpeciesTable.Count > neatParameters.targetSpeciesCountMax)
//			{
//				newThreshold = neatParameters.compatibilityThreshold + compatibilityThresholdDelta;
//
//				// Delta acceleration.
//				if(compatibilityThresholdDeltaDirection)
//				{	// Already going in the right direction. 
//					compatibilityThresholdDelta *= compatibilityThresholdDeltaAcceleration;
//				}
//				else
//				{	// Wrong direction - Direction change. Also reset compatibilityThresholdDelta.
//					compatibilityThresholdDelta = compatibilityThresholdDeltaBaseline;
//					compatibilityThresholdDeltaDirection=true;
//				}
//			}
//			else
//			{	// Current threshold is OK. Reset compatibilityThresholdDelta in case it has 'accelerated' to a large value.
//				// This would be a bad value to start with when the threshold next needs adjustment.
//				compatibilityThresholdDelta = compatibilityThresholdDeltaBaseline;
//				return;
//			}
//
//			neatParameters.compatibilityThreshold = newThreshold;
//
//			// If the speciation threshold was adjusted then we must disregard all previous speciation 
//			// and rebuild the species table.
//			pop.RedetermineSpeciation(this);
//		}

		#endregion

		#region Private Methods

		private void CreateOffSpring()
		{
			offspringList.Clear();
			CreateOffSpring_Asexual();
			CreateOffSpring_Sexual();
		}

		private void CreateOffSpring_Asexual()
		{
			// Create a new lists so that we can track which connections/neurons have been added during this routine.
			newConnectionGeneTable.Clear(); 
			newNeuronGeneStructTable.Clear();

			//----- Repeat the reproduction per species to give each species a fair chance at reproducion.
			//		Note that for this to work for small numbers of genomes in a species we need a reproduction 
			//		rate of 100% or more. This is analagous to the strategy used in NEAT.
			foreach(Species species in pop.SpeciesTable.Values)
			{
				// Determine how many asexual offspring to create. 
				// Minimum of 1. Any species with TargetSize of 0 are eliminated at the top of PerformOneGeneration(). This copes with the 
				// special case where every species may calculate offspringCount to be zero and therefor we loose the entire population!
				// This can happen e.g. when each genome is allocated it's own species with TargetSize of 1.
				int offspringCount = Math.Max(1,(int)Math.Round((species.TargetSize - species.ElitistSize) * neatParameters.pOffspringAsexual));
				for(int i=0; i<offspringCount; i++)
				{	// Add offspring to a seperate genomeList. We will add the offspring later to prevent corruption of the enumeration loop.
					IGenome parent=null;
					if(!neatParameters.multiobjective)
					 parent = RouletteWheelSelect(species);
					else
					 parent = TournamentSelect(species);
                    //Console.WriteLine("multiobjectiveINCREATEOFF=" + neatParameters.multiobjective);
                    if (parent == null)
                        continue;
					IGenome offspring = parent.CreateOffspring_Asexual(this);
					offspring.ParentSpeciesId1 = parent.SpeciesId;
					offspringList.Add(offspring);
				}
			}
//			AmalgamateInnovations();
		}

//		/// <summary>
//		/// Mutations can sometime create the same innovation more than once within a population.
//		/// If this occurs then we ensure like innovations are allocated the same innovation ID.
//		/// This is for this generation only - if the innovation occurs in a later generation we
//		/// leave it as it is.
//		/// </summary>
//		private void AmalgamateInnovations()
//		{
//			// TODO: Inefficient routine. Revise.
//			// Indicates that at least one list's order has been invalidated.
//			bool bOrderInvalidated=false;
//
//			// Check through the new NeuronGenes - and their associated connections.
//			int neuronListBound = newNeuronGeneStructList.Count;
//			for(int i=0; i<neuronListBound-1; i++)
//			{
//				for(int j=i+1; j<neuronListBound; j++)
//				{
//					NewNeuronGeneStruct neuronGeneStruct1 = (NewNeuronGeneStruct)newNeuronGeneStructList[i];
//					NewNeuronGeneStruct neuronGeneStruct2 = (NewNeuronGeneStruct)newNeuronGeneStructList[j];
//
//					if(neuronGeneStruct1.NewConnectionGene_Input.SourceNeuronId == neuronGeneStruct2.NewConnectionGene_Input.SourceNeuronId &&
//						neuronGeneStruct1.NewConnectionGene_Output.TargetNeuronId == neuronGeneStruct2.NewConnectionGene_Output.TargetNeuronId)
//					{
//						neuronGeneStruct2.NewNeuronGene.InnovationId = neuronGeneStruct1.NewNeuronGene.InnovationId;
//						neuronGeneStruct2.NewConnectionGene_Input.InnovationId = neuronGeneStruct1.NewConnectionGene_Input.InnovationId;
//						neuronGeneStruct2.NewConnectionGene_Input.TargetNeuronId = neuronGeneStruct2.NewNeuronGene.InnovationId;
//
//						neuronGeneStruct2.NewConnectionGene_Output.InnovationId = neuronGeneStruct1.NewConnectionGene_Output.InnovationId;
//						neuronGeneStruct2.NewConnectionGene_Output.SourceNeuronId = neuronGeneStruct2.NewNeuronGene.InnovationId;
//
//						// Switching innovation numbers over can cause the genes to be out of order with respect
//						// to their innovation id. This order should be maintained at all times, so we set a flag here
//						// and re-order all effected lists at the end of this method.
//						neuronGeneStruct2.OwningGenome.NeuronGeneList.OrderInvalidated = true;
//						neuronGeneStruct2.OwningGenome.ConnectionGeneList.OrderInvalidated = true;
//						bOrderInvalidated = true;
//					}
//				}
//			}
//
//			// Check through the new connections.
//			int connectionListBound = newConnectionGeneStructList.Count;
//			for(int i=0; i<connectionListBound-1; i++)
//			{
//				for(int j=i+1; j<connectionListBound; j++)
//				{
//					NewConnectionGeneStruct connectionGeneStruct1 = (NewConnectionGeneStruct)newConnectionGeneStructList[i];
//					NewConnectionGeneStruct connectionGeneStruct2 = (NewConnectionGeneStruct)newConnectionGeneStructList[j];
//
//					if(connectionGeneStruct1.NewConnectionGene.SourceNeuronId == connectionGeneStruct2.NewConnectionGene.SourceNeuronId && 
//						connectionGeneStruct1.NewConnectionGene.TargetNeuronId == connectionGeneStruct2.NewConnectionGene.TargetNeuronId)
//					{
//						connectionGeneStruct2.NewConnectionGene.InnovationId = connectionGeneStruct1.NewConnectionGene.InnovationId;
//						connectionGeneStruct2.OwningGenome.ConnectionGeneList.OrderInvalidated = true;
//						bOrderInvalidated = true;
//					}
//				}
//			}
//
//			if(bOrderInvalidated)
//			{	// Re-order all invalidated lists within the population.
//				foreach(NeatGenome.NeatGenome genome in offspringList)
//				{
//					if(genome.NeuronGeneList.OrderInvalidated)
//						genome.NeuronGeneList.SortByInnovationId();
//
//					if(genome.ConnectionGeneList.OrderInvalidated)
//						genome.ConnectionGeneList.SortByInnovationId();
//				}
//			}
//		}

		//TODO: review this routine. parent could be null?
		private void CreateOffSpring_Sexual()
		{
			//----- Repeat the reproduction per species to give each species a fair chance at reproducion.
			//		Note that for this to work for small numbers of genomes in a species we need a reproduction 
			//		rate of 100% or more. This is analagous to the strategy used in NEAT.
			foreach(Species species in pop.SpeciesTable.Values)
			{
				bool oneMember=false;
				bool twoMembers=false;

				if(species.Members.Count==1)
				{
					// We can't perform sexual reproduction. To give the species a fair chance we call the asexual routine instead.
					// This keeps the proportions of genomes per species steady.
					oneMember = true;
				} 
				else if(species.Members.Count==2)
					twoMembers = true;			
	
				// Determine how many sexual offspring to create. 
				int matingCount = (int)Math.Round((species.TargetSize - species.ElitistSize) * neatParameters.pOffspringSexual);
				for(int i=0; i<matingCount; i++)
				{
					IGenome parent1;
					IGenome parent2=null;
					IGenome offspring;

					if(Utilities.NextDouble() < neatParameters.pInterspeciesMating)
					{	// Inter-species mating!
						//System.Diagnostics.Debug.WriteLine("Inter-species mating!");
						if(oneMember)
							parent1 = species.Members[0];
						else  {
							if(!neatParameters.multiobjective)
							parent1 = RouletteWheelSelect(species);
							else
							parent1 = TournamentSelect(species);
						}
						// Select the 2nd parent from the whole popualtion (there is a chance that this will be an genome 
						// from this species, but that's OK).

						int j=0;
						do
						{
							if(!neatParameters.multiobjective)
							parent2 = RouletteWheelSelect(pop);
							else
							parent2 = TournamentSelect(pop);
						}
						while(parent1==parent2 && j++ < 4);	// Slightly wasteful but not too bad. Limited by j.	
					}
					else
					{	// Mating within the current species.
						//System.Diagnostics.Debug.WriteLine("Mating within the current species.");
						if(oneMember)
						{	// Use asexual reproduction instead.
							offspring = species.Members[0].CreateOffspring_Asexual(this);
							offspring.ParentSpeciesId1 = species.SpeciesId;
							offspringList.Add(offspring);
							continue;
						}

						if(twoMembers)
						{
							offspring = species.Members[0].CreateOffspring_Sexual(this, species.Members[1]);
							offspring.ParentSpeciesId1 = species.SpeciesId;
							offspring.ParentSpeciesId2 = species.SpeciesId;
							offspringList.Add(offspring);
							continue;
						}
						
						if(!neatParameters.multiobjective)
						parent1 = RouletteWheelSelect(species);
						else
						parent1 = TournamentSelect(species);
						
						int j=0;
						do
						{
							if(!neatParameters.multiobjective)
							parent2 = RouletteWheelSelect(species);
							else
							parent2 = TournamentSelect(species);
						}
						while(parent1==parent2 && j++ < 4);	// Slightly wasteful but not too bad. Limited by j.						
					}

					if(parent1 != parent2)
					{
						offspring = parent1.CreateOffspring_Sexual(this, parent2);
						offspring.ParentSpeciesId1 = parent1.SpeciesId;
						offspring.ParentSpeciesId2 = parent2.SpeciesId;
						offspringList.Add(offspring);
					}
					else
					{	// No mating pair could be found. Fallback to asexual reproduction to keep the population size constant.
						offspring = parent1.CreateOffspring_Asexual(this);
						offspring.ParentSpeciesId1 = parent1.SpeciesId;
						offspringList.Add(offspring);
					}
				}
			}
		}

		/// <summary>
		/// Biased select.
		/// </summary>
		/// <param name="species">Species to select from.</param>
		/// <returns></returns>
		private IGenome TournamentSelect(Species species) {
			double bestFound= 0.0;
			IGenome bestGenome=null;
			int bound = species.Members.Count;
			for(int i=0;i<neatParameters.tournamentSize;i++) {
				IGenome next= species.Members[Utilities.Next(bound)];
				if (next.Fitness > bestFound) {
					bestFound=next.Fitness;
					bestGenome=next;
				}
			}
			return bestGenome;
		}
		
		private IGenome RouletteWheelSelect(Species species)
		{
			double selectValue = (Utilities.NextDouble() * species.SelectionCountTotalFitness);
			double accumulator=0.0;

			int genomeBound = species.Members.Count;
			for(int genomeIdx=0; genomeIdx<genomeBound; genomeIdx++)
			{
				IGenome genome = species.Members[genomeIdx];

				accumulator += genome.Fitness;
				if(selectValue <= accumulator)
					return genome;
			}
			// Should never reach here.
			return null;
		}

//		private IGenome EvenDistributionSelect(Species species)
//		{
//			return species.Members[Utilities.Next(species.SelectionCount)];
//		}

		private IGenome TournamentSelect(Population p) {
			double bestFound= 0.0;
			IGenome bestGenome=null;
			int bound = p.GenomeList.Count;
			for(int i=0;i<neatParameters.tournamentSize;i++) {
				IGenome next= p.GenomeList[Utilities.Next(bound)];
				if (next.Fitness > bestFound) {
					bestFound=next.Fitness;
					bestGenome=next;
				}
			}
			return bestGenome;
		}
		
		/// <summary>
		/// Biased select.
		/// </summary>
		/// <param name="species">Species to select from.</param>
		/// <returns></returns>
		private IGenome RouletteWheelSelect(Population p)
		{
			double selectValue = (Utilities.NextDouble() * p.SelectionTotalFitness);
			double accumulator=0.0;

			int genomeBound = p.GenomeList.Count;
			for(int genomeIdx=0; genomeIdx<genomeBound;genomeIdx++)
			{
				IGenome genome = p.GenomeList[genomeIdx];

				accumulator += genome.Fitness;
				if(selectValue <= accumulator)
					return genome;
			}
			// Should never reach here.
			return null;
		}

        private void ns2UpdateFitnessStats()
        {
            if (bestGenome == null) bestGenome = pop.GenomeList[0]; // should only happen once

            // Check to see if one of the children (pop members) is the best genome so far
            foreach (IGenome g in pop.GenomeList)
            {
                if (g.RealFitness > bestGenome.RealFitness)
                {
                    bestGenome = g;
                }
            }

            // Calculate archive statistics
            averageFitness = 0.0;
            averageNovelty = 0.0;
            avgComplexity = 0;
            stdevComplexity = 0;
            minComplexity = double.MaxValue;
            maxComplexity = double.MinValue;
            double tempComplexity;
            List<double> allComplexities = new List<double>();
            
            foreach (IGenome g in ns2NoveltyArchive)
            {
                averageFitness += g.RealFitness;
                averageNovelty += g.FitnessBeforeNormalization;
                tempComplexity = (g as SharpNeatLib.NeatGenome.NeatGenome).ConnectionGeneList.Count;
                allComplexities.Add(tempComplexity); // we need to keep a list so we can calc stdev in a moment..
                avgComplexity += tempComplexity;
                if (tempComplexity > maxComplexity) maxComplexity = tempComplexity;
                if (tempComplexity < minComplexity) minComplexity = tempComplexity;
            }
            averageFitness /= ns2NoveltyArchive.Count;
            averageNovelty /= ns2NoveltyArchive.Count;
            archiveSize = ns2NoveltyArchive.Count;
            avgComplexity /= ns2NoveltyArchive.Count;

            // Calculate the stdev for complexity
            foreach (double d in allComplexities)
            {
                stdevComplexity += Math.Pow((avgComplexity - d), 2);
            }
            stdevComplexity /= allComplexities.Count;
            stdevComplexity = Math.Sqrt(stdevComplexity);

            if (ns1) // NS1 has to additional keep tabs on it's auxiliary archive
            {
                ns1archiveAverageNovelty = 0.0;
                foreach (IGenome g in ns1NoveltyArchive)
                {
                    ns1archiveAverageNovelty += g.FitnessBeforeNormalization;
                }
                ns1archiveAverageNovelty /= ns1NoveltyArchive.Count;
                archiveSize = ns1NoveltyArchive.Count;
            }
            

            if (NeatParameters.track_me_grid)
            {
                // Calculate the grid statistics
                numFilledBins = meGrid.Count; // update the number of grid bins that are filled
                IGenome[] gridmembers = new IGenome[meGrid.Count];
                meGrid.Values.CopyTo(gridmembers, 0);
                double avg = 0;
                foreach (IGenome g in gridmembers)
                {
                    avg += g.RealFitness;
                }
                avg /= meGrid.Count;
                gridAverageFitness = avg; // update the average fitness of the grid
            }

            // Calculate other statistics
            numEvaluations += (uint)(pop.GenomeList.Count);
            generationBase500 = numEvaluations / 500.0;

            // Calculate species statistics
            speciesSizes.Clear();
            totalSpeciesSize = 0;
            foreach (Species s in ns2species)
            {
                speciesSizes.Add(s.Members.Count);
                totalSpeciesSize += s.Members.Count;
                s.MeanFitness = 0;
                s.TotalFitness = 0;
                foreach (IGenome g in s.Members)
                {
                    s.TotalFitness += g.FitnessBeforeNormalization;
                }
                s.MeanFitness = s.Members.Count > 0 ? (s.TotalFitness / s.Members.Count) : 0;
            }
            //speciesSizes.Sort();
        }

        // Without all the frills and species hullabaloo
        private void meUpdateFitnessStats()
        {
            if (bestGenome == null) bestGenome = pop.GenomeList[0]; // should only happen once

            // Check to see if one of the children (pop members) is the best genome so far
            foreach (IGenome g in pop.GenomeList)
            {
                if (g.RealFitness > bestGenome.RealFitness)
                {
                    bestGenome = g;
                }
            }

            // Calculate the grid statistics
            numFilledBins = meGrid.Count; // update the number of grid bins that are filled
            IGenome[] gridmembers = new IGenome[meGrid.Count];
            meGrid.Values.CopyTo(gridmembers, 0);
            double avg = 0;
            avgComplexity = 0;
            stdevComplexity = 0;
            minComplexity = double.MaxValue;
            maxComplexity = double.MinValue;
            double tempComplexity;
            List<double> allComplexities = new List<double>();
            foreach (IGenome g in gridmembers)
            {
                avg += g.RealFitness;
                tempComplexity = (g as SharpNeatLib.NeatGenome.NeatGenome).ConnectionGeneList.Count;
                allComplexities.Add(tempComplexity); // we need to keep a list so we can calc stdev in a moment..
                avgComplexity += tempComplexity;
                if (tempComplexity > maxComplexity) maxComplexity = tempComplexity;
                if (tempComplexity < minComplexity) minComplexity = tempComplexity;
            }
            avg /= meGrid.Count;
            averageFitness = avg; // update the average fitness of the grid
            gridAverageFitness = avg; // update the average fitness of the grid
            avgComplexity /= meGrid.Count;

            // Calculate other statistics
            numEvaluations += (uint)(pop.GenomeList.Count);
            generationBase500 = numEvaluations / 500.0;

            // Calculate the stdev for complexity
            foreach (double d in allComplexities)
            {
                stdevComplexity += Math.Pow((avgComplexity - d), 2);
            }
            stdevComplexity /= allComplexities.Count;
            stdevComplexity = Math.Sqrt(stdevComplexity);


        }

		private void UpdateFitnessStats()
		{
			/// Indicates if the Candidate CullFlag has been set on any of the species in the first loop.
			bool bCandidateCullFlag=false;
			double bestFitness=double.MinValue;
            double bestRealFitness = double.MinValue;

			//----- Reset the population fitness values
			pop.ResetFitnessValues();
			pop.TotalNeuronCount = 0;
			pop.TotalConnectionCount = 0;

            List<double> allComplexities = new List<double>(); // JUSTIN -- need to keep track of complexities as we encounter them so we can calc stdev at the end..
            minComplexity = double.MaxValue; // also keep track of min/max :)
            maxComplexity = double.MinValue;

			//----- Loop through the speciesTable so that we can re-calculate fitness totals
			foreach(Species species in pop.SpeciesTable.Values)
			{
				species.ResetFitnessValues();
				species.TotalNeuronCount = 0;
				species.TotalConnectionCount = 0;

				// Members must be sorted so that we can calculate the fitness of the top few genomes
				// for the selection routines.
				species.Members.Sort();

				// Keep track of the population's best genome and max fitness.
				NeatGenome.NeatGenome fittestgenome = (NeatGenome.NeatGenome)(species.Members[0]);
				if(fittestgenome.Fitness > bestFitness)
				{
				    bestFitness = fittestgenome.Fitness;
				    bestGenome = fittestgenome;
				}
				
				if(this.neatParameters.noveltySearch)
				{
				  for(int x=1;x<species.Members.Count;x++)
				    {
                        if(((NeatGenome.NeatGenome)(species.Members[x])).RealFitness > fittestgenome.RealFitness)
                        {
                            fittestgenome = (NeatGenome.NeatGenome) species.Members[x];
                        }
				    }
				    
				    if(fittestgenome.RealFitness > bestRealFitness)
				    {   
					    bestGenome = fittestgenome;
					    bestRealFitness = bestGenome.RealFitness;
				    }
				}

				
                NeatGenome.NeatGenome genome = (NeatGenome.NeatGenome)(species.Members[0]);
				
				// Track the generation number when the species improves.
				if(genome.Fitness > species.MaxFitnessEver)
				{
					species.MaxFitnessEver = genome.Fitness;
					species.AgeAtLastImprovement = species.SpeciesAge;
				}
				else if(!pruningMode && (species.SpeciesAge-species.AgeAtLastImprovement > neatParameters.speciesDropoffAge))  
				{	// The species is a candidate for culling. It may be given a pardon (later) if it is a champion species.
					species.CullCandidateFlag=true;
					bCandidateCullFlag=true;
				}

				//----- Update species totals in this first loop.
				// Calculate and store the number of genomes that will be selected from.
				species.SelectionCount = (int)Math.Max(1.0, Math.Round((double)species.Members.Count * neatParameters.selectionProportion));
				species.SelectionCountTotalFitness = 0.0;

				int genomeBound = species.Members.Count;
				for(int genomeIdx=0; genomeIdx<genomeBound;genomeIdx++)
				{
					genome = (NeatGenome.NeatGenome)(species.Members[genomeIdx]);
					//DAVID: fitness very small after novelty?
                    //Debug.Assert(genome.Fitness>=EvolutionAlgorithm.MIN_GENOME_FITNESS, "Genome fitness must be non-zero. Use EvolutionAlgorithm.MIN_GENOME_FITNESS");
					species.TotalFitness += genome.Fitness;

					if(genomeIdx < species.SelectionCount)
						species.SelectionCountTotalFitness += genome.Fitness;

					species.TotalNeuronCount += genome.NeuronGeneList.Count;
					species.TotalConnectionCount += genome.ConnectionGeneList.Count;
                    // JUSTIN: new complexity tracking stuff next 3 lines
                    allComplexities.Add(genome.ConnectionGeneList.Count);
                    if (genome.ConnectionGeneList.Count > maxComplexity) maxComplexity = genome.ConnectionGeneList.Count;
                    if (genome.ConnectionGeneList.Count < minComplexity) minComplexity = genome.ConnectionGeneList.Count;
				}

				species.TotalStructureCount = species.TotalNeuronCount + species.TotalConnectionCount;
			}

			// If any species have had their CullCandidateFlag set then we need to execute some extra logic
			// to ensure we don't cull a champion species if it is the only champion species. 
			// If there is more than one champion species and all of them have the CullCandidateFlag set then
			// we unset the flag on one of them. Therefore we always at least one champion species in the 
			// population.
			if(bCandidateCullFlag)
			{
				ArrayList championSpecies = new ArrayList();

				//----- 2nd loop through species. Build list of champion species.
				foreach(Species species in pop.SpeciesTable.Values)
				{
					if(species.Members[0].Fitness == bestFitness)
						championSpecies.Add(species);
				}
				Debug.Assert(championSpecies.Count>0, "No champion species! There should be at least one.");

				if(championSpecies.Count==1)
				{	
					Species species = (Species)championSpecies[0];
					if(species.CullCandidateFlag==true)
					{
						species.CullCandidateFlag = false;

						// Also reset the species AgeAtLastImprovement so that it doesn't become 
						// a cull candidate every generation, which would inefficiently invoke this
						// extra logic on every generation.
						species.AgeAtLastImprovement=species.SpeciesAge;
					}
				}
				else
				{	// There are multiple champion species. Check for special case where all champions
					// are cull candidates.
					bool bAllChampionsAreCullCandidates = true; // default to true.
					foreach(Species species in championSpecies)
					{
						if(species.CullCandidateFlag)
							continue;

						bAllChampionsAreCullCandidates=false;
						break;
					}

					if(bAllChampionsAreCullCandidates)
					{	// Unset the flag on one of the champions at random.
						Species champ = (Species)championSpecies[(int)Math.Floor(Utilities.NextDouble()*championSpecies.Count)];
						champ.CullCandidateFlag = false;

						// Also reset the species AgeAtLastImprovement so that it doesn't become 
						// a cull candidate every generation, which would inefficiently invoke this
						// extra logic on every generation.
						champ.AgeAtLastImprovement=champ.SpeciesAge;
					}
				}
			}

			//----- 3rd loop through species. Update remaining stats.
			foreach(Species species in pop.SpeciesTable.Values)
			{
				const double MEAN_FITNESS_ADJUSTMENT_FACTOR = 0.01;

				if(species.CullCandidateFlag)
					species.MeanFitness = (species.TotalFitness / species.Members.Count) * MEAN_FITNESS_ADJUSTMENT_FACTOR;
				else
					species.MeanFitness = species.TotalFitness / species.Members.Count;

				//----- Update population totals.
				pop.TotalFitness += species.TotalFitness;
				pop.TotalSpeciesMeanFitness += species.MeanFitness;
				pop.SelectionTotalFitness += species.SelectionCountTotalFitness;
				pop.TotalNeuronCount += species.TotalNeuronCount;
				pop.TotalConnectionCount += species.TotalConnectionCount;
			}
			
			//----- Update some population stats /averages.
			if(bestFitness > pop.MaxFitnessEver)
			{
				Debug.WriteLine("UpdateStats() - bestFitness=" + bestGenome.Fitness.ToString() + ", " + bestFitness.ToString());
				pop.MaxFitnessEver = bestGenome.Fitness;
				pop.GenerationAtLastImprovement = this.generation;
			}

			pop.MeanFitness = pop.TotalFitness / pop.GenomeList.Count;
			pop.TotalStructureCount = pop.TotalNeuronCount + pop.TotalConnectionCount;
			pop.AvgComplexity = (float)pop.TotalStructureCount / (float)pop.GenomeList.Count;



            if (NeatParameters.track_me_grid)
            {
                // Calculate the grid statistics
                numFilledBins = meGrid.Count; // update the number of grid bins that are filled
                IGenome[] gridmembers = new IGenome[meGrid.Count];
                meGrid.Values.CopyTo(gridmembers, 0);
                double avg = 0;
                foreach (IGenome g in gridmembers)
                {
                    avg += g.RealFitness;
                }
                avg /= meGrid.Count;
                gridAverageFitness = avg; // update the average fitness of the grid
            }

            // Calculate other statistics
            numEvaluations += (uint)(pop.GenomeList.Count);
            generationBase500 = numEvaluations / 500.0;

            // Calculate the stdev for complexity
            avgComplexity = pop.AvgComplexity;
            foreach (double d in allComplexities)
            {
                stdevComplexity += Math.Pow((avgComplexity - d), 2);
            }
            stdevComplexity /= allComplexities.Count;
            stdevComplexity = Math.Sqrt(stdevComplexity);


		}

		/// <summary>
		/// Determine the target size of each species based upon the current fitness stats. The target size
		/// is stored against each Species object.
		/// </summary>
		/// <param name="p"></param>
		private void DetermineSpeciesTargetSize()
		{
			foreach(Species species in pop.SpeciesTable.Values)
			{
				species.TargetSize = (int)Math.Round((species.MeanFitness / pop.TotalSpeciesMeanFitness) * pop.PopulationSize);

				// Calculate how many elite genomes to keep in the next round. If this is a large number then we can only
				// keep as many genomes as we have!
				species.ElitistSize = Math.Min(species.Members.Count, (int)Math.Floor(species.TargetSize * neatParameters.elitismProportion));
				if(species.ElitistSize ==0 && species.TargetSize > 1)
				{	// If ElitistSize is calculated to be zero but the TargetSize non-zero then keep just one genome.
					// If the the TargetSize is 1 then we can't really do this since it would mean that no offspring would be generated.
					// So we throw away the one member and hope that the one offspring generated will be OK.
					species.ElitistSize = 1;
				}	
			}
		}

		/// <summary>
		/// Search for connections with the same end-points throughout the whole population and 
		/// ensure that like connections have the same innovation ID.
		/// </summary>
		private void MatchConnectionIds()
		{
			Hashtable connectionIdTable = new Hashtable();

			int genomeBound=pop.GenomeList.Count;
			for(int genomeIdx=0; genomeIdx<genomeBound; genomeIdx++)
			{
				NeatGenome.NeatGenome genome = (NeatGenome.NeatGenome)pop.GenomeList[genomeIdx];

				int connectionGeneBound = genome.ConnectionGeneList.Count;
				for(int connectionGeneIdx=0; connectionGeneIdx<connectionGeneBound; connectionGeneIdx++)
				{
					ConnectionGene connectionGene = genome.ConnectionGeneList[connectionGeneIdx];

					ConnectionEndpointsStruct ces = new ConnectionEndpointsStruct();
					ces.sourceNeuronId = connectionGene.SourceNeuronId;
					ces.targetNeuronId = connectionGene.TargetNeuronId;
					
					Object existingConnectionIdObject = connectionIdTable[ces];
					if(existingConnectionIdObject==null)
					{	// No connection withthe same end-points has been registered yet, so 
						// add it to the table.
						connectionIdTable.Add(ces, connectionGene.InnovationId);
					}
					else
					{	// This connection is already registered. Give our latest connection 
						// the same innovation ID as the one in the table.
						connectionGene.InnovationId = (uint)existingConnectionIdObject;
					}
				}

				// The connection genes in this genome may now be out of order. Therefore we must ensure 
				// they are sorted before we continue.
				genome.ConnectionGeneList.SortByInnovationId();
			}
		}

		#endregion

		#region Private Methods [Pruning Phase]

		private bool TestForPruningPhaseBegin()
		{
			// Enter pruning phase if the complexity has risen beyond the specified threshold AND no gains in fitness have
			// occured for specified number of generations.
			return (pop.AvgComplexity > pop.PrunePhaseAvgComplexityThreshold) &&
					((generation-pop.GenerationAtLastImprovement) >= neatParameters.pruningPhaseBeginFitnessStagnationThreshold);
		}

		private bool TestForPruningPhaseEnd()
		{
			// Don't expect simplification on every generation. But if nothing has happened for 
			// 'pruningPhaseEndComplexityStagnationThreshold' gens then end the prune phase.
			if(generation-prunePhase_generationAtLastSimplification > neatParameters.pruningPhaseEndComplexityStagnationThreshold)
				return true;

			return false;
		}


		private void BeginPruningPhase()
		{
			// Enter pruning phase.
			pruningMode = true;
			prunePhase_generationAtLastSimplification = generation;
			prunePhase_MinimumStructuresPerGenome = pop.AvgComplexity;
			neatParameters = neatParameters_PrunePhase;

			// Copy the speciation threshold as this is dynamically altered during a search and we wish to maintain
			// the tracking during pruning.
			neatParameters.compatibilityThreshold = neatParameters_Normal.compatibilityThreshold;

			System.Diagnostics.Debug.WriteLine(">>Prune Phase<< Complexity=" + pop.AvgComplexity.ToString("0.00"));
		}

		private void EndPruningPhase()
		{
			// Leave pruning phase.
			pruningMode = false;

			// Set new threshold 110% of current level or 10 more if current complexity is very low.
			pop.PrunePhaseAvgComplexityThreshold = pop.AvgComplexity + neatParameters.pruningPhaseBeginComplexityThreshold;
			System.Diagnostics.Debug.WriteLine("complexity=" + pop.AvgComplexity.ToString() + ", threshold=" + pop.PrunePhaseAvgComplexityThreshold.ToString());

			neatParameters = neatParameters_Normal;
			neatParameters.compatibilityThreshold = neatParameters_PrunePhase.compatibilityThreshold;

			// Update species.AgaAtLastimprovement. Originally we reset this age to give all of the species
			// a 'clean slate' following the pruning phase. This though has the effect of giving all of the 
			// species the same AgeAtLastImprovement - which in turn often results in all of the species 
			// reaching the dropoff age simulataneously which results in the species being culled and therefore
			// causes a radical fall in population diversity.
			// Therefore we give the species a new AgeAtLastImprovement which reflects their relative 
			// AgeAtLastImprovement, this gives the species a new chance following pruning but does not allocate
			// them all the same AgeAtLastImprovement.
			NormalizeSpeciesAges();

			if(connectionWeightFixingEnabled)
			{
				// Fix all of the connection weights that remain after pruning (proven to be good values).
				foreach(NeatGenome.NeatGenome genome in pop.GenomeList)
					genome.FixConnectionWeights();
			}
		}

		private void NormalizeSpeciesAges()
		{
			float quarter_of_dropoffage = (float)neatParameters.speciesDropoffAge / 4.0F;

			// Calculate the spread of AgeAtLastImprovement - first find the min and max values.
			long minAgeAtLastImprovement;
			long maxAgeAtLastImprovement;

			minAgeAtLastImprovement = long.MaxValue;
			maxAgeAtLastImprovement = 0;

			foreach(Species species in pop.SpeciesTable.Values)
			{
				minAgeAtLastImprovement = Math.Min(minAgeAtLastImprovement, species.AgeAtLastImprovement);
				maxAgeAtLastImprovement = Math.Max(maxAgeAtLastImprovement, species.AgeAtLastImprovement);
			}

			long spread = maxAgeAtLastImprovement-minAgeAtLastImprovement;

			// Allocate each species a new AgeAtLastImprovement. Scale the ages so that the oldest is
			// only 25% towards the cutoff age.
			foreach(Species species in pop.SpeciesTable.Values)
			{
				long droppOffAge = species.AgeAtLastImprovement-minAgeAtLastImprovement;
				long newDropOffAge = (long)(((float)droppOffAge / (float)spread) * quarter_of_dropoffage);
				species.AgeAtLastImprovement = species.SpeciesAge - newDropOffAge;
			}
		}

		#endregion

		#region Some routines useful for profiling.
//		System.Text.StringBuilder sb = new System.Text.StringBuilder();
//		int tickCountStart;
//		int tickDuration;
//
//		private void StartMonitor()
//		{
//			tickCountStart = System.Environment.TickCount;
//		}
//
//		private void EndMonitor(string msg)
//		{
//			tickDuration =  System.Environment.TickCount - tickCountStart;
//			sb.Append(msg + " : " + tickDuration + " ms\n");
//		}
//
//		private void DumpMessage()
//		{
//			System.Windows.Forms.MessageBox.Show(sb.ToString());
//			sb = new System.Text.StringBuilder();
//		}
		#endregion
	}
}
