//   Rectangle.java
//   Java Spatial Index Library
//   Copyright (C) 2002 Infomatiq Limited
//  
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//  
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//  
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA

// Ported to C# By Dror Gluska, April 9th, 2009

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;


namespace QL4BIMprimitives
{

    /**
     * Currently hardcoded to 3 dimensions, but could be extended.
     * 
     * @author  aled@sourceforge.net
     * @version 1.0b2p1
     */
    public class Box : IHasBounds
    {
        public readonly int Dimensions = 3;

        private double[] Max = new double[3];

        private double[] Min = new double[3];

        private Interval[] intervals = null;

        private int id;

        public int NameId { get; set; }

        private static int instanceCount;

        private Box offsetBox;
        private double offsetValue = 0;



        /// <summary>
        ///     Gets or sets a interval representing the box'es extent along the x axis.
        /// </summary>
        public Interval X { get; set; }

        /// <summary>
        ///     Gets or sets a interval representing the box'es extent along the y axis.
        /// </summary>
        public Interval Y { get; set; }

        /// <summary>
        ///     Gets or sets a interval representing the box'es extent along the z axis.
        /// </summary>
        public Interval Z { get; set; }

        public const string IndexFaceSetIndices = "0 1 2 3 -1 4 5 6 7 -1 0 4 5 1 -1 2 6 7 3  -1 1 5 6 2 -1 0 3 7 4";

        /**
         * Constructor.
         * 
         * @param x1 coordinate of any corner of the rectangle
         * @param y1 (see x1)
         * @param x2 coordinate of the opposite corner
         * @param y2 (see x2)
         */

        public Box(Interval xInterval, Interval yInterval, Interval zInterval)
        {
            Init(xInterval.Min, xInterval.Max, yInterval.Min, yInterval.Max, zInterval.Min, zInterval.Max);
        }

        public Box(double x1, double x2, double y1, double y2, double z1, double z2)
        {
            instanceCount++;
            Init(x1, x2, y1, y2, z1, z2);
        }


        private Box(double[] min, double[] max, int dimensions)
        {
            instanceCount++;
            id = instanceCount;
            Dimensions = dimensions;

            Array.Copy(min, 0, this.Min, 0, Dimensions);
            Array.Copy(max, 0, this.Max, 0, Dimensions);

            X = new Interval(Min[0], Max[0]);
            Y = new Interval(Min[1], Max[1]);
            Z = new Interval(Min[2], Max[2]);

        }

        private void Init(double x1, double x2, double y1, double y2, double z1, double z2)
        {
            Min[0] = Math.Min(x1, x2);
            Min[1] = Math.Min(y1, y2);
            Min[2] = Math.Min(z1, z2);

            Max[0] = Math.Max(x1, x2);
            Max[1] = Math.Max(y1, y2);
            Max[2] = Math.Max(z1, z2);

            X = new Interval(Min[0], Max[0]);
            Y = new Interval(Min[1], Max[1]);
            Z = new Interval(Min[2], Max[2]);


            id = instanceCount;
        }

        /**
         * Sets the size of this rectangle to the size of the given rectangle
         */
        public void Set(Box r)
        {
            if (Dimensions != r.Dimensions)
            {
                throw new Exception("Error in Rectangle.Set: both rectangles must be of dimension " + Dimensions);
            }

            Array.Copy(r.Min, 0, this.Min, 0, Dimensions);
            Array.Copy(r.Max, 0, this.Max, 0, Dimensions);
        }

        public static Box Union(IEnumerable<IHasBounds> items)
        {
            return Union(items.Select(e => e.Bounds));
        }

        public static Box Union(IEnumerable<Box> rectangles)
        {
            int dimensions = rectangles.First().Dimensions;
            double[] min = new double[] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity };
            double[] max = new double[] { double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity };

            foreach (var r in rectangles)
            {
                if (r.Dimensions != dimensions) throw new Exception();

                for (int d = 0; d < dimensions; d++)
                {
                    min[d] = Math.Min(min[d], r.Min[d]);
                    max[d] = Math.Max(max[d], r.Max[d]);
                }
            }

