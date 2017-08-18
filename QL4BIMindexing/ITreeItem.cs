using System.Collections.Generic;
using QL4BIMprimitives;

namespace QL4BIMindexing
{
    public interface ITreeItem : IEnumerable<ITreeItem>, IHasBounds
    {
        bool CanSubdivide { get; }

        int ID { get; }

        int Level { get; }

        Interval MinMaxDistanceInterval { get; set; }

    }
}
