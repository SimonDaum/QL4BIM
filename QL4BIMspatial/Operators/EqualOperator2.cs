using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using Microsoft.Practices.Unity.Utility;
using PairTriangleMesh = Microsoft.Practices.Unity.Utility.Pair<QL4BIMspatial.TriangleMesh, QL4BIMspatial.TriangleMesh>;

namespace QL4BIMspatial
{
    public class EqualOperator : IEqualOperator
    {
        private readonly IPointSampler pointSampler;
        private readonly IRepository repository;
        private readonly ISettings settings;

        public EqualOperator(IPointSampler pointSampler, IRepository repository, ISettings settings)
        {
            this.pointSampler = pointSampler;
            this.repository = repository;
            this.settings = settings;
        }


        public IEnumerable<Tuple<TriangleMesh, TriangleMesh, double>> EqualOctree(IEnumerable<Pair<TriangleMesh, List<TriangleMesh>>> enumerable)
        {
            var stopwatch1 = new Stopwatch();
            stopwatch1.Start("Octree Version");

            foreach (var mesh in repository.TriangleMeshes)
            {   
                pointSampler.ResetSessionPointCount();
                foreach (var triangle in mesh.Triangles)
                    pointSampler.DistributePoints(triangle, settings.Equal.SamplePerSquareMeter);

                mesh.SampleCount = pointSampler.SessionPointCount;

            }

            return null;
        }
    }
}
