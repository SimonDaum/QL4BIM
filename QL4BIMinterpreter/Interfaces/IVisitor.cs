using System.Collections.Generic;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface ISymbolVisitor
    {
        void Visit(StatementNode statementNode);
        void Visit(FuncNode funcNode);

    }
}