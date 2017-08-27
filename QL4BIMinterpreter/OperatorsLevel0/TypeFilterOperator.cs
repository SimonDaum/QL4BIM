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
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter.OperatorsLevel0
{
    public class TypeFilterOperator : ITypeFilterOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym
        
        private readonly IP21Reader p21Reader;

        public TypeFilterOperator(IP21Reader p21Reader)
        {
            this.p21Reader = p21Reader;
        }

        public void TypeFilterSet(SetSymbol parameterSym1, string typeName, SetSymbol returnSym)
        {
            Console.WriteLine("TypeFilter'ing...");
            var allTypes = p21Reader.GetAllSubtypNames(typeName);
            var typedEntites = parameterSym1.EntityDic.Values.Where(e => allTypes.Any(s =>
                    string.Compare(s, e.ClassName, StringComparison.InvariantCultureIgnoreCase) == 0));
            returnSym.EntityDic = typedEntites.ToDictionary(e => e.Id);
        }

        public void TypeFilterRelation(RelationSymbol parameterSym1, Tuple<int, string>[] indexAndTypeNames, RelationSymbol returnSym)
        {
            Console.WriteLine("TypeFilter'ing...");

            var tuples = parameterSym1.Tuples;
            var tuplesOut = GetTuples(tuples, indexAndTypeNames);

            returnSym.SetTuples(tuplesOut);
        }

        public IEnumerable<QLEntity[]> GetTuples(IEnumerable<QLEntity[]> tuples, Tuple<int, string>[] indexAndTypeNames)
        {
            var typeCount = indexAndTypeNames.Length;

            for (int i = 0; i < typeCount; i++)
            {
                var typeName = indexAndTypeNames[i].Item2;
                if (String.IsNullOrEmpty(typeName))
                    continue;

                var allTypes = p21Reader.GetAllSubtypNames(typeName);

                var index = indexAndTypeNames[i].Item1;
                tuples = tuples.Where(e => allTypes.Any(s => string.Compare(s, e[index].ClassName,
                                                                 StringComparison.InvariantCultureIgnoreCase) == 0)).ToList();
            }
            return tuples;
        }
    }
}
