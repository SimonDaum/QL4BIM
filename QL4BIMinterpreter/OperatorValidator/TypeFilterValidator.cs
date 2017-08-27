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

using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public class TypeFilterValidator : OperatorValidator, ITypeFilterValidator
    {
        public TypeFilterValidator()
        {
            Name = "TypeFilter";

            var sig1 = new FunctionSignatur(SyUseVal.Set, new[] { SyUseVal.Set, SyUseVal.ExType}, null, null);
            var sig2 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.RelAtt, SyUseVal.ExType }, null, null);
            var sig3 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.Rel, SyUseVal.ExTypeVa }, null, null);
            FunctionSignaturs.Add(sig1);
            FunctionSignaturs.Add(sig2);
            FunctionSignaturs.Add(sig3);
        }

    }


}
