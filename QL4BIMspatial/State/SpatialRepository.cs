using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Practices.Unity.Utility;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public class SpatialRepository : ISpatialRepository
    {
        public SpatialRepository()
        {
            TriangleMeshes = new List<TriangleMesh>();
            TriangleMeshById = new Dictionary<string, TriangleMesh>();
            SpatialOperators = new List<string>();
        }

        public List<string> SpatialOperators { get; set; }

        public IEnumerable<TriangleMesh> TriangleMeshes { get; set; }


        public void AddMeshes(List<TriangleMesh> meshes)
        {   
            var temp = new List<TriangleMesh>(TriangleMeshes);
            temp.AddRange(meshes);
            TriangleMeshes = temp;

            try
            {
                foreach (var mesh in meshes)
                    TriangleMeshById.Add(mesh.Name, mesh);
            }
            catch (Exception)
            {
                throw new QueryException("Mesh already stored. Delete symbols.");
            }

        }

        public void RemoveMeshByGlobalId(string globalId)
        {
            if (TriangleMeshById.ContainsKey(globalId))
                TriangleMeshById.Remove(globalId);
        }

        public void Reset()
        {
            TriangleMeshById.Clear();
            TriangleMeshes = new List<TriangleMesh>();
        }

        public Dictionary<string, TriangleMesh> TriangleMeshById { get; set; }

        public List<TriangleMesh> TriangleMeshesSet1
        {
            get
            {
                return TriangleMeshes.Where(m => !m.Name.Contains("_")).ToList();
            }
        }

        public List<TriangleMesh> TriangleMeshesSet2
        {
            get
            {
                return TriangleMeshes.Where(m => m.Name.Contains("_")).ToList();
            }
        }

        public TriangleMesh MeshByGlobalId(string globalId)
        {
            if (!TriangleMeshById.ContainsKey(globalId))
                return null;
            return TriangleMeshById[globalId];
        }


        public IEnumerable<Pair<TriangleMesh, List<TriangleMesh>>> MeshesAsMatrixList
        {
            get
            {
                var originalMeshes = TriangleMeshesSet1;
                var otherMeshes = TriangleMeshesSet2;

                var originalCount = originalMeshes.Count;
                var otherCount = otherMeshes.Count;

                var matrixList = new List<Pair<TriangleMesh, List<TriangleMesh>>>();
                for (int i = 0; i < originalCount; i++)
                {
                    var list = new List<TriangleMesh>();
                    for (int j = 0; j < otherCount; j++)
                            list.Add(otherMeshes[j]);

                    matrixList.Add(new Pair<TriangleMesh, List<TriangleMesh>>(originalMeshes[i], list));
                }

                return matrixList;
            }

        }

        public IEnumerable<Pair<TriangleMesh, List<TriangleMesh>>> MeshesAsMatrixListOld
        {
            get
            {
                var meshesArrayed = TriangleMeshes.ToArray();
                var count = meshesArrayed.Length;

                var matrixList = new List<Pair<TriangleMesh, List<TriangleMesh>>>();
                for (int i = 0; i < count; i++)
                {
                    var list = new List<TriangleMesh>();
                    for (int j = 0; j < count; j++)
                        if (i != j)
                            list.Add(meshesArrayed[j]);

                    matrixList.Add(new Pair<TriangleMesh, List<TriangleMesh>>(meshesArrayed[i], list));
                }

                return matrixList;
            }

        }

        public IEnumerable<Pair<TriangleMesh, TriangleMesh>> MeshesAsTriangleMatrix
        {
            get
            {
                var meshesArrayed = TriangleMeshes.ToArray();
                var count = meshesArrayed.Length;

                var meshPairs = new List<Pair<TriangleMesh, TriangleMesh>>();
                for (int i = 1; i < count; i++)
                {
                    for (int j = 0; j < i; j++)
                        meshPairs.Add(new Pair<TriangleMesh, TriangleMesh>(meshesArrayed[i], meshesArrayed[j]));
                }

                return meshPairs;
            }
            
        }

        public IEnumerable<Pair<TriangleMesh, TriangleMesh>> MeshesAsMatrix
        {
            get
            {
                var meshesArrayed = TriangleMeshes.ToArray();
                var count = meshesArrayed.Length;

                var meshPairs = new List<Pair<TriangleMesh, TriangleMesh>>();

                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < count; j++)
                        if(i!=j)
                            meshPairs.Add(new Pair<TriangleMesh, TriangleMesh>(meshesArrayed[i], meshesArrayed[j]));
                }

                return meshPairs;
            }
            
        }
    }
}
