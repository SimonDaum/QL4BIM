using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMprimitives;

namespace QL4BIMindexing
{
    public class RStarTree<T> : QL4BIMindexing.RTree<T> where T : IHasBounds
    {
        private const bool forcedReinsert = true;
        private const bool closeReinsert = true;
        private readonly int reinsertEntries;

        public RStarTree() 
        {
            // let user choose?
            reinsertEntries = (int)(0.3 * MaxNodeEntriesGlobal);
        }

        // for each level, gives whether OverflowTreatment was called the first or the second time during this outer Add-process
        private readonly Dictionary<int, bool> firstOverflowOnLevel = new Dictionary<int, bool>();

        protected override Node OverflowTreatment(Node n, ITreeItem newItem)
        {
            //if (!forcedReinsert)
            //{
            //    return base.OverflowTreatment(n, newItem);
            //}

            bool first;
            if ((firstOverflowOnLevel.TryGetValue(n.Level, out first) && !first) || n.Level == TreeHeight)
            {
                // If the node is on root level or this is the second call of OverflowTreatment on this level, invoke SplitNode
                firstOverflowOnLevel[n.Level] = true;
                //splitCount++;

                return base.OverflowTreatment(n, newItem);
            }

            // If the level is not the root level and this is the first call of OverflowTreatment in the given level 
            // during the Insertion of one data rectangle, then invoke Reinsert
            firstOverflowOnLevel[n.Level] = false;

            var groups = Reinsert(((IEnumerable<ITreeItem>)n).Concat(new[] { newItem }), n.Bounds.Union(newItem.Bounds));

            // the first group of items remains in n:
            n.Clear();
            n.AddRange(groups.Item1);

            // propagate the modification of n's bounds upwards:
            int upID = n.ID;
            while (upID != RootNodeId)
            {
                var p = ParentMap[upID];
                p.Item1.Bounds = Box.Union(p.Item1 as IEnumerable<IHasBounds>);
                upID = p.Item1.ID;
            }

            // the second group of items is reinserted:
            foreach (var item in groups.Item2)
            {
                Insert(item, n.Level);
            }

            // as no new node was created, return null:
            return null;
        }

        private Tuple<IEnumerable<ITreeItem>, IEnumerable<ITreeItem>> Reinsert(IEnumerable<ITreeItem> items, Box wholeBounds)
        {
            // combine the M entries of N and the new entry to M+1 entries

            // For all M+l entries of a node N, compute the distance between the centers of their rectangles and the center of the bounding rectangle of N
            // Sort the entries in DECREASING order of their computed distances
            var center = wholeBounds.Center;
            items = items.OrderByDescending(e => center.Distance(e.Bounds.Center));

            // Remove the first p entries from N and adjust the bounding rectangle of N
            var itemsRemoved = items.Take(reinsertEntries);
            var itemsRemaining = items.Skip(reinsertEntries);


            // Starting with the maximum distance (= far reinsert) or minimum distance (= close reinsert), invoke Insert to reinsert the removed entries
            // For close reinsert, reverse the order:
            if (closeReinsert) items = items.Reverse();

            // returns the items to remain in the node and the items to be reinserted
            // uses ToArray method to force evaluation of the LINQ expression
            return Tuple.Create(itemsRemaining.ToArray().AsEnumerable(), itemsRemoved.ToArray().AsEnumerable());
        }

