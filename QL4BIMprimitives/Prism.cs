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
using QL4BIMprimitives;

namespace QL4BIMprimitives
{
    public class Prism
    {
        public Prism(Triangle baseFace, double length, Axis axis, bool positiv)
        {
            Base = baseFace;

            if (!positiv)
                length = length*-1;

            if(axis == Axis.X)
                Translation = new DenseVector(new[] { length, 0d, 0d });

            if (axis == Axis.Y)
                Translation = new DenseVector(new[] { 0d, length, 0d });

            if (axis == Axis.Z)
                Translation = new DenseVector(new[] { 0d, 0d, length });
        }

        public Triangle Base { get; private set; }

        public Vector<double> Translation { get; private set; }

        public Box Bounds
        {
            get { throw new NotImplementedException(); }
        }
    }
}
