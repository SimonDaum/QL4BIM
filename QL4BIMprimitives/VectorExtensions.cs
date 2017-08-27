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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMprimitives
{
    public static class VectorExtensions
    {
        public static Vector<double> CrossProduct(this Vector<double> u, Vector<double> v)
        {
            if (u.Count != 3 || v.Count != 3)
                throw new ArgumentException("The cross product is only valid for 3 dimensional vectors.");

            return new DenseVector(new[]
                                   {
                                       u[1]*v[2] - u[2]*v[1],
                                       u[2]*v[0] - u[0]*v[2],
                                       u[0]*v[1] - u[1]*v[0]
                                   });
        }
    }
}
