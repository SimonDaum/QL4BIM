using System.Collections.Generic;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IProjectorOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        void ProjectRelAttRelation(RelationSymbol parameterSym1, int[] attributeIndices, RelationSymbol returnSym);

        void ProjectRelAttSet(RelationSymbol parameterSym1, SetSymbol returnSym);
        List<QLEntity[]> ProjectLocal(IEnumerable<QLEntity[]> tuples, int[] argumentIndices);
    }
}