        protected override ITreeItem ChooseNode(ITreeItem n, Box r, int level)
        {
            // CL1 [Initialize] Set N to be the root node
            //Node n = RootNode;

            // CL2 [Leaf check] If N is a leaf, return N
            while (n.Level != level)
            {
                // index of chosen entry
                ITreeItem chosen = null;
                // If the chlldpointers in N point to leaves [determine the mmimum overlap cost]
                if (n.Level == level + 1)
                {
                    double leastOverlapEnlar = double.PositiveInfinity;
                    foreach (var child in n)
                    {
                        Box original = child.Bounds;
                        Box enlarged = original.Union(r);
                        double tempOverlapEnlar = 0;
                        // calculate the overlap enlargement for each node
                        foreach (var sibl in n)
                        {
                            if (object.ReferenceEquals(child, sibl)) continue;
                            // enlargement due to particular sibling node j
                            tempOverlapEnlar += enlarged.Overlap(sibl.Bounds) - original.Overlap(sibl.Bounds);
                        }
                        // choose the entry in N whose rectangle needs least overlap enlargement to include the new data rectangle
                        if (tempOverlapEnlar < leastOverlapEnlar)
                        {
                            chosen = child;
                            leastOverlapEnlar = tempOverlapEnlar;
                        }
                        // Resolve ties by choosing the entry whose rectangle needs least area enlargement, then the entry with the rectangle of smallest area
                        else if (tempOverlapEnlar == leastOverlapEnlar)
                        {
                            double dEnlar = original.Enlargement(r) - chosen.Bounds.Enlargement(r);
                            if (dEnlar < 0 || (dEnlar == 0 && original.Area < chosen.Bounds.Area))
                            {
                                chosen = child;
                                leastOverlapEnlar = tempOverlapEnlar;
                            }
                        }
                    }
                }
                // if the childpomters in N do not point to leaves [determine the minimum area cost], (same as original RTree implementation)
                else
                {
                    double leastEnlargement = double.PositiveInfinity;
                    foreach (var child in n)
                    //for (int i = 1; i < n.Count; i++)
                    {
                        double tempEnlargement = child.Bounds.Enlargement(r);
                        // choose the entry in N whose rectangle needs least area enlargement to include the new data rectangle
                        // Resolve ties by choosing the entry with the rectangle of smallest area
                        if ((tempEnlargement < leastEnlargement) ||
                           ((tempEnlargement == leastEnlargement) && (child.Bounds.Area < chosen.Bounds.Area)))
                        {
                            chosen = child;
                            leastEnlargement = tempEnlargement;
                        }
                    }
                }

                // CL3 [Descend until a leaf is reached] Set N to be the childnode pointed to by the childpointer of the chosen entry and repeat from CL2
                n = chosen;
            }

            return n;
        }

        protected override Tuple<Tuple<IEnumerable<ITreeItem>, Box>, Tuple<IEnumerable<ITreeItem>, Box>> Split(IEnumerable<ITreeItem> items, Box wholeBounds)
        {
            //int count = n.Count + 1;
            // combine the M entries of N and the new entry to M+1 entries

            // For each sort M-2m+2 dlstributions of the M+l entries into two groups are determined
            int distributions = MaxNodeEntries - 2 * MinNodeEntries + 2;

            ITreeItem[][] itemsOrdered;
            Box[][] group1BB, group2BB;

            // S1 Determine the axis, perpendicular to which the split has to be performed
            int splitAxis = ChooseSplitAxis(items, out itemsOrdered, out group1BB, out group2BB);

            // S2 Determine the best distribution into two groups along the chosen axis:
            int splitIndex = ChooseSplitIndex(group1BB[splitAxis], group2BB[splitAxis]);


            // S3 Distribute the entries into two groups according to the determined distribution

            int count1 = MinNodeEntries + splitIndex;

            // return the determined distribution into two groups and the corresponding bounding boxes
            // ToArray method is used to force evaluation of the LINQ expression
            return Tuple.Create(Tuple.Create(itemsOrdered[splitAxis].Take(count1).ToArray().AsEnumerable(), group1BB[splitAxis][splitIndex]),
                                Tuple.Create(itemsOrdered[splitAxis].Skip(count1).ToArray().AsEnumerable(), group2BB[splitAxis][splitIndex]));
        }


