using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter.OperatorsLevel1
{
    class TaskTimerOperator : ITaskTimerOperator
    {
        private readonly IDeassociaterOperator deassociater;
        private readonly IDereferenceOperator dereferenceOperator;
        private readonly ITypeFilterOperator typeFilter;


        public TaskTimerOperator(IDeassociaterOperator deassociater, IDereferenceOperator dereferenceOperator, ITypeFilterOperator typeFilter)
        {
            this.deassociater = deassociater;
            this.dereferenceOperator = dereferenceOperator;
            this.typeFilter = typeFilter;
        }

        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        public void TaskTimerRelAtt(RelationSymbol parameterSym1, RelationSymbol returnSym)
        {
            Console.WriteLine("TaskTimer'ing...");

            var index = parameterSym1.Index.Value;
            var tuples = deassociater.GetTuplesRelAtt(parameterSym1.Tuples, index , new []{"ReferencedBy"}).ToArray();

            var lastIndex = returnSym.Attributes.Count-1;

            tuples = dereferenceOperator.ResolveReferenceTuplesIn(tuples, lastIndex, true, "TaskTime").ToArray();

            var typeList = new List<Tuple<int,string>>();
            for (int i = 0; i < returnSym.Attributes.Count; i++)
            {
                if (i == index)
                {
                    typeList.Add(new Tuple<int, string>(index, "IfcProduct")); //todo index is a guess
                    continue;
                }

                if (i == lastIndex)
                {
                    typeList.Add(new Tuple<int, string>(lastIndex, "IfcTaskTime")); //todo index is a guess
                    continue;
                }
            }

            //type filtering ifcTask (plus products?)
            var tuplesTyped = typeFilter.GetTuples(tuples, typeList.ToArray());

            returnSym.SetTuples(tuplesTyped);
        }
    }
}
