using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public interface IRayTriangleIntersector
    {
        bool TestStrict(Ray ray, Triangle tri);
        bool Test(Ray ray, Triangle tri);
    }
}