using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.Experiments;
using System.IO;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.Evolution;
using System.Xml;
using SharpNeatLib.Evolution.Xml;
using SharpNeatLib.NeatGenome.Xml;
using SharpNeatLib.NeuralNetwork;

namespace MazeSimulator
{
    /// <summary>
    /// Manages the evolutionary algorithm and statistics tracking/output. 
    /// </summary>
    public class HyperNEATEvolver
    {
        #region InstanceVariables

        // Domain
        public IExperiment experiment;
        SimulatorExperiment SimExperiment;

        // Evolution
        EvolutionAlgorithm EA = null;
        public bool NEATBrain = false;
        double MaxFitness = 0;
        
        // Logging / output 
        int FolderNum = 0;
        int NumTrajectoriesPerFolder;
        int NumTrajectoriesRecorded = 0;
        string TrajectoryFolderPath = "";
        string OutputFolder = "";
        public bool Logging = true;
        public bool FinalPositionLogging = false; 
        public bool TrajectoryLogging = false;
        public bool ArchiveModificationLogging = false;
        StreamWriter LogOutput;
        StreamWriter FinalPositionOutput;
        StreamWriter ArchiveModificationOutput;
        StreamWriter ComplexityOutput;
        XmlDocument XmlDoc;
        FileInfo OutputFileInfo;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new HyperNEATEvolver object from an existing SimulatorExperiment domain object.
        /// </summary>
        /// <param name="simExp"></param>
        public HyperNEATEvolver(SimulatorExperiment simExperiment)
        {
            SimExperiment = simExperiment;
            experiment = new Experiment(SimExperiment);
            NumTrajectoriesPerFolder = SimExperiment.populationSize;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Enables novelty search by modifying the NEAT parameters.
        /// </summary>
        public void enableNoveltySearch(bool enable) 
        {
			if(enable) 
            {
			experiment.DefaultNeatParameters.noveltySearch=true;
			experiment.DefaultNeatParameters.noveltyFloat=true;
			}
			else 
            {
			experiment.DefaultNeatParameters.noveltySearch=false;
			experiment.DefaultNeatParameters.noveltyFloat=false;
			}
		}

        /// <summary>
        /// Initializes the EA with a random intial population.
        /// </summary>
        public void initializeEvolution(int populationSize)
        {
            LogOutput = Logging ? new StreamWriter(Path.Combine(OutputFolder, "log.txt")) : null;
            FinalPositionOutput = FinalPositionLogging ? new StreamWriter(Path.Combine(OutputFolder,"final-position.txt")) : null;
            ArchiveModificationOutput = FinalPositionLogging ? new StreamWriter(Path.Combine(OutputFolder, "archive-mods.txt")) : null;
            ComplexityOutput = new StreamWriter(Path.Combine(OutputFolder, "complexity.txt"));
            ComplexityOutput.WriteLine("avg,stdev,min,max");
            if (FinalPositionLogging)
            {
                FinalPositionOutput.WriteLine("ID,x,y");
                ArchiveModificationOutput.WriteLine("ID,action,time,x,y");
            }
            
            IdGenerator idgen = new IdGenerator();
            EA = new EvolutionAlgorithm(new Population(idgen, GenomeFactory.CreateGenomeList(experiment.DefaultNeatParameters, idgen, experiment.InputNeuronCount, experiment.OutputNeuronCount, experiment.DefaultNeatParameters.pInitialPopulationInterconnections, populationSize, SimExperiment.neatBrain)), experiment.PopulationEvaluator, experiment.DefaultNeatParameters);
            EA.outputFolder = OutputFolder;
            EA.neatBrain = NEATBrain;
        }

        /// <summary>
        /// Initializes the EA with an initial population generated from a single seed genome.
        /// </summary>
        public void initializeEvolution(int populationSize, NeatGenome seedGenome)
        {
            if (seedGenome == null)
            {
                initializeEvolution(populationSize);
                return;
            }

            LogOutput = Logging ? new StreamWriter(Path.Combine(OutputFolder, "log.txt")) : null;
            FinalPositionOutput = FinalPositionLogging ? new StreamWriter(Path.Combine(OutputFolder,"final-position.txt")) : null;
            ArchiveModificationOutput = FinalPositionLogging ? new StreamWriter(Path.Combine(OutputFolder, "archive-mods.txt")) : null;
            ComplexityOutput = new StreamWriter(Path.Combine(OutputFolder, "complexity.txt"));
            ComplexityOutput.WriteLine("avg,stdev,min,max");
            if (FinalPositionLogging)
            {
                FinalPositionOutput.WriteLine("x,y");
                ArchiveModificationOutput.WriteLine("ID,action,time,x,y");
            }
            IdGenerator idgen = new IdGeneratorFactory().CreateIdGenerator(seedGenome);
            EA = new EvolutionAlgorithm(new Population(idgen, GenomeFactory.CreateGenomeList(seedGenome, populationSize, experiment.DefaultNeatParameters, idgen)), experiment.PopulationEvaluator, experiment.DefaultNeatParameters);
            EA.outputFolder = OutputFolder;
            EA.neatBrain = NEATBrain;
        }

        /// <summary>
        /// Initializes the EA using an initial population from a static file.
        /// </summary>
        /// <param name="fname">XML file encoding the initial population.</param>
		public void initializeEvolutionFromPopFile(string fname) 
        {
			XmlDocument doc = new XmlDocument();
            doc.Load(fname);
			Population pop = XmlPopulationReader.Read(doc,new XmlNeatGenomeReader(),new SharpNeatLib.NeatGenome.IdGeneratorFactory());
			initalizeEvolution(pop);
		}
		
        /// <summary>
        /// Initializes the EA using an initial population that has already been read into object format.
        /// </summary>
        /// <param name="pop"></param>
        public void initalizeEvolution(Population pop)
        {
            LogOutput = Logging ? new StreamWriter(Path.Combine(OutputFolder, "log.txt")) : null;
            FinalPositionOutput = FinalPositionLogging ? new StreamWriter(Path.Combine(OutputFolder, "final-position.txt")) : null;
            ArchiveModificationOutput = FinalPositionLogging ? new StreamWriter(Path.Combine(OutputFolder, "archive-mods.txt")) : null;
            ComplexityOutput = new StreamWriter(Path.Combine(OutputFolder, "complexity.txt"));
            ComplexityOutput.WriteLine("avg,stdev,min,max");

            if (FinalPositionLogging)
            {
                FinalPositionOutput.WriteLine("ID,x,y");
                ArchiveModificationOutput.WriteLine("ID,action,time,x,y");
            }

            EA = new EvolutionAlgorithm(pop, experiment.PopulationEvaluator, experiment.DefaultNeatParameters);
            EA.outputFolder = OutputFolder;
            EA.neatBrain = NEATBrain;
        }

        /// <summary>
        /// Sets the NumTrajectoriesPerFolder property.
        /// </summary>
        public void setNumTrajectoriesPerFolder(int newNumTrajectories)
        {
            NumTrajectoriesPerFolder = newNumTrajectories;
        }

        /// <summary>
        /// Sets the output folder to Results/<folder>. The folder is created if it does not already exist.
        /// </summary>
        public void setOutputFolder(string folder)
        {
            // Append the user-specified folder name to the working directory path
            OutputFolder = Path.Combine(Directory.GetCurrentDirectory(),"Results");
            OutputFolder = Path.Combine(OutputFolder, folder);

            // If the folder does not already exist, create it
            if (!Directory.Exists(OutputFolder))
                System.IO.Directory.CreateDirectory(OutputFolder);
        }

        /// <summary>
        /// Runs one generation of the evolutionary algorithm.
        /// </summary>
        public void oneGeneration(int currentGeneration)
        {
            DateTime dt = DateTime.Now;
            EA.PerformOneGeneration();
            if (EA.BestGenome.RealFitness > MaxFitness)
            {
                SimExperiment.bestGenomeSoFar = (NeatGenome)EA.BestGenome;
                MaxFitness = EA.BestGenome.RealFitness;
                XmlDoc = new XmlDocument();
                XmlGenomeWriterStatic.Write(XmlDoc, (NeatGenome)EA.BestGenome);
                OutputFileInfo = new FileInfo(Path.Combine(OutputFolder, "bestGenome" + currentGeneration.ToString() + "_" + ((int)MaxFitness).ToString() + ".xml"));
                XmlDoc.Save(OutputFileInfo.FullName);
            }
            if (experiment.DefaultNeatParameters.NS2 || experiment.DefaultNeatParameters.NSLC)
            { 
                // The only reason NS2 has special output is so we can profile the novelty calc time versus the fitness calc time
                Console.Write(EA.Generation.ToString() + " " + EA.BestGenome.RealFitness);
                if (experiment.DefaultNeatParameters.NS1)
                {
                    Console.Write(" nov: " + EA.ns1archiveAverageNovelty + " " + EA.Population.GenomeList.Count + "[" + EA.archiveSize + "]");
                }
                else
                {
                    Console.Write(" nov: " + EA.averageNovelty + " " + EA.Population.GenomeList.Count + "[" + EA.archiveSize + "]");
                }
                
                if (experiment.DefaultNeatParameters.track_me_grid)
                {
                    Console.Write("[" + EA.numFilledBins + "] " + EA.gridAverageFitness);
                }
                Console.WriteLine(" T-sim/nov/spe: " + EA.timeSpentInFitnessEval + " " + EA.timeSpentOutsideFitnessEval + " " + EA.timeSpentInSpeciation);
            }
            else if (experiment.DefaultNeatParameters.mapelites)
            {
                Console.WriteLine(EA.Generation.ToString() + " " + EA.BestGenome.RealFitness + " " + EA.Population.GenomeList.Count + "[" + EA.numFilledBins + "] " + EA.gridAverageFitness + " " + (DateTime.Now.Subtract(dt)));
            }
            else
            {
                Console.Write(EA.Generation.ToString() + " " + EA.BestGenome.RealFitness + " " + EA.Population.GenomeList.Count); // TODO: Add in average novelty
                if (experiment.DefaultNeatParameters.track_me_grid)
                {
                    Console.Write("[" + EA.numFilledBins + "] " + EA.gridAverageFitness);
                }
                Console.WriteLine(" " + (DateTime.Now.Subtract(dt)));
            }

            if (FinalPositionLogging)
            {
                foreach (IGenome g in EA.Population.GenomeList)
                {
                    FinalPositionOutput.WriteLine(g.GenomeId + "," + g.Behavior.finalLocation[0] + "," + g.Behavior.finalLocation[1]);
                }
            }

            if (TrajectoryLogging)
            {
                // If this is the first generation, create the new output folder
                if (currentGeneration == 0)
                {
                    TrajectoryFolderPath = Path.Combine(OutputFolder, "trajectories-" + FolderNum.ToString());
                    if (!Directory.Exists(TrajectoryFolderPath))
                        System.IO.Directory.CreateDirectory(TrajectoryFolderPath);
                }

                foreach (IGenome g in EA.Population.GenomeList)
                {
                    // Check to see if we need to create a new folder
                    if (NumTrajectoriesRecorded == NumTrajectoriesPerFolder)
                    {
                        NumTrajectoriesRecorded = 0;
                        FolderNum++;
                        TrajectoryFolderPath = Path.Combine(OutputFolder, "trajectories-" + FolderNum.ToString());
                        if (!Directory.Exists(TrajectoryFolderPath))
                            System.IO.Directory.CreateDirectory(TrajectoryFolderPath);
                    }

                    // Print the Trajectory
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(TrajectoryFolderPath, NumTrajectoriesRecorded.ToString() + ".txt"), true))
                    {
                        file.WriteLine("x,y");
                        foreach (double component in g.Behavior.trajectory)
                            file.Write(component.ToString() + ",");
                    }

                    // Increment the Trajectory counter
                    NumTrajectoriesRecorded++;
                }
            }
            