        private int ChooseSplitAxis(IEnumerable<ITreeItem> items, out ITreeItem[][] itemsOrdered, out Box[][] group1BB, out Box[][] group2BB)
        {
            // For each sort M-2m+2 dlstributions of the M+l entries into two groups are determined
            int distributions = MaxNodeEntries - 2 * MinNodeEntries + 2;
            int count = items.Count();
            int dimensions = items.First().Bounds.Dimensions;

            double minS = double.PositiveInfinity;
            int splitAxis = -1;

            itemsOrdered = new ITreeItem[dimensions][];
            group1BB = new Box[dimensions][];
            group2BB = new Box[dimensions][];

            // S1 Determine the axis, perpendicular to which the split has to be performed
            for (int d = 0; d < dimensions; d++)
            {
                // Along each axis, the entries are first sorted by the lower value, then sorted by the upper value of their rectangles
                //var dataOrdered = data.OrderBy(e => e.Rect.GetMin(d)).ThenBy(e => e.Rect.GetMax(d));
                //itemsOrdered[d] = items.OrderBy(e => Math.Abs(e.Bounds.GetMin(d))).ThenBy(e => Math.Abs(e.Bounds.GetMax(d))).ToArray();
                itemsOrdered[d] = items.OrderBy(e => e.Bounds.GetMin(d)).ThenBy(e => e.Bounds.GetMax(d)).ToArray();

                // For each sort M-2m+2 dlstributions of the M+l entries into two groups are determined. where the k-th distribution (k = 0, ,(M-2m+1)) 
                // is described as follows: The first group contains the first m+k entries, the second group contains the remaining entries
                Box[] bb1 = new Box[distributions];
                Box[] bb2 = new Box[distributions];

                // for all distributions calculate the bounding boxes for both groups
                bb1[0] = itemsOrdered[d][0].Bounds.Copy();
                bb2[distributions - 1] = itemsOrdered[d][count - 1].Bounds.Copy();
                for (int i = 1; i < MinNodeEntries; i++)
                {
                    bb1[0].Add(itemsOrdered[d][i].Bounds);
                    bb2[distributions - 1].Add(itemsOrdered[d][(count - 1) - i].Bounds);
                }
                for (int k = 1; k < distributions; k++)
                {
                    bb1[k] = bb1[k - 1].Union(itemsOrdered[d][MinNodeEntries - 1 + k].Bounds);
                    bb2[distributions - 1 - k] = bb2[distributions - k].Union(itemsOrdered[d][count - MinNodeEntries - k].Bounds);
                }
                group1BB[d] = bb1;
                group2BB[d] = bb2;

                // For each distribution goodness values are determined (area-value, margin-value, overlap-value)
                // Compute S, the sum of all margin-values of the different distributions
                double S = 0;
                for (int k = 0; k < distributions; k++)
                {
                    //double area = bb1[k].Area() + bb2[k].Area();
                    double margin = bb1[k].Margin + bb2[k].Margin;
                    //double overlap = bb1[k].Intersection(bb2[k]);
                    S += margin;
                }
                // Choose the axis with the minimum S as split axis
                if (S < minS)
                {
                    minS = S;
                    splitAxis = d;
                }
            }

            // return chosen axis
            return splitAxis;
        }

        private int ChooseSplitIndex(Box[] group1BB, Box[] group2BB)
        {
            // S2 Determine the best distribution into two groups along the chosen axis:
            // Along the chosen split axis, choose the distribution with the minimum overlap-value
            // Resolve ties by choosing the distribution with minimum area-value
            int distributions = group1BB.Length;
            int splitIndex = -1;
            double leastOverlap = double.PositiveInfinity;

            for (int k = 0; k < distributions; k++)
            {
                double overlap = group1BB[k].Overlap(group2BB[k]);
                if ((overlap < leastOverlap) ||
                   ((overlap == leastOverlap) && ((group1BB[k].Area + group2BB[k].Area) < (group1BB[splitIndex].Area + group2BB[splitIndex].Area))))
                {
                    splitIndex = k;
                    leastOverlap = overlap;
                }
            }

            // return chosen index
            return splitIndex;
        }
    }
}
