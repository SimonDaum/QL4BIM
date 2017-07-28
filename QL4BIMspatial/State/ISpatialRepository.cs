using System.Collections.Generic;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    public interface ISpatialRepository
    {
        IEnumerable<TriangleMesh> TriangleMeshes { get; set; }
        Dictionary<string, TriangleMesh> TriangleMeshById { get; set; }

        IEnumerable<Pair<TriangleMesh, TriangleMesh>> MeshesAsTriangleMatrix { get; }

        IEnumerable<Pair<TriangleMesh, TriangleMesh>> MeshesAsMatrix { get; }

        IEnumerable<Pair<TriangleMesh, List<TriangleMesh>>> MeshesAsMatrixList { get; }
        List<TriangleMesh> TriangleMeshesSet1 { get; }
        List<TriangleMesh> TriangleMeshesSet2 { get; }
        List<string> SpatialOperators { get; set; }
        TriangleMesh MeshByGlobalId(string globalId);
        void AddMeshes(List<TriangleMesh> meshes);
        void RemoveMeshByGlobalId(string globalId);
        void Reset();
    }
}