            return new Box(min, max, dimensions);
        }

        public static Box Union(params Box[] r)
        {
            return Union((IEnumerable<Box>)r);
        }

        public Interval GetInterval(Axis axis)
        {
            if (axis == Axis.X)
                return X;

            if (axis == Axis.Y)
                return Y;

            if (axis == Axis.Z)
                return Z;

            throw  new InvalidParameterException();
        }

        public bool TriangleInsideRelaxed(Triangle triangle)
        {
            return  PointInsideRelaxed(triangle.A.Vector) &&
                    PointInsideRelaxed(triangle.B.Vector) && 
                    PointInsideRelaxed(triangle.C.Vector);
        }

        public bool PointInsideRelaxed(Vector<double> point )
        {
            return  X.ValueContainedRelaxed(point[0]) &&
                    Y.ValueContainedRelaxed(point[1]) &&
                    Z.ValueContainedRelaxed(point[2]);
        }

        public Interval[] GetIntervals ()
        {   
            if (intervals == null)
            {   
                intervals = new Interval[3];
                intervals[0] = new Interval(XMin, XMax);
                intervals[1] = new Interval(YMin, YMax);
                intervals[2] = new Interval(ZMin, ZMax);
            }


            return intervals;
        }

        public double XMax
        {
            get { return Max[0]; }
            set { Max[0] = value; }
        }

        public double YMax
        {
            get { return Max[1]; }
            set { Max[1] = value; }
        }

        public double ZMax
        {
            get { return Max[2]; }
            set { Max[2] = value; }
        }

        public double XMin
        {
            get { return Min[0]; }
            set { Min[0] = value; }
        }

        public double YMin
        {
            get { return Min[1]; }
            set { Min[1] = value; }
        }

        public double ZMin
        {
            get { return Min[2]; }
            set { Min[2] = value; }
        }

        public double XSize
        {
            get { return XMax - XMin; }
        }

        public double YSize
        {
            get { return YMax - YMin; }
        }

        public double ZSize
        {
            get { return ZMax - ZMin; }
        }

        public double GetMax(int axis)
        {
            return Max[axis];
        }

        public double GetMin(int axis)
        {
            return Min[axis];
        }

        public void SetMax(int axis, double value)
        {
            Max[axis] = value;
        }

        public void SetMin(int axis, double value)
        {
            Min[axis] = value;
        }

        /**
         * Make a copy of this rectangle
         * 
         * @return copy of this rectangle
         */
        public Box Copy()
        {
            return new Box(Min, Max, Dimensions);
        }

        public Box[] Split()
        {
            var intervalsX = Interval.SplitInterval(this.X);
            var intervalsY = Interval.SplitInterval(this.Y);
            var intervalsZ = Interval.SplitInterval(this.Z);

            return new[]
            {
                new Box(intervalsX[0], intervalsY[0], intervalsZ[0]),
                new Box(intervalsX[0], intervalsY[0], intervalsZ[1]),
                new Box(intervalsX[0], intervalsY[1], intervalsZ[0]),
                new Box(intervalsX[0], intervalsY[1], intervalsZ[1]),
                new Box(intervalsX[1], intervalsY[0], intervalsZ[0]),
                new Box(intervalsX[1], intervalsY[0], intervalsZ[1]),
                new Box(intervalsX[1], intervalsY[1], intervalsZ[0]),
                new Box(intervalsX[1], intervalsY[1], intervalsZ[1])
            };
        }

