using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity.Utility;
using QL4BIMindexing;
using QL4BIMprimitives;
using PairTriangleMesh = Microsoft.Practices.Unity.Utility.Pair<QL4BIMspatial.TriangleMesh, QL4BIMspatial.TriangleMesh>;

namespace QL4BIMspatial
{
    class TouchOperator : ITouchOperator
    {
        private readonly ITriangleIntersector triangleIntersector;
        private readonly ISettings settings;
        private readonly IInsideTester insideTester;
        private RTree<Triangle> treeA;
        private RTree<Triangle> treeB;
        private TriangleMesh meshA;
        private TriangleMesh meshB;
        private double currentPositivOffset;
        private double currentNegativeOffset;
        private bool outerIntersectFound;
        private int intersectingCall;

        public TouchOperator(ITriangleIntersector triangleIntersector, ISettings settings, IInsideTester insideTester)
        {
            this.triangleIntersector = triangleIntersector;
            this.settings = settings;
            this.insideTester = insideTester;
        }

        public IEnumerable<PairTriangleMesh> Touch(IEnumerable<PairTriangleMesh> enumerable, double positiveOffset, double negativeOffset)
        {
            foreach (var pair in enumerable)
            {
                if (Touch(pair.First, pair.Second, positiveOffset,negativeOffset))
                    yield return pair;
            }
        }

        public IEnumerable<PairTriangleMesh> Touch(IEnumerable<PairTriangleMesh> enumerable)
        {
            return Touch(enumerable, settings.Touch.PositiveOffset, settings.Touch.NegativeOffset);
        }

        public bool Touch(TriangleMesh meshA, TriangleMesh meshB)
        {
            return Touch(meshA, meshB, settings.Touch.PositiveOffset, settings.Touch.NegativeOffset);
        }

        public bool Touch(TriangleMesh meshA, TriangleMesh meshB, double positiveOffset, double negativeOffset)
        {
            if (!TouchWithoutInnerOuterTest2(meshA, meshB, positiveOffset, negativeOffset))
                return false;

            var dfdf = !insideTester.BIsInside(meshA, meshB);
            return dfdf;
        }

        public bool TouchWithoutInnerOuterTest2(TriangleMesh meshA, TriangleMesh meshB, double positiveOffset, double negativeOffset)
        {
            this.meshA = meshA;
            this.meshB = meshB;
            treeA = meshA.RTreeRoot;
            treeB = meshB.RTreeRoot;

            var pair = new Tuple<ITreeItem, ITreeItem>(treeA.Root, treeB.Root);

            currentPositivOffset = positiveOffset;
            currentNegativeOffset = negativeOffset;

            outerIntersectFound = false;
          
            var result = OverlapWithOffset(pair);
            //Console.WriteLine(intersectingCall);
            return result;
        }

        public bool TouchWithoutInnerOuterTest(TriangleMesh meshA, TriangleMesh meshB, double positiveOffset, double negativeOffset)
        {
            this.meshA = meshA;
            this.meshB = meshB;
            treeA = meshA.RTreeRoot;
            treeB = meshB.RTreeRoot;

            var input = new List<List<ITreeItem>>();
            input.Add(new List<ITreeItem>() {treeA.Root, treeB.Root});

            currentPositivOffset = positiveOffset;
            currentNegativeOffset = negativeOffset;

            outerIntersectFound = false;
      
            var result = OverlapWithOffset(input);
            //Console.WriteLine(intersectingCall);
            return result;
        }

