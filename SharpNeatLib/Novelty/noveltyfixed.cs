// noveltyhistogram.cs created with MonoDevelop
// User: joel at 2:22 AM 7/23/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using SharpNeatLib;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeatGenome.Xml;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeuralNetwork.Xml;

namespace SharpNeatLib.Novelty
{
	public class Pair<T,U>:IComparable where T:System.IComparable<T> {
	 public Pair() { }
	 public Pair(T first, U second) {
	  this.First=first;
	  this.Second=second;
	 }
	 public T First { get; set; }
	 public U Second { get; set; }
	 int IComparable.CompareTo(object obj) {
	  Pair<T,U> c=(Pair<T,U>)obj;
	  return this.First.CompareTo(c.First);
	 }
	};
	
	public class noveltyfixed
	{	
	    public int nearest_neighbors;
	    public bool initialized; 
	    public double archive_threshold;
	    public GenomeList measure_against;
	    public GenomeList archive;
	    public GenomeList pending_addition;

        public Random archiveAddingRNG = new Random();
        public const double archiveAddingProbability = 0.024; // Probability to add an individual to the archive.
        public bool usingProbabilisticArchive = true;
        public bool probTournament = true; // Should we hold a diversity tournament when adding individuals to the archive probibilistically? (tournament size = 2)

        public bool enforceArchiveLimit = false; // Should we enforce an archive size limit? If so: delete excess probabilistically
        public bool probDeletionTournament = true; // If so: while the limit is exceeded, we run random tournaments for deletion (tournament size = 2) (instead of raw probabilistic deletion)
                
        public const int archiveSizeLimit = 3000;
	    
	    public void addPending(bool suppressThresholdUpdate = false)
	    {
	        int length = pending_addition.Count;

            if (usingProbabilisticArchive)
            { // Do something different for probabilistic archive. No thresholds involved. Just add everyone that is pending.
                if (!probTournament)
                {
                    for (int i = 0; i < length; i++)
                    {
                        archive.Add(pending_addition[i]);
                    }
                    pending_addition.Clear();
                    Console.WriteLine("Added " + length + " to the archive. Current size: " + archive.Count);
                }
                else
                { // Hold a diversity tournament to decide to gets added to the archive
                    int countThem = 0;
                    double averageAdd = 0;
                    for (int i = 0; i + 1 < length; i = i + 2)
                    {
                        double first = valueAgainstArchive(pending_addition[i]);
                        double second = valueAgainstArchive(pending_addition[i + 1]);
                        if (first > second)
                        {
                            archive.Add(pending_addition[i]);
                            averageAdd += first;
                        }
                        else
                        {
                            archive.Add(pending_addition[i + 1]);
                            averageAdd += second;
                        }
                        countThem++;
                    }
                    // The straggler is almost never added... need a more fair method of adding stragglers. Maybe give them an early tournament against someone random?
                    /*if (length % 2 == 1) // The last one was not considered if there was an odd number in the list. We should consider it against the average.
                    {
                        if (countThem != 0)
                        {
                            averageAdd /= countThem;
                            if (valueAgainstArchive(pending_addition[length - 1]) > averageAdd)
                            {
                                Console.WriteLine("Straggler added! " + valueAgainstArchive(pending_addition[length - 1]) + " beats avg " + averageAdd);
                                archive.Add(pending_addition[length - 1]);
                            }
                            else
                            {
                                Console.WriteLine("Straggler not added. " + valueAgainstArchive(pending_addition[length - 1]) + " loses to avg " + averageAdd);
                            }
                        }
                    }//*/
                    pending_addition.Clear();
                    Console.WriteLine("Added " + countThem + " (" + length + ") to the archive. Current size: " + archive.Count);
                }
            }
            else
            {
                if (!suppressThresholdUpdate)
                {
                    if (length == 0)
                    {
                        archive_threshold *= 0.95;
                        Console.WriteLine("Decreasing threshold. New: " + archive_threshold + ", adding " + length + " to archive. (" + archive.Count + ")");
                    }
                    if (length > 5)
                    {
                        archive_threshold *= 1.3;
                        Console.WriteLine("Increasing threshold. New: " + archive_threshold + ", adding " + length + " to archive. (" + archive.Count + ")");
                    }
                }

                for (int i = 0; i < length; i++)
                {
                    if (measureAgainstArchive((NeatGenome.NeatGenome)pending_addition[i], false))
                    {
                        archive.Add(pending_addition[i]);
                    }
                }
                pending_addition.Clear();
            }

            if (enforceArchiveLimit)
            {
                if (probDeletionTournament)
                {
                    // Check the archive against the size limit and if it exceeds the limit, run a tournament to delete members
                    while (archive.Count > archiveSizeLimit)
                    {
                        int firstIdx = archiveAddingRNG.Next(archive.Count);
                        int secondIdx = firstIdx;
                        while (firstIdx == secondIdx) // Make sure the indexes are unique
                        {
                            secondIdx = archiveAddingRNG.Next(archive.Count);
                        }
                        // Evaluate the candidate victims against the archive
                        double first = valueAgainstArchive(archive[firstIdx]);
                        double second = valueAgainstArchive(archive[secondIdx]);
                        // Remove the one with the lower novelty score
                        if (first < second)
                        {
                            archive.RemoveAt(firstIdx);
                            //Console.WriteLine(first + " vs " + second + " : removing first.");
                        }
                        else
                        {
                            archive.RemoveAt(secondIdx);
                            //Console.WriteLine(first + " vs " + second + " : removing second.");
                        }
                    }
                }
                else // No tournament, just delete randomly
                {
                    while (archive.Count > archiveSizeLimit)
                    {
                        int firstIdx = archiveAddingRNG.Next(archive.Count);
                        // Evaluate the candidate victims against the archive
                        double first = valueAgainstArchive(archive[firstIdx]);
                        archive.RemoveAt(firstIdx);
                    }
                }
            }
            
	    }
	    
