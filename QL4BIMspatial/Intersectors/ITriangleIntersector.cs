/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMspatial.

QL4BIMspatial is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMspatial is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMspatial. If not, see <http://www.gnu.org/licenses/>.
*/

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public interface ITriangleIntersector
    {
        bool DoIntersect(Triangle triangleA, Triangle triangleB);

        bool EdgeAgainsEdge(PolygonPoint V0, PolygonPoint V1, PolygonPoint U0, PolygonPoint U1, int i0, int i1,
            out PolygonPoint secPointA, out PolygonPoint secPointB);

        bool EdgeAgainsEdge(DenseVector V0, DenseVector V1, DenseVector U0, DenseVector U1, int i0, int i1, out DenseVector p, out double alpha, out double beta);
    }
}
