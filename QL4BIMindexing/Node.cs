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

//   Node.java
//   Java Spatial Index Library
//   Copyright (C) 2002 Infomatiq Limited
//  
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//  
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//  
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

// Ported to C# By Dror Gluska, April 9th, 2009


using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMprimitives;


namespace QL4BIMindexing
{
    //import com.infomatiq.jsi.Rectangle;

    /**
     * <p>Used by RTree. There are no public methods in this class.</p>
     * 
     * @author aled@sourceforge.net
     * @version 1.0b2p1
     */
    public partial class RTree<T>
    {
        public class Node : ITreeItem
        {
            private readonly int level;
            private readonly RTree<T> tree;
            private Box bounds = null;
            public ITreeItem[] Items = null;

            public int Count { get; set; }
            public int Level { get { return level; } }
            public Interval MinMaxDistanceInterval { get; set; }


            public Node(int nodeId, int level, RTree<T> tree)
            {
                ID = nodeId;
                this.level = level;
                this.tree = tree;
                Items = new ITreeItem[tree.MaxNodeEntries];
            }

            /// <summary>
            /// Adds the specified item to this node and updates the item's entry in parentMap.
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public void Add(ITreeItem item)
            {
                Items[Count] = item;
                tree.ParentMap[item.ID] = Tuple.Create(this, Count);
                Count++;

                if (bounds == null)
                {
                    bounds = item.Bounds.Copy();
                }
                else
                {
                    bounds.Add(item.Bounds);
                }
            }

            /// <summary>
            /// Adds all given items to this node and calculates the new bounds for this node by calculating the overall bounds for the new items.
            /// Note: this method does not prevent the node from overflowing
            /// </summary>
            /// <param name="newItems"></param>
            public void AddRange(IEnumerable<ITreeItem> newItems)
            {
                AddRange(newItems, null);
            }

            /// <summary>
            /// Adds all given items to this node and calculates the new bounds for this node assuming that the overall bounds for the new items are given.
            /// </summary>
            /// <param name="newItems"></param>
            /// <param name="newBounds">if null, the bounds for the new items are calculated explicitly</param>
            public void AddRange(IEnumerable<ITreeItem> newItems, Box newBounds)
            {
                int c = newItems.Select(e => e.Bounds).Count();
                newBounds = newBounds ?? Box.Union(newItems.Select(e => e.Bounds));
                if (bounds == null)
                {
                    bounds = newBounds;
                }
                else
                {
                    bounds.Add(newBounds);
                }

                foreach (var item in newItems)
                {
                    Items[Count] = item;
                    tree.ParentMap[item.ID] = Tuple.Create(this, Count);
                    Count++;
                }
            }

            /// <summary>
            /// Removes all items from this node.
            /// </summary>
            public void Clear()
            {
                bounds = null;
                Items = new ITreeItem[tree.MaxNodeEntries];
                Count = 0;
            }

            /// <summary>
            /// Removes the item at the specified entry and moves the last one in its slot.
            /// Note: this method does not ensure, that the minimum items count condition is still fulfilled after removing the item.
            /// </summary>
            /// <param name="index"></param>
            public void RemoveAt(int index)
            {
                Count--;
                tree.ParentMap.Remove(Items[index].ID);
                if (index < Count)
                {
                    Items[index] = Items[Count];
                    Items[Count] = null;
                    tree.ParentMap[Items[index].ID] = Tuple.Create(this, index);
                }
                else
                {
                    Items[index] = null;
                }
                bounds = Box.Union(Items.Take(Count) as IEnumerable<IHasBounds>);
            }

            public bool IsLeaf
            {
                get { return (Level == 1); }
            }

            public bool CanSubdivide
            {
                get { return true; }
            }

            public int ID { get; private set; }

            public Box Bounds
            {
                get { return bounds; }
                set { bounds = value; }
            }

            public IEnumerator<ITreeItem> GetEnumerator()
            {
                foreach (var item in Items.Take(Count))
                {
                    yield return item;
                }
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
