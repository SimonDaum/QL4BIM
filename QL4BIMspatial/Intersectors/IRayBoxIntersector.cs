using QL4BIMprimitives;

namespace QL4BIMspatial
{
    internal interface IRayBoxIntersector
    {
        bool Test(Ray ray, Box box);
    }
}