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

using System.Linq;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMprimitives;

namespace QL4BIMinterpreter
{
    public class ProjectorValidator : OperatorValidator, IProjectorValidator
    {
        public ProjectorValidator() 
        {
            Name = "Projector";

            var sig1 = new FunctionSignatur(SyUseVal.RelAtt, new []{SyUseVal.RelAttVa }, null, null);
            FunctionSignaturs.Add(sig1);
        }

        protected override void AdditionalValidation(SymbolTable symbolTable, StatementNode statement)
        {
            AllRelationalArguments(statement);

            AllArgumentsFromOneRelation(statement);

            if (statement.Arguments.Count == 1 && statement.ReturnSetNode == null)
            {
                throw new QueryException($"{Name}: If one relational attribute is used as parameter, a set is returned");
            }

            if (statement.Arguments.Count > 1 && statement.ReturnRelationNode.Attributes.Count != statement.Arguments.Count)
                throw new QueryException($"{Name}: Nummber of relational arguments (attributes) must be identical to number of return attributes.");



        }


    }


}
