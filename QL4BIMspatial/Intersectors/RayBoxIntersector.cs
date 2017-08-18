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
