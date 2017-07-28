using System.Collections.Generic;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    public interface IDirectionalOperators
    {
        List<Pair<TriangleMesh, TriangleMesh>> AboveOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        List<Pair<TriangleMesh, TriangleMesh>> AboveOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        bool AboveOfRelaxed(TriangleMesh meshA, TriangleMesh meshB);
        bool AboveOfStrict(TriangleMesh meshA, TriangleMesh meshB);
        List<Pair<TriangleMesh, TriangleMesh>> BelowOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        List<Pair<TriangleMesh, TriangleMesh>> BelowOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        bool BelowOfRelaxed(TriangleMesh meshA, TriangleMesh meshB);
        bool BelowOfStrict(TriangleMesh meshA, TriangleMesh meshB);
        List<Pair<TriangleMesh, TriangleMesh>> EastOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        List<Pair<TriangleMesh, TriangleMesh>> EastOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        bool EastOfRelaxed(TriangleMesh meshA, TriangleMesh meshB);
        bool EastOfStrict(TriangleMesh meshA, TriangleMesh meshB);
        List<Pair<TriangleMesh, TriangleMesh>> WestOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        List<Pair<TriangleMesh, TriangleMesh>> WestOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        bool WestOfRelaxed(TriangleMesh meshA, TriangleMesh meshB);
        bool WestOfStrict(TriangleMesh meshA, TriangleMesh meshB);
        List<Pair<TriangleMesh, TriangleMesh>> NorthOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        List<Pair<TriangleMesh, TriangleMesh>> NorthOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        bool NorthOfRelaxed(TriangleMesh meshA, TriangleMesh meshB);
        bool NorthOfStrict(TriangleMesh meshA, TriangleMesh meshB);
        List<Pair<TriangleMesh, TriangleMesh>> SouthOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        List<Pair<TriangleMesh, TriangleMesh>> SouthOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs);
        bool SouthOfRelaxed(TriangleMesh meshA, TriangleMesh meshB);
        bool SouthOfStrict(TriangleMesh meshA, TriangleMesh meshB);
    }
}