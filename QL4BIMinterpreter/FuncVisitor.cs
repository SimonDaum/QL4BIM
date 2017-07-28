using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    class FuncVisitor : IFuncVisitor
    {
        private readonly IInterpreterRepository interpreterRepository;

        public FuncVisitor(IInterpreterRepository interpreterRepository)
        {
            this.interpreterRepository = interpreterRepository;
        }

        public void Visit(FuncNode funcNode)
        {
            if (funcNode == null)
                return;

            if (funcNode.Value == "Global")
                funcNode = funcNode.Next;

            if (funcNode == null)
                return;

            var funcValidator = new FuncValidator() {Name = funcNode.Value};

            var syUsages = new List<SyUseVal>();
            foreach (var argument in funcNode.Arguments)
            {
                if (argument is CompLitNode)
                {
                    syUsages.Add(SyUseVal.Rel);
                    continue;
                }

                if (argument is LiteralNode)
                {
                    syUsages.Add(SyUseVal.Set);
                    continue;
                }

                throw new QueryException("Only sets and relations supported as function arguments");
            }

            var returnUsage = funcNode.LastStatement.ReturnCompLitNode != null ? SyUseVal.Rel : SyUseVal.Set;

            var sig1 = new FunctionSignatur(returnUsage, new[] { SyUseVal.Set }, null, null);
            funcValidator.FunctionSignaturs.Add(sig1);

            interpreterRepository.AddValidator(funcValidator);

            funcNode = funcNode.Next;
            Visit(funcNode);
        }

        public void Visit(StatementNode statementNode)
        {
            throw new NotImplementedException();
        }
    }
}
