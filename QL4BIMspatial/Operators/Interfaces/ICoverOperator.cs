using System.Collections.Generic;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    public interface ICoverOperator
    {
        IEnumerable<Pair<TriangleMesh, TriangleMesh>> Cover(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable,
            double positiveOffset, double negativeOffset);

        IEnumerable<Pair<TriangleMesh, TriangleMesh>> Cover(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable);

        bool Cover(TriangleMesh meshA, TriangleMesh meshB, double positiveOffset, double negativeOffset);
        bool Cover(TriangleMesh meshA, TriangleMesh meshB);

    }
}