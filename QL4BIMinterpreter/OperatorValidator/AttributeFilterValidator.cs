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
    public class ArguementFilterValidator : OperatorValidator, IArgumentFilterValidator
    {
        public ArguementFilterValidator()
        {
            Name = "AttributeFilter";

            var sig1 = new FunctionSignatur(SyUseVal.Set, new[] { SyUseVal.Set }, new[] { SyUseVal.ExAtt}, "=,!=");
            var sig2 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.RelAtt}, new[] { SyUseVal.ExAtt }, "=,!=");
            FunctionSignaturs.Add(sig1);
            FunctionSignaturs.Add(sig2);
        }

        protected override void AdditionalValidation(SymbolTable symbolTable, StatementNode statement)
        {
            var isSetAttribute = IsSetAttribute(statement, 0);

            if (isSetAttribute && statement.ReturnSetNode == null)
                throw new QueryException($"{Name}: If a set is used as first parameter, a set is returned ");

            if(isSetAttribute)
                return;

            if (statement.ReturnRelationNode == null)
                throw new QueryException($"{Name}: If a relation is used as first parameter, a relation is returned ");

            var attributeCountOfRelation = ReferencedRelationAttributeCount(statement.Arguments[0] as SetNode);


            if (statement.ReturnRelationNode.Attributes.Count != attributeCountOfRelation)
                throw new QueryException($"{Name}: If a relation attribute is used as first parameter, a relation with the same number of attributes is returned ");
        }
    }
}
