using System;

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
