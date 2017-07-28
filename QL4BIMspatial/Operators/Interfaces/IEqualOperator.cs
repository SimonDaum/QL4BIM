using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    public interface IEqualOperator
    {
        IEnumerable<Tuple<TriangleMesh, TriangleMesh, double>> EqualBruteForce(IEnumerable<Pair<TriangleMesh, List<TriangleMesh>>> enumerable);
        IEnumerable<Tuple<TriangleMesh, TriangleMesh, double>> EqualRTree(IEnumerable<Pair<TriangleMesh, List<TriangleMesh>>> enumerable);
    }
}