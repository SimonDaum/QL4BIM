using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public class IntervalIntersector : IIntervalIntersector
    {
        public bool TestStrict(Interval first, Interval second)
        {
            return (first.Min < second.Max && second.Min < first.Max);
        }

        public bool Test(Interval first, Interval second)
        {
            return (first.Min <= second.Max && second.Min <= first.Max);
        }
    }
}