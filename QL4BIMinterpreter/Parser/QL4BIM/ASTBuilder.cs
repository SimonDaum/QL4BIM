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

        private ContextSwitch CurrentContextSwitch { get; set; }

        private void Parser_ContextChanged(object sender, ContextChangedEventArgs e)
        {
            Console.WriteLine(e.ToString());

            contextSwitch?.TearDownContext();

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
                    contextSwitch = new StatementContextSwitch() {ParentFuncNode = currentFunctionNode};
                    contextSwitch.AddNode("Statement", ParserParts.NullPart);
                    break;


                case ParserContext.Variable:
                    contextSwitch = new ParseVariableContextSwitch() {ParentFuncNode = currentFunctionNode};
                    break;

                case ParserContext.Operator:
                    contextSwitch = new OperatorContextSwitch() {ParentFuncNode = currentFunctionNode};
                    break;

                case ParserContext.Argument:
                    if (!(contextSwitch is ParseArgumentContextSwitch))
                        contextSwitch = new ParseArgumentContextSwitch() {ParentFuncNode = currentFunctionNode};
                    break;

                case ParserContext.AttPredicate:
                    contextSwitch = new AttributePredicateSwitch() { ParentFuncNode = currentFunctionNode };
                    break;

                case ParserContext.ArgumentEnd:
                    //contextSwitch?.TearDownContext();
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


        private ContextSwitch contextSwitch
        {
            get { return contextSwitch1; }
            set
            {
                contextSwitch1 = value; 
                contextSwitch1.InitContext();
            }
        }

        private FunctionNode currentFunctionNode = null;
        private ContextSwitch contextSwitch1;


        abstract class ContextSwitch
        {
            public FunctionNode ParentFuncNode { get; set; }

            public abstract void AddNode(string value, ParserParts parserPart);

            public abstract void TearDownContext();
            public virtual void InitContext() {}

            public void AddArgument(Node node)
            {
                ParentFuncNode.LastStatement.Arguments.Add(node);
            }

        }

        class GlobalBlockContextSwitch : ContextSwitch
        {
               
            public FunctionNode GlobalFunctionNode { get; private set; }

            public override void AddNode(string value, ParserParts parserPart)
            {
                GlobalFunctionNode = new FunctionNode(value);
            }

            public override void TearDownContext() {}


        }

        class StatementContextSwitch : ContextSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                ParentFuncNode.AddStatement(new StatementNode());
            }

            public override void TearDownContext() { }


        }

        class OperatorContextSwitch : ContextSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                if( parserPart != ParserParts.Operator)
                    throw new ArgumentOutOfRangeException(nameof(parserPart), parserPart, null);
                
                ParentFuncNode.LastStatement.SetOperator(value);
            }

            public override void TearDownContext() { }


        }

        abstract class PredicateSwitch : SetRelContextSwitch
        {
            public ParserParts Compare { get; set; }

            public AttributeAccessNode AttributeAccessNodeStart { get;  set; }
            public Node AttributeAccessNodeEnd { get;  set; }
        }

        class AttributePredicateSwitch : PredicateSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {   
                    case ParserParts.EqualsPred:
                        Compare = parserPart;
                        return;
                    case ParserParts.InPred:
                        Compare = parserPart;
                        return;
                    case ParserParts.MorePred:
                        Compare = parserPart;
                        return;
                    case ParserParts.MoreEqualPred:
                        Compare = parserPart;
                        return;
                    case ParserParts.LessPred:
                        Compare = parserPart;
                        return;
                    case ParserParts.LessEqualPred:
                        Compare = parserPart;
                        return;

                    case ParserParts.String:
                        AttributeAccessNodeEnd = new CStringNode(value);
                        return;
                    case ParserParts.Number:
                        AttributeAccessNodeEnd = new CNumberNode(value);
                        return;
                    case ParserParts.Float:
                        AttributeAccessNodeEnd = new CFloatNode(value);
                        return;
                    case ParserParts.Bool:
                        AttributeAccessNodeEnd = new CBoolNode(value);
                        return;

                    case ParserParts.SetRelArg: //ParserParts.SetRelArg
                        PrimeVariable = value;
                        break;
                    case ParserParts.RelAtt: //ParserParts.EmptyRelAtt
                        SecondaryVariable.Add(value);
                        break;
                    case ParserParts.ExAtt:
                        AttributeAccessNodeEnd = DoExAtt(value);
                        break;
                }
            }

            public override void TearDownContext()
            {
                if (AttributeAccessNodeStart == null)
                    return;

                var predicate = new PredicateNode(AttributeAccessNodeStart, Compare, AttributeAccessNodeEnd);
                AddArgument(predicate);

                AttributeAccessNodeStart = null;
            }

            public override void InitContext()
            {
                var lastArguement = PopLastArguement();
                if (!(lastArguement is AttributeAccessNode))
                    throw new ArgumentException();

                AttributeAccessNodeStart = lastArguement as AttributeAccessNode;
            }
        }

        abstract class SetRelContextSwitch : ContextSwitch
        {
            protected string PrimeVariable;
            protected readonly List<string> SecondaryVariable = new List<string>();


            public override void TearDownContext()
            {
                PrimeVariable = null;
                SecondaryVariable.Clear();

            }

            protected AttributeAccessNode DoExAtt(string value)
            {
                AttributeAccessNode attributeAccess = null;
                if (PrimeVariable != null && SecondaryVariable.Count == 0)
                    attributeAccess = new AttributeAccessNode(new SetNode(PrimeVariable), new ExAttNode(value));
                else
                    attributeAccess = new AttributeAccessNode(
                        new RelAttNode(SecondaryVariable.First(), PrimeVariable), new ExAttNode(value));

                PrimeVariable = null;
                SecondaryVariable.Clear();

                return attributeAccess;
            }

            protected Node PopLastArguement()
            {
                var argCount = ParentFuncNode.LastStatement.Arguments.Count;
                var lastArg = ParentFuncNode.LastStatement.Arguments.LastOrDefault();

                if(lastArg == null)
                    throw new ArgumentException("Argument is missing");

                ParentFuncNode.LastStatement.Arguments.RemoveAt(argCount - 1);
                return lastArg;
            }
        }

        class ParseVariableContextSwitch : SetRelContextSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.SetRelVar: 
                        PrimeVariable = value;
                        break;
                    case ParserParts.RelAtt: 
                        SecondaryVariable.Add(value);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public override void TearDownContext()
            {
                if (SecondaryVariable.Count == 0)
                    ParentFuncNode.LastStatement.SetSetReturn(new SetNode(PrimeVariable));
                else
                {
                    var relation = new RelationNode(PrimeVariable);
                    relation.Attributes = SecondaryVariable.ToList();
                    ParentFuncNode.LastStatement.SetRelationReturn(relation);
                }

                base.TearDownContext();
            }

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

                        var predecessorArg = PopLastArguement();
                        if (predecessorArg == null)
                            throw new ArgumentException("a relational Attribute can only be used after a relational Argument.");

                        if (!String.IsNullOrEmpty(PrimeVariable) && PrimeVariable != predecessorArg.Value)     
                            throw new ArgumentException("a relational Attribute can only be used after a relational Argument with the same Name.");
                            
                        if (!(predecessorArg is SetNode))
                            throw new ArgumentException("a relational Attribute can only be used after a relational Argument.");
                            
                        var relNameNode = new RelNameNode(predecessorArg.Value);
                        AddArgument(relNameNode);

                        break;
                    //varX =  Op1(var1, [arg1] -> var1 ist kein SetNode und muss relname node werden
                    case ParserParts.ExAtt:
                        AddArgument(DoExAtt(value));

                        break;

                    //Type Predicate
                    case ParserParts.ExType:
                        TypePredNode typePredNode = null;
                        if (PrimeVariable != null && SecondaryVariable.Count == 0)
                            typePredNode = new TypePredNode(new SetNode(PrimeVariable), value);
                        else
                            typePredNode = new TypePredNode(
                                new RelAttNode(SecondaryVariable.First(), PrimeVariable), value);
                        AddArgument(typePredNode);
                        PrimeVariable = null;
                        SecondaryVariable.Clear();
                        break;

                    //constants
                    case ParserParts.String: 
                        ParentFuncNode.LastStatement.AddArgument(new CStringNode(value));
                        break;
                    case ParserParts.Number: 
                        ParentFuncNode.LastStatement.AddArgument(new CNumberNode(value));
                        break;
                    case ParserParts.Float: 
                        ParentFuncNode.LastStatement.AddArgument(new CFloatNode(value));
                        break;
                    case ParserParts.Bool: 
                        ParentFuncNode.LastStatement.AddArgument(new CBoolNode(value));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }


            public override void TearDownContext()
            {
                //if (PrimeVariable == null && SecondaryVariable.Count == 0)
                //throw  new ArgumentException("PrimeVariable = null and SecondaryVariable.Count = 0");

                //set
                if (PrimeVariable != null && SecondaryVariable.Count == 0)
                {
                    AddArgument(new SetNode(PrimeVariable));
                }

                //rel
                if (SecondaryVariable.Count > 0)
                {
                    var attribut = SecondaryVariable.First();
                    var relAttNode = new RelAttNode(attribut, PrimeVariable);
                    AddArgument(relAttNode);
                }

                base.TearDownContext();

            }

        }
    }
}