        private bool OverlapWithOffset(Tuple<ITreeItem, ITreeItem> pair)
        {   
            var stack = new Stack<Tuple<ITreeItem, ITreeItem>>();
            stack.Push(pair);
            while (stack.Count > 0)
            {
                pair = stack.Pop();
                var itemA = pair.Item1;
                var itemB = pair.Item2;
                if (!itemA.CanSubdivide && !itemB.CanSubdivide)
                {
                    var triA = treeA.GetItem(itemA.ID);
                    var triB = treeB.GetItem(itemB.ID);

                    if (!outerIntersectFound)
                    {
                        var offSetTriA = meshA.CreateOuterTriangle(triA, currentPositivOffset);
                        var offSetTriB = meshB.CreateOuterTriangle(triB, currentPositivOffset);
                        outerIntersectFound = triangleIntersector.DoIntersect(offSetTriA, offSetTriB);
                    }

                    var offSetTriA2 = meshA.CreateOuterTriangle(triA, currentNegativeOffset);
                    var offSetTriB2 = meshB.CreateOuterTriangle(triB, currentNegativeOffset);
                    var innerIntersectFound = triangleIntersector.DoIntersect(offSetTriA2, offSetTriB2);
                    if (innerIntersectFound)
                        return false;
                }
                else
                {
                    var mainItemChildren = MeOrMyChildren(itemA);
                    foreach (var childA in mainItemChildren)
                    {
                        var itemBChildren = MeOrMyChildren(itemB);
                        foreach (var childB in itemBChildren)
                        {
                            var childAOffsetBox = childA.Bounds.Offset(settings.Touch.PositiveOffset);
                            var childBOffsetBox = childB.Bounds.Offset(settings.Touch.PositiveOffset);
                            intersectingCall++;

                            if (childAOffsetBox.Intersects(childBOffsetBox))
                                stack.Push(new Tuple<ITreeItem, ITreeItem>(childA, childB));
                        }
                    }
                }
            }
            return outerIntersectFound;
        }

        private bool OverlapWithOffset(List<List<ITreeItem>> treeItemListList)
        {
            var listListOut = new List<List<ITreeItem>>();
            if (treeItemListList.Count == 0)
                return outerIntersectFound;

            foreach (var treeItems in treeItemListList)
            {
                var mainItem = treeItems[0];
                var mainCanSubdivide = mainItem.CanSubdivide;

                var mainItemChildren = MeOrMyChildren(mainItem);

                for (int i = 1; i < treeItems.Count; i++)
                {
                    var testeeCanSubdivide = treeItems[i].CanSubdivide;

                    if (!mainCanSubdivide && !testeeCanSubdivide)
                    {
                        var triA = treeA.GetItem(mainItem.ID);
                        var triB = treeB.GetItem(treeItems[i].ID);

                        if (!outerIntersectFound)
                        {
                            var offSetTriA = meshA.CreateOuterTriangle(triA, currentPositivOffset);
                            var offSetTriB = meshB.CreateOuterTriangle(triB, currentPositivOffset);
                            outerIntersectFound = triangleIntersector.DoIntersect(offSetTriA, offSetTriB);         
                        }

                        var offSetTriAReverse = meshA.CreateOuterTriangle(triA, currentNegativeOffset);
                        var offSetTriBReverse = meshB.CreateOuterTriangle(triB, currentNegativeOffset);
                        if (triangleIntersector.DoIntersect(offSetTriAReverse, offSetTriBReverse))
                        {
                            treeItemListList.Clear();
                            outerIntersectFound = false;
                            return false;
                        }

                        continue;

                    }

                    var testeeChildren = MeOrMyChildren(treeItems[i]);
    
                    foreach (var mainChild in mainItemChildren)
                    {
                        var listOut = new List<ITreeItem>();
                        listOut.Add(mainChild);

                        foreach (var testeeChild in testeeChildren)
                        {
                            var extendedBox = mainChild.Bounds.Offset(currentPositivOffset);
                            intersectingCall++;
                            var isIntersecting = extendedBox.Intersects(testeeChild.Bounds.Offset(currentPositivOffset));
                            if (isIntersecting)
                                listOut.Add(testeeChild);
                        }

                        if (listOut.Count > 1)
                            listListOut.Add(listOut);
                    }
                }


            }

            return OverlapWithOffset(listListOut);
        }

        private static IList<ITreeItem> MeOrMyChildren(ITreeItem mainItem)
        {
            var mainCanSubdivide = mainItem.CanSubdivide;
            IList<ITreeItem> mainItemChildren = null;
            if (mainCanSubdivide)
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
