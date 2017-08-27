/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMspatial.

QL4BIMspatial is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMspatial is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMspatial. If not, see <http://www.gnu.org/licenses/>.
*/

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
