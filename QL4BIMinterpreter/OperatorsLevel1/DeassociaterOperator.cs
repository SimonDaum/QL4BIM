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


        private Dictionary<string, string> mapObjectiviedRelations = new Dictionary<string, string>() //todo add reversed
        {
            { "HasOpenings", "RelatedOpeningElement" },
            { "HasFillings", "RelatedBuildingElement" },
            { "ContainedInStructure", "RelatingStructure" },
            { "ReferencedBy", "RelatedObjects" }
        };
    }
}
