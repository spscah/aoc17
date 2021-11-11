using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DigitalPlumber.App
{
    class Program
    {
        static void Main(string[] args)
        {
            string testdata = "0 <-> 2\n1 <-> 1\n2 <-> 0, 3, 4\n3 <-> 2, 4\n4 <-> 2, 3, 6\n5 <-> 6\n6 <-> 4, 5";
            Debug.Assert(testdata.ConnectedTo(0) == 6);
            string actualdata = System.IO.File.ReadAllText("pipes.txt");
            Console.WriteLine($"part 1: {actualdata.ConnectedTo(0)}"); // 306 

            Debug.Assert(testdata.CountGroups() == 2);
            Console.WriteLine($"part 2: {actualdata.CountGroups()}"); // 200 
        }
    }

    public static class StringUtils
    {
        public static int CountGroups(this string data)
        {
            // sort the data into the dictionary 
            IDictionary<int, IList<int>> pipes = data.Group();
            // we count the groups - we know it's a least 1
            int groups = 1;
            // seed the seen list with the initial group 
            IList<int> seen = pipes.FromNode(0);
            // loop while the seen list is not all the keys from the pipes' dictionary  
            while(seen.Count < pipes.Keys.Count)
            {
                // grab the first of the pipes' keys which has not been seen - using the LINQ .First method 
                int next = pipes.Keys.First(i => !seen.Contains(i));
                // combine the new list with the seen list - there should be no overlaps 
                seen = seen.Union(pipes.FromNode(next)).ToList();
                ++groups;
            }
            return groups;
        }
        /// <summary>
        /// Part 1: count the nodes from node 0
        /// </summary>
        /// <param name="data">the string data</param>
        /// <param name="node">the node from where we start - for part 1 this is 0</param>
        /// <returns>the count of the list of seen nodes</returns>
        public static int ConnectedTo(this string data, int node)
        {
            return data.Group().FromNode(node).Count;                 
        }

        /// <summary>
        /// Creates a list of all the nodes accessible eventually from the given nodes 
        /// </summary>
        /// <param name="pipes">the dictionary of sources/destinations </param>
        /// <param name="node">the node from where we start</param>
        /// <returns>the set of all accessible nodes</returns>
        public static IList<int> FromNode(this IDictionary<int, IList<int>> pipes, int node)
        {
            // we maintain a list of the nodes we've seen and those we've yet to explore 
            IList<int> seen = new List<int>();
            // starting with the first unseen node, that which we have been given 
            IList<int> unseen = new List<int> { node };
            // repeat until loop - we know we'll need to run this at least once 
            do
            {
                // the next node is the first in the list of the unseen - remove it from that list 
                // this is an implicit queue, we are in effect doing a breadth first search 
                int next = unseen[0];
                unseen.RemoveAt(0);

                // first check that it has not already been seen, i.e. checking for a cycle 
                if (!seen.Contains(next))
                {
                    // no we've seen it, add to the list of seen nodes 
                    seen.Add(next);
                    // use the set function Union to ensure that the unseen list has no duplicates 
                    // (we could probably also remove any of the seen items to mitigate the need to check above) 
                    unseen = unseen.Union(pipes[next]).ToList();
                }
            } while (unseen.Count > 0);
            // the unseen list is exhausted. We're done. 
            return seen;
        }

        /// <summary>
        /// Uses LINQ to take each string in turn, run the local splitter functions, creating a dictionary 
        /// using the source as the key and the destinations as the data 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IDictionary<int, IList<int>> Group(this string data)
        {
            return 
                data
                    .Split("\n")
                    .Select(s => Splitter(s))
                    .ToDictionary(a => a.Item1, a => a.Item2);
        }
        /// <summary>
        /// Create a tuple - the node from and a list of the connections
        /// </summary>
        /// <param name="incoming">the incoming structured data</param>
        /// <returns>a pair indicating the source and all the destinations</returns>
        public static ValueTuple<int, IList<int>> Splitter(string incoming)
        {
            IList<string> parts = new List<string>(incoming.Split(" <-> "));
            IList<int> destinations = parts[1].Split(", ").Select(i => Convert.ToInt32(i)).ToList();
            return new(Convert.ToInt32(parts[0]), destinations);
            
        }
    }
}
