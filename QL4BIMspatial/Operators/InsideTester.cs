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

using System;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public class InsideTester : IInsideTester
    {
        private readonly IRayTriangleIntersector rayTriangleIntersector;
        private readonly ISettings settings;
        private readonly double negativeOffset;

        public InsideTester(IRayTriangleIntersector rayTriangleIntersector, ISettings settings)
        {
            this.rayTriangleIntersector = rayTriangleIntersector;
            this.settings = settings;
            negativeOffset = settings.Contain.NegativeOffset;
        }

        public bool BIsInside(TriangleMesh meshA, TriangleMesh meshB)
        {
            var boxA = meshA.Bounds;
            var boxB = meshB.Bounds;

            if (!boxA.Contains(boxB))
                return false;

            return IsInside(meshA, meshB);
        }

        private bool IsInside(TriangleMesh meshA, TriangleMesh meshB)
        {
            var boxA = meshA.Bounds;

            var aBTri = meshB.Triangles[0];
            aBTri = meshB.CreateOuterTriangle(aBTri, negativeOffset);

            var center = aBTri.Center.Vector;
            var rayPositive = new Ray(center, Axis.X, AxisDirection.Positive);
            var rayNegative = new Ray(center, Axis.X, AxisDirection.Negative);
            var aBTriBoxExtendedPositive = aBTri.Bounds.ExtendInDirection(boxA, Axis.X, true);
            var aBTriBoxExtendedNegative = aBTri.Bounds.ExtendInDirection(boxA, Axis.X, false);

            var isInsideP = IntersectionCount(meshA, aBTriBoxExtendedPositive, rayPositive);
            var isInsideN = IntersectionCount(meshA, aBTriBoxExtendedNegative, rayNegative);

            if(isInsideP != isInsideN)
                throw new InvalidOperationException();

            return isInsideP;
        }

        private bool IntersectionCount(TriangleMesh meshA, Box aBTriBoxExtendedPositive, Ray rayPositive)
        {
            var aTree = meshA.RTreeRoot;
            var triCandidatesFromA = aTree.FindOverlap(aBTriBoxExtendedPositive);

            var intersectionCount = 0;
            foreach (var candidateTriangle in triCandidatesFromA)
            {
                if (rayTriangleIntersector.TestStrict(rayPositive, candidateTriangle))
                    intersectionCount++;
            }

            return (intersectionCount%2 != 0);
        }
    }
}
