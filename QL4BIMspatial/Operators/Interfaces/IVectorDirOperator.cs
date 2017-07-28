using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    public interface IVectorDirOperator
    {
        void AddIndexedFaceSet(IndexedFaceSet faceSet);
        void AddDirection(string name, DenseVector denseVector);
        bool Intersects(TriangleMesh meshA, TriangleMesh meshB, string dirName, bool strict = false);

        IEnumerable<Pair<TriangleMesh, TriangleMesh>> Intersects(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable, string dirName, bool strict = false);
        IEnumerable<TriangleMesh> GetTransMesh(string direction);
        void Reset();
    }
}