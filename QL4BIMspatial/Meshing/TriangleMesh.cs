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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using QL4BIMindexing;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public class TriangleMesh : IEnumerable<Triangle>, IHasBounds
    {
        private readonly Vector<double>[] offsetVectors;
        private readonly List<Triangle> triangles;

        public int SampleCount { get; set; }

        public TriangleMesh(IEnumerable<Triangle> tris, string name, bool createRTree,  Vector<double>[] offsetVectors = null)
        {
            this.offsetVectors = offsetVectors;
            Name = name;
            triangles = new List<Triangle>();
            triangles.AddRange(tris);

            Bounds = Box.Union(triangles.Select(tri => tri.Bounds));

            if (!createRTree)
                return;
           CreateRTree();
        }

        public void CreateRTree()
        {
            RTreeRoot = new RStarTree<Triangle>();
            RTreeRoot.Add(triangles);
        }

        public Triangle CreateOuterTriangle(Triangle triangle, double offSet)
        {
            var offsetA = offsetVectors[triangle.AIndex];
            var offsetB = offsetVectors[triangle.BIndex];
            var offsetC = offsetVectors[triangle.CIndex];

            var newPointA = new Point(triangle.A.Vector + offsetA * offSet as DenseVector);
            var newPointB = new Point(triangle.B.Vector + offsetB * offSet as DenseVector);
            var newPointC = new Point(triangle.C.Vector + offsetC * offSet as DenseVector);

            return new Triangle(newPointA, newPointB, newPointC);
        }




        public string Name { get;  set; }

        public List<Triangle> Triangles
        {
            get { return triangles; }
        }

        public int Count
        {
            get { return triangles.Count; }
        }

        public IEnumerator<Triangle> GetEnumerator()
        {
            return Triangles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Box Bounds { get; private set; }

        public RTree<Triangle> RTreeRoot { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
