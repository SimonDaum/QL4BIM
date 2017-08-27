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
using System.Collections.Generic;
using System.Security.Cryptography;
using MathNet.Numerics.LinearAlgebra;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public class PointSampler : IPointSampler
    {
        private readonly Random random;
        private int sessionPointCount;

        public PointSampler(ISettings settings)
        {
            random = new Random();
        }

        public int SessionPointCount
        {
            get { return sessionPointCount; }
        }

        public void ResetSessionPointCount()
        {
            sessionPointCount = 0;
        }

        public void DistributePoints(IEnumerable<Triangle> tris, int pointsPerSquareMeter)
        {
            foreach (var triangle in tris)
                DistributePoints(triangle, pointsPerSquareMeter);
        }

        public void DistributePoints(Triangle tri, int pointsPerSquareMeter)
        {
            var samplingPoints = new List<Vector<double>>();
            var barycenter = tri.Center.Vector;

            var A = tri.A.Vector - barycenter;
            var B = tri.B.Vector - barycenter;
            var C = tri.C.Vector - barycenter;

            var currentPointCount = (int)(pointsPerSquareMeter * tri.Area);

            //Console.WriteLine("Samples per Tri: " + currentPointCount);
            if (currentPointCount < 10)
                currentPointCount = 10;

            sessionPointCount += currentPointCount;

            for (int i = 0; i < currentPointCount; i++)
            {

                var r1 = random.NextDouble();
                var r2 = random.NextDouble();
               
                var point = ((1 - Math.Sqrt(r1)) * A) + (Math.Sqrt(r1) * (1 - r2) * B) + (Math.Sqrt(r1) * r2 * C);
                samplingPoints.Add(point + barycenter);
            }

            tri.SamplingPoints = samplingPoints;
        }
    }
}
