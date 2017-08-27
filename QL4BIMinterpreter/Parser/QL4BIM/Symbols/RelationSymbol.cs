/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMinterpreter.

QL4BIMinterpreter is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMinterpreter is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMinterpreter. If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMinterpreter.P21;

namespace QL4BIMinterpreter.QL4BIM
{
    public class RelationSymbol : Symbol
    {
        private  List<String> attributes;
        private  int attributeCount;
        private  List<QLEntity[]> tuples;

        public bool HasEmptyAtts { get; private set; }

        public RelationSymbol(RelationNode node) : base(node)
        {
            if (node.Attributes == null)
            {
                attributeCount = -1;
                HasEmptyAtts = true;
            }
            else
            {
                attributeCount = node.Attributes.Count;
                attributes = node.Attributes;            
            }

            tuples = new List<QLEntity[]>();
            Header = "Relation->";
        }

        public void AddTuple(QLEntity[] tuple)
        {
            HandleAttributeCount(tuple);
            tuples.Add(tuple);
        }

        private void HandleAttributeCount(QLEntity[] tuple)
        {
            if (!HasEmptyAtts && tuple.Length != attributeCount)
                throw new ArgumentException();

            if (attributeCount == -1 && HasEmptyAtts)
            {
                attributeCount = tuple.Length;
                attributes = new List<string>();
                for (int i = 0; i < attributeCount; i++)
                    attributes.Add((i + 1).ToString());
            }
        }

        public void SetTuples(IEnumerable<QLEntity[]> entityTuples)
        {
            var tupleList = entityTuples.ToList();
            var tuple = tupleList.FirstOrDefault(e => e != null);
            if (tuple == null)
            {
                tuples = new List<QLEntity[]>();
                return;
            }

            HandleAttributeCount(tuple);
            tuples = tupleList;
        }

        public sealed override IEnumerable<QLEntity[]> Tuples => tuples.ToArray();
        public override List<string> Attributes => attributes;

        public override bool IsEmpty => tuples.Count == 0;
        public int? Index { get; set; }

        public override void Reset()
        {
            tuples.Clear();
        }

        public int EntityCount => tuples.Count;
    }
}
