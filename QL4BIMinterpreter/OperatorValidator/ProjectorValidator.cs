using System.Linq;
using QL4BIMinterpreter.QL4BIM;

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

            if (statement.Arguments.Count == 1 && statement.ReturnLiteralNode == null)
            {
                throw new QueryException($"{Name}: If one relational attribute is used as parameter, a set is returned");
            }

            if (statement.Arguments.Count > 1 && statement.ReturnCompLitNode.Literals.Count != statement.Arguments.Count)
                throw new QueryException($"{Name}: Nummber of relational arguments (attributes) must be identical to number of return attributes.");



        }


    }


}
