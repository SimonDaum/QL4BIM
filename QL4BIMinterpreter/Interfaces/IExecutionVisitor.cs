using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IExecutionVisitor
    {
        void Visit(StatementNode statementNode);
        void Visit(FunctionNode functionNode);
    }
}