/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMindexing.

QL4BIMindexing is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMindexing is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMindexing. If not, see <http://www.gnu.org/licenses/>.
*/

//   RTree.java
//   Java Spatial Index Library
//   Copyright (C) 2002 Infomatiq Limited
//   Copyright (C) 2008 Aled Morris aled@sourceforge.net
//  
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//  
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//  
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

//  Ported to C# By Dror Gluska, April 9th, 2009


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QL4BIMprimitives;

namespace QL4BIMindexing
{

    /// <summary>
    /// This is a lightweight RTree implementation, specifically designed 
    /// for the following features (in order of importance): 
    ///
    /// Fast intersection query performance. To achieve this, the RTree 
    /// uses only main memory to store entries. Obviously this will only improve
    /// performance if there is enough physical memory to avoid paging.
    /// Low memory requirements.
    /// Fast add performance.
    ///
    ///
    /// The main reason for the high speed of this RTree implementation is the 
    /// avoidance of the creation of unnecessary objects, mainly achieved by using
    /// primitive collections from the trove4j library.
    /// author aled@sourceforge.net
    /// version 1.0b2p1
    /// Ported to C# By Dror Gluska, April 9th, 2009
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class RTree<T> : IHasBounds where T : IHasBounds
    {
        // for debugging:
        protected int SplitCount = 0;

        // parameters of the tree
        private const int DefaultMaxNodeEntries = 10;
        public int MaxNodeEntries { get; private set; }
        public int MinNodeEntries { get; private set; }

        public static int MaxNodeEntriesGlobal { get; set; }
        public static int MinNodeEntriesGlobal { get; set; }

        // map of nodeId -&gt; Node&lt;T&gt; object
        // [x] TODO eliminate this map - it should not be needed. Nodes
        // can be found by traversing the tree.
        private readonly Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();

        public Box[] GetAllBoxes ()
        {
            var boxes = new List<Box>();
            foreach (var node in nodeMap.Values.ToArray())
                boxes.Add(node.Bounds);

            return boxes.ToArray();
        }

        protected Dictionary<int, Tuple<Node, int>> ParentMap = new Dictionary<int, Tuple<Node, int>>();

        // initialisation
        protected int TreeHeight = 1; // leaves are always level 1
        protected int RootNodeId = 0;
        private int count = 0;

        // Enables creation of new nodes
        //private int highestUsedNodeId = rootNodeId; 
        private int highestUsedId = 0;

        // Deleted Node&lt;T&gt; objects are retained in the nodeMap, 
        // so that they can be reused. Store the IDs of nodes
        // which can be reused.
        private readonly Stack<int> deletedIds = new Stack<int>();

        //Added dictionaries to support generic objects..
        //possibility to change the code to support objects without dictionaries.
        private readonly Dictionary<int, T> idsToItems = new Dictionary<int, T>();
        private readonly Dictionary<T, int> itemsToIds = new Dictionary<T, int>();

        //the recursion methods require a delegate to retrieve data
        private delegate void FoundDelegate(T item);

        /// <summary>
        /// Initialize implementation dependent properties of the RTree.
        /// </summary>
        public RTree()
        {
            this.MinNodeEntries = MinNodeEntriesGlobal;
            this.MaxNodeEntries = MaxNodeEntriesGlobal;
            Init();
        }

        static RTree()
        {
            MinNodeEntriesGlobal = 4;
            MaxNodeEntriesGlobal = 10;
        }


        private void Init()
        {


            // Obviously a Node&lt;T&gt; with less than 2 entries cannot be split.
            // The Node&lt;T&gt; splitting algorithm will work with only 2 entries
            // per node, but will be inefficient.
            if (MaxNodeEntries < 2)
            {
                MaxNodeEntries = DefaultMaxNodeEntries;
            }

            // The MinNodeEntries must be less than or equal to (int) (MaxNodeEntries / 2)
            if (MinNodeEntries < 1 || MinNodeEntries > MaxNodeEntries / 2)
            {
                MinNodeEntries = MaxNodeEntries / 2;
            }

            Node root = new Node(RootNodeId, 1, this);
            nodeMap.Add(RootNodeId, root);

        }

        public void Add(IEnumerable<T>  items)
        {
            foreach (var item in items)
                Add(item);
        }

        /// <summary>
        /// Adds an item to the spatial index
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            int id = GetNextId();

            idsToItems.Add(id, item);
            itemsToIds.Add(item, id);

            var entry = new LeafNode(item.Bounds, id);

            Insert(entry, 1);

            count++;
        }

