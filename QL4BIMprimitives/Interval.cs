/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMprimitives.

QL4BIMindexing is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMindexing is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMprimitives. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace QL4BIMprimitives
{
    public class Interval
    {
        /// <summary>
        ///     Creates a new interval between the given values.
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        public Interval(double val1, double val2)
        {
            Min = Math.Min(val1, val2);
            Max = Math.Max(val1, val2);
            Length = Max - Min;
        }

        public override string ToString()
        {
            return "Min: " + Min + " Max: " + Max;
        }

        /// <summary>
        ///     Gets the start value.
        /// </summary>
        public double Min { get; private set; }

        /// <summary>
        ///     Gets the end value.
        /// </summary>
        public double Max { get; private set; }

        /// <summary>
        ///     Gets the interval's length.
        /// </summary>
        public double Length { get; private set; }

        public bool ValueContainedRelaxed(double value)
        {
            return (Min <= value && value <= Max);
        }

        public static double[] MinMaxLength(Interval a, Interval b)
        {
            return new[] { GapLength(a, b), UnionLength(a, b) };
        }

        public static Interval[] SplitInterval(Interval interval)
        {
            var mid = interval.Min + interval.Length/2;
            return new []{new Interval(interval.Min, mid), new Interval(mid, interval.Min) };
        }

        public static double UnionLength(Interval a, Interval b)
        {
            var min = Math.Min(a.Min, b.Min);
            var max = Math.Max(a.Max, b.Max);

            return Math.Abs(max - min);
        }

        public static double UnionLength(double a, Interval b)
        {
            var min = Math.Min(a, b.Min);
            var max = Math.Max(a, b.Max);

            return Math.Abs(max - min);
        }



        public static double GapLength(double a, Interval b)
        {
            var maxmin = Math.Max(a, b.Min);
            var minmax = Math.Min(a, b.Max);

            var dist = minmax - maxmin;
            if (a > b.Min && a < b.Max)
                return 0;

            return Math.Abs(dist);
        }


        public static double GapLength(Interval a, Interval b)
        {
            var maxmin = Math.Max(a.Min, b.Min);
            var minmax = Math.Min(a.Max, b.Max);

            var dist = minmax - maxmin;
            if (Intersects(a,b))
                return 0;

            return Math.Abs(dist);
        }


        /// <summary>
        ///     Creates a new interval as the smallest interval containing all given values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Interval Union(IEnumerable<double> values)
        {
            double min = double.MaxValue;
            double max = double.MinValue;

            foreach (double val in values)
            {
                min = Math.Min(min, val);
                max = Math.Max(max, val);
            }

            return new Interval(min, max);
        }

        /// <summary>
        ///     Creates a new interval as the smallest interval containing all given values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Interval Union(params double[] values)
        {
            return Union(values.AsEnumerable());
        }

        /// <summary>
        ///     Creates a new interval as the smallest interval containing all given interval.
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        public static Interval Union(IEnumerable<Interval> intervals)
        {
            Interval[] enumerable = intervals.ToArray();
            Interval first = enumerable.First();
            double min = first.Min;
            double max = first.Max;

            foreach (Interval inter in enumerable.Skip(1))
            {
                min = Math.Min(min, inter.Min);
                max = Math.Max(max, inter.Max);
            }

            return new Interval(min, max);
        }

        /// <summary>
        ///     Creates a new interval as the smallest interval containing all given intervals.
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        public static Interval Union(params Interval[] intervals)
        {
            return Union(intervals.AsEnumerable());
        }



        /// <summary>
        ///     Creates a new interval as the intersection of all given intervals. If the given intervals do not intersect, a
        ///     zero-sized interval centered at 0 will be returned.
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        public static Interval Intersection(IEnumerable<Interval> intervals)
        {
            Interval[] enumerable = intervals.ToArray();
            Interval first = enumerable.First();
            double min = first.Min;
            double max = first.Max;

            foreach (Interval inter in enumerable.Skip(1))
            {
                min = Math.Max(min, inter.Min);
                max = Math.Min(max, inter.Max);

                // if max had become smaller than min, there was an interval not intersecting the others:
                if (max < min)
                    return new Interval(0, 0);
            }

            return new Interval(min, max);
        }

        public static bool Intersects(Interval a, Interval b)
        {
            return a.Max > b.Min && b.Max > a.Min;
        }

        /// <summary>
        ///     Creates a new interval as the intersection of all given intervals. If the given intervals do not intersect, a
        ///     zero-sized interval centered at 0 will be returned.
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        public static Interval Intersection(params Interval[] intervals)
        {
            return Intersection(intervals.AsEnumerable());
        }

        public static double[] MinMinMaxDists(Interval a, Interval b)
        {
            bool aBeforeB = a.Max < b.Min;
            bool bBeforeA = b.Max < a.Min;
            bool doIntersect = !aBeforeB && !bBeforeA;

            double minDist;
            double maxMinDist;
            if (aBeforeB)
            {
                minDist = b.Min - a.Max;
                maxMinDist = b.Min - a.Min;
            }
            else
            {
                minDist = a.Min - b.Max;
                maxMinDist = a.Min - b.Min;
            }

            if (doIntersect)
                minDist = 0;

            return new[] { minDist, maxMinDist };
        }
    }
}
