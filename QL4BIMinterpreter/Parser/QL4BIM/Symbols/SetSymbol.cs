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
    public class SetSymbol : Symbol
    {
        private Dictionary<int, QLEntity> entityDic;
        private List<QLEntity> entites;

        public QLEntity EntityAt(int index) => entites[index];

        public QLEntity this[int index] => entityDic[index];

        public QLEntity this[int index, bool fake] => entityDic.ContainsKey(index) ? entityDic[index] : null;

        public SetSymbol(SetNode node) : base(node)
        {
            Header = "Set->";
            entites = new List<QLEntity>();
            entityDic = new Dictionary<int, QLEntity>();
        }

        public Dictionary<int, QLEntity> EntityDic
        {
            get { return entityDic; }
            set
            {
                entityDic = value;
                entites = value.Values.ToList();
            }
        }

        public override IEnumerable<QLEntity[]> Tuples
        {
            get { return entityDic.Values.Select(e => new []{e}); }
        }

        public QLEntity[] Entites => entites.ToArray();

        public override List<string> Attributes => new List<string>() { Value };

        public override bool IsEmpty => entites.Count == 0;
        public override void Reset()
        {
            entityDic.Clear();
        }

        public int EntityCount => entityDic.Count;
    }


}