			int gen_mult=200;
            if (Logging)
            {	
                if (experiment.DefaultNeatParameters.noveltySearch && currentGeneration % gen_mult == 0)
                {
                    XmlDocument archiveout = new XmlDocument();

                    XmlPopulationWriter.WriteGenomeList(archiveout, EA.noveltyFixed.archive);
                    OutputFileInfo = new FileInfo(Path.Combine(OutputFolder,"archive.xml"));
                    archiveout.Save(OutputFileInfo.FullName);
                }
                // If doing MapElites: print base500 generation instead of regular, and also print averageFitness and number of filled bins (instead of not)
                // regular: generation bestfitness
                // mapelites: generation500 bestfitness [numfilledgrids] averagefitness
                if (experiment.DefaultNeatParameters.mapelites)
                {
                    LogOutput.WriteLine(EA.generationBase500.ToString() + " " + (MaxFitness).ToString() + " " + EA.numFilledBins.ToString() + " " + EA.averageFitness.ToString());
                    LogOutput.Flush();

                    if (FinalPositionLogging)
                    {
                        foreach (IGenome g in EA.addedToArchive)
                        {
                            ArchiveModificationOutput.WriteLine(g.GenomeId + ",+," + EA.numEvaluations + "," + g.Behavior.finalLocation[0] + "," + g.Behavior.finalLocation[1]);
                        }
                        EA.addedToArchive.Clear();
                        foreach (IGenome g in EA.removedFromArchive)
                        {
                            ArchiveModificationOutput.WriteLine(g.GenomeId + ",-," + EA.numEvaluations + "," + g.Behavior.finalLocation[0] + "," + g.Behavior.finalLocation[1]);
                        }
                        EA.removedFromArchive.Clear();
                    }
                }
                else if (experiment.DefaultNeatParameters.NS2 || experiment.DefaultNeatParameters.NSLC)
                {
                    LogOutput.Write(EA.generationBase500.ToString() + " " + (MaxFitness).ToString() + " " + EA.archiveSize + " " + EA.averageFitness.ToString() + " " + (EA.ns1 ? EA.ns1archiveAverageNovelty.ToString() : EA.averageNovelty.ToString()));
                    if (experiment.DefaultNeatParameters.track_me_grid)
                    {
                        LogOutput.Write(" " + EA.numFilledBins.ToString() + " " + EA.gridAverageFitness.ToString());
                    }
                    LogOutput.WriteLine();
                    LogOutput.Flush();

                    if (FinalPositionLogging)
                    {
                        foreach (IGenome g in EA.addedToArchive)
                        {
                            ArchiveModificationOutput.WriteLine(g.GenomeId + ",+," + EA.numEvaluations + "," + g.Behavior.finalLocation[0] + "," + g.Behavior.finalLocation[1]);
                        }
                        EA.addedToArchive.Clear();
                        foreach (IGenome g in EA.removedFromArchive)
                        {
                            ArchiveModificationOutput.WriteLine(g.GenomeId + ",-," + EA.numEvaluations + "," + g.Behavior.finalLocation[0] + "," + g.Behavior.finalLocation[1]);
                        }
                        EA.removedFromArchive.Clear();
                    }
                }
                else
                {
                    LogOutput.Write(EA.generationBase500.ToString() + " " + (MaxFitness).ToString());
                    if (experiment.DefaultNeatParameters.track_me_grid)
                    {
                        LogOutput.Write(" " + EA.numFilledBins.ToString() + " " + EA.gridAverageFitness.ToString());
                    }
                    LogOutput.WriteLine();
                    LogOutput.Flush();
                }

                // Output complexity statistics
                ComplexityOutput.WriteLine(EA.avgComplexity + ", " + EA.stdevComplexity + ", " + EA.minComplexity + ", " + EA.maxComplexity);
                ComplexityOutput.Flush();
            }
        }

