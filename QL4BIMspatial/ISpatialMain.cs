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
using Microsoft.Practices.Unity;

namespace QL4BIMspatial
{
    internal interface ISpatialMain
    {
        void ImportAndReset(IEnumerable<IndexedFaceSet> faceSets);
        void ImportAndReset(IEnumerable<TriangleMesh> meshes);

        ISettings GetSettings();

        void Touch();
        void Overlap();
        void Distance();

        void ArbitraryDirection();
        void AboveOfRelaxed();
        void AboveOfStrict();
        void BelowOfRelaxed();
        void BelowOfStrict();
        void EastOfRelaxed();
        void EastOfStrict();
        void WestofRelaxed();
        void WestofStrict();
        void NorthOfRelaxed();
        void NorthOfStrict();
        void SouthOfRelaxed();
        void SouthOfStrict();

        void Export();
        void ExportTrees();
        void Contain();
        void Equal();
        int GetTriangleCount();
        int CurrentTriangleCount { get; }

    }
}