	    public bool measureAgainstArchive(NeatGenome.NeatGenome neatgenome,bool addToPending) 
	    {
	        foreach (IGenome genome in archive)
	        {
	            if(BehaviorDistance.Distance(neatgenome.Behavior,((NeatGenome.NeatGenome)genome).Behavior) < archive_threshold)
	                return false;
	        }
	        
	        if(addToPending)
	        {
				
	            pending_addition.Add(neatgenome);
	        }
	        
	        return true;
	    }

        // Returns the minimum behavioral distance to all individuals in the archive
        public double valueAgainstArchive(IGenome neatgenome)
        {
            double minSoFar = Double.MaxValue;
            double temp;
            foreach (IGenome genome in archive)
            {
                if (genome.Equals(neatgenome)) continue; // Don't let a genome be tested against itself, or else we'd always return 0
                temp = BehaviorDistance.Distance(((NeatGenome.NeatGenome)neatgenome).Behavior, ((NeatGenome.NeatGenome)genome).Behavior);
                if (temp < minSoFar) minSoFar = temp;
            }
            return minSoFar;
        }

        public bool probabalisticMeasureAgainstArchive(NeatGenome.NeatGenome neatgenome)
        {
            double roll = archiveAddingRNG.NextDouble();
            double prob = archiveAddingProbability;
            if (probTournament) prob = prob * 2; // Double the chance of adding to pending_addition if there is a tournament (because half of them won't actually be added)
            if (roll < prob)
            {
                pending_addition.Add(neatgenome);
                return true;
            }

            return false;
        }
	    
	    //measure the novelty of an organism against the fixed population
	    public double measureNovelty(NeatGenome.NeatGenome neatgenome)
	    {
		   double sum = 0.0;

           if (!initialized)
           {
               //Console.WriteLine("NOVELTY NOT INITIALIZED"); //JUSTIN: Debug, Remove this!
               return Double.MinValue;
           }
               
	        List< Pair<double,NeatGenome.NeatGenome> > noveltyList = new List<Pair<double,NeatGenome.NeatGenome>>();
	        
	        foreach(IGenome genome in measure_against)
	        {
	            noveltyList.Add(new Pair<double,NeatGenome.NeatGenome>(BehaviorDistance.Distance(((NeatGenome.NeatGenome)genome).Behavior,neatgenome.Behavior),((NeatGenome.NeatGenome)genome)));
	        }
	        /*foreach(IGenome genome in archive) //JUSTIN: Deciding not to use the archive in the neighborhood. Hope this helps?
	        {
	            noveltyList.Add(new Pair<double,NeatGenome.NeatGenome>(BehaviorDistance.Distance(((NeatGenome.NeatGenome)genome).Behavior,neatgenome.Behavior),((NeatGenome.NeatGenome)genome)));
//				noveltyList.Add(BehaviorDistance.Distance(((NeatGenome.NeatGenome)genome).Behavior,neatgenome.Behavior));
	        }//*/
            //Console.WriteLine("NOVELTYLIST SIZE: " + noveltyList.Count + " m_a: " + measure_against.Count + " archive: " + archive.Count);
            
            //see if we should add this genome to the archive
            if (usingProbabilisticArchive)
            {
                probabalisticMeasureAgainstArchive(neatgenome);
            }
            else
            {
                measureAgainstArchive(neatgenome, true);
            }
                
	        noveltyList.Sort();
			int nn = nearest_neighbors;
			if(noveltyList.Count<nearest_neighbors) {
				nn=noveltyList.Count;
			}
	        for(int x=0;x<nn;x++)
	        {
	            sum+=noveltyList[x].First;
                if (neatgenome.RealFitness == 0 || noveltyList[x].Second.RealFitness == 0) Console.WriteLine("WARNING! SOMEBODY IS ZERO!");
                if (neatgenome.RealFitness > noveltyList[x].Second.RealFitness)
                {
                    neatgenome.competition += 1;
                    //Console.WriteLine(neatgenome.RealFitness + " > " + (noveltyList[x].Second).RealFitness + " so incrementing competition");
                }
                //if (neatgenome.objectives[neatgenome.objectives.Length - 1] > noveltyList[x].Second.objectives[neatgenome.objectives.Length - 1])
                    //neatgenome.localGenomeNovelty += 1;
                if (neatgenome.geneticDiversity > noveltyList[x].Second.geneticDiversity)
                {
                    neatgenome.localGenomeNovelty += 1;
                    //Console.WriteLine("localGenomeNovelty: " + neatgenome.geneticDiversity + " > " + noveltyList[x].Second.geneticDiversity);
                }
				noveltyList[x].Second.locality+=1;
                neatgenome.nearestNeighbors++;
				// sum+=10000.0; //was 100
	        }
	        return Math.Max(sum,EvolutionAlgorithm.MIN_GENOME_FITNESS);
	    }
	    
