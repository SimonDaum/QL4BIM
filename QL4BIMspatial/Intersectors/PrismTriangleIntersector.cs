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

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public class PrismTriangleIntersector : IPrismTriangleIntersector
    {
        private readonly IIntervalIntersector intervalIntersection;

        public PrismTriangleIntersector(IIntervalIntersector intervalIntersector)
        {
            this.intervalIntersection = intervalIntersector;
        }

        public bool Test(Prism prism, Triangle tri)
        {
            var triPoints = new Vector<double>[] { tri.A.Vector, tri.B.Vector, tri.C.Vector };
            IEnumerable<Vector<double>> axes;
            var prismPoints = PrismPoints(prism, tri, out axes);

            foreach (var axis in axes)
            {
                // Item1 is min, Item2 is max
                var prismProj = Interval.Union(prismPoints.Select(p => p * axis));
                var triProj = Interval.Union(triPoints.Select(p => p * axis));

                if (!intervalIntersection.Test(prismProj, triProj))
                    return false;
            }

            return true;
        }

        private static IEnumerable<Vector<double>> PrismPoints(Prism prism, Triangle tri, out IEnumerable<Vector<double>> axes)
        {
            var prismPoints =
                prism.Base.Vertices.Select(v => v.Vector).Concat(prism.Base.Vertices.Select(v => v.Vector + prism.Translation));

            // top and bottom face have the same normal, so only 1+n distinct normal vectors
            var prismNormals =
                new Vector<double>[] {prism.Base.Normal}.Concat(prism.Base.Edges.Select(e => e.CrossProduct(prism.Translation)));
            var triNormals = new Vector<double>[]
            {tri.Normal, tri.Normal.CrossProduct(tri.AB), tri.Normal.CrossProduct(tri.AC), tri.Normal.CrossProduct(tri.BC)};

            // top and bottom face are parallel and congruent, so they have the same edges
            var prismEdges = new Vector<double>[] {prism.Translation}.Concat(prism.Base.Edges);
            var triEdges = new Vector<double>[] {tri.AB, tri.AC, tri.BC};

            // cross products of edges are possible sep axes, too:
            var crossProducts = prismEdges.SelectMany(pe => triEdges.Select(te => pe.CrossProduct(te)));

            axes = prismNormals.Concat(triNormals).Concat(crossProducts);
            return prismPoints;
        }




    }
}
