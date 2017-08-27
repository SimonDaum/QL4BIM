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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace QL4BIMprimitives
{
    public class Ray
    {
        /// <summary>
        ///     Creates a new ray with given starting point and direction.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="direction"></param>
        public Ray(Vector<double> start, Vector<double> direction)
        {
            Start = start;
            Direction = direction;
        }

        /// <summary>
        ///     Creates a new ray from the given starting point into the specified cartesian axis direction.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="axis"></param>
        /// <param name="dir"></param>
        public Ray(Vector<double> start, Axis axis, AxisDirection dir)
        {
            Start = start;

            int val = (dir == AxisDirection.Positive) ? 1 : -1;
            Direction = DenseVector.Create(3, i => ((Axis) i == axis) ? val : 0);
        }

        /// <summary>
        ///     Gets the ray's start point.
        /// </summary>
        public Vector<double> Start { get; private set; }

        /// <summary>
        ///     Gets the direction in which the ray points.
        /// </summary>
        public Vector<double> Direction { get; private set; }
    }
}
