using System.Collections.Generic;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public interface IPolygonMerger
    {
        void Reset();
        void AddPolysFromTris(IEnumerable<Triangle> triangles);
        void AddPoly(Triangle triangle);
        List<Polygon> Polylist { get; }
    }
}