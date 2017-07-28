using System.Collections.Generic;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IDeassociaterOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        void DeassociaterSet(SetSymbol parameterSym1, string[] exAtts, RelationSymbol returnSym);

        void DeassociaterRelAtt(RelationSymbol parameterSym1, string[] exAtts, RelationSymbol returnSym);

        IEnumerable<QLEntity[]> GetTuplesRelAtt(IEnumerable<QLEntity[]> tuples, int attributeIndex, string[] exAtts);
        IEnumerable<QLEntity[]> GetTuplesSet(IEnumerable<QLEntity> entites, string[] exAtts);
    }
}