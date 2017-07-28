using System.Collections.Generic;

namespace QL4BIMspatial
{
    public interface IPointSampler
    {
        void DistributePoints(IEnumerable<Triangle> tris, int pointsPerSquareMeter);
        void DistributePoints(Triangle tri, int pointsPerSquareMeter);
        int SessionPointCount { get; }
        void ResetSessionPointCount();
    }
}