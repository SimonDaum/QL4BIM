namespace QL4BIMspatial
{
    public interface IVolume : IHasBounds
    {
        double Volume { get; }

        double Surface { get; }
    }
}