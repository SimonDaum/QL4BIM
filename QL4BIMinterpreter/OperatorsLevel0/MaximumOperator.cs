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
using System.Runtime.CompilerServices;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter.OperatorsLevel0
{
    public class MaximumOperator : IMaximumOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        private readonly IAttributeFilterOperator attributeFilterOperator;

        public MaximumOperator(IAttributeFilterOperator attributeFilterOperator)
        {
            this.attributeFilterOperator = attributeFilterOperator;
        }

        public void MaximumRelAtt(RelationSymbol parameterSym1, string exAttribute, RelationSymbol returnSym)
        {
            Console.WriteLine("Maximum'ing...");

            var index = parameterSym1.Index.Value;

            QLPart partForTypeExtraction = null; //first present attribute provides type info
            var tuples = parameterSym1.Tuples.ToArray();
            foreach (var tuple in tuples)
            {
                var part = tuple[index].GetPropertyValue(exAttribute);
                if (part != null)
                {
                    partForTypeExtraction = part;
                    break;
                }
            }

            if (partForTypeExtraction == null)
                return;

            IEnumerable<QLEntity[]> tuplesOut = null;
            if (partForTypeExtraction.QLNumber != null)
            {
                var max = tuples.Max(t => t[index].GetPropertyValue(exAttribute).QLNumber.Value);
                tuplesOut = tuples.Where(t => t[index].GetPropertyValue(exAttribute).QLNumber.Value == max);
            }

            returnSym.SetTuples(tuplesOut);
        }
    }
}
