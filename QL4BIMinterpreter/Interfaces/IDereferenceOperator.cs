using System.Collections.Generic;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IDereferenceOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        void ReferenceSet(SetSymbol parameterSym1, string[] references, RelationSymbol returnSym);
        void ReferenceRelAtt(RelationSymbol parameterSym1, string[] references, RelationSymbol returnSym);

        List<QLEntity[]> ResolveReferenceTuplesIn(IEnumerable<QLEntity[]> tuples, int attributeIndex, bool replace, string referenceName);
        IEnumerable<QLEntity[]> ResolveReferenceSetIn(IEnumerable<QLEntity> entites, string referenceName);
    }
}