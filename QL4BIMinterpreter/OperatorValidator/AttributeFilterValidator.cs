using System.Linq;
using QL4BIMinterpreter.QL4BIM;

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
