using SharpNeatLib;
using SharpNeatLib.CPPNs;
using SharpNeatLib.Evolution.Xml;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.NeatGenome.Xml;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace MazeSimulator
{
    class MainClass
    {
        public static string info = "MultiAgent-HyperSharpNEAT Simulator v0.5";

        [STAThreadAttribute]
        public static void Main(string[] args)
        {   
            #region Instance variables

            string folder = "";
            int generations = 1001, populationSize = 0;
            int numTrajectoriesPerFolder = 0;
            string experimentName = null;
            bool doMapElites = false;
            bool doMEnoveltyPressure = false;
            bool doNSLC = false;
            bool doNS = false;
            bool recordTrajectory = false; // tracks Trajectory by dumping all trajectories to file
            bool recordTrajectorySimple = false; // Used to track Trajectory on genomes WITHOUT dumping every single Trajectory to file (but it will dump to the mapelites grid if tracked, or just to champion genomes that are printed out)
            bool recordEndpoints = false;
            bool doMapelitesStyleGrid = false; // used to tell algorithms other than ME to keep track of a grid (in the style of mapelites) and report it at the end

            #endregion

            if (args.Length!=0 && args[0] == "-help")
            {
                showHelp();
                return;
            }
            
            if (!(args.Length == 0) && args[0] == "evolve")
            {
                for (int j = 1; j < args.Length; j++)
                {
                    if (j <= args.Length - 2)
                        switch (args[j])
                        {
                            case "-ns": // Novelty Search 1.0 (this is novelty search with a large non-breeding archive kept on the side)
                                doNS = true;
                                break;

                            case "-nslc": // Steady-State NSLC 
                                doNSLC = true;
                                break;

                            case "-menovelty":
                                doMEnoveltyPressure = true;
                                break;

                            case "-mapelites":
                                doMapElites = true;
                                break;

                            case "-trackmegrid":
                                doMapelitesStyleGrid = true;
                                break;

                            case "-experiment":
                                experimentName = args[++j];
                                break;

                            case "-generations": if (!int.TryParse(args[++j], out generations))
                                    Console.WriteLine("Invalid number of generations specified.");
                                break;
						
                            case "-folder": folder = args[++j];
                                Console.WriteLine("Attempting to output to folder " + folder);
                                break;

                            case "-recordTrajectory":
                                recordTrajectory = true;
                                break;

                            case "-recordTrajectorySimple":
                                recordTrajectory = true;
                                recordTrajectorySimple = true;
                                break;

                            case "-NumTrajectoriesPerFolder":
                                numTrajectoriesPerFolder = Convert.ToInt32(args[++j]);
                                break;

                            case "-recordEndpoints":
                                recordEndpoints = true;
                                break;
                        }
                }

                if (experimentName == null)
                {
                    Console.WriteLine("Missing [experimentName].");
                    Console.WriteLine("See help \"-help\"");
                    return;
                }
				
                ExperimentWrapper wr = ExperimentWrapper.load(experimentName);
                SimulatorExperiment experiment = wr.experiment;

                if(populationSize!=0)
                    experiment.populationSize = populationSize;
				else
					populationSize=experiment.populationSize;

                experiment.initialize();
                HyperNEATEvolver evolve = new HyperNEATEvolver(experiment);

                if (doNS)
                {
                    evolve.experiment.DefaultNeatParameters.NS1 = true;
                    evolve.experiment.DefaultNeatParameters.NS2 = true;
                    evolve.experiment.DefaultNeatParameters.NS2_archiveCap = evolve.experiment.DefaultNeatParameters.NS1_popsize;
                }

                if (doMapElites)
                {
                    evolve.experiment.DefaultNeatParameters.mapelites = true;
                    if (doMEnoveltyPressure)
                        evolve.experiment.DefaultNeatParameters.me_noveltyPressure = true;
                }

                if (doNSLC)
                {
                    evolve.experiment.DefaultNeatParameters.NS2_archiveCap = evolve.experiment.DefaultNeatParameters.NS1_popsize;
                    evolve.experiment.DefaultNeatParameters.NSLC = true;
                }

                if (recordEndpoints)
                {
                    if (experiment is SimulatorExperiment)
                        ((SimulatorExperiment)experiment).recordEndpoints = true;
                    evolve.FinalPositionLogging = true;
                }

                if (recordTrajectory)
                {
                    if (experiment is SimulatorExperiment)
                        ((SimulatorExperiment)experiment).recordTrajectories = true;
                    evolve.TrajectoryLogging = true;

                    if (numTrajectoriesPerFolder != 0)
                        evolve.setNumTrajectoriesPerFolder(numTrajectoriesPerFolder);
                }
                if (recordTrajectorySimple)
                { 
                    evolve.TrajectoryLogging = false;
                }

                if (doMapelitesStyleGrid)
                {
                    evolve.experiment.DefaultNeatParameters.track_me_grid = true;
                }
				
				evolve.setOutputFolder(folder);
                evolve.NEATBrain = experiment.neatBrain; 
				evolve.initializeEvolution(populationSize);
                evolve.evolve(generations);
            }
            else // Run the GUI version.
            {
                experimentName = "QDExperiment.xml";
                SimulatorVisualizer vis = new SimulatorVisualizer(experimentName,null);
                vis.Refresh();
                Application.Run(vis);
                vis.Refresh();
                
            }
        }

        public static void showHelp()
        {
            Console.WriteLine(info);
            Console.WriteLine("If called with \"evolve\" the command line tool is used. Otherwise the visual simulator is started.");
            Console.WriteLine();
            Console.WriteLine("-experiment [filename]       Load this experiment.");
            Console.WriteLine("-populationSize [number]");
            Console.WriteLine("-generations [number]");
            Console.WriteLine("-folder [name]               Output files to the specified folder");
        }
    }
}

