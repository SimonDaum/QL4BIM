using System.Collections.Generic;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    public interface ITouchOperator
    {
        IEnumerable<Pair<TriangleMesh, TriangleMesh>> Touch(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable,
            double positiveOffset, double negativeOffset);

        IEnumerable<Pair<TriangleMesh, TriangleMesh>> Touch(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable);

        bool Touch(TriangleMesh meshA, TriangleMesh meshB, double positiveOffset, double negativeOffset);
        bool Touch(TriangleMesh meshA, TriangleMesh meshB);

        bool TouchWithoutInnerOuterTest(TriangleMesh meshA, TriangleMesh meshB, double positiveOffset, double negativeOffset);
    }
}