namespace QL4BIMprimitives
{
    public interface IVolume : IHasBounds
    {
        double Volume { get; }

        double Surface { get; }
    }
}