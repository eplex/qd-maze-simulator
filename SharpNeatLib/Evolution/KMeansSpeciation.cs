using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeatGenome;

namespace SharpNeatLib.Evolution
{
    /// <summary>
    /// This class was mostly ported from SharpNeat2 (original author: Colin Green) with modifications erring on the side of being less generic and less flexible, but easier to implement.
    /// </summary>
    public class KMeansSpeciation
    {
        const int __MAX_KMEANS_LOOPS = 5;
        // Change these constants to adjust from classical manhattan distance (1, 1, 0) to classical NEAT speciation (?, ?, ?)
        const double _matchDistanceCoeff = 1.0;
        const double _mismatchDistanceCoeff = 1.0;
        const double _mismatchDistanceConstant = 0.0;


        #region Public Methods

        /// <summary>
        /// Speciates the genomes in genomeList into the number of species specified by specieCount
        /// and returns a newly constructed list of Species objects containing the speciated genomes.
        /// </summary>
        public List<Species> InitializeSpeciation(List<IGenome> genomeList, int specieCount)
        {
            // Create empty specieList.
            List<Species> specieList = new List<Species>(specieCount);
            for (int i = 0; i < specieCount; i++)
            {
                Species specie = new Species();
                specie.Centroid = new SortedDictionary<double, double>();
                specie.SpeciesId = i; // JUSTIN: Is this a good idea? Or should we use a unique id for species? (nah, I think we should use the index..)
                specieList.Add(specie);
            }

            // Speciate genomes into the empty species.
            SpeciateGenomes(genomeList, specieList);
            return specieList;
        }

        /// <summary>
        /// Speciates the genomes in genomeList into the provided specieList. It is assumed that
        /// the genomeList represents all of the required genomes and that the species are currently empty.
        /// 
        /// This method can be used for initialization or completely respeciating an existing genome population.
        /// </summary>
        public void SpeciateGenomes(List<IGenome> genomeList, List<Species> specieList)
        {
            Debug.Assert(genomeList.Count >= specieList.Count, string.Format("SpeciateGenomes(IList<TGenome>,IList<Species<TGenome>>). Species count [{0}] is greater than genome count [{1}].", specieList.Count, genomeList.Count));

            // Randomly allocate the first k genomes to their own specie. Because there is only one genome in these
            // species each genome effectively represents a specie centroid. This is necessary to ensure we get k species.
            // If we randomly assign all genomes to species from the outset and then calculate centroids then typically some
            // of the species become empty.
            // This approach ensures that each species will have at least one genome - because that genome is the specie 
            // centroid and therefore has distance of zero from the centroid (itself).
            int specieCount = specieList.Count;
            for (int i = 0; i < specieCount; i++)
            {
                Species specie = specieList[i];
                genomeList[i].SpeciesId = specie.SpeciesId;
                specie.Members.Add(genomeList[i]);

                // Just set the specie centroid directly.
                
                // The centroid is a dictionary keyed on connection innovation IDs containing the weights as values
                NeatGenome.NeatGenome g = genomeList[i] as NeatGenome.NeatGenome;
                foreach (ConnectionGene cg in g.ConnectionGeneList)
                {
                    if (specie.Centroid.ContainsKey(cg.InnovationId)) Console.WriteLine("[!] Duplicate Innovation ID detected within same genome!"); // JUSTIN: DEBUG: This shouldn't happen, so remove it once we are sure that it doesn't.
                    specie.Centroid[cg.InnovationId] = cg.Weight;
                }
            }

            // Now allocate the remaining genomes based on their distance from the centroids.
            int genomeCount = genomeList.Count;
            for (int i = specieCount; i < genomeCount; i++)
            {
                IGenome genome = genomeList[i];
                Species closestSpecie = FindClosestSpecie(genome, specieList);
                genome.SpeciesId = closestSpecie.SpeciesId;
                closestSpecie.Members.Add(genome);
            }

            // Recalculate each specie's centroid.
            foreach (Species specie in specieList)
            {
                specie.Centroid = CalculateSpecieCentroid(specie);
            }

            // Perform the main k-means loop until convergence.
            SpeciateUntilConvergence(genomeList, specieList);
        }

