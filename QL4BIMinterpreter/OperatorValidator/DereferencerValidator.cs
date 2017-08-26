using System.Linq;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMprimitives;

namespace QL4BIMinterpreter
{
    public class DereferencerValidator : OperatorValidator, IDereferencerValidator
    {
        public DereferencerValidator()
        {
            Name = "Dereferencer";

            var sig1 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.Set, SyUseVal.ExAtt, SyUseVal.ExAtt  }, null, null);
            var sig2 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.Set, SyUseVal.ExAtt }, null, null);
            var sig3 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.RelAtt, SyUseVal.ExAtt }, null, null);
            var sig4 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.RelAtt, SyUseVal.ExAtt, SyUseVal.ExAtt }, null, null);
            FunctionSignaturs.Add(sig1);
            FunctionSignaturs.Add(sig2);
            FunctionSignaturs.Add(sig3);
            FunctionSignaturs.Add(sig4);
        }

        protected override void AdditionalValidation(SymbolTable symbolTable, StatementNode statement)
        {
            var isSet = IsSetAttribute(statement, 0);

            if (isSet && statement.ReturnRelationNode.Attributes.Count != 2)
                throw new QueryException($"{Name}: If a set is used as first parameter, a relation with 2 attributes is returned ");

            if (isSet)
                return;

            var relAttributCount = symbolTable.GetRelationSymbol((RelNameNode) statement.Arguments[0]);

            if (statement.ReturnRelationNode.Attributes.Count != (relAttributCount.Attributes.Count + 1))
                throw new QueryException($"{Name}: If a relation attribute is used as first parameter, a relation with one more attribute is returned ");
        }
    }


}
