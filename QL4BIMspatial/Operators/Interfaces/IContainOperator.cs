using System.Collections.Generic;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    public interface IContainOperator
    {
        bool Contain(TriangleMesh meshA, TriangleMesh meshB);
        bool Contain(TriangleMesh meshA, TriangleMesh meshB, double minusOffset);
        IEnumerable<Pair<TriangleMesh, TriangleMesh>> Contain( IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable);
        IEnumerable<Pair<TriangleMesh, TriangleMesh>> Contain(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable, double minusOffset);
    }
}