        protected void Insert(ITreeItem item, int level)
        {
            Node n = (Node)ChooseNode(RootNode, item.Bounds, level);
            Node newNode = null;

            // I2 If N has less than M entries, accommodate E in N
            // If N has M entries, invoke OverflowTreatment with the level of N as a parameter [for reinsertion or split]
            if (n.Count < MaxNodeEntries)
            {
                n.Add(item);
            }
            else
            {
                newNode = OverflowTreatment(n, item);
            }

            // I3 [Propagate changes upwards] Invoke AdjustTree on L, also passing LL
            // if a split was performed
            Node rootSplit = AdjustTree(n, newNode);

            // I4 [Grow tree taller] If Node split propagation caused the root to 
            // split, create a new root whose children are the two resulting nodes.
            if (rootSplit != null)
            {
                Node oldRoot = RootNode;

                TreeHeight++;
                Node root = GetNewNode(TreeHeight);
                RootNodeId = root.ID;
                root.Add(rootSplit);
                root.Add(oldRoot);
            }
        }

        protected virtual Node OverflowTreatment(Node n, ITreeItem newItem)
        {
            SplitCount++;
            var groups = Split(((IEnumerable<ITreeItem>)n).Concat(new[] { newItem }), n.Bounds.Union(newItem.Bounds));

            n.Clear();
            n.AddRange(groups.Item1.Item1, groups.Item1.Item2);

            Node newNode = GetNewNode(n.Level);
            newNode.AddRange(groups.Item2.Item1, groups.Item2.Item2);

            return newNode;
        }


        public bool Remove(T item)
        {
            int id;
            if (!itemsToIds.TryGetValue(item, out id)) return false;

            itemsToIds.Remove(item);
            idsToItems.Remove(id);
            deletedIds.Push(id);

            var p = ParentMap[id];
            p.Item1.RemoveAt(p.Item2);
            CondenseTree(p.Item1);

            count--;

            return true;
        }

        public IEnumerator<ITreeItem> TraverseDepthFirst()
        {
            yield return Root;

            var stack = new Stack<IEnumerator<ITreeItem>>();
            stack.Push(Root.GetEnumerator());

            while (stack.Count > 0)
            {
                IEnumerator<ITreeItem> en = stack.Peek();

                if (en.MoveNext())
                {
                    ITreeItem n = en.Current;
                    yield return n;

                    if (n.CanSubdivide)
                    {
                        en = n.GetEnumerator();
                        stack.Push(en);
                    }
                }
                else { stack.Pop(); }
            }
        }

        /// <summary>
        /// Returns the path to the specified item, starting at the root.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public IEnumerable<ITreeItem> GetPath(T item)
        {
            int id;
            if (!itemsToIds.TryGetValue(item, out id)) return null;

            List<ITreeItem> path = new List<ITreeItem>();
            Node node;
            do //while (id != rootNodeId)
            {
                node = ParentMap[id].Item1;
                id = node.ID;
                path.Add(node);
            } while (id != RootNodeId);

            return (path as IEnumerable<ITreeItem>).Reverse();
        }

        /// <summary>
        /// Retrieve nearest items to a point in radius furthestDistance
        /// </summary>
        /// <param name="p">Point of origin</param>
        /// <param name="furthestDistance">maximum distance</param>
        /// <returns>List of items</returns>
        //public List<T> Nearest(Point p, double furthestDistance)
        //{
        //    List<T> retval = new List<T>();
        //    Nearest(p, delegate(int id)
        //    {
        //        retval.Add(idsToItems[id]);
        //    }, furthestDistance);
        //    return retval;
        //}


        //private void Nearest(Point p, Intproc v, double furthestDistance)
        //{
        //    Node rootNode = GetNode(rootNodeId);

        //    Nearest(p, rootNode, furthestDistance);

        //    foreach (int id in nearestIds)
        //        v(id);
        //    nearestIds.Clear();
        //}

