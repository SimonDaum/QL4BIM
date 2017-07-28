using System.Collections.Generic;
using System.Linq;
using PairTriangleMesh = Microsoft.Practices.Unity.Utility.Pair<QL4BIMspatial.TriangleMesh, QL4BIMspatial.TriangleMesh>;

namespace QL4BIMspatial
{
    class CoverOperator : ICoverOperator
    {
        private readonly ITouchOperator touchOperator;
        private readonly IInsideTester insideTester;
        private readonly ISettings settings;


        public CoverOperator(ITouchOperator touchOperator, IInsideTester insideTester, ISettings settings)
        {
            this.touchOperator = touchOperator;
            this.insideTester = insideTester;
            this.settings = settings;
        }

        public IEnumerable<PairTriangleMesh> Cover(IEnumerable<PairTriangleMesh> enumerable, double positiveOffset, double negativeOffset)
        {
            foreach (var pair in enumerable)
            {
                if (Cover(pair.First, pair.Second, positiveOffset,negativeOffset))
                    yield return pair;
            }
        }

        public IEnumerable<PairTriangleMesh> Cover(IEnumerable<PairTriangleMesh> enumerable)
        {
            return Cover(enumerable, settings.Touch.PositiveOffset, settings.Touch.NegativeOffset);
        }

        public bool Cover(TriangleMesh meshA, TriangleMesh meshB)
        {
            return Cover(meshA, meshB, settings.Touch.PositiveOffset, settings.Touch.NegativeOffset);
        }

        public bool Cover(TriangleMesh meshA, TriangleMesh meshB, double positiveOffset, double negativeOffset)
        {
            if (touchOperator.TouchWithoutInnerOuterTest(meshA, meshB, positiveOffset, negativeOffset))
                return false;

            return insideTester.BIsInside(meshA, meshB); ;

        }


    }
}
