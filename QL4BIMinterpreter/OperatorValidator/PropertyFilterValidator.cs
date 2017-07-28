using System.Linq;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public class PropertyFilterValidator : OperatorValidator, IPropertyFilterValidator
    {
        public PropertyFilterValidator()
        {
            Name = "PropertyFilterOperator";

            var sig1 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.Set, SyUseVal.String, SyUseVal.String }, 
                null, null);
            //var sig2 = new FunctionSignatur(SymbolUsage.Relation, new[] { "LiteralNode"}, "CStringNode", "=,!=");
            FunctionSignaturs.Add(sig1);
            //FunctionSignaturs.Add(sig2);
        }
        

    }
}
