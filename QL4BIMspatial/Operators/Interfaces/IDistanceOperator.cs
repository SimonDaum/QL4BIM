using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    public interface IDistanceOperator
    {
        Tuple<TriangleMesh, TriangleMesh, double> Distance(TriangleMesh meshA, TriangleMesh meshB);

        IEnumerable<Tuple<TriangleMesh, TriangleMesh, double>> Distance(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable);
    }
}