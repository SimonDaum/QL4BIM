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

using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using QL4BIMprimitives;


namespace QL4BIMspatial
{
    public interface IX3DExporter
    {
        void ExportMeshes(string file, IEnumerable<TriangleMesh> triangleMeshes);
        void ExportMeshAsTriangles(string file, IEnumerable<Triangle> mesh);
        void ExportBoxes(string file, string prefix, Box[] boxes);
        void ExportBoxes(string file, string prefix, IEnumerable<Tuple<Box, Vector<double>>> transBoxes);
        void ExportPolygon(string file, Polygon polygon);
        void ExportPolygons(string file, IEnumerable<Polygon> polygons);
        void ExportPoints(string file, IEnumerable<Vector<double>> points);
    }
}
