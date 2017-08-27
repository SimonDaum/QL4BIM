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
