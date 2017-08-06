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
            parser.ContextChanged += Parser_ContextChanged;
        }

        private void Parser_ContextChanged(object sender, ContextChangedEventArgs e)
        {   
            Console.WriteLine(e.ToString());

            contextSwitch?.BeforeContextSwitch();

            switch (e.Context)
                {
                    case ParserContext.GlobalBlock:
                        globalContextSwitch = new GlobalBlockContextSwitch();
                        contextSwitch = globalContextSwitch;
                        contextSwitch.AddNode("Global Function", ParserParts.DefOp);
                        currentFunctionNode = GlobalBlock;
                        //currentFunctionNode = globalContextSwitch.GlobalFunctionNode; // todo set in func context
                        break;


                    case ParserContext.Statement:
                        contextSwitch = new StatementContextSwitch() { ParentFuncNode = currentFunctionNode };
                        contextSwitch.AddNode("Statement", ParserParts.NullPart);
                        break;


                    case ParserContext.Variable:
                        contextSwitch = new ParseVariableContextSwitch() { ParentFuncNode = currentFunctionNode };
                        break;

                    case ParserContext.Operator:
                        contextSwitch = new OperatorContextSwitch() { ParentFuncNode = currentFunctionNode };
                        //contextSwitch.AddNode(partParsedEventArgs.CurrentToken, ParserParts.NoChange);
                        break;

                    case ParserContext.Argument:
                        contextSwitch = new ParseArgumentContextSwitch() { ParentFuncNode = currentFunctionNode };
                        break;
                    case ParserContext.ArgumentEnd:
                        contextSwitch.BeforeContextSwitch();
                        break;
            }
        }

        private void ParserOnPartParsed(object sender, PartParsedEventArgs e)
        {
            Console.WriteLine('\t' + e.ToString());
            contextSwitch.AddNode(e.CurrentToken, e.ParsePart);
        }

        public FunctionNode GlobalBlock => globalContextSwitch.GlobalFunctionNode;

        private GlobalBlockContextSwitch globalContextSwitch;


        private ContextSwitch contextSwitch = null;

        private FunctionNode currentFunctionNode = null;



         abstract class ContextSwitch
        {
            public FunctionNode ParentFuncNode { get; set; }

            public abstract void AddNode(string value, ParserParts parserPart);

            public abstract void BeforeContextSwitch();
            public abstract void InitContext(string value);


        }

        class GlobalBlockContextSwitch : ContextSwitch
        {
               
            public FunctionNode GlobalFunctionNode { get; private set; }

            public override void AddNode(string value, ParserParts parserPart)
            {
                GlobalFunctionNode = new FunctionNode(value);
            }

            public override void BeforeContextSwitch() {}

            public override void InitContext(string value)
            {

            }
        }

        class StatementContextSwitch : ContextSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                ParentFuncNode.AddStatement(new StatementNode());
            }

            public override void BeforeContextSwitch() { }

            public override void InitContext(string value)
            {

            }
        }

        class OperatorContextSwitch : ContextSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                if( parserPart != ParserParts.Operator)
                    throw new ArgumentOutOfRangeException(nameof(parserPart), parserPart, null);
                
                ParentFuncNode.LastStatement.SetOperator(value);
            }

            public override void BeforeContextSwitch() { }

            public override void InitContext(string value)
            {
            }
        }


        class SimpleConstantParameter : ContextSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.Float:
                        ParentFuncNode.LastStatement.AddArgument(new CFloatNode(value));
                        break;
                    case ParserParts.Number:
                        ParentFuncNode.LastStatement.AddArgument(new CNumberNode(value));
                        break;
                    case ParserParts.String:
                        ParentFuncNode.LastStatement.AddArgument(new CStringNode(value));
                        break;
                    case ParserParts.Bool:
                        ParentFuncNode.LastStatement.AddArgument(new CBoolNode(value));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parserPart), parserPart, null);
                }
            }

            public override void BeforeContextSwitch() { }

            public override void InitContext(string value) { }
        }

        abstract class SetRelContextSwitch : ContextSwitch
        {
            protected string PrimeVariable;
            protected readonly List<string> SecondaryVariable = new List<string>();


            public override void BeforeContextSwitch()
            {
                PrimeVariable = String.Empty;
                SecondaryVariable.Clear();
            }
        }

        class ParseVariableContextSwitch : SetRelContextSwitch
        {


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

            public override void BeforeContextSwitch()
            {
                if (SecondaryVariable.Count == 0)
                    ParentFuncNode.LastStatement.SetSetReturn(new SetNode(PrimeVariable));
                else
                {
                    var relation = new RelationNode(PrimeVariable);
                    relation.Attributes = SecondaryVariable.ToList();
                    ParentFuncNode.LastStatement.SetRelationReturn(relation);
                    base.BeforeContextSwitch();
                }
            }

            public override void InitContext(string value) { }
        }

        class ParseArgumentContextSwitch : SetRelContextSwitch
        {

            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.SetRelArg: //ParserParts.SetRelArg
                        PrimeVariable = value;
                        break;
                    case ParserParts.RelAtt: //ParserParts.EmptyRelAtt
                        SecondaryVariable.Add(value);
                        break;
                    case ParserParts.String: //ParserParts.EmptyRelAtt
                        ParentFuncNode.LastStatement.AddArgument(new CStringNode(value));
                        break;
                    case ParserParts.Number: //ParserParts.EmptyRelAtt
                        ParentFuncNode.LastStatement.AddArgument(new CNumberNode(value));
                        break;
                    case ParserParts.Float: //ParserParts.EmptyRelAtt
                        ParentFuncNode.LastStatement.AddArgument(new CFloatNode(value));
                        break;
                    case ParserParts.Bool: //ParserParts.EmptyRelAtt
                        ParentFuncNode.LastStatement.AddArgument(new CBoolNode(value));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }


            public override void BeforeContextSwitch()
            {   
                if(PrimeVariable == null)
                    return;

                if (SecondaryVariable.Count == 0)
                    ParentFuncNode.LastStatement.AddArgument(new SetNode(PrimeVariable));
                else
                {
                    var relation = new RelationNode(PrimeVariable);
                    relation.Attributes = SecondaryVariable.ToList();
                    ParentFuncNode.LastStatement.AddArgument(relation);
                    base.BeforeContextSwitch();
                }
            }

            public override void InitContext(string value) { }
        }
    }
}