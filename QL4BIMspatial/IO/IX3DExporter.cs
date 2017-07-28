using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;


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