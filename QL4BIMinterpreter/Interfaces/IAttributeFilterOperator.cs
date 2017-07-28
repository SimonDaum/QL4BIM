using QL4BIMinterpreter.OperatorsLevel0;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IAttributeFilterOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        void AttributeFilterSet(SetSymbol parameterSym1, AttributeFilterOperator.PredicateData data, SetSymbol returnSym);
        void AttributeFilterRelAtt(RelationSymbol parameterSym1, AttributeFilterOperator.PredicateData data, RelationSymbol returnSym);
        bool AttributeSetTestLocal(QLEntity entity, AttributeFilterOperator.PredicateData data);
    }
}