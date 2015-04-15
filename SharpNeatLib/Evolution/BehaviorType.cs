// BehaviorType.cs created with MonoDevelop
// User: joel at 1:44 PM 8/5/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
namespace SharpNeatLib
{
    public class BehaviorType
    {
        public List<double> finalLocation; // JUSTIN: Used to store the final location of robots[0] (after the last trial on the last environment). Pretty much only for HardMaze-like domains.
        public List<double> behaviorList;
        public List<int> trajectory;
		public double[] objectives;
        public bool wraparoundRange = false;
        //public int uniqId;          // for cache
        //private static int nextUniqId = 0;          // for cache
        //public Dictionary<int, double> cachedDistances = new Dictionary<int, double>();          // for cache
        public BehaviorType()
        {
            //uniqId = nextUniqId; // JUSTIN: assign a unique id to this instance so we can implement a behavior-distance cache           // for cache
            //nextUniqId++;           // for cache
        }
        public BehaviorType(BehaviorType copyFrom)
        {
            if(copyFrom.behaviorList!=null)
                behaviorList = new List<double>(copyFrom.behaviorList);

            if (copyFrom.finalLocation != null)
                finalLocation = new List<double>(copyFrom.finalLocation);

            if (copyFrom.trajectory != null)
                trajectory = new List<int>(copyFrom.trajectory);
        }
    }
    
    public static class BehaviorDistance
    {
        // {Assumes the BC values are in the range 0-1} and calculates distance as if the range wrapped around.
        public static double DistanceWithWraparound(BehaviorType x, BehaviorType y)
        {
            double dist = 0.0;
            double delta;
            
            for (int k = 0; k < x.behaviorList.Count; k++)
            {
                // JUSTIN: I *think* this works....
                delta = 0.5 - Math.Abs(Math.Abs(x.behaviorList[k] - y.behaviorList[k]) - 0.5);

                dist += delta * delta;
            }
            return dist;
        }

        public static double Distance(BehaviorType x, BehaviorType y)
        {
            if (x.wraparoundRange) return DistanceWithWraparound(x, y);


            double dist = 0.0;
            for (int k = 0; k < x.behaviorList.Count; k++)
            {
                double delta = x.behaviorList[k] - y.behaviorList[k];
                dist += delta * delta;
            }
            return dist;//*/


            // JUSTIN: Below is code for a cache of distance calculations.. it doesn't help for small BCs (~2 dimensions) but probably helps a lot for large BCs.
            // Note: also need to uncomment the uniqId variables and some other stuff at the top of this file.
            // Note: this cache is quick and dirty and is subject to a memory leak since it keeps around cache values for individuals long since deleted from the archive.
            /*double dist;
            if (!x.cachedDistances.TryGetValue(y.uniqId, out dist)) // JUSTIN: Cache of distance calculations... hopefully this speeds things up and doesn't waste too much memory...
            {
                dist = 0.0;
                for (int k = 0; k < x.behaviorList.Count; k++)
                {
                    double delta = x.behaviorList[k] - y.behaviorList[k];
                    dist += delta * delta;
                }
                x.cachedDistances[y.uniqId] = dist;
            }
            return dist;//*/
        }
    }
}
