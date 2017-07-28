using System;
using System.Collections.Generic;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter.OperatorsLevel1
{
    class DeassociaterOperator : IDeassociaterOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        private readonly IDereferenceOperator dereferenceOperator;

        public DeassociaterOperator(IDereferenceOperator dereferenceOperator)
        {
            this.dereferenceOperator = dereferenceOperator;
        }

        public void DeassociaterRelAtt(RelationSymbol parameterSym1, string[] exAtts, RelationSymbol returnSym)
        {
            Console.WriteLine("Deassociater'ing...");
            returnSym.SetTuples(GetTuplesRelAtt(parameterSym1.Tuples, parameterSym1.Index.Value,  exAtts));
        }

        public IEnumerable<QLEntity[]> GetTuplesRelAtt(IEnumerable<QLEntity[]> tuples, int attributeIndex, string[] exAtts)
        {
            var firstPairs = dereferenceOperator.ResolveReferenceTuplesIn(tuples, attributeIndex, false, exAtts[0]);

            if (firstPairs.Count == 0)
                return new List<QLEntity[]>();

            //pair original second arg
            var secondPairs = dereferenceOperator.ResolveReferenceTuplesIn(firstPairs, firstPairs[0].Length-1, true, mapObjectiviedRelations[exAtts[0]]);

            if (exAtts.Length == 1)
                return secondPairs;

            var thirdPair = dereferenceOperator.ResolveReferenceTuplesIn(secondPairs, 1, true, exAtts[1]);
            return dereferenceOperator.ResolveReferenceTuplesIn(thirdPair, 1, true, mapObjectiviedRelations[exAtts[1]]);
        }


        public void DeassociaterSet(SetSymbol parameterSym1, string[] exAtts, RelationSymbol returnSym)
        {
            Console.WriteLine("Deassociater'ing...");
            returnSym.SetTuples(GetTuplesSet(parameterSym1.Entites, exAtts));
        }

        public IEnumerable<QLEntity[]> GetTuplesSet(IEnumerable<QLEntity> entites, string[] exAtts)
        {
            var firstPairs = dereferenceOperator.ResolveReferenceSetIn(entites, exAtts[0]);

            //pair original second arg
            var secondPairs = dereferenceOperator.ResolveReferenceTuplesIn(firstPairs, 1, true, mapObjectiviedRelations[exAtts[0]]);

            if (exAtts.Length == 1)
                return secondPairs;

            var thirdPair = dereferenceOperator.ResolveReferenceTuplesIn(secondPairs, 1, true, exAtts[1]);
            return dereferenceOperator.ResolveReferenceTuplesIn(thirdPair, 1, true, mapObjectiviedRelations[exAtts[1]]);
        }


        private Dictionary<string, string> mapObjectiviedRelations = new Dictionary<string, string>()
        {
            { "HasOpenings", "RelatedOpeningElement" },
            { "HasFillings", "RelatedBuildingElement" },
            { "ContainedInStructure", "RelatingStructure" },
            { "ReferencedBy", "RelatedObjects" }
        };
    }
}
