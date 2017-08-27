/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMinterpreter.

QL4BIMinterpreter is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMinterpreter is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMinterpreter. If not, see <http://www.gnu.org/licenses/>.

*/

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

        public void Visit(UserFunctionNode functionNode)
        {
            if (functionNode == null)
                return;

            //todo user func
            //var funcValidator = new FuncValidator() {Name = functionNode.Value};

            //var syUsages = new List<SyUseVal>();
            //foreach (var argument in functionNode.FormalArguments)
            //{
            //    if (argument is RelationNode)
            //    {
            //        syUsages.Add(SyUseVal.Rel);
            //        continue;
            //    }

            //    if (argument is SetNode)
            //    {
            //        syUsages.Add(SyUseVal.Set);
            //        continue;
            //    }

            //    throw new QueryException("Only sets and relations supported as function arguments");
            //}

            //var returnUsage = functionNode.LastStatement.ReturnRelationNode != null ? SyUseVal.Rel : SyUseVal.Set;

            //var sig1 = new FunctionSignatur(returnUsage, new[] { SyUseVal.Set }, null, null);
            //funcValidator.FunctionSignaturs.Add(sig1);

            //interpreterRepository.AddValidator(funcValidator);

            //functionNode = functionNode.Next;
            //Visit(functionNode);
        }

        public void Visit(StatementNode statementNode)
        {
            throw new NotImplementedException();
        }
    }
}
