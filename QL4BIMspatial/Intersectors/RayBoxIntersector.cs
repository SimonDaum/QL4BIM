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
    class RayBoxIntersector : IRayBoxIntersector
    {
        public bool Test(Ray ray, Box box)
        {
            double tmin = double.NegativeInfinity, tmax = double.PositiveInfinity;

            MinMax(ray, box, 0, ref tmin, ref tmax);
            MinMax(ray, box, 1, ref tmin, ref tmax);
            MinMax(ray, box, 2, ref tmin, ref tmax);

            return tmax >= tmin;
        }

        private void MinMax(Ray ray, Box box, int index, ref double tmin, ref double tmax)
        {
            double tx1 = (box.XMin - ray.Start[index])/ ray.Direction[index];
            double tx2 = (box.XMax - ray.Start[index])/ ray.Direction[index];

            tmin = Math.Max(tmin, Math.Min(tx1, tx2));
            tmax = Math.Min(tmax, Math.Max(tx1, tx2));
        }
    }
}
