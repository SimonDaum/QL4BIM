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
    public class RayTriangleIntersector : IRayTriangleIntersector
    {
        private readonly double epsilon;

        public RayTriangleIntersector(ISettings settings)
        {
            epsilon = settings.Distance.RoundToZero;
        }

        public bool TestStrict(Ray ray, Triangle tri)
        {
            // using Moeller-Trumbore algorithm:

            var pvec = ray.Direction.CrossProduct(tri.AC);
            var det = tri.AB * pvec;

            if (Math.Abs(det) < epsilon)
                return false;

            var inv_det = 1d / det;
            var tvec = ray.Start - tri.A.Vector;
            var u = tvec * pvec * inv_det;

            if (u <= 0d || u >= 1d)
                return false;

            var qvec = tvec.CrossProduct(tri.AB);
            var v = ray.Direction * qvec * inv_det;

            if (v <= 0d || u + v >= 1d)
                return false;

            var t = tri.AC * qvec * inv_det;

            return t >= 0;
        }

        public bool Test(Ray ray, Triangle tri)
        {
            // using Moeller-Trumbore algorithm:

            var pvec = ray.Direction.CrossProduct(tri.AC);
            var det = tri.AB * pvec;

            if (Math.Abs(det) < epsilon)
                return false;

            var inv_det = 1d / det;
            var tvec = ray.Start - tri.A.Vector;
            var u = tvec * pvec * inv_det;

            if (u < 0d || u > 1d)
                return false;

            var qvec = tvec.CrossProduct(tri.AB);
            var v = ray.Direction * qvec * inv_det;

            if (v < 0d || u + v > 1d)
                return false;

            var t = tri.AC * qvec * inv_det;

            return t >= 0;
        }
    }
}
