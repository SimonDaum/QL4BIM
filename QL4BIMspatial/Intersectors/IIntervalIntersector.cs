namespace QL4BIMspatial
{
    public interface IIntervalIntersector
    {
        bool TestStrict(Interval first, Interval second);
        bool Test(Interval first, Interval second);
    }
}