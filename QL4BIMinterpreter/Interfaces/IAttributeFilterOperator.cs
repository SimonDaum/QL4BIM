using QL4BIMinterpreter.OperatorsLevel0;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IAttributeFilterOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        void AttributeFilterSet(SetSymbol parameterSym1, PredicateNode predicateNode, SetSymbol returnSym);
        void AttributeFilterRelAtt(RelationSymbol parameterSym1, PredicateNode[] predicateNode, RelationSymbol returnSym);
        bool AttributeSetTestLocal(QLEntity entity, PredicateNode predicateNode);
    }
}