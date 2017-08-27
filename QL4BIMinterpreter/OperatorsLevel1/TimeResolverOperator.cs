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

namespace QL4BIMinterpreter.OperatorsLevel1
{
    class TimeResolverOperator : ITaskTimerOperator
    {
        private readonly IDeassociaterOperator deassociater;
        private readonly IDereferenceOperator dereferenceOperator;
        private readonly ITypeFilterOperator typeFilter;


        public TimeResolverOperator(IDeassociaterOperator deassociater, IDereferenceOperator dereferenceOperator, ITypeFilterOperator typeFilter)
        {
            this.deassociater = deassociater;
            this.dereferenceOperator = dereferenceOperator;
            this.typeFilter = typeFilter;
        }

        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        public void TimeResolverRel(RelationSymbol parameterSym1, RelationSymbol returnSym)
        {
            Console.WriteLine("TimeResolver'ing...");

            var index = parameterSym1.Index.Value;
            var tuples = deassociater.GetTuplesRelAtt(parameterSym1.Tuples, index , new []{"ReferencedBy"}).ToArray();

            var lastIndex = returnSym.Attributes.Count-2; 

            tuples = dereferenceOperator.ResolveReferenceTuplesIn(tuples, lastIndex, false, "TaskTime").ToArray();

            var typeList = new List<Tuple<int, string>>()
            {
                new Tuple<int, string>(index, "IfcProduct"),
                new Tuple<int, string>(lastIndex, "IfcTask"),
                new Tuple<int, string>(lastIndex+1, "IfcTaskTime"),
            };

            //type filtering ifcTask (plus products?)
            var tuplesTyped = typeFilter.GetTuples(tuples, typeList.ToArray());

            returnSym.SetTuples(tuplesTyped);
        }
    }
}
