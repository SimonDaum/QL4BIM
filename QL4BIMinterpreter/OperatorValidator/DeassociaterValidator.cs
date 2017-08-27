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
using System.Text;
using System.Threading.Tasks;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    class DeassociaterValidator : OperatorValidator, IDeaccociaterValidator
    {
        public DeassociaterValidator()
        {
            {
                Name = "Deassociater";

                var sig1 = new FunctionSignatur(SyUseVal.Rel, new[] {SyUseVal.Set, SyUseVal.ExAtt}, null, null);
                var sig2 = new FunctionSignatur(SyUseVal.Rel, new[] {SyUseVal.Set, SyUseVal.ExAtt, SyUseVal.ExAtt}, null, null);
                var sig3 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.RelAtt, SyUseVal.ExAtt }, null, null);
                var sig4 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.RelAtt, SyUseVal.ExAtt, SyUseVal.ExAtt }, null, null);
                FunctionSignaturs.Add(sig1);
                FunctionSignaturs.Add(sig2);
                FunctionSignaturs.Add(sig3);
                FunctionSignaturs.Add(sig4);
            }

        }
    }
}
