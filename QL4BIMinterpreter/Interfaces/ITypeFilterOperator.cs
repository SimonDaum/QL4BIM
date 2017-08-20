using System;
using System.Collections.Generic;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface ITypeFilterOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym
        void TypeFilterSet(SetSymbol parameterSym1, string typeName, SetSymbol returnSym);
        void TypeFilterRelation(RelationSymbol parameterSym1,  Tuple<int, string>[] indexAndTypeNames, RelationSymbol returnSym);
        IEnumerable<QLEntity[]> GetTuples(IEnumerable<QLEntity[]> tuples, Tuple<int, string>[] indexAndTypeNames);
    }
}