        /// <summary>
        /// Speciates the offspring genomes in offspringList into the provided specieList. In contrast to
        /// SpeciateGenomes() offspringList is taken to be a list of new genomes (offspring) that should be 
        /// added to existing species. That is, the species contain genomes that are not in offspringList
        /// that we wish to keep; typically these would be elite genomes that are the parents of the
        /// offspring.
        /// </summary>
        public void SpeciateOffspring(List<IGenome> offspringList, List<Species> specieList)
        {
            // Update the centroid of each specie. If we're adding offspring this means that old genomes 
            // have been removed from the population and therefore the centroids are out-of-date.
            foreach (Species specie in specieList)
            {
                specie.Centroid = CalculateSpecieCentroid(specie);
            }

            // Allocate each offspring genome to the specie it is closest to. 
            foreach (IGenome genome in offspringList)
            {
                Species closestSpecie = FindClosestSpecie(genome, specieList);
                closestSpecie.Members.Add(genome);
                genome.SpeciesId = closestSpecie.SpeciesId;
            }

            // Recalculate each specie's centroid now that we have additional genomes in the specieList.
            foreach (Species specie in specieList)
            {
                specie.Centroid = CalculateSpecieCentroid(specie);
            }

            // Accumulate *all* genomes into a flat genome list.
            int genomeCount = 0;
            foreach (Species specie in specieList)
            {
                genomeCount += specie.Members.Count;
            }

            List<IGenome> genomeList = new List<IGenome>(genomeCount);
            foreach (Species specie in specieList)
            {
                genomeList.AddRange(specie.Members);
            }

            // Perform the main k-means loop until convergence.
            SpeciateUntilConvergence(genomeList, specieList);
        }

        #endregion

        #region Private Methods [k-means]

