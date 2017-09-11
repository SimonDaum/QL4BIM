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
using System.Globalization;
using System.Linq;
using Microsoft.Practices.Unity.Utility;
using QL4BIMindexing;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    internal class SpatialMain : ISpatialMain
    {
        private readonly ISpatialRepository spatialRepository;
        private readonly IOverlapOperator overlapOperator;
        private readonly IDistanceOperator distanceOperator;
        private readonly IDirectionalOperators directionalOperators;
        private readonly ITouchOperator touchOperator;
        private readonly IContainOperator containOperator;
        private readonly IEqualOperator equalOperator;
        private readonly IVectorDirOperator vectorDirOperator;
        private readonly ISettings settings;
        private readonly IX3DExporter x3DExporter;
        private int currentTriangleCount;

        public SpatialMain(ISpatialRepository spatialRepository, IX3DExporter x3DExporter, 
            IOverlapOperator overlapOperator, IDistanceOperator distanceOperator,
            IDirectionalOperators directionalOperators, ITouchOperator touchOperator,
            IContainOperator containOperator, IEqualOperator equalOperator,
            IVectorDirOperator vectorDirOperator, ISettings settings)
        {
            this.spatialRepository = spatialRepository;
            this.x3DExporter = x3DExporter;
            this.overlapOperator = overlapOperator;
            this.distanceOperator = distanceOperator;
            this.directionalOperators = directionalOperators;
            this.touchOperator = touchOperator;
            this.containOperator = containOperator;
            this.equalOperator = equalOperator;
            this.vectorDirOperator = vectorDirOperator;
            this.settings = settings;
        }

        public int CurrentTriangleCount => currentTriangleCount;

        public void ImportAndReset(IEnumerable<IndexedFaceSet> faceSets)
        {
            var faceSetsArray = faceSets.ToArray();
            if (settings.Direction.SupportAnyDirection)
                foreach (var indexedFaceSet in faceSetsArray)
                    vectorDirOperator.AddIndexedFaceSet(indexedFaceSet);

            spatialRepository.TriangleMeshes = faceSetsArray.Select(f => f.CreateMesh()).ToList();

            currentTriangleCount = 0;
            foreach (var triangleMesh in spatialRepository.TriangleMeshes)
            {
                var rtree = new RStarTree<Triangle>();
                rtree.Add(triangleMesh.Triangles);

                currentTriangleCount += triangleMesh.Triangles.Count;

                triangleMesh.RTreeRoot = rtree;     
            }
        }

        public void ImportAndReset(IEnumerable<TriangleMesh> meshes)
        {
            spatialRepository.TriangleMeshes = meshes.ToArray();
            spatialRepository.TriangleMeshById = spatialRepository.TriangleMeshes.ToDictionary(m => m.Name);

            currentTriangleCount = 0;
            foreach (var triangleMesh in spatialRepository.TriangleMeshes)      
                currentTriangleCount += triangleMesh.Triangles.Count;
        }

        public ISettings GetSettings()
        {
            return settings;
        }

        public void Overlap()
        {
            var result = overlapOperator.Overlap(spatialRepository.MeshesAsTriangleMatrix);
            PrintOut(result, "Overlap");
        }

        public void Contain()
        {
            var result = containOperator.Contain(spatialRepository.MeshesAsTriangleMatrix);
            PrintOut(result, "Contain");
        }
        
        public void Equal()
        {
            var result = equalOperator.EqualRTree(spatialRepository.MeshesAsMatrixList);
            PrintOut(result, "Equal", false);
        }

        public int GetTriangleCount()
        {
            return currentTriangleCount;
        }

        public void Distance()
        {
            var result = distanceOperator.Distance(spatialRepository.MeshesAsTriangleMatrix);
            PrintOut(result, "Distance", true);
        }

        public void ArbitraryDirection()
        {
           var cubeX = spatialRepository.MeshByGlobalId("3hXGhYvd59SukMqHINmzDS");
           var others = spatialRepository.TriangleMeshes.ToList();
           var removed = others.Remove(cubeX);

           var result = new List<Pair<TriangleMesh,TriangleMesh>>();
            foreach (var meshB in others)
            {   
                if(vectorDirOperator.Intersects(cubeX,meshB, "d1"))
                    result.Add(new Pair<TriangleMesh, TriangleMesh>(cubeX, meshB));
            }

            PrintOut(result, "ArbitraryDirectionD1");
        }

        public void AboveOfRelaxed()
        {
            var result = directionalOperators.AboveOfRelaxed(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "AboveOfRelaxed");
        }

        public void AboveOfStrict()
        {
            var result = directionalOperators.AboveOfStrict(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "AboveOfStrict");
        }

        public void BelowOfRelaxed()
        {
            var result = directionalOperators.BelowOfRelaxed(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "BelowOfRelaxed");
        }

        public void BelowOfStrict()
        {
            var result = directionalOperators.BelowOfStrict(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "BelowOfStrict");
        }

        public void EastOfRelaxed()
        {
            var result = directionalOperators.EastOfRelaxed(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "EastOfRelaxed");
        }

        public void EastOfStrict()
        {
            var result = directionalOperators.EastOfStrict(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "EastOfStrict");
        }

        public void WestofRelaxed()
        {
            var result = directionalOperators.WestOfRelaxed(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "WestOfRelaxed");
        }

        public void WestofStrict()
        {
            var result = directionalOperators.WestOfStrict(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "WestOfStrict");
        }

        public void NorthOfRelaxed()
        {
            var result = directionalOperators.NorthOfRelaxed(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "NorthOfRelaxed");
        }

        public void NorthOfStrict()
        {
            var result = directionalOperators.NorthOfStrict(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "NorthOfStrict");
        }

        public void SouthOfRelaxed()
        {
            var result = directionalOperators.SouthOfRelaxed(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "SouthOfRelaxed");
        }

        public void SouthOfStrict()
        {
            var result = directionalOperators.SouthOfStrict(spatialRepository.MeshesAsMatrix);
            PrintOut(result, "SouthOfStrict");
        }

        public void Export()
        {
            var ds = spatialRepository.MeshByGlobalId("3hXGhYvd59SukMqHINmzDS");
            var dsOuter = overlapOperator.OuterMesh(ds, 0.1);
            var dsinner = overlapOperator.OuterMesh(ds, -0.1);
		
            x3DExporter.ExportMeshes(@"OutNorm.x3d", new List<TriangleMesh>() { ds });
            x3DExporter.ExportMeshes(@"OutOut.x3d", new List<TriangleMesh>() { dsOuter });
            x3DExporter.ExportMeshes(@"OutInner.x3d", new List<TriangleMesh>() { dsinner });

            var meshes = vectorDirOperator.GetTransMesh("d1");
            x3DExporter.ExportMeshes(@"d1.x3d", meshes);

            meshes = spatialRepository.TriangleMeshes;
            x3DExporter.ExportMeshes(@"dini.x3d", meshes);
        }

        public void ExportTrees()
        {
            foreach (var triMesh in spatialRepository.TriangleMeshes)
            {
                var allboxes = triMesh.RTreeRoot.GetAllBoxes();
                x3DExporter.ExportBoxes(System.IO.Path.Combine("C:\\Temp\\", triMesh.Name), "Out", allboxes);
            }

        }

        public void Touch()
        {
            var result = touchOperator.Touch(spatialRepository.MeshesAsTriangleMatrix);
            PrintOut(result, "Touch");
        }


        private void PrintOut(IEnumerable<Pair<TriangleMesh, TriangleMesh>> finalItemPairs, string name)
        {
            foreach (var finalItemPair in finalItemPairs)
                Console.WriteLine(name + ": " + finalItemPair.First + ", " + finalItemPair.Second);
        }

        private void PrintOut(IEnumerable<Tuple<TriangleMesh, TriangleMesh, double>> finalItemPairs, string name, bool skipOverThreshold)
        {
            foreach (var finalItemPair in finalItemPairs)
            {
                if (skipOverThreshold && finalItemPair.Item3 >= settings.Distance.GlobalThreshold)
                    continue;

                Console.WriteLine(name + ": " + finalItemPair.Item1 + ", " + finalItemPair.Item2 + ": " + finalItemPair.Item3.ToString("000.00000000000000000"));
            }
        }

        private string DistancePrintOut(double distance)
        {
            string distanceAsString = distance.ToString(CultureInfo.CurrentCulture);
            if (distance >= settings.Distance.GlobalThreshold)
                distanceAsString = ">" + distanceAsString;
            return distanceAsString;
        }
    }
}
