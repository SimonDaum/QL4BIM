using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using MathNet.Numerics.LinearAlgebra;

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
