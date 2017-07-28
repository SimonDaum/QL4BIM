using System.Collections.Generic;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    public interface IOverlapOperator
    {
        bool Overlap(TriangleMesh meshA, TriangleMesh meshB, double minusOffset);
        bool Overlap(TriangleMesh meshA, TriangleMesh meshB);

        IEnumerable<Pair<TriangleMesh, TriangleMesh>> Overlap(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable, double minusOffset);
        IEnumerable<Pair<TriangleMesh, TriangleMesh>> Overlap(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable);
        TriangleMesh OuterMesh(TriangleMesh mesh, double minusOffset);
    }
}