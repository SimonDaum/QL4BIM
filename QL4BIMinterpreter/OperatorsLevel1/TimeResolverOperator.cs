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
