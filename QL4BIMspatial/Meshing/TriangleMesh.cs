using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMspatial
{
    public class TriangleMesh : IEnumerable<Triangle>, IHasBounds
    {
        private readonly Vector<double>[] offsetVectors;
        private readonly List<Triangle> triangles;

        public int SampleCount { get; set; }

        public TriangleMesh(IEnumerable<Triangle> tris, string name, bool createRTree,  Vector<double>[] offsetVectors = null)
        {
            this.offsetVectors = offsetVectors;
            Name = name;
            triangles = new List<Triangle>();
            triangles.AddRange(tris);

            Bounds = Box.Union(triangles.Select(tri => tri.Bounds));

            if (!createRTree)
                return;
           CreateRTree();
        }

        public void CreateRTree()
        {
            RTreeRoot = new RStarTree<Triangle>();
            RTreeRoot.Add(triangles);
        }

        public Triangle CreateOuterTriangle(Triangle triangle, double offSet)
        {
            var offsetA = offsetVectors[triangle.AIndex];
            var offsetB = offsetVectors[triangle.BIndex];
            var offsetC = offsetVectors[triangle.CIndex];

            var newPointA = new Point(triangle.A.Vector + offsetA * offSet as DenseVector);
            var newPointB = new Point(triangle.B.Vector + offsetB * offSet as DenseVector);
            var newPointC = new Point(triangle.C.Vector + offsetC * offSet as DenseVector);

            return new Triangle(newPointA, newPointB, newPointC);
        }




        public string Name { get;  set; }

        public List<Triangle> Triangles
        {
            get { return triangles; }
        }

        public int Count
        {
            get { return triangles.Count; }
        }

        public IEnumerator<Triangle> GetEnumerator()
        {
            return Triangles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Box Bounds { get; private set; }

        public RTree<Triangle> RTreeRoot { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}