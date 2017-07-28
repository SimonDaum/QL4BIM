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
