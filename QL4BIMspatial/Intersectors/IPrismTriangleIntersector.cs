using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public interface IPrismTriangleIntersector
    {
        bool Test(Prism prism, Triangle tri);
    }
}