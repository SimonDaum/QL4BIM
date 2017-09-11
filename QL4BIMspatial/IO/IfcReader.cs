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
using System.Linq;
using QL4BIMindexing;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    class IfcReader : IIfcReader
    {
        private readonly ISpatialRepository spatialRepository;
        private readonly IVectorDirOperator vectorDirOperator;
        private readonly ISettings settings;
        private System.Diagnostics.Stopwatch stopwatch;

        public IfcReader(ISpatialRepository spatialRepository, IVectorDirOperator vectorDirOperator, ISettings settings)
        {
            this.spatialRepository = spatialRepository;
            this.vectorDirOperator = vectorDirOperator;
            this.settings = settings;
            stopwatch = new System.Diagnostics.Stopwatch();
        }

        public long[] LoadIfc(string path)
        {
            var ifcImporter = new IfcImporter.MainInterface();

            stopwatch.Start();
            var engineFaceSets = ifcImporter.OpenIfcFile(path);
            stopwatch.Stop();
            var ifcEngineTiming = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();


            Console.WriteLine("Total Memory: {0}", GC.GetTotalMemory(true) / (1024d * 1024));
            stopwatch.Start();

            var faceSets = engineFaceSets.Select(f => new IndexedFaceSet(f.Item1, f.Item2, f.Item3)).ToArray();

            Console.WriteLine("Total Memory after Faceseting: {0}", GC.GetTotalMemory(true) / (1024d * 1024));

            var meshes = faceSets.Select(f =>
            {
                var m = f.CreateMesh(null,false);
                if(settings.Direction.SupportAnyDirection)
                    vectorDirOperator.AddIndexedFaceSet(f);
                return m;
            }).ToList();

            Console.WriteLine("Total Memory after Meshing: {0}", GC.GetTotalMemory(true) / (1024d * 1024));

            foreach (var mesh in meshes.Where( m => m != null))
                mesh.CreateRTree();

            Console.WriteLine("Total Memory after RTreeing: {0}", GC.GetTotalMemory(true) / (1024d * 1024));


            stopwatch.Stop();
            var meshingTiming = stopwatch.ElapsedMilliseconds;

            AddImport(meshes);

            return new []{ifcEngineTiming, meshingTiming, meshes.Count};
        }

        public void AddImport(List<TriangleMesh> meshes)
        {
            spatialRepository.AddMeshes(meshes);
            var currentTriangleCount = 0;

            foreach (var triangleMesh in spatialRepository.TriangleMeshes)
            {
                var rtree = new RStarTree<Triangle>();
                rtree.Add(triangleMesh.Triangles);

                currentTriangleCount += triangleMesh.Triangles.Count;
                triangleMesh.RTreeRoot = rtree;
            }

            Console.WriteLine("Triangles imported:" + currentTriangleCount);
        }

    }


}
