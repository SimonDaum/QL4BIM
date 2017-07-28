using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IPropertyFilterOperator
    {  
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        void PropertyFilterSet(SetSymbol parameterSym1, string parameter2, string parameter3, RelationSymbol returnSym);
    }
}