        /// <summary>
        /// Perform the main k-means loop until no genome reallocations occur or some maximum number of loops
        /// has been performed. Theoretically a small number of reallocations may occur for a great many loops 
        /// therefore we require the additional max loops threshold exit strategy - the clusters should be pretty
        /// stable and well defined after a few loops even if the the algorithm hasn't converged completely.
        /// </summary>
        private void SpeciateUntilConvergence(List<IGenome> genomeList, List<Species> specieList)
        {
            List<Species> emptySpecieList = new List<Species>();
            int specieCount = specieList.Count;

            // Array of flags that indicate if a specie was modified (had genomes allocated to and/or from it).
            bool[] specieModArr = new bool[specieCount];

            // Main k-means loop.
            for (int loops = 0; loops < __MAX_KMEANS_LOOPS; loops++)
            {
                // Track number of reallocations made on each loop.
                int reallocations = 0;

                // Loop over genomes. For each one find the specie it is closest to; if it is not the specie
                // it is currently in then reallocate it.
                foreach (IGenome genome in genomeList)
                {
                    Species closestSpecie = FindClosestSpecie(genome, specieList);
                    if (genome.SpeciesId != closestSpecie.SpeciesId)
                    {
                        // Track which species have been modified.
                        specieModArr[genome.SpeciesId] = true;
                        specieModArr[closestSpecie.SpeciesId] = true;

                        // Add the genome to its new specie and set its speciesId accordingly.
                        // For now we leave the genome in its original species; It's more efficient to determine
                        // all reallocations and then remove reallocated genomes from their origin specie all together;
                        // This is because we can shuffle down the remaining genomes in a specie to fill the gaps made by
                        // the removed genomes - and do so in one round of shuffling instead of shuffling to fill a gap on
                        // each remove.
                        closestSpecie.Members.Add(genome);
                        genome.SpeciesId = closestSpecie.SpeciesId;
                        reallocations++;
                    }
                }

                // Complete the reallocations.
                for (int i = 0; i < specieCount; i++)
                {
                    if (!specieModArr[i])
                    {   // Specie not changed. Skip.
                        continue;
                    }

                    // Reset flag.
                    specieModArr[i] = false;

                    // Remove the genomes that have been allocated to other other species. We fill the resulting 
                    // gaps by shuffling down the remaining genomes.
                    Species specie = specieList[i];
                    specie.Members.RemoveAll(delegate(IGenome genome)
                    {
                        return genome.SpeciesId != specie.SpeciesId;
                    });

                    // Track empty species. We will allocate genomes to them after this loop.
                    // This is necessary as some distance metrics can result in empty species occuring.
                    if (0 == specie.Members.Count)
                    {
                        emptySpecieList.Add(specie);
                    }
                    else
                    {
                        // Recalc the specie centroid now that it contains a different set of genomes.
                        specie.Centroid = CalculateSpecieCentroid(specie);
                    }
                }

                // Check for empty species. We need to reallocate some genomes into the empty specieList to maintain the 
                // required number of species.
                if (0 != emptySpecieList.Count)
                {
                    // We find the genomes in the population as a whole that are furthest from their containing specie's 
                    // centroid genome - we call these outlier genomes. We then move these genomes into the empty species to
                    // act as the sole member and centroid of those speciea; These act as specie seeds for the next k-means loop.
                    IGenome[] genomeByDistanceArr = GetGenomesByDistanceFromSpecie(genomeList, specieList);

                    // Reallocate each of the outlier genomes from their current specie to an empty specie.
                    int emptySpecieCount = emptySpecieList.Count;
                    int outlierIdx = 0;
                    for (int i = 0; i < emptySpecieCount; i++)
                    {
                        // Find the next outlier genome that can be re-allocated. Skip genomes that are the
                        // only member of a specie - that would just create another empty specie.
                        IGenome genome;
                        Species sourceSpecie;
                        do
                        {
                            genome = genomeByDistanceArr[outlierIdx++];
                            sourceSpecie = specieList[genome.SpeciesId];
                        }
                        while (sourceSpecie.Members.Count == 1 && outlierIdx < genomeByDistanceArr.Length);

                        if (outlierIdx == genomeByDistanceArr.Length)
                        {   // Theoretically impossible. We do the test so that we get an easy to trace error message if it does happen.
                            throw new Exception("Error finding outlier genome. No outliers could be found in any specie with more than 1 genome.");
                        }

                        // Get ref to the empty specie and register both source and target specie with specieModArr.
                        Species emptySpecie = emptySpecieList[i];
                        specieModArr[emptySpecie.SpeciesId] = true;
                        specieModArr[sourceSpecie.SpeciesId] = true;

                        // Reallocate the genome. Here we do the remove operation right away; We aren't expecting to deal with many empty
                        // species, usually it will be one or two at most; Any more and there's probably something wrong with the distance
                        // metric, e.g. maybe it doesn't satisfy the triangle inequality (see wikipedia).
                        // Another reason to remove right is to eliminate the possibility of removing multiple outlier genomes from the 
                        // same specie and potentially leaving it empty; The test in the do-while loop above only takes genomes from
                        // currently non-empty species.
                        sourceSpecie.Members.Remove(genome);
                        emptySpecie.Members.Add(genome);
                        genome.SpeciesId = emptySpecie.SpeciesId;
                        reallocations++;
                    }

                    // Recalculate centroid for all affected species.
                    for (int i = 0; i < specieCount; i++)
                    {
                        if (specieModArr[i])
                        {   // Reset flag while we're here. Do this first to help maintain CPU cache coherency (we just tested it).
                            specieModArr[i] = false;
                            specieList[i].Centroid = CalculateSpecieCentroid(specieList[i]);
                        }
                    }

                    // Clear emptySpecieList after using it. Otherwise we are holding old references and thus creating
                    // work for the garbage collector.
                    emptySpecieList.Clear();
                }

                // Exit the loop if no genome reallocations have occured. The species are stable, speciation is completed.
                if (0 == reallocations)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Recalculate the specie centroid based on the genomes currently in the specie.
        /// This is the euclidean distance centroid, which is average vector across all dimensions
        /// </summary>
        private SortedDictionary<double,double> CalculateSpecieCentroid(Species specie)
        {
            specie.Centroid.Clear();

            double numInSpecies = specie.Members.Count; // we will pre-divide every weight by this factor before adding it to the centroid, so by the end of the summation we will have the average

            foreach (IGenome g in specie.Members)
            {
                foreach (ConnectionGene cg in (g as NeatGenome.NeatGenome).ConnectionGeneList)
                {
                    if (specie.Centroid.ContainsKey(cg.InnovationId))
                    {
                        specie.Centroid[cg.InnovationId] = specie.Centroid[cg.InnovationId] + (cg.Weight / numInSpecies);
                    }
                    else
                    {
                        specie.Centroid[cg.InnovationId] = (cg.Weight / numInSpecies);
                    }
                }
            }

            // The centroid calculation is a function of the distance metric.
            return specie.Centroid;
        }

        // ENHANCEMENT: Optimization candidate.
        /// <summary>
        /// Gets an array of all genomes ordered by their distance from their current specie.
        /// </summary>
        private IGenome[] GetGenomesByDistanceFromSpecie(List<IGenome> genomeList, List<Species> specieList)
        {
            // Build a list of all genomes paired with their distance from their centriod.
            int genomeCount = genomeList.Count;
            KeyValuePair<IGenome, double>[] genomeDistanceArr = new KeyValuePair<IGenome, double>[genomeCount];


            for (int i = 0; i < genomeCount; i++)
            {
                IGenome genome = genomeList[i];
                double distance = MeasureManhattanDistance((genome as NeatGenome.NeatGenome).ConnectionGeneList, specieList[genome.SpeciesId].Centroid);
                genomeDistanceArr[i] = new KeyValuePair<IGenome, double>(genome, distance);
            }

            // Sort list. Longest distance first.
            Array.Sort(genomeDistanceArr, (x, y) =>
            { // TODO: Does this work to produce descending order? Check it..
                if (x.Value > y.Value) return -1;
                if (x.Value < y.Value) return 1;
                return 0;
            });

            // Put the sorted genomes in an array and return it.
            IGenome[] genomeArr = new IGenome[genomeCount];
            for (int i = 0; i < genomeCount; i++)
            {
                genomeArr[i] = genomeDistanceArr[i].Key;
            }

            return genomeArr;
        }

        /// <summary>
        /// Measures the distance between two positions.
        /// </summary>
        /// // TODO: Is there a way to do this efficiently? The ported method assumes two lists sorted by ID. Perhaps I need to switch to a sorted dictionary! Is ConnectionGeneList sorted? Hmmm.
        public double MeasureManhattanDistance(ConnectionGeneList genome, SortedDictionary<double,double> centroid) // p2 = centroid,      p1 = genome [that we are measuring against the centroid]
        {
            //KeyValuePair<ulong, double>[] arr1 = genome.CoordArray;
            //KeyValuePair<ulong, double>[] arr2 = centroid.CoordArray;
            double[] centroidKeys = new double[centroid.Count];
            double[] centroidValues = new double[centroid.Count];
            centroid.Keys.CopyTo(centroidKeys, 0);
            centroid.Values.CopyTo(centroidValues, 0);
            
            // Store these heavily used values locally.
            int arr1Length = genome.Count;
            int arr2Length = centroid.Count;

            //--- Test for special cases.
            if (0 == arr1Length && 0 == arr2Length)
            {   // Both arrays are empty. No disparities, therefore the distance is zero.
                return 0.0;
            }

            double distance = 0;
            if (0 == arr1Length)
            {   // All arr2 genes are mismatches.
                for (int i = 0; i < arr2Length; i++)
                {
                    distance += Math.Abs(centroidValues[i]);
                }
                return (_mismatchDistanceConstant * arr2Length) + (distance * _mismatchDistanceCoeff);
            }

            if (0 == arr2Length)
            {   // All arr1 elements are mismatches.
                for (int i = 0; i < arr1Length; i++)
                {
                    distance += Math.Abs(genome[i].Weight);
                }
                return (_mismatchDistanceConstant * arr1Length) + (distance * _mismatchDistanceCoeff);
            }

            //----- Both arrays contain elements. 
            int arr1Idx = 0;
            int arr2Idx = 0;
            // JUSTIN: OPTIMIZATION FINISHED: Do not create two KVPs here. Instead, just reference their component values directly (copy-paste from constructor down into the code)
            //KeyValuePair<double, double> elem1 = new KeyValuePair<double, double>(genome[arr1Idx].InnovationId, genome[arr1Idx].Weight);
            //KeyValuePair<double, double> elem2 = new KeyValuePair<double, double>(centroidKeys[arr2Idx], centroidValues[arr2Idx]);
            for (; ; )
            {
                if (genome[arr1Idx].InnovationId < centroidKeys[arr2Idx])
                {
                    // p2 doesn't specify a value in this dimension therefore we take it's position to be 0.
                    distance += _mismatchDistanceConstant + (Math.Abs(genome[arr1Idx].Weight) * _mismatchDistanceCoeff);

                    // Move to the next element in arr1.
                    arr1Idx++;
                }
                else if (genome[arr1Idx].InnovationId == centroidKeys[arr2Idx])
                {
                    // Matching elements.
                    distance += Math.Abs(genome[arr1Idx].Weight - centroidValues[arr2Idx]) * _matchDistanceCoeff;

                    // Move to the next element in both arrays.
                    arr1Idx++;
                    arr2Idx++;
                }
                else // centroidKeys[arr2Idx] < genome[arr1Idx].InnovationId
                {
                    // p1 doesn't specify a value in this dimension therefore we take it's position to be 0.
                    distance += _mismatchDistanceConstant + (Math.Abs(centroidValues[arr2Idx]) * _mismatchDistanceCoeff);

                    // Move to the next element in arr2.
                    arr2Idx++;
                }

                // Check if we have exhausted one or both of the arrays.
                if (arr1Idx == arr1Length)
                {   // All remaining arr2 elements are mismatches.
                    for (int i = arr2Idx; i < arr2Length; i++)
                    {
                        distance += _mismatchDistanceConstant + (Math.Abs(centroidValues[i]) * _mismatchDistanceCoeff);
                    }
                    return distance;
                }

                if (arr2Idx == arr2Length)
                {   // All remaining arr1 elements are mismatches.
                    for (int i = arr1Idx; i < arr1Length; i++)
                    {
                        distance += _mismatchDistanceConstant + (Math.Abs(genome[i].Weight) * _mismatchDistanceCoeff);
                    }
                    return distance;
                }

                //elem1 = new KeyValuePair<double, double>(genome[arr1Idx].InnovationId, genome[arr1Idx].Weight);
                //elem2 = new KeyValuePair<double, double>(centroidKeys[arr2Idx], centroidValues[arr2Idx]);
            }
        }

        /// <summary>
        /// Find the specie that a genome is closest to as determined by the distance metric.
        /// </summary>
        private Species FindClosestSpecie(IGenome genome, List<Species> specieList)
        {
            // Measure distance to first specie's centroid.
            Species closestSpecie = specieList[0];
            double closestDistance = MeasureManhattanDistance((genome as NeatGenome.NeatGenome).ConnectionGeneList, closestSpecie.Centroid);

            // Measure distance to all remaining species.
            int speciesCount = specieList.Count;
            for (int i = 1; i < speciesCount; i++)
            {
                double distance = MeasureManhattanDistance((genome as NeatGenome.NeatGenome).ConnectionGeneList, specieList[i].Centroid);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSpecie = specieList[i];
                }
            }

            return closestSpecie;
        }

        #endregion














    } // end class

}
