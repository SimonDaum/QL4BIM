using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMspatial
{
    class PolygonMerger : IPolygonMerger
    {
        private readonly ITriangleIntersector triangleIntersector;

        private List<Polygon> subjects;
        private List<Polygon> polylist;

        public PolygonMerger(ITriangleIntersector triangleIntersector)
        {
            this.triangleIntersector = triangleIntersector;
        }

        public IEnumerable<Polygon> Subject
        {
            get { return subjects; }
        }

        public List<Polygon> Polylist
        {
            get { return polylist; }
        }

        public void Reset()
        {
            subjects = new List<Polygon>();
            polylist = new List<Polygon>();
        }

        public void AddPolysFromTris(IEnumerable<Triangle> triangles)
        {
            foreach (var tri in triangles)
                AddPoly(tri);
        }

        public void AddPoly(Triangle triangle)
        {   

            var currentAddPoly = TriToPoly(triangle);

            polylist.Add(currentAddPoly);

            if (subjects.Count == 0)
            {
                subjects.Add(currentAddPoly);
                return;
            }

            //todo for many subjects
            ComputeIntersections(subjects[0], currentAddPoly);
        }

        private Polygon TriToPoly(Triangle triangle)
        {
            var poly = new Polygon();
            poly.AddPoint(new PolygonPoint(triangle.A.Vector[0], triangle.A.Vector[1]));
            poly.AddPoint(new PolygonPoint(triangle.B.Vector[0], triangle.B.Vector[1]));
            poly.AddPoint(new PolygonPoint(triangle.C.Vector[0], triangle.C.Vector[1]));

            return poly;
        }

        private void ComputeIntersections(Polygon polygonA, Polygon polygonB)
        {
            var polyCountA = polygonA.PointCount;
            var polyCountB = polygonB.PointCount;

            //todo close polygon

            var dictSecPointsA = new Dictionary<PolygonPoint, List<PolygonPoint>>();
            var dictSecPointsB = new Dictionary<PolygonPoint, List<PolygonPoint>>();

            for (int i = 0; i < polyCountA - 1; i++)
            {
                var pointA1 = polygonA[i];
                var pointA2 = polygonA[i+1];

                for (int j = 0; j < polyCountB - 1; j++)
                {
                    var pointB1 = polygonB[j];
                    var pointB2 = polygonB[j + 1];

                    PolygonPoint intersectionPointA;
                    PolygonPoint intersectionPointB;
                    var doesIntersect = triangleIntersector.EdgeAgainsEdge(pointA1, pointA2, pointB1, pointB2, 0, 1, out intersectionPointA, out intersectionPointB);

                    if(!doesIntersect)
                        continue;

                    AddIntersectionPoint(dictSecPointsA, pointA1, intersectionPointA);
                    AddIntersectionPoint(dictSecPointsB, pointB1, intersectionPointB);
                }
            }

            polygonA.AddIntersectionPoints(dictSecPointsA);
            polygonA.AddIntersectionPoints(dictSecPointsB);
        }

        private static void AddIntersectionPoint(Dictionary<PolygonPoint, List<PolygonPoint>> dictSecPointsA, PolygonPoint pointA1,
            PolygonPoint intersectionPointA)
        {
            if (dictSecPointsA.ContainsKey(pointA1))
                dictSecPointsA.Add(pointA1, new List<PolygonPoint>() {intersectionPointA});
            else
                dictSecPointsA[pointA1].Add(intersectionPointA);
        }
    }
}
