using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public class X3DExporter : IX3DExporter
    {
        private const string BoxIndexFaceSetIndices = "0 1 2 3 -1 4 5 6 7 -1 0 4 5 1 -1 2 6 7 3  -1 1 5 6 2 -1 0 3 7 4";
        private const string TemplatesName = "QL4BIMspatial.IO.X3DTemplates.";

        public void ExportMeshes(string file, IEnumerable<TriangleMesh> triangleMeshes)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string fileTemplate = GetString(TemplatesName + "X3dFileTemplate.txt");

            var groups = new StringBuilder();
            foreach (TriangleMesh mesh in triangleMeshes)
                groups.Append(ExportMesh(mesh));

            StreamWriter streamWriter = File.CreateText(file);
            streamWriter.Write(fileTemplate, groups);
            streamWriter.Close();
        }

        public void ExportMeshAsTriangles(string file, IEnumerable<Triangle> mesh)
        {
            string fileTemplate = GetString(TemplatesName + "X3dFileTemplate.txt");

            var groups = new StringBuilder();
            foreach (Triangle triangle in mesh)
                groups.Append(ExportTriangle(triangle));

            StreamWriter streamWriter = File.CreateText(file);
            streamWriter.Write(fileTemplate, groups);
            streamWriter.Close();
        }

        public void ExportBoxes(string file, string prefix, Box[] boxes)
        {
            string fileTemplate = GetString(TemplatesName + "X3dFileTemplate.txt");

            var i = 0;
            var groups = new StringBuilder();
            foreach (Box box in boxes)
            {
                i++;
                groups.Append(ExportBox(box, prefix + i));
                //if(i > 10)
                //    break;
            }


            StreamWriter streamWriter = File.CreateText(file + ".x3d");
            streamWriter.Write(fileTemplate, groups);
            streamWriter.Close();
        }

        public void ExportPolygon(string file, Polygon polygon)
        {
            string fileTemplate = GetString(TemplatesName + "X3dFileTemplate.txt");

            StreamWriter streamWriter = File.CreateText(file);
            streamWriter.Write(fileTemplate, ExportLineSet(polygon));
            streamWriter.Close();
        }


        public void ExportPoints(string file, IEnumerable<Vector<double>> points)
        {
            string fileTemplate = GetString(TemplatesName + "X3dFileTemplate.txt");

            StreamWriter streamWriter = File.CreateText(file);

            var sb = new StringBuilder();
            foreach (var point in points)
                sb.Append(point[0].ToString("F4") + " " + point[1].ToString("F4") + " " + point[2].ToString("F4") + " ");

            string pointSetTemplate = GetString(TemplatesName + "X3dPointSet.txt");
            var coords = string.Format(pointSetTemplate, sb);

            streamWriter.Write(fileTemplate, coords);
            streamWriter.Close();
        }

        public void ExportPolygons(string file, IEnumerable<Polygon> polygons)
        {
            string fileTemplate = GetString(TemplatesName + "X3dFileTemplate.txt");

            var lines = new StringBuilder();
            foreach (var polygon in polygons)
                lines.Append(ExportLineSet(polygon));
            

            StreamWriter streamWriter = File.CreateText(file);
            streamWriter.Write(fileTemplate, lines);
            streamWriter.Close();
        }

        public void ExportBoxes(string file, string prefix, IEnumerable<Tuple<Box, Vector<double>>> transBoxes)
        {
            string fileTemplate = GetString(TemplatesName + "X3dFileTemplate.txt");

            var groups = new StringBuilder();
            foreach (var transBox in transBoxes)
                groups.Append(ExportBox(transBox, prefix));

            StreamWriter streamWriter = File.CreateText(file);
            streamWriter.Write(fileTemplate, groups);
            streamWriter.Close();
        }

        private string ExportMesh(TriangleMesh mesh)
        {
            string groupTemplate = GetString(TemplatesName + "X3dWorldGroupTemplate.txt");

            Triangle[] triangles = mesh.Triangles.ToArray();

            var indices = new StringBuilder();
            for (int i = 0; i < triangles.Length*3; i++)
            {
                indices.Append(i + " ");
                if ((i + 1)%3 == 0)
                    indices.Append("-1 ");
            }

            var vertices = new StringBuilder();
            foreach (Triangle triangle in triangles)
            {
                vertices.Append(triangle.A.GetCoordString() + triangle.B.GetCoordString() + triangle.C.GetCoordString());
            }

            string oupText = string.Format(groupTemplate, mesh.Name, indices, vertices);
            return oupText;
        }

        private string ExportTriangle(Triangle triangle)
        {
            string groupTemplate = GetString(TemplatesName + "X3dWorldGroupTemplate.txt");
            //var name = string.Format("Tri.{0:D5}", triangle.Id);
            return string.Format(groupTemplate, "", "0 1 2", triangle.A + " " + triangle.B + " " + triangle.C);
        }

        private string ExportBox(Box box, string prefix)
        {
            string groupTemplate = GetString(TemplatesName + "X3dWorldGroupTemplate.txt");
            return string.Format(groupTemplate, prefix + "", BoxIndexFaceSetIndices, BoxToIndexedFaceSet(box));
        }

        private string ExportLineSet(Polygon polygon)
        {
            string lineSetTemplate = GetString(TemplatesName + "X3dLineSet.txt");
            return string.Format(lineSetTemplate, polygon.PointCount, polygon.ToString());
        }


        private String BoxToIndexedFaceSet(Box box)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(PointToString(box.X.Min, box.Y.Min, box.Z.Min)); //point11 0
            stringBuilder.Append(PointToString(box.X.Max, box.Y.Min, box.Z.Min)); //point21 1
            stringBuilder.Append(PointToString(box.X.Max, box.Y.Max, box.Z.Min)); //point31 2
            stringBuilder.Append(PointToString(box.X.Min, box.Y.Max, box.Z.Min)); //point41 3

            stringBuilder.Append(PointToString(box.X.Min, box.Y.Min, box.Z.Max)); //point12 4
            stringBuilder.Append(PointToString(box.X.Max, box.Y.Min, box.Z.Max)); //point22 5
            stringBuilder.Append(PointToString(box.X.Max, box.Y.Max, box.Z.Max)); //point32 6
            stringBuilder.Append(PointToString(box.X.Min, box.Y.Max, box.Z.Max)); //point42 7

            return stringBuilder.ToString();
        }

        private string PointToString(double x, double y, double z)
        {
            return x.ToString("F4", CultureInfo.InvariantCulture) + " "
                   + y.ToString("F4", CultureInfo.InvariantCulture) + " "
                   + z.ToString("F4", CultureInfo.InvariantCulture) + " ";
        }

        private string ExportBox(Tuple<Box, Vector<double>> transBox, string prefix)
        {
            Box box = transBox.Item1;
            Vector<double> trans = transBox.Item2;
            string groupTemplate = GetString(TemplatesName + "X3dLocalGroupTemplate.txt");
            return string.Format(groupTemplate, prefix + "",
                string.Format("{0:f} {1:f} {2:f}", trans[0], trans[1], trans[2]),
                BoxIndexFaceSetIndices, BoxToIndexedFaceSet(box));
        }

        private string GetString(string file)
        {
            return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(file)).ReadToEnd();
        }
    }
}