	    //initialize fixed novelty measure
		public noveltyfixed(double threshold)
		{
		    initialized = false;
		    nearest_neighbors = 20;
		    archive_threshold = threshold;
		    archive = new GenomeList();
		    pending_addition = new GenomeList();
		}
		
		
		//Todo REFINE... adding highest fitness might
		//not correspond with most novel?
		public void add_most_novel(Population p)
		{
		    double max_novelty =0;
		    IGenome best= null;
		    for(int i=0;i<p.GenomeList.Count;i++)
		    {
		        if(p.GenomeList[i].Fitness > max_novelty)
		        {
		            best = p.GenomeList[i];
		            max_novelty = p.GenomeList[i].Fitness;
		        }
		    }
		    archive.Add(best);
		}
		public void initialize(GenomeList p)
		{
			initialized = true;
		  
			measure_against = new GenomeList();
		    
		    if(p!=null)
		    for(int i=0;i<p.Count;i++)
		    {
		        //we might not need to make copies
		        measure_against.Add(new NeatGenome.NeatGenome((NeatGenome.NeatGenome)p[i],0));    
		    }
		}
		
		public void initialize(Population p)
		{
		    initialize(p.GenomeList);
		}
		
		//update the measure population by intelligently sampling
		//the current population + archive + fixed population
		public void update_measure(Population p)
		{
			update_measure(p.GenomeList);
		}
		
		public void update_measure(GenomeList p)
		{
			GenomeList total = new GenomeList();
		
		    total.AddRange(p);
		    total.AddRange(measure_against);
		    total.AddRange(archive);
		    
		    merge_together(total, p.Count);
		    Console.WriteLine("size: " + Convert.ToString(measure_against.Count));
		}
		
		public void merge_together(GenomeList list,int size)
		{
		    Console.WriteLine("total count: "+ Convert.ToString(list.Count));
		    
		    Random r = new Random();
		    GenomeList newList = new GenomeList();
		    
		    List<bool> dirty = new List<bool>();
		    List<double> closest = new List<double>();
		    //set default values
		    for(int x=0;x<list.Count;x++)
		    {
		        dirty.Add(false);
		        closest.Add(Double.MaxValue);
		    }
		    //now add the first individual randomly to the new population
		    int last_added = r.Next() % list.Count;
		    dirty[last_added] = true;
		    newList.Add(list[last_added]);
		    
		    while(newList.Count < size)
		    {
		        double mostNovel = 0.0;
		        int mostNovelIndex = 0;
		        for(int x=0;x<list.Count;x++)
		        {
		            if (dirty[x])
		                continue;
		            double dist_to_last = BehaviorDistance.Distance(((NeatGenome.NeatGenome)list[x]).Behavior,
		                                                            ((NeatGenome.NeatGenome)list[last_added]).Behavior);
		            if (dist_to_last < closest[x])
		                closest[x] = dist_to_last;
		            
		            if (closest[x] > mostNovel)
		            {
		                mostNovel = closest[x];
		                mostNovelIndex = x;
		            }
		        }
		        
		        dirty[mostNovelIndex] = true;
		        newList.Add(new NeatGenome.NeatGenome((NeatGenome.NeatGenome)list[mostNovelIndex],0));
		        last_added = mostNovelIndex;
		    }

		    measure_against = newList;
		}
	}
}
