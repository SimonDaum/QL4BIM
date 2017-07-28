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

        public void MaximumRelAtt(RelationSymbol parameterSym1,  string parameter2, RelationSymbol returnSym)
        {
            Console.WriteLine("Maximum'ing...");

            var index = parameterSym1.Index.Value;

            QLPart partForTypeExtraction = null; //first present attribute provides type info
            var tuples = parameterSym1.Tuples.ToArray();
            foreach (var tuple in tuples)
            {
                var part = tuple[index].GetPropertyValue(parameter2);
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
                var max = tuples.Max(t => t[index].GetPropertyValue(parameter2).QLNumber.Value);
                tuplesOut = tuples.Where(t => t[index].GetPropertyValue(parameter2).QLNumber.Value == max);
            }

            returnSym.SetTuples(tuplesOut);
        }
    }
}