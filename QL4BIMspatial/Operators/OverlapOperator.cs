using System.Collections.Generic;
using PairTriangleMesh = Microsoft.Practices.Unity.Utility.Pair<QL4BIMspatial.TriangleMesh, QL4BIMspatial.TriangleMesh>;
using PairTriangleMeshDist = System.Tuple<QL4BIMspatial.TriangleMesh, QL4BIMspatial.TriangleMesh, double>;
using triNode = QL4BIMspatial.RTree<QL4BIMspatial.Triangle>.Node;

namespace QL4BIMspatial
{
    public class OverlapOperator : IOverlapOperator
    {
        private readonly ITriangleIntersector triangleIntersector;
        private readonly ISettings settings;
        private readonly IX3DExporter exporter;
        private RTree<Triangle> treeA;
        private RTree<Triangle> treeB;
        private TriangleMesh meshA;
        private TriangleMesh meshB;
        private double currentMinusOffset;

        public OverlapOperator(ITriangleIntersector triangleIntersector, ISettings settings, IX3DExporter exporter)
        {
            this.triangleIntersector = triangleIntersector;
            this.settings = settings;
            this.exporter = exporter;
        }

        public bool Overlap(TriangleMesh meshA, TriangleMesh meshB)
        {
            return Overlap(meshA, meshB, settings.Overlap.NegativeOffset);
        }

        public IEnumerable<PairTriangleMesh> Overlap(IEnumerable<PairTriangleMesh> enumerable, double minusOffset)
        {
            foreach (var pair in enumerable)
            {
                if (Overlap(pair.First, pair.Second, minusOffset))
                    yield return pair;
            }
        }

        public IEnumerable<PairTriangleMesh> Overlap(IEnumerable<PairTriangleMesh> enumerable)
        {
            return Overlap(enumerable, settings.Touch.NegativeOffset);
        }

        public bool Overlap(TriangleMesh meshA, TriangleMesh meshB, double minusOffset)
        {
            this.meshA = meshA;
            this.meshB = meshB;
            treeA = this.meshA.RTreeRoot;
            treeB = meshB.RTreeRoot;

            var input = new List<List<ITreeItem>>();
            input.Add(new List<ITreeItem>() {meshA.RTreeRoot.Root, meshB.RTreeRoot.Root});

            currentMinusOffset = minusOffset;
            
            return OverlapWithMinusOffset(input);
        }

        public TriangleMesh OuterMesh(TriangleMesh mesh, double minusOffset)
        {   

            var tris = new List<Triangle>();
            foreach (var triangle in mesh.Triangles)
            {
                var offSetTriA = mesh.CreateOuterTriangle(triangle, minusOffset);
                tris.Add(offSetTriA);
            }

            return new TriangleMesh(tris, "test", false);
        }

        private bool OverlapWithMinusOffset(List<List<ITreeItem>> treeItemListList)
        {
            var listListOut = new List<List<ITreeItem>>();
            if (treeItemListList.Count == 0)
                return false;

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

                        var offSetTriA = meshA.CreateOuterTriangle(triA, currentMinusOffset);
                        var offSetTriB = meshB.CreateOuterTriangle(triB, currentMinusOffset);
                        if (triangleIntersector.DoIntersect(offSetTriA, offSetTriB))
                        {   
                            exporter.ExportMeshes(@"C: \Users\Simon\Documents\Data\Ifc\alexTriOri.x3d", new List<TriangleMesh>()
                            { new TriangleMesh(new List<Triangle>() {triA, triB},"original", false)});

                            exporter.ExportMeshes(@"C: \Users\Simon\Documents\Data\Ifc\alexTriInner.x3d", new List<TriangleMesh>()
                            { new TriangleMesh(new List<Triangle>() { offSetTriA, offSetTriB},"iinner", false)});

                            return true;
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

                            var isIntersecting = mainChild.Bounds.Intersects(testeeChild.Bounds);
                            if (isIntersecting)
                                listOut.Add(testeeChild);

                        }

                        if (listOut.Count > 1)
                            listListOut.Add(listOut);
                    }
                }


            }

            return OverlapWithMinusOffset(listListOut);
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
