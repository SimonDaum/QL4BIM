using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMspatial
{
    public class IndexedFaceSet
    {
        private readonly Tuple<int, int, int>[] indices;
        private readonly Tuple<double, double, double>[] vertices;
        private string name;

        private Dictionary<int, List<Vector<double>>> indexToNormal;
        private Vector<double>[] offsetVectors;
        private readonly DenseVector zAxis = new DenseVector(new[] { 0d, 0d, 1d });

        public IndexedFaceSet(Tuple<double, double, double>[] vertices,Tuple<int, int, int>[] indices, string name, int tag)
        {
            this.vertices = vertices;
            if (indices == null)
            {
                var triangleCount = vertices.Length/3;
                var indicesLoc = new List<Tuple<int, int, int>>();
                for (var i = 0; i < triangleCount; i++)
                {
                    var ti = i*3;
                    indicesLoc.Add(new Tuple<int, int, int>(ti, ti + 1, ti + 2));
                }
                this.indices = indicesLoc.ToArray();
            }
            else
            {
                this.indices = indices.Select(i => new Tuple<int, int, int>(i.Item3, i.Item2, i.Item1)).ToArray();   
            }
            
            Name = name;
            Tag = tag;
        }

        public IndexedFaceSet(string name, int[] indices, double[] vertices)
        {
            var tempVertices = new List<Tuple<double, double, double>>();

            var divisor = 3;
            if (Environment.Is64BitProcess)
                divisor = 6;

            for (int i = 0; i < vertices.Length / divisor; i++)
                tempVertices.Add(new Tuple<double, double, double>(vertices[i * divisor + 0], vertices[i * divisor + 1], vertices[i * divisor + 2]));


            var tempIndices = new List<Tuple<int, int, int>>();
            for (int i = 0; i < indices.Length / 3; i++)
            {

                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];
                tempIndices.Add(new Tuple<int, int, int>(index1, index2, index3));
            }


            this.vertices = tempVertices.ToArray();
            this.indices = tempIndices.ToArray();
            Name = name;
        }

        public int Tag { get; set; }

        public string Name
        {
            get { return name; }
            set { name = value.EndsWith("_ifs_TRANSFORM") ? value.Substring(0, value.Length - 14) : value; }
        }

        private Matrix<double> RotMatrix(Vector<double> a, Vector<double> b)
        {
            var gg = new DenseMatrix(3,3);

            a = a.Normalize(2);
            b = b.Normalize(2);

            var abdot = a.DotProduct(b);
            var abcrossnorm = a.CrossProduct(b).L2Norm();

            gg[0, 0] = abdot;
            gg[0, 1] = -abcrossnorm;
            gg[0, 2] = 0;

            gg[1, 0] = abcrossnorm;
            gg[1, 1] = abdot;
            gg[1, 2] = 0;

            gg[2, 0] = 0;
            gg[2, 1] = 0;
            gg[2, 2] = 1;

            var ffi = new DenseMatrix(3,3);
            ffi.SetRow(0, a);
            var dd = (b- abdot*a)/(b - abdot * a).L2Norm();
            ffi.SetRow(1, dd);
            ffi.SetRow(2, a.CrossProduct(b));

            var uu = ffi.Inverse()*gg*ffi;
            var t = uu.L2Norm();
            var test = (b - uu*a).L2Norm();

            return uu; 
        }


        public TriangleMesh CreateMesh(DenseVector registeredDirection = null, bool createTree = true)
        {
           var triangles = new List<Triangle>(indices.Length);
           indexToNormal = new Dictionary<int, List<Vector<double>>>();
           offsetVectors = new Vector<double>[vertices.Length];

            foreach (var ind in indices)
            {
                int index1 = ind.Item3;
                int index2 = ind.Item1;
                int index3 = ind.Item2;

                var v1 = TupleToVector(vertices[index1]);
                var v2 = TupleToVector(vertices[index2]);
                var v3 = TupleToVector(vertices[index3]);


                //var dir = new DenseVector(new[] { -0.809627, 0.684497, 1.530143 }); //-0.809627 0.684497 1.530143 
                if (registeredDirection != null)
                {
                    var rot = RotMatrix(zAxis, registeredDirection);
                    v1 = (v1 * rot) as DenseVector;
                    v2 = (v2 * rot) as DenseVector;
                    v3 = (v3 * rot) as DenseVector;
                }

                var point1 = new Point(v1);
                var point2 = new Point(v2);
                var point3 = new Point(v3);

                var tri = new Triangle(point1, point2, point3, index1, index2, index3);

                SaveNormalToIndex(indexToNormal, index1, tri);
                SaveNormalToIndex(indexToNormal, index2, tri);
                SaveNormalToIndex(indexToNormal, index3, tri);

                triangles.Add(tri);
            }

            CreateOffsetVectors();

            return new TriangleMesh(triangles, name, createTree, offsetVectors);
        }

        private static DenseVector TupleToVector(Tuple<double, double, double> v1)
        {
            var asArray = new DenseVector(new[] {v1.Item1, v1.Item2, v1.Item3});
            return asArray;
        }

        private void CreateOffsetVectors()
        {
            foreach (var indexNormal in indexToNormal)
            {
                int index = indexNormal.Key;

                Vector<double>[] distinctNormals = indexNormal.Value.Distinct(new VectorEqualityComparer()).ToArray();

                Vector<double> offsetVector = new DenseVector(3);
                foreach (var normal in distinctNormals)
                    offsetVector = offsetVector + normal;

                offsetVector = (offsetVector / distinctNormals.Length);

                //for normal pointing to the outside *-1
                offsetVectors[index] = offsetVector.Normalize(2)*(-1);
            }
        }


        private static void SaveNormalToIndex(Dictionary<int, List<Vector<double>>> indexToNormal, int index1, Triangle tri)
        {
            if (indexToNormal.ContainsKey(index1))
                indexToNormal[index1].Add(tri.Normal);
            else
                indexToNormal.Add(index1, new List<Vector<double>> {tri.Normal});
        }

        private class VectorEqualityComparer : IEqualityComparer<Vector<double>>
        {
            public bool Equals(Vector<double> x, Vector<double> y)
            {
                double div = Math.Abs((x - y).L2Norm());
                return div < 0.01;
            }

            public int GetHashCode(Vector<double> obj)
            {
                return 1;
            }
        }
    }
}