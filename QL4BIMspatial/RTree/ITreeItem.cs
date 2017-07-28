using System.Collections.Generic;

namespace QL4BIMspatial
{
    public interface ITreeItem : IEnumerable<ITreeItem>, IHasBounds
    {
        bool CanSubdivide { get; }

        int ID { get; }

        int Level { get; }

        Interval MinMaxDistanceInterval { get; set; }

    }
}
