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

        public FunctionNode GlobalBlock => globalContextSwitch.GlobalFunctionNode;

        private GlobalBlockContextSwitch globalContextSwitch;
        private ParserContext parsingContext = ParserContext.NoChange;
        private ContextSwitch contextSwitch = null;


        private FunctionNode currentFunctionNode = null;

        private void ParserOnPartParsed(object sender, PartParsedEventArgs partParsedEventArgs)
        {
            if (partParsedEventArgs.Context != parsingContext)
            {
                contextSwitch?.OnContextSwitch();

                parsingContext = partParsedEventArgs.Context;
                switch (parsingContext)
                {
                    case ParserContext.GlobalBlock:
                        globalContextSwitch = new GlobalBlockContextSwitch();
                        contextSwitch = globalContextSwitch;
                        contextSwitch.AddNode(partParsedEventArgs.CurrentToken, partParsedEventArgs.ParsePart);
                        currentFunctionNode = globalContextSwitch.GlobalFunctionNode; // todo set in func context
                        break;


                    case ParserContext.Statement:
                        contextSwitch = new StatementContextSwitch() { ParentFuncNode = currentFunctionNode };
                        contextSwitch.AddNode(partParsedEventArgs.CurrentToken, ParserParts.NoChange);
                        break;
                    

                    case ParserContext.Variable:
                        contextSwitch = new ParseVariableContextSwitch() {ParentFuncNode = currentFunctionNode };
                        break;

                    case ParserContext.Operator:
                        contextSwitch = new OperatorContextSwitch() { ParentFuncNode = currentFunctionNode };
                        contextSwitch.AddNode(partParsedEventArgs.CurrentToken, ParserParts.NoChange);
                        break;

                    case ParserContext.SimpleSetRelParameter:
                        contextSwitch = new ParseSetRelArgContextSwitch() { ParentFuncNode = currentFunctionNode };
                        break;
                }

                return;
            }

            contextSwitch.AddNode(partParsedEventArgs.CurrentToken, partParsedEventArgs.ParsePart);


        }

         abstract class ContextSwitch
        {
            public FunctionNode ParentFuncNode { get; set; }


            public abstract void AddNode(string value, ParserParts parserPart);

            public abstract void OnContextSwitch();


        }

         class GlobalBlockContextSwitch : ContextSwitch
        {
               
            public FunctionNode GlobalFunctionNode { get; private set; }

            public override void AddNode(string value, ParserParts parserPart)
            {
                GlobalFunctionNode = new FunctionNode(value);
            }

            public override void OnContextSwitch(){}
        }

        class StatementContextSwitch : ContextSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.NoChange:
                        ParentFuncNode.AddStatement(new StatementNode());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parserPart), parserPart, null);
                }
            }

            public override void OnContextSwitch() { }
        }

        class OperatorContextSwitch : ContextSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.NoChange:
                        ParentFuncNode.LastStatement.SetOperator(value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parserPart), parserPart, null);
                }
            }

            public override void OnContextSwitch() { }
        }


        class SimpleConstantParameter : ContextSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.Float:
                        ParentFuncNode.AddArgument(new CFloatNode(value));
                        break;
                    case ParserParts.Number:
                        ParentFuncNode.AddArgument(new CNumberNode(value));
                        break;
                    case ParserParts.String:
                        ParentFuncNode.AddArgument(new CStringNode(value));
                        break;
                    case ParserParts.Bool:
                        ParentFuncNode.AddArgument(new CBoolNode(value));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parserPart), parserPart, null);
                }
            }

            public override void OnContextSwitch() { }
        }

        abstract class SetRelContextSwitch : ContextSwitch
        {
            protected string PrimeVariable;
            protected readonly List<string> SecondaryVariable = new List<string>();

            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.SetRelVar: //ParserParts.EmptyRelAtt
                        PrimeVariable = value;
                        break;
                    case ParserParts.RelAtt: //ParserParts.EmptyRelAtt
                        SecondaryVariable.Add(value);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public override void OnContextSwitch()
            {
                PrimeVariable = String.Empty;
                SecondaryVariable.Clear();
            }
        }

        class ParseVariableContextSwitch : SetRelContextSwitch
        {

            public override void OnContextSwitch()
            {
                if (SecondaryVariable.Count == 0)
                    ParentFuncNode.LastStatement.SetSetReturn(new SetNode(PrimeVariable));
                else
                {
                    var relation = new RelationNode(PrimeVariable);
                    relation.Attributes = SecondaryVariable.ToList();
                    ParentFuncNode.LastStatement.SetRelationReturn(relation);
                    base.OnContextSwitch();
                }
            }
        }

        class ParseSetRelArgContextSwitch : SetRelContextSwitch
        {
            public override void OnContextSwitch()
            {
                if (SecondaryVariable.Count == 0)
                    ParentFuncNode.AddArgument(new SetNode(PrimeVariable));
                else
                {
                    var relation = new RelationNode(PrimeVariable);
                    relation.Attributes = SecondaryVariable.ToList();
                    ParentFuncNode.AddArgument(relation);
                    base.OnContextSwitch();
                }
            }
        }
    }
}