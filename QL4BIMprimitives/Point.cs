/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMprimitives.

QL4BIMprimitives is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMprimitives is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMprimitives. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMprimitives
{
    public class Point
    {
        /// <summary>
        ///     Number of dimensions in a point. In theory this
        ///     could be exended to three or more dimensions.
        /// </summary>
        private const int Dimensions = 3;
        private double p;


        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="x">The x coordinate of the point</param>
        /// <param name="y">The y coordinate of the point</param>
        /// <param name="z">The z coordinate of the point</param>
        public Point(double x, double y, double z)
        {
            var coordinates = new double[Dimensions];
            coordinates[0] = x;
            coordinates[1] = y;
            coordinates[2] = z;

            // caution: vector is bound to coordinates array, changes affect each other
            Vector = new DenseVector(coordinates);
        }

        public Point(DenseVector denseVector)
        {
            // caution: vector is bound to coordinates array, changes affect each other
            Vector = denseVector;
        }

        public Point(Tuple<double, double, double> point)
            : this(point.Item1, point.Item2, point.Item3)
        {
        }

        public Point(double p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }

        public Vector<double> Vector { get; private set; }


        public double X
        {
            get { return Vector[0]; }
            set { Vector[0] = value; }
        }

        public double Y
        {
            get { return Vector[1]; }
            set { Vector[1] = value; }
        }

        public double Z
        {
            get { return Vector[2]; }
            set { Vector[2] = value; }
        }

        public double Distance(Point p)
        {
            return (Vector - p.Vector).Norm(2);
        }

        public string GetCoordString()
        {
            return string.Format("{0:F4} {1:F4} {2:F4} ", X, Y, Z);
        }
    }
}