        /**
         * Determine whether an edge of this rectangle overlies the equivalent 
         * edge of the passed rectangle
         */
        public bool EdgeOverlaps(Box b)
        {
            for (int i = 0; i < Dimensions; i++)
            {
                if (Min[i] == b.Min[i] || Max[i] == b.Max[i])
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Determine whether this rectangle intersects the passed rectangle
         * 
         * @param r The rectangle that might intersect this rectangle
         * 
         * @return true if the rectangles intersect, false if they do not intersect
         */
        public bool Intersects(Box r)
        {
            // Every dimension must intersect. If any dimension
            // does not intersect, return false immediately.
            for (int i = 0; i < Dimensions; i++)
            {
                if (Max[i] < r.Min[i] || Min[i] > r.Max[i])
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * Determine whether this rectangle contains the passed rectangle
         * 
         * @param r The rectangle that might be contained by this rectangle
         * 
         * @return true if this rectangle contains the passed rectangle, false if
         *         it does not
         */
        public bool Contains(Box r)
        {
            for (int i = 0; i < Dimensions; i++)
            {
                if (Max[i] < r.Max[i] || Min[i] > r.Min[i])
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * Determine whether this rectangle is contained by the passed rectangle
         * 
         * @param r The rectangle that might contain this rectangle
         * 
         * @return true if the passed rectangle contains this rectangle, false if
         *         it does not
         */
        public bool ContainedBy(Box r)
        {
            for (int i = 0; i < Dimensions; i++)
            {
                if (Max[i] > r.Max[i] || Min[i] < r.Min[i])
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * Return the distance between this rectangle and the passed point.
         * If the rectangle contains the point, the distance is zero.
         * 
         * @param p Point to find the distance to
         * 
         * @return distance beween this rectangle and the passed point.
         */
        //internal double Distance(Point p)
        //{
        //    double distanceSquared = 0;
        //    for (int i = 0; i < Dimensions; i++)
        //    {
        //        var greatestMin = Math.Max(Min[i], p.coordinates[i]);
        //        var leastMax = Math.Min(Max[i], p.coordinates[i]);
        //        if (greatestMin > leastMax)
        //        {
        //            distanceSquared += ((greatestMin - leastMax) * (greatestMin - leastMax));
        //        }
        //    }
        //    return (double)Math.Sqrt(distanceSquared);
        //}

        /**
         * Return the distance between this rectangle and the passed rectangle.
         * If the rectangles overlap, the distance is zero.
         * 
         * @param r Rectangle to find the distance to
         * 
         * @return distance between this rectangle and the passed rectangle
         */

        //internal double Distance(Rectangle r)
        //{
        //    double distanceSquared = 0;
        //    for (int i = 0; i < Dimensions; i++)
        //    {
        //        double greatestMin = Math.Max(Min[i], r.Min[i]);
        //        double leastMax = Math.Min(Max[i], r.Max[i]);
        //        if (greatestMin > leastMax)
        //        {
        //            distanceSquared += ((greatestMin - leastMax) * (greatestMin - leastMax));
        //        }
        //    }
        //    return (double)Math.Sqrt(distanceSquared);
        //}

        /**
         * Return the squared distance from this rectangle to the passed point
         */
        //internal double DistanceSquared(int dimension, double point)
        //{
        //    double distanceSquared = 0;
        //    double tempDistance = point - Max[dimension];
        //    for (int i = 0; i < 2; i++)
        //    {
        //        if (tempDistance > 0)
        //        {
        //            distanceSquared = (tempDistance * tempDistance);
        //            break;
        //        }
        //        tempDistance = Min[dimension] - point;
        //    }
        //    return distanceSquared;
        //}

        /**
         * Return the furthst possible distance between this rectangle and
         * the passed rectangle. 
         * 
         * Find the distance between this rectangle and each corner of the
         * passed rectangle, and use the maximum.
         *
         */
        //internal double FurthestDistance(Rectangle r)
        //{
        //    double distanceSquared = 0;

        //    for (int i = 0; i < Dimensions; i++)
        //    {
        //        distanceSquared += Math.Max(r.Min[i], r.Max[i]);

        //        //distanceSquared += Math.Max(distanceSquared(i, r.min[i]), distanceSquared(i, r.max[i]));
        //    }

        //    return (double)Math.Sqrt(distanceSquared);
        //}

        /**
         * Calculate the area by which this rectangle would be enlarged if
         * added to the passed rectangle. Neither rectangle is altered.
         * 
         * @param r Rectangle to union with this rectangle, in order to 
         *          compute the difference in area of the union and the
         *          original rectangle
         */
        public double Enlargement(Box r)
        {
            double enlargedArea = (Math.Max(Max[0], r.Max[0]) - Math.Min(Min[0], r.Min[0]))*
                                  (Math.Max(Max[1], r.Max[1]) - Math.Min(Min[1], r.Min[1]))*
                                  (Math.Max(Max[2], r.Max[2]) - Math.Min(Min[2], r.Min[2]));

            return enlargedArea - Area;
        }

        public double Overlap(Box r)
        {
            if (!Intersects(r)) return 0.0;

            double overlap = (Math.Min(Max[0], r.Max[0]) - Math.Max(Min[0], r.Min[0]))*
                             (Math.Min(Max[1], r.Max[1]) - Math.Max(Min[1], r.Min[1]))*
                             (Math.Min(Max[2], r.Max[2]) - Math.Max(Min[2], r.Min[2]));

            return overlap;
        }

        /**
         * Compute the area of this rectangle.
         * 
         * @return The area of this rectangle
         */
        public double Area
        {
            get
            {
                double area = (Max[0] - Min[0]) * (Max[1] - Min[1]) * (Max[2] - Min[2]);

                return area;
            }
        }

        public double Margin
        {
            get
            {
                double margin = 2 * ((Max[0] - Min[0]) * (Max[1] - Min[1]) + (Max[0] - Min[0]) * (Max[2] - Min[2]) + (Max[1] - Min[1]) * (Max[2] - Min[2]));
                return margin;
            }
        }

        /**
         * Computes the union of this rectangle and the passed rectangle, storing
         * the result in this rectangle.
         * 
         * @param r Rectangle to add to this rectangle
         */
        public void Add(Box r)
        {
            for (int i = 0; i < Dimensions; i++)
            {
                if (r.Min[i] < Min[i])
                {
                    Min[i] = r.Min[i];
                }
                if (r.Max[i] > Max[i])
                {
                    Max[i] = r.Max[i];
                }
            }
        }

        /**
         * Find the the union of this rectangle and the passed rectangle.
         * Neither rectangle is altered
         * 
         * @param r The rectangle to union with this rectangle
         */
        public Box Union(Box r)
        {
            Box union = this.Copy();
            union.Add(r);
            return union;
        }

        private bool CompareArrays(double[] a1, double[] a2)
        {
            if ((a1 == null) || (a2 == null))
                return false;
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;
            return true;
        }

        /**
         * Determine whether this rectangle is equal to a given object.
         * Equality is determined by the bounds of the rectangle.
         * 
         * @param o The object to compare with this rectangle
         */
        public override bool Equals(object obj)
        {
            bool equals = false;
            if (obj.GetType() == typeof(Box))
            {
                Box r = (Box)obj;
                if (CompareArrays(r.Min, Min) && CompareArrays(r.Max, Max))
                {
                    equals = true;
                }
            }
            return equals;
        }

        /** 
         * Determine whether this rectangle is the same as another object
         * 
         * Note that two rectangles can be equal but not the same object, 
         * if they both have the same bounds.
         * 
         * @param o The object to compare with this rectangle.
         */
        internal bool SameObject(object o)
        {
            return base.Equals(o);
        }

        /**
         * Return a string representation of this rectangle, in the form: 
         * (1.2, 3.4), (5.6, 7.8)
         * 
         * @return String String representation of this rectangle.
         */

        public override string ToString()
        {
            var sb = new StringBuilder();

            // min coordinates
            sb.Append('(');
            for (int i = 0; i < Dimensions; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(Min[i]);
            }
            sb.Append("), (");

            // max coordinates
            for (int i = 0; i < Dimensions; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(Max[i]);
            }
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Returns a copy of this rectangle offset in all directions.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Box Offset(double offset)
        {
            if (offset == offsetValue && offsetBox != null)
                return offsetBox;

            offsetBox = new Box(Min[0] - offset, Max[0] + offset,
                     Min[1] - offset, Max[1] + offset,
                     Min[2] - offset, Max[2] + offset);

            offsetValue = offset;


            return offsetBox;
        }

        public Point Center
        {
            get { return new Point((XMax - XMin) / 2, (YMax - YMin) / 2, (ZMax - ZMin) / 2); }
        }

        Box IHasBounds.Bounds
        {
            get { return this; }
        }

        public int Id
        {
            get { return id; }
        }

        public String ToIndexedFaceSet()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(new Point(XMin, YMin, ZMin)); //point11 0
            stringBuilder.Append(new Point(XMax, YMin, ZMin)); //point21 1
            stringBuilder.Append(new Point(XMax, YMax, ZMin)); //point31 2
            stringBuilder.Append(new Point(XMin, YMax, ZMin)); //point41 3
                                                       
            stringBuilder.Append(new Point(XMin, YMin, ZMax)); //point12 4
            stringBuilder.Append(new Point(XMax, YMin, ZMax)); //point22 5
            stringBuilder.Append(new Point(XMax, YMax, ZMax)); //point32 6
            stringBuilder.Append(new Point(XMin, YMax, ZMax)); //point42 7

            return stringBuilder.ToString();
        }

        public static Box ToWorldBox(double[] trans, Box localBox)
        {
            return new Box(localBox.XMin + trans[0], localBox.XMax + trans[0],
                            localBox.YMin + trans[1], localBox.XMax + trans[1],
                            localBox.ZMin + trans[2], localBox.ZMax + trans[2]);
        }


        public Box Bounds
        {
            get { return this; }

        }

        public static double BoxDistanceMax(Box boxA, Box boxB)
        {
            var maxX = Interval.UnionLength(boxA.X, boxB.X);
            var maxY = Interval.UnionLength(boxA.Y, boxB.Y);
            var maxZ = Interval.UnionLength(boxA.Z, boxB.Z);

            return maxX * maxX + maxY * maxY + maxZ * maxZ;
        }

        public static double BoxDistanceMin(Box boxA, Box boxB)
        {
            var minX = Interval.GapLength(boxA.X, boxB.X);
            var minY = Interval.GapLength(boxA.Y, boxB.Y);
            var minZ = Interval.GapLength(boxA.Z, boxB.Z);

            return minX * minX + minY * minY + minZ * minZ;
        }

        public static Interval BoxDistanceMinMax(Vector<double> vectorA, Box boxB)
        {
            return new Interval(BoxDistanceMin(vectorA, boxB), BoxDistanceMax(vectorA, boxB));
        }


        public static double BoxDistanceMax(Vector<double> vectorA, Box boxB)
        {
            var maxX = Interval.UnionLength(vectorA[0], boxB.X);
            var maxY = Interval.UnionLength(vectorA[1], boxB.Y);
            var maxZ = Interval.UnionLength(vectorA[2], boxB.Z);

            return maxX * maxX + maxY * maxY + maxZ * maxZ;
        }

        public static double BoxDistanceMin(Vector<double> vectorA, Box boxB)
        {
            var minX = Interval.GapLength(vectorA[0], boxB.X);
            var minY = Interval.GapLength(vectorA[1], boxB.Y);
            var minZ = Interval.GapLength(vectorA[2], boxB.Z);

            return minX * minX + minY * minY + minZ * minZ;
        }

        public static Interval BoxDistanceMinMax(Box boxA, Box boxB)
        {
            var mmX = Interval.MinMaxLength(boxA.X, boxB.X);
            var mmY = Interval.MinMaxLength(boxA.Y, boxB.Y);
            var mmZ = Interval.MinMaxLength(boxA.Z, boxB.Z);

            var maxMinX = mmX[1] * mmX[1] + mmY[0] * mmY[0] + mmZ[0] * mmZ[0];
            var maxMinY = mmX[0] * mmX[0] + mmY[1] * mmY[1] + mmZ[0] * mmZ[0];
            var maxMinZ = mmX[0] * mmX[0] + mmY[0] * mmY[0] + mmZ[1] * mmZ[1];
            var maxMin = Math.Min(Math.Min(maxMinX, maxMinY), maxMinZ);

            var minDist = mmX[0] * mmX[0] + mmY[0] * mmY[0] + mmZ[0] * mmZ[0];

            return new Interval(minDist, maxMin);
        }

                // NOTE: positive x-axis determines east-direction
        // NOTE: negative x-axis determines west-direction
        // NOTE: positive y-axis determines north-direction
        // NOTE: negative y-axis determines south-direction
        // NOTE: positive z-axis determines up-direction
        // NOTE: negative z-axis determines down-direction
        public bool IntersectsColumm(Box r, Axis ax, bool positiveDirection)
        {
            if (ax == Axis.X && positiveDirection)
            {
                //west of me
                if (X.Min > r.X.Max)
                    return false;

                //disjoint z
                if (Y.Max < r.Y.Min || Y.Min > r.Y.Max)
                    return false;

                //disjoint z
                if (Z.Max < r.Z.Min || Z.Min > r.Z.Max)
                    return false;

                return true;
            }

            if (ax == Axis.X && !positiveDirection)
            {
                //east of me
                if (X.Max < r.X.Min)
                    return false;

                //disjoint z
                if (Y.Max < r.Y.Min || Y.Min > r.Y.Max)
                    return false;

                //disjoint z
                if (Z.Max < r.Z.Min || Z.Min > r.Z.Max)
                    return false;

                return true;
            }

            if (ax == Axis.Y && positiveDirection)
            {
                //disjoint x
                if (X.Max < r.X.Min || X.Min > r.X.Max)
                    return false;

                //south of me
                if (Y.Min > r.Y.Max)
                    return false;

                //disjoint z
                if (Z.Max < r.Z.Min || Z.Min > r.Z.Max)
                    return false;

                return true;
            }

            if (ax == Axis.Y && !positiveDirection)
            {
                //disjoint x
                if (X.Max < r.X.Min || X.Min > r.X.Max)
                    return false;

                //north of me
                if (Y.Max < r.Y.Min)
                    return false;

                //disjoint z
                if (Z.Max < r.Z.Min || Z.Min > r.Z.Max)
                    return false;

                return true;
            }



            if (ax == Axis.Z && positiveDirection)
            {
                
                //disjoint x
                if (X.Max < r.X.Min|| X.Min > r.X.Max)
                    return false;

                //disjoint z
                if (Y.Max < r.Y.Min|| Y.Min > r.Y.Max)
                    return false;

                //below me
                if (Z.Min > r.Z.Max)
                    return false;

                return true;
            }

            if (ax == Axis.Z && !positiveDirection)
            {
                //disjoint x
                if (X.Max < r.X.Min || X.Min > r.X.Max)
                    return false;

                //disjoint z
                if (Y.Max < r.Y.Min || Y.Min > r.Y.Max)
                    return false;

                //above me
                if (Z.Max < r.Z.Min)
                    return false;

                return true;
            }

            throw new InvalidOperationException();
        }

        public Box ExtendInDirection(Box second, Axis ax, bool positiveDirection)
        {
            if (ax == Axis.X && positiveDirection)
                return new Box(X.Min, second.X.Max, Y.Min, Y.Max, Z.Min, Z.Max);

            if (ax == Axis.X && !positiveDirection)
                return new Box(second.X.Min, X.Max, Y.Min, Y.Max, Z.Min, Z.Max);


            if (ax == Axis.Y && positiveDirection)
                return new Box(X.Min, X.Max, Y.Min, second.Y.Max, Z.Min, Z.Max);

            if (ax == Axis.Y && !positiveDirection)
                return new Box(X.Min, X.Max, second.Y.Min, Y.Max, Z.Min, Z.Max);


            if (ax == Axis.Z && positiveDirection)
                return new Box(X.Min, X.Max, Y.Min, Y.Max, Z.Min, second.Z.Max);

            if (ax == Axis.Z && !positiveDirection)
                return new Box(X.Min, X.Max, Y.Min, Y.Max, second.Z.Min, Z.Max);

            return null;
        }
    }
}