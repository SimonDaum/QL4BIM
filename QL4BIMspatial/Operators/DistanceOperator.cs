using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity.Utility;
using NodeDistPair = System.Tuple<QL4BIMspatial.RTree<QL4BIMspatial.Triangle>.Node, QL4BIMspatial.RTree<QL4BIMspatial.Triangle>.Node, bool>;
using NodePair = System.Tuple<QL4BIMspatial.RTree<QL4BIMspatial.Triangle>.Node, QL4BIMspatial.RTree<QL4BIMspatial.Triangle>.Node>;

namespace QL4BIMspatial
{
    class DistanceOperator : IDistanceOperator
    {
        private readonly ITriangleIntersector triangleIntersector;
        private readonly ISettings settings;
        private RTree<Triangle> treeA;
        private RTree<Triangle> treeB;

        public DistanceOperator(ITriangleIntersector triangleIntersector, ISettings settings)
        {
            this.triangleIntersector = triangleIntersector;
            this.settings = settings;
        }

        public Tuple<TriangleMesh, TriangleMesh, double> Distance(TriangleMesh meshA, TriangleMesh meshB)
        {
            treeA = meshA.RTreeRoot;
            treeB = meshB.RTreeRoot;

            var rootBoxA = treeA.Bounds;
            var rootBoxB = treeB.Bounds;

            var minDistGloabal = Box.BoxDistanceMinMax(rootBoxA, rootBoxB);
            if(minDistGloabal.Min > settings.Distance.GlobalThreshold)
                return new Tuple<TriangleMesh, TriangleMesh, double>(meshA, meshB, settings.Distance.GlobalThreshold);

            var outList = new List<Tuple<ITreeItem, ITreeItem>>();
            var inList = new List<List<ITreeItem>> {new List<ITreeItem> {treeA.RootNode, treeB.RootNode}};

            //var stopW = new Stopwatch();
            //stopW.Start("RTree");
            DistanceCandidates(inList, outList, null);
            var minDist = Distance(outList);
            //stopW.Stop();


            //stopW.Start("BruteForce");
            //var minDistBF = double.MaxValue;
            //foreach (var triA in meshA.Triangles)
            //{
            //    foreach (var triB in meshB.Triangles)
            //    {
            //        var distBF = triA.MinSqrDistance(triB);
            //        if (distBF < minDistBF)
            //            minDistBF = distBF;
            //    }
            //}
            //stopW.Stop();

            //minDistBF = Math.Sqrt(minDistBF);
            //Console.WriteLine("BF: " + minDistBF + " , Tree: " + Math.Sqrt(minDist));

            return new Tuple<TriangleMesh, TriangleMesh, double>(meshA, meshB, Math.Sqrt(minDist));
        }

        private double Distance(IEnumerable<Tuple<ITreeItem, ITreeItem>> outList)
        {
            var minDist = double.MaxValue;
            foreach (var tuple in outList)
            {
                var nodeA = tuple.Item1;
                var nodeB = tuple.Item2;

                var triesA = treeA.GetItem(nodeA.ID);
                var triesB = treeB.GetItem(nodeB.ID);

                var dist = triesA.MinSqrDistance(triesB);
                if (dist < minDist)
                    minDist = dist;

                if (minDist < settings.Distance.RoundToZero)
                    return 0;
            }

            return minDist;
        }

        public IEnumerable<Tuple<TriangleMesh, TriangleMesh, double>> Distance(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable)
        {
            return enumerable.Select(pair => Distance(pair.First, pair.Second));
        }


        private void DistanceCandidates(List<List<ITreeItem>> treeItemListList, List<Tuple<ITreeItem, ITreeItem>>  ouList, Interval minInterval)
        {
            if (treeItemListList.Count == 0)
                return;

            var globalListListOut = new List<List<ITreeItem>>();
            foreach (var treeItems in treeItemListList)
            {
                var mainItem = treeItems[0];
                var mainCanSubdivide = mainItem.CanSubdivide;

                IList<ITreeItem> mainItemChildren = ChildOrMyself(mainItem);

                for (int i = 1; i < treeItems.Count; i++)
                {
                    var testeeCanSubdivide = treeItems[i].CanSubdivide;

                    //leaf leaf to outlist
                    if (!mainCanSubdivide && !testeeCanSubdivide)
                    {
                        ouList.Add(new Tuple<ITreeItem, ITreeItem>(mainItem, treeItems[i]));
                        continue;
                    }

                    var testeeChildren = ChildOrMyself(treeItems[i]);
     
                    foreach (var mainChild in mainItemChildren)
                    {   
                        //add A to outList
                        var outList = new List<ITreeItem>() {mainChild};

                        foreach (var testeeChild in testeeChildren)
                        {
                            testeeChild.MinMaxDistanceInterval = Box.BoxDistanceMinMax(mainChild.Bounds, testeeChild.Bounds);

                            //Console.WriteLine(testeeChild.MinMaxDistanceInterval);

                            //save all crss tuples, B now has minMaxInterval

                            if (minInterval == null || testeeChild.MinMaxDistanceInterval.Min < minInterval.Max)
                                outList.Add(testeeChild);

                            //smalles Min MinMaxIntervall
                            if (minInterval == null || testeeChild.MinMaxDistanceInterval.Min < minInterval.Min)
                                minInterval = testeeChild.MinMaxDistanceInterval;
                        }

                        globalListListOut.Add(outList);
                    }
                }
            }


            DistanceCandidates(globalListListOut, ouList, minInterval);
        }

        private static IList<ITreeItem> ChildOrMyself(ITreeItem mainItem)
        {
            List<ITreeItem> mainItemChildren;
            if (mainItem.CanSubdivide)
                mainItemChildren = new List<ITreeItem>(mainItem);
            else
            {
                mainItemChildren = new List<ITreeItem>();
                mainItemChildren.Add(mainItem);
            }
            return mainItemChildren;
        }
    }
}
