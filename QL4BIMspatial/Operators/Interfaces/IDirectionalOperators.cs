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
