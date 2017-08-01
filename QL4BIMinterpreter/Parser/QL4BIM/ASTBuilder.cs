using System;
using System.Collections.Generic;
using System.Linq;

namespace QL4BIMinterpreter.QL4BIM
{
    public class AstBuilder : IAstBuilder
    {
        public void RegisterParseEvent(Parser parser)
        {
            parser.PartParsed += ParserOnPartParsed;
        }

        public FunctionNode GlobalBlock => roots[GlobalFunctionId];

        private const string GlobalFunctionId = "GlobalBlock";
        private string currentContexct = String.Empty;
        private readonly Dictionary<string, FunctionNode> roots = new Dictionary<string, FunctionNode>();
        private string primeVariable;
        private readonly List<string> secondaryVariable = new List<string>();

        private void ParserOnPartParsed(object sender, PartParsedEventArgs partParsedEventArgs)
        {
            switch (partParsedEventArgs.Context)
            {
                case ParserParts.GlobalBlock:
                    roots.Add(GlobalFunctionId, new FunctionNode());
                    currentContexct = GlobalFunctionId;
                    break;
                case ParserParts.FuncDefBlock:
                    roots.Add(partParsedEventArgs.CurrentToken, new FunctionNode());
                    currentContexct = partParsedEventArgs.CurrentToken;
                    break;
                case ParserParts.Statement:
                    roots[currentContexct].AddStatement(new StatementNode());
                    break;
                case ParserParts.VariableBegin:
                    primeVariable = partParsedEventArgs.CurrentToken;
                    break;
                case ParserParts.VariableRelAtt: //ParserParts.EmptyRelAtt
                    secondaryVariable.Add(partParsedEventArgs.CurrentToken);
                    break;
                case ParserParts.VariableEnd: //ParserParts.EmptyRelAtt
                    if (secondaryVariable.Count == 0)
                        roots[currentContexct].LastStatement.SetSetReturn(new SetNode(primeVariable));
                    else
                    {
                        var relation = new RelationNode(primeVariable);
                        relation.Attributes = secondaryVariable.ToList();
                        roots[currentContexct].LastStatement.SetRelationReturn(relation);
                        secondaryVariable.Clear();
                    }
                    break;
                case ParserParts.Operator:
                    roots[currentContexct].LastStatement.SetOperator(partParsedEventArgs.CurrentToken);
                    break;

                case ParserParts.ArguementSetRelBegin:
                    primeVariable = partParsedEventArgs.CurrentToken;
                    break;

                case ParserParts.ArguementRelAtt:
                    secondaryVariable.Add(partParsedEventArgs.CurrentToken);
                    break;
                case ParserParts.ArgumentRelSetEnd: //ParserParts.EmptyRelAtt
                    if (secondaryVariable.Count == 0)
                        roots[currentContexct].LastStatement.AddArgument(new SetNode(primeVariable));
                    else
                    {
                        var relation = new RelationNode(primeVariable);
                        relation.Attributes = secondaryVariable.ToList();
                        roots[currentContexct].LastStatement.AddArgument(relation);
                        secondaryVariable.Clear();
                    }
                    break;

                case ParserParts.Float: // keine unterscheidung für predicate
                    roots[currentContexct].LastStatement.AddArgument(new CFloatNode(partParsedEventArgs.CurrentToken));
                    break;
                case ParserParts.Number: // keine unterscheidung für predicate
                    roots[currentContexct].LastStatement.AddArgument(new CNumberNode(partParsedEventArgs.CurrentToken));
                    break;
                case ParserParts.String: // keine unterscheidung für predicate
                    roots[currentContexct].LastStatement.AddArgument(new CStringNode(partParsedEventArgs.CurrentToken));
                    break;
                case ParserParts.Bool: // keine unterscheidung für predicate
                    roots[currentContexct].LastStatement.AddArgument(new CBoolNode(partParsedEventArgs.CurrentToken));
                    break;
            }
        }
    }
}