        /// <summary>
        /// Finds all items contained or overlapped by the specified rectangle.
        /// </summary>
        /// <param name="searchArea"></param>
        /// <returns></returns>
        public IEnumerable<T> FindOverlap(Box searchArea)
        {
            List<T> foundItems = new List<T>();
            FindOverlap(searchArea, RootNode, item => foundItems.Add(item));
            return foundItems;
        }

        /// <summary>
        /// Finds all items contained by the specified rectangle.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public IEnumerable<T> FindContain(Box searchArea)
        {
            List<T> foundItems = new List<T>();
            FindContain(searchArea, RootNode, item => foundItems.Add(item));

            return foundItems;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchArea"></param>
        /// <param name="found"></param>
        /// <param name="node"></param>
        private void FindOverlap(Box searchArea, Node node, FoundDelegate found)
        {

            for (int i = 0; i < node.Count; i++)
            {
                if (searchArea.Intersects(node.Items[i].Bounds))
                {
                    if (node.IsLeaf)
                    {
                        found(GetItem(node.Items[i].ID));
                    }
                    else
                    {
                        Node childNode = GetNode(node.Items[i].ID);
                        FindOverlap(searchArea, childNode, found);
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchArea"></param>
        /// <param name="found"></param>
        /// <param name="node"></param>
        private void FindContain(Box searchArea, Node node, FoundDelegate found)
        {
            for (int i = 0; i < node.Count; i++)
            {
                if (searchArea.Contains(node.Items[i].Bounds))
                {
                    if (node.IsLeaf)
                    {
                        found(GetItem(node.Items[i].ID));
                    }
                    else
                    {
                        Node childNode = GetNode(node.Items[i].ID);
                        FindContain(searchArea, childNode, found);
                    }
                }
            }
        }


        /// <summary>
        /// Split a node. Algorithm is taken pretty much verbatim from
        /// Guttman's original paper.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="newRect"></param>
        /// <param name="newId"></param>
        protected virtual Tuple<Tuple<IEnumerable<ITreeItem>, Box>, Tuple<IEnumerable<ITreeItem>, Box>> Split(IEnumerable<ITreeItem> items, Box wholeBounds)
        {
            // [Pick first entry for each group] Apply algorithm pickSeeds to 
            // choose two entries to be the first elements of the groups. Assign
            // each to a group.


            List<ITreeItem> toAssign = new List<ITreeItem>(items);

            var seeds = PickSeeds(toAssign, wholeBounds);

            var seed1 = toAssign[seeds.Item1];
            var seed2 = toAssign[seeds.Item2];

            // first remove the latter seed, then the other
            toAssign.RemoveAt(Math.Max(seeds.Item1, seeds.Item2));
            toAssign.RemoveAt(Math.Min(seeds.Item1, seeds.Item2));


            var group1 = new List<ITreeItem>();
            group1.Add(seed1);
            var group1BB = seed1.Bounds.Copy();
            var group2 = new List<ITreeItem>();
            group2.Add(seed2);
            var group2BB = seed2.Bounds.Copy();

            // [Check if done] If all entries have been assigned, stop. If one
            // group has so few entries that all the rest must be assigned to it in 
            // order for it to have the minimum number m, assign them and stop. 
            while (toAssign.Count > 0)
            {
                if (MaxNodeEntries + 1 - group2.Count == MinNodeEntries)
                {
                    foreach (var item in toAssign)
                    {
                        group1.Add(item);
                        group1BB.Add(item.Bounds);
                    }
                    toAssign.Clear();
                    break;
                }
                if (MaxNodeEntries + 1 - group1.Count == MinNodeEntries)
                {
                    foreach (var item in toAssign)
                    {
                        group2.Add(item);
                        group2BB.Add(item.Bounds);
                    }
                    toAssign.Clear();
                    break;
                }

                // [Select entry to assign] Invoke algorithm pickNext to choose the
                // next entry to assign. Add it to the group whose covering rectangle 
                // will have to be enlarged least to accommodate it. Resolve ties
                // by adding the entry to the group with smaller area, then to the 
                // the one with fewer entries, then to either. Repeat from S2
                //PickNext(n, newNode);
                bool firstGroup;
                int nextIndex = PickNext(toAssign, group1BB, group2BB, out firstGroup);

                var next = toAssign[nextIndex];
                toAssign.RemoveAt(nextIndex);

                if (firstGroup)
                {
                    group1.Add(next);
                    group1BB.Add(next.Bounds);
                }
                else
                {
                    group2.Add(next);
                    group2BB.Add(next.Bounds);
                }
            }


            // return the determined distribution into two groups and the corresponding bounding boxes
            return Tuple.Create(Tuple.Create(group1.AsEnumerable(), group1BB), Tuple.Create(group2.AsEnumerable(), group2BB));
        }

        //int pickSeedsFirstHit = 0;
        //int pickSeedsSecondHit = 0;

        /// <summary>
        /// Pick the seeds used to split a node.
        /// Select two entries to be the first elements of the groups
        /// </summary>
        /// <param name="node"></param>
        /// <param name="newRect"></param>
        /// <param name="newId"></param>
        /// <param name="newNode"></param>

        private Tuple<int, int> PickSeeds(IList<ITreeItem> toAssign, Box wholeBounds)
        {
            // Find extreme rectangles along all dimension. Along each dimension,
            // find the entry whose rectangle has the highest low side, and the one 
            // with the lowest high side. Record the separation.
            double maxNormalizedSeparation = 0;
            int highestLowIndex = 0;
            int lowestHighIndex = 0;

            for (int d = 0; d < wholeBounds.Dimensions; d++)
            {
                double tempHighestLow = double.NegativeInfinity;
                int tempHighestLowIndex = -1;

                double tempLowestHigh = double.PositiveInfinity;
                int tempLowestHighIndex = -1;

                //double tempHighestLow2 = double.NegativeInfinity;
                //int tempHighestLowIndex2 = -1;

                //double tempLowestHigh2 = double.PositiveInfinity;
                //int tempLowestHighIndex2 = -1;

                for (int i = 0; i < toAssign.Count; i++)
                {
                    double tempLow = toAssign[i].Bounds.GetMin(d);
                    //double tempHigh = toAssign[i].Bounds.GetMax(d);
                    if (tempLow > tempHighestLow)
                    {
                        tempHighestLow = tempLow;
                        tempHighestLowIndex = i;
                    }
                    //if (tempHigh < tempLowestHigh2)
                    //{
                    //    tempLowestHigh2 = tempHigh;
                    //    tempLowestHighIndex2 = i;
                    //}

                }
                for (int i = 0; i < toAssign.Count; i++)
                {
                    //double tempLow = toAssign[i].Bounds.GetMin(d);
                    double tempHigh = toAssign[i].Bounds.GetMax(d);

                    // ensures that the same index cannot be both lowestHigh and highestLow
                    if (tempHighestLowIndex != i && tempHigh < tempLowestHigh)
                    {
                        tempLowestHigh = tempHigh;
                        tempLowestHighIndex = i;
                    }

                    // ensures that the same index cannot be both lowestHigh and highestLow
                    //if (tempLowestHighIndex2 != i &&tempLow > tempHighestLow2)
                    //{
                    //    tempHighestLow2 = tempLow;
                    //    tempHighestLowIndex2 = i;
                    //}
                }

                double separation = Math.Abs(tempHighestLow - tempLowestHigh);
                //double separation2 = Math.Abs(tempHighestLow2 - tempLowestHigh2);
                //double separation = tempHighestLow - tempLowestHigh;
                //double separation2 = tempHighestLow2 - tempLowestHigh2;

                //if (separation2 > separation)
                //{
                //    separation = separation2;
                //    tempLowestHighIndex = tempLowestHighIndex2;
                //    tempHighestLowIndex = tempHighestLowIndex2;
                //    pickSeedsSecondHit++;
                //}
                //else pickSeedsFirstHit++;

                if (tempHighestLowIndex == tempLowestHighIndex)//|| tempHighestLowIndex2 == tempLowestHighIndex2)
                    throw new InvalidOperationException("tempHighestLowIndex == tempLowestHighIndex");


                // PS2 [Adjust for shape of the rectangle cluster] Normalize the separations
                // by dividing by the widths of the entire set along the corresponding
                // dimension
                double normalizedSeparation = separation / (wholeBounds.GetMax(d) - wholeBounds.GetMin(d));

                //if (normalizedSeparation > 1 || normalizedSeparation < -1)
                if (normalizedSeparation > 1)
                {
                    throw new InvalidOperationException("PickSeeds: Invalid normalized separation");
                }

                // PS3 [Select the most extreme pair] Choose the pair with the greatest
                // normalized separation along any dimension.
                if (normalizedSeparation > maxNormalizedSeparation)
                {
                    maxNormalizedSeparation = normalizedSeparation;
                    highestLowIndex = tempHighestLowIndex;
                    lowestHighIndex = tempLowestHighIndex;
                }

            }

            return Tuple.Create(lowestHighIndex, highestLowIndex);
        }




        /// <summary>
        /// Pick the next entry to be assigned to a group during a Node&lt;T&gt; split.
        /// [Determine cost of putting each entry in each group] For each 
        /// entry not yet in a group, calculate the area increase required
        /// in the covering rectangles of each group  
        /// </summary>
        /// <param name="n"></param>
        /// <param name="newNode"></param>
        /// <returns></returns>

        private int PickNext(IList<ITreeItem> toAssign, Box bounds1, Box bounds2, out bool firstGroup)
        {
            double maxDifference = double.NegativeInfinity;
            int next = -1;
            firstGroup = true;

            maxDifference = double.NegativeInfinity;


            for (int i = 0; i < toAssign.Count; i++)
            {
                double firstIncrease = bounds1.Enlargement(toAssign[i].Bounds);
                double secondIncrease = bounds2.Enlargement(toAssign[i].Bounds);
                double difference = Math.Abs(firstIncrease - secondIncrease);

                if (difference > maxDifference)
                {
                    next = i;

                    if (firstIncrease < secondIncrease)
                    {
                        firstGroup = true;
                    }
                    else if (secondIncrease < firstIncrease)
                    {
                        firstGroup = false;
                    }
                    else if (bounds1.Area < bounds2.Area)
                    {
                        firstGroup = true;
                    }
                    else if (bounds2.Area < bounds1.Area)
                    {
                        firstGroup = false;
                    }
                    else
                    {
                        firstGroup = false;
                    }
                    maxDifference = difference;
                }
            }

            return next;
        }


        /// <summary>
        /// Recursively searches the tree for the nearest entry. Other queries
        /// call execute() on an IntProcedure when a matching entry is found; 
        /// however nearest() must store the entry Ids as it searches the tree,
        /// in case a nearer entry is found.
        /// Uses the member variable nearestIds to store the nearest
        /// entry IDs.
        /// </summary>
        /// <remarks>TODO rewrite this to be non-recursive?</remarks>
        /// <param name="p"></param>
        /// <param name="n"></param>
        /// <param name="nearestDistance"></param>
        /// <returns></returns>
        //private double Nearest(Point p, Node n, double nearestDistance)
        //{
        //    for (int i = 0; i < n.EntryCount; i++)
        //    {
        //        double tempDistance = n.Entries[i].Distance(p);
        //        if (n.IsLeaf)
        //        { // for leaves, the distance is an actual nearest distance 
        //            if (tempDistance < nearestDistance)
        //            {
        //                nearestDistance = tempDistance;
        //                nearestIds.Clear();
        //            }
        //            if (tempDistance <= nearestDistance)
        //            {
        //                nearestIds.Add(n.Ids[i]);
        //            }
        //        }
        //        else
        //        { // for index nodes, only go into them if they potentially could have
        //            // a rectangle nearer than actualNearest
        //            if (tempDistance <= nearestDistance)
        //            {
        //                // search the child node
        //                nearestDistance = Nearest(p, GetNode(n.Ids[i]), nearestDistance);
        //            }
        //        }
        //    }
        //    return nearestDistance;
        //}



        /**
         * Used by delete(). Ensures that all nodes from the passed node
         * up to the root have the minimum number of entries.
         * 
         * Note that the parent and parentEntry stacks are expected to
         * contain the nodeIds of all parents up to the root.
         */

        //private int callCount;

        protected void CondenseTree(Node l)
        {
            // CT1 [Initialize] Set n=l. Set the list of eliminated
            // nodes to be empty.
            Node n = l;
            Node parent = null;
            int parentEntry = 0;

            Stack<Node> eliminatedNodes = new Stack<Node>();

            // CT2 [Find parent entry] If N is the root, go to CT6. Otherwise 
            // let P be the parent of N, and let En be N's entry in P  
            while (n.Level != TreeHeight)
            {
                var p = ParentMap[n.ID];
                parent = p.Item1;
                parentEntry = p.Item2;

                // CT3 [Eliminiate under-full node] If N has too few entries,
                // delete En from P and add N to the list of eliminated nodes
                if (n.Count < MinNodeEntries)
                {
                    parent.RemoveAt(parentEntry);
                    eliminatedNodes.Push(n);
                }
                else
                {
                    // CT4 [Adjust covering rectangle] If N has not been eliminated,
                    // adjust EnI to tightly contain all entries in N
                    parent.Bounds = Box.Union(parent as IEnumerable<IHasBounds>);
                }
                // CT5 [Move up one level in tree] Set N=P and repeat from CT2
                n = parent;
            }

            // CT6 [Reinsert orphaned entries] Reinsert all entries of nodes in set Q.
            // Entries from eliminated leaf nodes are reinserted in tree leaves as in 
            // Insert(), but entries from higher level nodes must be placed higher in 
            // the tree, so that leaves of their dependent subtrees will be on the same
            // level as leaves of the main tree
            while (eliminatedNodes.Count > 0)
            {
                Node e = eliminatedNodes.Pop();
                foreach (var item in e)
                {
                    Insert(item, e.Level);
                }
                e.Clear();
                deletedIds.Push(e.ID);
                nodeMap.Remove(e.ID);
            }

            // [Shorten tree.] If the root has only one child after the tree has been adjusted, make the child the new root
            if (RootNode.Count == 1)
            {
                var oldRoot = RootNode;

                deletedIds.Push(oldRoot.ID);
                nodeMap.Remove(oldRoot.ID);

                RootNodeId = oldRoot.Items[0].ID;
                ParentMap.Remove(RootNodeId);
                TreeHeight--;
            }
        }

        /// <summary>
        /// Starting at the specified Node, chooses the node at the desired level which accomodates best the given rectangle.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="r">The rectangle to accomodate.</param>
        /// <param name="level"></param>
        /// <returns></returns>
#if UNITTEST
        public virtual ITreeItem ChooseNode(ITreeItem n, Rectangle r, int level)
#else
        protected virtual ITreeItem ChooseNode(ITreeItem n, Box r, int level)
#endif
        {
            // CL1 [Initialize] Set N to be the root node

            // CL2 [Leaf check] If N is a leaf, return N
            while (n.Level != level)
            {

                // CL3 [Choose subtree] If N is not at the desired level, let F be the entry in N 
                // whose rectangle FI needs least enlargement to include EI. Resolve
                // ties by choosing the entry with the rectangle of smaller area.
                double leastEnlargement = double.PositiveInfinity;
                ITreeItem chosen = null;
                foreach (var child in n)
                {
                    Box tempBox = child.Bounds;
                    double tempEnlargement = tempBox.Enlargement(r);
                    if ((tempEnlargement < leastEnlargement) ||
                       ((tempEnlargement == leastEnlargement) &&
                       (tempBox.Area < chosen.Bounds.Area)))
                    {
                        chosen = child;
                        leastEnlargement = tempEnlargement;
                    }
                }

                // CL4 [Descend until a leaf is reached] Set N to be the child Node&lt;T&gt; 
                // pointed to by Fp and repeat from CL2
                n = chosen;
            }

            return n;
        }

        /**
         * Ascend from a leaf Node&lt;T&gt; L to the root, adjusting covering rectangles and
         * propagating Node&lt;T&gt; splits as necessary.
         */
        protected Node AdjustTree(Node n, Node nn)
        {
            // AT1 [Initialize] Set N=L. If L was split previously, set NN to be 
            // the resulting second node.

            // AT2 [Check if done] If N is the root, stop
            while (n.Level != TreeHeight)
            {

                // AT3 [Adjust covering rectangle in parent entry] Let P be the parent 
                // Node of N, and let En be N's entry in P. Adjust EnI so that it tightly
                // encloses all entry rectangles in N.


                var p = ParentMap[n.ID];
                Node parent = p.Item1;
                int entry = p.Item2;

                // as only n was modified, n is the only entry in parent with possibly enlarged bounds
                // hence, it is sufficient to add n's bounds to parent's bounds:
                parent.Bounds.Add(n.Bounds);

                if (parent.Items[entry] != n)
                {
                    throw new InvalidOperationException(String.Format("AdjustTree: Error: entry {0} in Node {1} should point to Node {2}, but actually points to Node {3}.",
                                 entry, parent.ID, n.ID, parent.Items[entry].ID));
                }

                // AT4 [Propagate Node split upward] If N has a partner NN resulting from 
                // an earlier split, create a new entry Enn with Ennp pointing to NN and 
                // Enni enclosing all rectangles in NN. Add Enn to P if there is room. 
                // Otherwise, invoke splitNode to produce P and PP containing Enn and
                // all P's old entries.
                Node newNode = null;
                if (nn != null)
                {
                    if (parent.Count < MaxNodeEntries)
                    {
                        parent.Add(nn);
                    }
                    else
                    {
                        newNode = OverflowTreatment(parent, nn);
                    }
                }

                // AT5 [Move up to next level] Set N = P and set NN = PP if a split 
                // occurred. Repeat from AT2
                n = parent;
                nn = newNode;
            }

            return nn;
        }


        public int Count
        {
            get
            {
                return count;
            }
        }

        public Box Bounds
        {
            get
            {
                return RootNode.Bounds;
            }
        }

        protected int GetNextId()
        {
            int id = 0;
            if (deletedIds.Count > 0)
            {
                id = deletedIds.Pop();
            }
            else
            {
                id = ++highestUsedId;
            }
            return id;
        }

        protected Node GetNewNode(int level)
        {
            Node newNode = new Node(GetNextId(), level, this);
            nodeMap[newNode.ID] = newNode;
            return newNode;
        }


        public Node GetNode(int id)
        {
            return nodeMap[id];
        }

        public Node RootNode
        {
            get { return nodeMap[RootNodeId]; }
        }

        public ITreeItem Root
        {
            get { return RootNode; }
        }

        public T GetItem(int id)
        {
            return idsToItems[id];
        }

        public T GetAnyItem()
        {
            return idsToItems.First().Value;
        }

        public Tuple<string, string> BreathFirstBoxIterator()
        {
            int i = 0;
            var nodeDelimiter = Environment.NewLine;

            var stringBuilderGetChildren = new StringBuilder();
            RootNode.Bounds.NameId = i;

            var queue = new Queue<ITreeItem>();
            queue.Enqueue(RootNode);

            var first = true;

            var stringBuilderGetParent = new StringBuilder();

            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();

                if(!first)
                    stringBuilderGetChildren.Append(nodeDelimiter);

                stringBuilderGetChildren.Append(currentNode.Bounds.NameId + " ");
                first = false;

                var j = 0;
                foreach (var treeitem in (currentNode as Node).Items)
                {
                    
                    if(treeitem == null)
                        continue;

                    i++;
                    treeitem.Bounds.NameId = i;

                    if ((treeitem as Node) != null)
                    {
                        queue.Enqueue(treeitem as Node);

                        if (j == 0)
                            stringBuilderGetChildren.Append("-1 " + treeitem.Bounds.NameId + " ");
                        else
                            stringBuilderGetChildren.Append(treeitem.Bounds.NameId + " ");

                        stringBuilderGetParent.Append(treeitem.Bounds.NameId + " " + currentNode.Bounds.NameId);
                    }

                    if ((treeitem as LeafNode) != null)
                    {
                        var id = treeitem.ID;
                        var tri = GetItem(id) as Triangle;
                   
                       if(j == 0)
                            stringBuilderGetChildren.Append("-2 " + tri.Id + " ");
                        else
                            stringBuilderGetChildren.Append(tri.Id + " ");


                       stringBuilderGetParent.Append(-tri.Id + " " + currentNode.Bounds.NameId);
                    }

                    j++;


                    stringBuilderGetParent.Append(Environment.NewLine);
                }

            }

            return new Tuple<string, string>(stringBuilderGetChildren.ToString(), stringBuilderGetParent.ToString());

        }
    }
}
