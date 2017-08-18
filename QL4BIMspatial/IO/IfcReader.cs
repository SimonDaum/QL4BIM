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

            foreach (var mesh in meshes)
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
