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
