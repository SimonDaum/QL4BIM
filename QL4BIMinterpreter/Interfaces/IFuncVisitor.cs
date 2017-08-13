using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IFuncVisitor
    {
        void Visit(StatementNode statementNode);
        void Visit(UserFunctionNode functionNode);
    }
}