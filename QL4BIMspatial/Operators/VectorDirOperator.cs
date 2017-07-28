using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Practices.Unity.Utility;

namespace QL4BIMspatial
{
    class VectorDirOperator : IVectorDirOperator
    {
        private readonly IDirectionalOperators directionalOperators;
        private readonly List<IndexedFaceSet> faceSets = new List<IndexedFaceSet>();
        private readonly Dictionary<string, Dictionary<string, TriangleMesh>> dirMeshes = new Dictionary<string, Dictionary<string, TriangleMesh>>();
        private readonly Dictionary<string, DenseVector> directions = new Dictionary<string, DenseVector>();

        public void AddIndexedFaceSet(IndexedFaceSet faceSet)
        {
            faceSets.Add(faceSet);
        }

        public void Reset()
        {
            faceSets.Clear();
            foreach (var dirMesh in dirMeshes)
                dirMesh.Value.Clear();

            dirMeshes.Clear();
            directions.Clear();
        }

        public IEnumerable<TriangleMesh> GetTransMesh(string direction)
        {
            return dirMeshes[direction].Values;
        }

        public void AddDirection(string name, DenseVector denseVector)
        {
            dirMeshes.Add(name, new Dictionary<string, TriangleMesh>());
            directions.Add(name, denseVector);
            TransformFaceSet(name, denseVector);
        }

        private void TransformFaceSet(string name, DenseVector denseVector)
        {
            foreach (var faceSet in faceSets)
            {
                var mesh = faceSet.CreateMesh(denseVector);
                dirMeshes[name].Add(mesh.Name, mesh);
            }
        }

        public VectorDirOperator(IDirectionalOperators directionalOperators)
        {
            this.directionalOperators = directionalOperators;
        }

        public bool Intersects(TriangleMesh meshA, TriangleMesh meshB, string dirName, bool strict = false)
        {
            if(!dirMeshes.ContainsKey(dirName))
                throw new InvalidParameterException(2);

            if(dirMeshes[dirName].Count == 0)
                TransformFaceSet(dirName, directions[dirName]);

            var meshATrans = dirMeshes[dirName][meshA.Name];
            var meshBTrans = dirMeshes[dirName][meshB.Name];

            return strict ? directionalOperators.AboveOfStrict(meshATrans, meshBTrans) : directionalOperators.AboveOfRelaxed(meshATrans, meshBTrans);
        }

        public IEnumerable<Pair<TriangleMesh, TriangleMesh>> Intersects(IEnumerable<Pair<TriangleMesh, TriangleMesh>> enumerable, string dirName, bool strict = false)
        {
            foreach (var pair in enumerable)
            {
                if (Intersects(pair.First, pair.Second, dirName))
                    yield return pair;
            }

        }
    }
}