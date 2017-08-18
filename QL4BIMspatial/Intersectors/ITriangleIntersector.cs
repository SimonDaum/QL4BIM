using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public interface ITriangleIntersector
    {
        bool DoIntersect(Triangle triangleA, Triangle triangleB);

        bool EdgeAgainsEdge(PolygonPoint V0, PolygonPoint V1, PolygonPoint U0, PolygonPoint U1, int i0, int i1,
            out PolygonPoint secPointA, out PolygonPoint secPointB);

        bool EdgeAgainsEdge(DenseVector V0, DenseVector V1, DenseVector U0, DenseVector U1, int i0, int i1, out DenseVector p, out double alpha, out double beta);
    }
}