        /// <summary>
        /// Performs multiple generations of the evolutionary algorithm.
        /// </summary>
        public void evolve(int generations)
        {
            for (int j = 0; j < generations; j++)
            {
                oneGeneration(j);
            }
            LogOutput.Close();

            XmlDoc = new XmlDocument();
            XmlGenomeWriterStatic.Write(XmlDoc, (NeatGenome)EA.BestGenome, ActivationFunctionFactory.GetActivationFunction("NullFn"));
            OutputFileInfo = new FileInfo(Path.Combine(OutputFolder,"bestGenome.xml"));
            XmlDoc.Save(OutputFileInfo.FullName);


            //if doing novelty search, write out archive
            if (experiment.DefaultNeatParameters.noveltySearch)
            {
                XmlDocument archiveout = new XmlDocument();

                XmlPopulationWriter.WriteGenomeList(archiveout, EA.noveltyFixed.archive);
                OutputFileInfo = new FileInfo(Path.Combine(OutputFolder,"archive.xml"));
                archiveout.Save(OutputFileInfo.FullName);
            }

            // dump the MapElites grid contents (empty if we aren'type doing ME and we aren'type tracking ME-style-grid for some other algorithm)
            XmlDocument gridout = new XmlDocument();
            XmlPopulationWriter.WriteGenomeList(gridout, EA.meDumpGrid());
            OutputFileInfo = new FileInfo(Path.Combine(OutputFolder,"finalgrid.xml"));
            gridout.Save(OutputFileInfo.FullName);
        }

        /// <summary>
        /// Closes the output logger at the end of a run.
        /// </summary>
        public void end()
        {
            if (Logging)
                LogOutput.Close();
        }

        #endregion
    }
}
