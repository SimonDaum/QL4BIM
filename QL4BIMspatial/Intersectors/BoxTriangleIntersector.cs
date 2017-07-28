using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMspatial
{
    public class BoxTriangleIntersector : IIntersector<Box, Triangle>
    {
        private static readonly Vector<double>[] CartAxes;

        private readonly BoxIntersector boxIntersector = new BoxIntersector(new IntervalIntersector());

        static BoxTriangleIntersector()
        {
            CartAxes = new Vector<double>[]
                       {
                           new DenseVector(new double[] {1, 0, 0}), new DenseVector(new double[] {0, 1, 0}),
                           new DenseVector(new double[] {0, 0, 1})
                       };
        }


        public bool TestStrict(Box box, Triangle tri)
        {
            if (!boxIntersector.TestStrict(box, tri.Bounds))
                return false;

            Vector<double> boxCenter = box.Center.Vector;

            var triPoints = new[] {tri.A.Vector - boxCenter, tri.B.Vector - boxCenter, tri.C.Vector - boxCenter};

            // the cross products between the triangle's edges and the box'es edges (parallel to the cartesian axes) are possible separating axes:
            IEnumerable<Vector<double>> crossProducts = CartAxes.SelectMany(c => tri.Edges.Select(c.CrossProduct));

            // the box'es normal vectors (i.e. the cartesian axes) don't have to be tested as the bounding box test above rules them out already:
            IEnumerable<Vector<double>> axes = (new[] {tri.Normal}).Concat(crossProducts);
            //var axes = cartAxes.Concat(new[] { tri.Normal }).Concat(crossProducts);

            foreach (var axis in axes)
            {
                Vector<double> axis1 = axis;
                Interval triProj = Interval.Union(triPoints.Select(p => p*axis1));
                double boxRadius = (box.X.Length*Math.Abs(axis[0]) + box.Y.Length*Math.Abs(axis[1]) +
                                    box.Z.Length*Math.Abs(axis[2]))/2;

                if (triProj.Max <= -boxRadius || boxRadius <= triProj.Min)
                    return false;
            }

            return true;
        }

        public bool Test(Box box, Triangle tri)
        {
            if (!boxIntersector.Test(box, tri.Bounds))
                return false;

            Vector<double> boxCenter = box.Center.Vector;

            var triPoints = new[] {tri.A.Vector - boxCenter, tri.B.Vector - boxCenter, tri.C.Vector - boxCenter};

            // the cross products between the triangle's edges and the box'es edges (parallel to the cartesian axes) are possible separating axes:
            IEnumerable<Vector<double>> crossProducts = CartAxes.SelectMany(c => tri.Edges.Select(c.CrossProduct));

            // the box'es normal vectors (i.e. the cartesian axes) don't have to be tested as the bounding box test above rules them out already:
            IEnumerable<Vector<double>> axes = (new[] {tri.Normal}).Concat(crossProducts);
            //var axes = cartAxes.Concat(new[] { tri.Normal }).Concat(crossProducts);

            foreach (var axis in axes)
            {
                Vector<double> axis1 = axis;
                Interval triProj = Interval.Union(triPoints.Select(p => p*axis1));
                double boxRadius = (box.X.Length*Math.Abs(axis[0]) + box.Y.Length*Math.Abs(axis[1]) +
                                    box.Z.Length*Math.Abs(axis[2]))/2;

                if (triProj.Max < -boxRadius || boxRadius < triProj.Min)
                    return false;
            }

            return true;
        }

        public bool TestStrict(Triangle first, Box second)
        {
            return TestStrict(second, first);
        }

        public bool Test(Triangle first, Box second)
        {
            return Test(second, first);
        }
    }
}