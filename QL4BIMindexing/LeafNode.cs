/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMindexing.

QL4BIMindexing is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMindexing is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMindexing. If not, see <http://www.gnu.org/licenses/>.
*/

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
