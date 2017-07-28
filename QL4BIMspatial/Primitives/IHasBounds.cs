
namespace QL4BIMspatial
{
    public interface IHasBounds
    {
        /// <summary>
        ///     Gets the smallest axis aligned bounding box containing this object.
        /// </summary>
        Box Bounds { get; }

    }
}