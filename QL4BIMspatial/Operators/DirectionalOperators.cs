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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Practices.Unity.Utility;
using QL4BIMprimitives;


namespace QL4BIMspatial
{
    public class DirectionalOperators : IDirectionalOperators
    {
        private readonly IPrismTriangleIntersector prismTriangleIntersection;

        private readonly IPointSampler pointSampler;
        private readonly IRayTriangleIntersector rayTriangleIntersector;
        private readonly ISettings settings;


        public DirectionalOperators(IPrismTriangleIntersector prismTriangleIntersection, IPointSampler pointSampler, 
            IRayTriangleIntersector rayTriangleIntersector, ISettings settings )
        {
            this.prismTriangleIntersection = prismTriangleIntersection;
            this.pointSampler = pointSampler;
            this.rayTriangleIntersector = rayTriangleIntersector;
            this.settings = settings;
        }

        public List<Pair<TriangleMesh, TriangleMesh>> AboveOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.Z, true, true);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> AboveOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {   
            return DirectionTest(finalItemPairs, Axis.Z, true, false);
        }

        public bool AboveOfRelaxed(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionRelaxed(meshA, meshB, Axis.Z, true);
        }

        public bool AboveOfStrict(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionStrict(meshA, meshB, Axis.Z, true);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> BelowOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.Z, false, true);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> BelowOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.Z, false, false);
        }

        public bool BelowOfRelaxed(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionRelaxed(meshA, meshB, Axis.Z, false);
        }

        public bool BelowOfStrict(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionStrict(meshA, meshB, Axis.Z, false);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> EastOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.Y, true, true);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> EastOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.Y, true, false);
        }

        public bool EastOfRelaxed(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionRelaxed(meshA, meshB, Axis.Y, true);
        }

        public bool EastOfStrict(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionStrict(meshA, meshB, Axis.Y, true);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> WestOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.Y, false, true);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> WestOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.Y, false, false);
        }

        public bool WestOfRelaxed(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionRelaxed(meshA, meshB, Axis.Y, false);
        }

        public bool WestOfStrict(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionStrict(meshA, meshB, Axis.Y, false);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> NorthOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.X, true, true);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> NorthOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.X, true, false);
        }

        public bool NorthOfRelaxed(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionRelaxed(meshA, meshB, Axis.X, true);
        }

        public bool NorthOfStrict(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionStrict(meshA, meshB, Axis.X, true);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> SouthOfRelaxed(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.X, false, true);
        }

        public List<Pair<TriangleMesh, TriangleMesh>> SouthOfStrict(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs)
        {
            return DirectionTest(finalItemPairs, Axis.X, false, false);
        }

        public bool SouthOfRelaxed(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionRelaxed(meshA, meshB, Axis.X, false);
        }

        public bool SouthOfStrict(TriangleMesh meshA, TriangleMesh meshB)
        {
            return DirectionStrict(meshA, meshB, Axis.X, false);
        }

        private List<Pair<TriangleMesh, TriangleMesh>> DirectionTest(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs,
                                                        Axis axis, bool isPositiveDirection, bool isRelaxed)
        {
            var outList = new List<Pair<TriangleMesh, TriangleMesh>>();
            foreach (var finalItemPair in finalItemPairs)
            {
                var meshA = finalItemPair.First;
                var meshB = finalItemPair.Second;
                if (meshA.Name == meshB.Name)
                    continue;

                if (isRelaxed)
                {
                    if(DirectionRelaxed(meshA, meshB, axis, isPositiveDirection))
                        outList.Add(new Pair<TriangleMesh, TriangleMesh>(meshA, meshB));
                }
                else
                {   
                    //reversed relaxed test
                    //if (DirectionRelaxed(meshA, meshB, axis, !isPositiveDirection))
                    //    continue;

                    if (DirectionStrict(meshA, meshB, axis, isPositiveDirection))
                        outList.Add(new Pair<TriangleMesh, TriangleMesh>(meshA, meshB));
                }                
            }

            return outList;
        }


        private bool DirectionRelaxed(TriangleMesh meshA, TriangleMesh meshB , Axis axis, bool positiveDirection)
        {

            var boxesEx = new List<Box>();

            var box1 = meshA.Bounds;
            var box2 = meshB.Bounds;

            var isAboveRelaxed = box1.IntersectsColumm(box2, axis, positiveDirection);
            if (!isAboveRelaxed)
                return false;



            var transLength = Box.Union(box1, box2).GetInterval(axis).Length;

            foreach (var triangle1 in meshA.Triangles)
            {
                var triangle1Inner = meshA.CreateOuterTriangle(triangle1, settings.Direction.PositiveOffset);

                var tree1BoxExtended = triangle1Inner.Bounds.ExtendInDirection(box2, axis, positiveDirection);

                boxesEx.Add(tree1BoxExtended);

                var tree2TrianglesCandidates = meshB.RTreeRoot.FindOverlap(tree1BoxExtended).ToList();
                   
                if (tree2TrianglesCandidates.Count == 0)
                    continue;

                var prism = new Prism(triangle1Inner, transLength, axis, positiveDirection);
                foreach (var triangle2 in tree2TrianglesCandidates)
                {   

                    if (prismTriangleIntersection.Test(prism, triangle2))
                        return true;
                }
            }

            return false;
        }

        private bool DirectionStrict(TriangleMesh meshA , TriangleMesh meshB, Axis axis, bool positiveDirection)
        {
            var opositeDirection = !positiveDirection;

            var box1 = meshA.Bounds;
            var box2 = meshB.Bounds;

            var intersectsColumm = box2.IntersectsColumm(box1, axis, opositeDirection);
            if (!intersectsColumm)
                return false;

            var transLength = Box.Union(box1, box2).GetInterval(axis).Length;

            foreach (var triangle2 in meshB.Triangles)
            {

                var tree2BoxExtended = triangle2.Bounds.ExtendInDirection(box1, axis, opositeDirection);
                var tree1TrianglesCandidates = meshA.RTreeRoot.FindOverlap(tree2BoxExtended).ToList();

                if (tree1TrianglesCandidates.Count == 0)
                    return false;

                var prism = new Prism(triangle2, transLength, axis, opositeDirection);
                var intersetingTree1Tris = tree1TrianglesCandidates.Where(t1 => prismTriangleIntersection.Test(prism, t1)).ToList();

                if (intersetingTree1Tris.Count == 0)
                    return false;

                pointSampler.DistributePoints(triangle2, settings.Direction.RaysPerSquareMeter);
                var allRaysOk = EmitRays(triangle2, intersetingTree1Tris,meshA, axis, opositeDirection);

                if (!allRaysOk)
                    return false;            
            }

            return true;
        }


        private bool EmitRays(Triangle triangle2, List<Triangle> candidateTriangles1, TriangleMesh mesh1, Axis axis, bool positiveDirection)
        {
            var positiveOffset = settings.Direction.PositiveOffset;

            foreach (var samplePoint in triangle2.SamplingPoints)
            {
                var direction = positiveDirection ? AxisDirection.Positive : AxisDirection.Negative;
                var ray = new Ray(samplePoint, axis, direction);

                var doesIntersect = false;
                foreach (var candidateTriangle in candidateTriangles1)
                {
                    var outerTri = mesh1.CreateOuterTriangle(candidateTriangle, positiveOffset);
                    doesIntersect = rayTriangleIntersector.TestStrict(ray, outerTri);
                    if(doesIntersect)
                        break;
                }

                if (!doesIntersect)
                    return false;
                
            }
            return true;
        }
    }
}
