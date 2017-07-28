namespace QL4BIMspatial
{
    public interface IInsideTester
    {
        bool BIsInside(TriangleMesh meshA, TriangleMesh meshB);
    }
}