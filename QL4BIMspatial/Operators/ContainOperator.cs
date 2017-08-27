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
