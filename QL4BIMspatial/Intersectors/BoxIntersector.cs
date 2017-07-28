namespace QL4BIMspatial
{
    public class BoxIntersector : IIntersector<Box>
    {
        private readonly IntervalIntersector intervalIntersector;

        public BoxIntersector(IntervalIntersector intervalIntersector)
        {
            this.intervalIntersector = intervalIntersector;
        }

        public bool TestStrict(Box first, Box second)
        {
            return (intervalIntersector.TestStrict(first.X, second.X))
                   && (intervalIntersector.TestStrict(first.Y, second.Y))
                   && (intervalIntersector.TestStrict(first.Z, second.Z));
        }


        public bool Test(Box first, Box second)
        {
            return (intervalIntersector.Test(first.X, second.X))
                   && (intervalIntersector.Test(first.Y, second.Y))
                   && (intervalIntersector.Test(first.Z, second.Z));
        }
    }

    public class BoxTrianglePairIntersector : IIntersector<Box, TrianglePair>
    {
        private readonly IntervalIntersector intervalIntersector;

        public BoxTrianglePairIntersector(IntervalIntersector intervalIntersector)
        {
            this.intervalIntersector = intervalIntersector;
        }

        public bool TestStrict(Box first, TrianglePair second)
        {
            var secondBound = second.Original.Bounds;
            return (intervalIntersector.TestStrict(first.X, secondBound.X))
                   && (intervalIntersector.TestStrict(first.Y, secondBound.Y))
                   && (intervalIntersector.TestStrict(first.Z, secondBound.Z));
        }

        public bool Test(Box first, TrianglePair second)
        {
            var secondBound = second.Original.Bounds;
            return (intervalIntersector.Test(first.X, secondBound.X))
                   && (intervalIntersector.Test(first.Y, secondBound.Y))
                   && (intervalIntersector.Test(first.Z, secondBound.Z));
        }
    }
}