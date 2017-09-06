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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace QL4BIMprimitives
{
    public class Polygon : IEnumerable<PolygonPoint>
    {
        private List<PolygonPoint> points = new List<PolygonPoint>();

        public int PointCount => points.Count;

        public void AddPoint(PolygonPoint point)
        {
            points.Add(point);
        }

        public PolygonPoint this[int index] => points[index];

        IEnumerator<PolygonPoint> IEnumerable<PolygonPoint>.GetEnumerator()
        {
            return points.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return points.GetEnumerator();
        }

        public void AddIntersectionPoints(Dictionary<PolygonPoint, List<PolygonPoint>> dictIntersectionPoints)
        {
            var allPoints = new List<PolygonPoint>();
            foreach (var polygonPoint in points)
            {
                allPoints.Add(polygonPoint);

                if (!dictIntersectionPoints.ContainsKey(polygonPoint))
                    continue;

                var sortedIntersections = dictIntersectionPoints[polygonPoint].OrderBy(p => p.Alpha);
                allPoints.AddRange(sortedIntersections);
            }

            points = allPoints;
        }

        public override string ToString()
        {
            return String.Join(" ", points);
        }
    }

    public class PolygonPoint
    {
        private DenseVector vector;

        public bool IsIntersection { get; set; }

        public bool IsEntry { get; set; }

        public PolygonPoint Neighbor { get; set; }

        public double? Alpha { get; set; }

        public DenseVector Vector { get; }

        public PolygonPoint(double x, double y)
        {
            this.Vector = new []{x, y};
            IsIntersection = false;
        }

        public PolygonPoint(DenseVector vector, bool isIntersection, double? alpha = null)
        {
            IsIntersection = isIntersection;
            this.Alpha = alpha;
            this.vector = vector;
        }

        public override string ToString()
        {
            return Vector[0].ToString("F") + " " + Vector[1].ToString("F") + " 0.0";
        }
    }
}
