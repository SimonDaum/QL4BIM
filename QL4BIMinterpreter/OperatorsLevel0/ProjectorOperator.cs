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
    public class ProjectorOperator : IProjectorOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        public void ProjectRelAttRelation(RelationSymbol parameterSym1, int[] attributeIndices, RelationSymbol returnSym)
        {
            Console.WriteLine("Projector'ing...");

            var result = ProjectLocal(parameterSym1.Tuples, attributeIndices);

            returnSym.SetTuples(result);
        }

        public void ProjectRelAttSet(RelationSymbol parameterSym1, SetSymbol returnSym)
        {
            Console.WriteLine("Projector'ing...");

            var result = ProjectLocal(parameterSym1.Tuples, new [] { parameterSym1.Index.Value}).Select(t => t[0]).Distinct();

            returnSym.EntityDic = result.ToDictionary(e => e.Id); ;
        }

        public List<QLEntity[]> ProjectLocal(IEnumerable<QLEntity[]> tuples, int[] argumentIndices)
        {
            var result = new List<QLEntity[]>();

            foreach (var tuple in tuples)
            {
                var newTuple = new List<QLEntity>();
                for (int i = 0; i < tuple.Length; i++)
                {
                    if (argumentIndices.Contains(i))
                        newTuple.Add(tuple[i]);
                }
                result.Add(newTuple.ToArray());
            }
            return result;
        }
    }
}
