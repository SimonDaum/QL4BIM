using System.Collections.Generic;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    class ContainOperator : IContainOperator
    {
        private readonly IOverlapOperator overlapOperator;
        private readonly IInsideTester insideTester;
        private readonly ISettings settings;

        public ContainOperator(IOverlapOperator overlapOperator, IInsideTester insideTester, ISettings settings)
        {
            this.overlapOperator = overlapOperator;
            this.insideTester = insideTester;
            this.settings = settings;
        }

        public bool Contain(TriangleMesh meshA, TriangleMesh meshB)
        {
            return Contain(meshA, meshB, settings.Contain.NegativeOffset);
        }

        public bool Contain(TriangleMesh meshA, TriangleMesh meshB, double minusOffset)
        {
            if (overlapOperator.Overlap(meshA, meshB, minusOffset))
                return false;

            return insideTester.BIsInside(meshA, meshB);
        }

        public IEnumerable<Pair<TriangleMesh, TriangleMesh>> Contain( IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable)
        {
            return Contain(enumerable, settings.Contain.NegativeOffset);
        }

        public IEnumerable<Pair<TriangleMesh, TriangleMesh>> Contain(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable, double minusOffset)
        {
            foreach (var pair in enumerable)
            {
                if (Contain(pair.First, pair.Second, minusOffset))
                    yield return pair;
            }
        }
    }
}
