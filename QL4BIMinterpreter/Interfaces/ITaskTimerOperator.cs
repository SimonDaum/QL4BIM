using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface ITaskTimerOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        void TaskTimerRelAtt(RelationSymbol parameterSym1, RelationSymbol returnSym);
    }
}