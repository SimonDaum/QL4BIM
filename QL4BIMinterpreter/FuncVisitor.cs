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

        public void Visit(FunctionNode functionNode)
        {
            if (functionNode == null)
                return;

            if (functionNode.Value == "Global")
                functionNode = functionNode.Next;

            if (functionNode == null)
                return;

            var funcValidator = new FuncValidator() {Name = functionNode.Value};

            var syUsages = new List<SyUseVal>();
            foreach (var argument in functionNode.Arguments)
            {
                if (argument is RelationNode)
                {
                    syUsages.Add(SyUseVal.Rel);
                    continue;
                }

                if (argument is SetNode)
                {
                    syUsages.Add(SyUseVal.Set);
                    continue;
                }

                throw new QueryException("Only sets and relations supported as function arguments");
            }

            var returnUsage = functionNode.LastStatement.ReturnRelationNode != null ? SyUseVal.Rel : SyUseVal.Set;

            var sig1 = new FunctionSignatur(returnUsage, new[] { SyUseVal.Set }, null, null);
            funcValidator.FunctionSignaturs.Add(sig1);

            interpreterRepository.AddValidator(funcValidator);

            functionNode = functionNode.Next;
            Visit(functionNode);
        }

        public void Visit(StatementNode statementNode)
        {
            throw new NotImplementedException();
        }
    }
}
