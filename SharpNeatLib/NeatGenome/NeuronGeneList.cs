using System;
using System.Collections.Generic;


namespace SharpNeatLib.NeatGenome
{
	public class NeuronGeneList : List<NeuronGene>
	{
		static NeuronGeneComparer neuronGeneComparer = new NeuronGeneComparer();
		public bool OrderInvalidated=false;

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		public NeuronGeneList()
		{}

        public NeuronGeneList(int count)
        {
            Capacity = (int)(count*1.5);
        }

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="copyFrom"></param>
		public NeuronGeneList(NeuronGeneList copyFrom)
		{
			int count = copyFrom.Count;
			Capacity = count;
			
			for(int i=0; i<count; i++)
				Add(new NeuronGene(copyFrom[i]));

//			foreach(NeuronGene neuronGene in copyFrom)
//				InnerList.Add(new NeuronGene(neuronGene));
		}

		#endregion

		#region Public Methods


		new public void Remove(NeuronGene neuronGene)
		{
			Remove(neuronGene.InnovationId);

			// This invokes a linear search. Invoke our binary search instead.
			//InnerList.Remove(neuronGene);
		}

		public void Remove(uint neuronId)
		{
			int idx = BinarySearch(neuronId);
            if (idx >= 0)
            {
                RemoveAt(idx);
            }
            else
            { // If BinarySearch failed, we should try a linear search just to make sure it's not there.... (have had problems with BinarySearch failing in the past)
                int bound = this.Count;
                for (int i = bound - 1; i >= 0; i--) // Traverse in reverse because we are ALMOST CERTAINLY looking for the last element (our BinSearch implementation can't find the last element? wtf)
                {
                    if (((NeuronGene)this[i]).InnovationId == neuronId)
                    {
                        //Console.WriteLine("In Remove(" + neuronId + "), binary search failed where linear search did not. Strange.");
                        //foreach (NeuronGene gg in this) Console.Write(gg.InnovationId + " "); Console.WriteLine(); // A HA! It's always the last element that we failed to find with Binary Search.
                        RemoveAt(i);
                        return;
                    }
                }
                // Still not found? Okay, you may error now..
                throw new ApplicationException("Attempt to remove neuron with an unknown neuronId");
            }
			/*if(idx<0)
				throw new ApplicationException("Attempt to remove neuron with an unknown neuronId");
			else
				RemoveAt(idx);//*/

//			// Inefficient scan through the neuron list.
//			// TODO: Implement a binary search method for NeuronList (Will generics resolve this problem anyway?).
//			int bound = List.Count;
//			for(int i=0; i<bound; i++)
//			{
//				if(((NeuronGene)List[i]).InnovationId == neuronId)
//				{
//					InnerList.RemoveAt(i);
//					return;
//				}
//			}
//			throw new ApplicationException("Attempt to remove neuron with an unknown neuronId");
		}

		public NeuronGene GetNeuronById(uint neuronId)
		{
			int idx = BinarySearch(neuronId);
            if (idx >= 0)
            {
                return this[idx];
            }
            else
            { // BinarySearch did not find it, but we should try linear search to make sure it is not actually there. Have had problems with Binary Search failing in the past...
                int bound = this.Count;
                for (int i = bound-1; i >= 0; i--) // Traverse in reverse because we are ALMOST CERTAINLY looking for the last element (our BinSearch implementation can't find the last element? wtf)
                {
                    if (((NeuronGene)this[i]).InnovationId == neuronId)
                    {
                        //Console.WriteLine("In GetNeuronById(" + neuronId + "), binary search failed where linear search did not. Strange.");
                        //foreach (NeuronGene gg in this) Console.Write(gg.InnovationId + " "); Console.WriteLine();
                        return (NeuronGene)this[i];
                    }
                }

                // Still not found. Returning null here will cause an error. Good.
                return null;

            }
			//if(idx<0)
			//	return null;
			//else
			//	return this[idx];
		}

		public void SortByInnovationId()
		{
			Sort(neuronGeneComparer);
			OrderInvalidated=false;
		}

		public int BinarySearch(uint innovationId) 
		{            
			int lo = 0;
			int hi = Count-1;

			while (lo <= hi) 
			{
				int i = (lo + hi) >> 1; // Look, a teeny optimization at large expense to code readability #prostatus  -Justin

				if(this[i].InnovationId<innovationId)
					lo = i + 1;
				else if(this[i].InnovationId>innovationId)
					hi = i - 1;
				else
					return i;


				// TODO: This is wrong. It will fail for large innovation numbers because they are of type uint.
				// Fortunately it's very unlikely anyone has reached such large numbers!
//				int c = (int)((NeuronGene)InnerList[i]).InnovationId - (int)innovationId;
//				if (c == 0) return i;
//
//				if (c < 0) 
//					lo = i + 1;
//				else 
//					hi = i - 1;
			}
			
			return ~lo;
		}

		// For debug purposes only.
//		public bool IsSorted()
//		{
//			uint prevId=0;
//			foreach(NeuronGene gene in InnerList)
//			{
//				if(gene.InnovationId<prevId)
//					return false;
//				prevId = gene.InnovationId;
//			}
//			return true;
//		}

		#endregion
	}
}
