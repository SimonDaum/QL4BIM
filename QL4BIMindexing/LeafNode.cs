using System.Collections.Generic;
using QL4BIMprimitives;

namespace QL4BIMindexing
{
    public partial class RTree<T>
    {

        public class LeafNode : ITreeItem
        {
            private int id;
            private Box bounds;

            public LeafNode(Box bounds, int id)
            {
                this.bounds = bounds;
                this.id = id;
            }

            bool ITreeItem.CanSubdivide
            {
                get { return false; }
            }

            int ITreeItem.ID
            {
                get { return id; }
            }

            int ITreeItem.Level
            {
                get { return 0; }
            }

            public Interval MinMaxDistanceInterval { get; set; }

            Box IHasBounds.Bounds
            {
                get { return bounds; }
            }

            IEnumerator<ITreeItem> IEnumerable<ITreeItem>.GetEnumerator()
            {
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return ((ITreeItem)this).GetEnumerator();
            }
        }
    }
}
