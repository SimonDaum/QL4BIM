using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IMaximumOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        void MaximumRelAtt(RelationSymbol parameterSym1, string parameter2, RelationSymbol returnSym);
    }
}