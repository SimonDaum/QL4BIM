using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMprimitives;
using StrInt = System.Tuple<string, int?>;
namespace QL4BIMinterpreter.QL4BIM
{
    public class AstBuilder : IAstBuilder
    {
        public void RegisterParseEvent(Parser parser)
        {
            parser.PartParsed += ParserOnPartParsed;
            parser.ContextChanged += Parser_ContextChanged;
        }

        public FunctionNode GlobalFunctionNode => globalFunctionNode; //todo user func


        private const string GlobalFunctionId = "GlobalFunction";

        private void Parser_ContextChanged(object sender, ContextChangedEventArgs e)
        {
            //Console.WriteLine(e.ToString());

            contextSwitch?.TearDownContext();

            switch (e.Context)
            {
                case ParserContext.GlobalBlock:
                    globalContextSwitch = new FunctionContextSwitch(this);
                    globalContextSwitch.AddNode(GlobalFunctionId, ParserParts.DefOp);
                    break;

                case ParserContext.UserFuncBlock:
                    contextSwitch = new FunctionContextSwitch(this);
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
                    break;

                case ParserContext.Argument:
                    if (!(contextSwitch is ParseArgumentContextSwitch))
                        contextSwitch = new ParseArgumentContextSwitch() { ParentFuncNode = currentFunctionNode };
                    break;

                case ParserContext.AttPredicate:
                    contextSwitch = new AttributePredicateSwitch() { ParentFuncNode = currentFunctionNode };
                    break;

                case ParserContext.CountPredicate:
                    contextSwitch = new CountPredicateContextSwitch() { ParentFuncNode = currentFunctionNode };
                    break;
            }
        }

        private void ParserOnPartParsed(object sender, PartParsedEventArgs e)
        {
            //Console.WriteLine('\t' + e.ToString());
            contextSwitch.AddNode(e.CurrentToken, e.ParsePart);
        }


        private FunctionContextSwitch globalContextSwitch;


        private ContextSwitch contextSwitch
        {
            get { return contextSwitch1; }
            set
            {
                contextSwitch1 = value; 
                contextSwitch1.InitContext();
            }
        }

        private FunctionNode globalFunctionNode;
        private FunctionNode currentFunctionNode;

        private ContextSwitch contextSwitch1;


        abstract class ContextSwitch
        {
            public FunctionNode ParentFuncNode { get; set; }

            public abstract void AddNode(string value, ParserParts parserPart);

            public abstract void TearDownContext();
            public virtual void InitContext() {}

            protected void AddArgument(Node node)
            {
                ParentFuncNode.LastStatement.Arguments.Add(node);
            }

            protected Node PopLastArgument(bool skipIfRelPresent)
            {
                if (skipIfRelPresent)
                {
                    var revertedArgs = ParentFuncNode.LastStatement.Arguments.Reverse();
                    var relName = revertedArgs.FirstOrDefault(a => a is RelNameNode);
                    if (relName != null)
                        return null;
                }

                var argCount = ParentFuncNode.LastStatement.Arguments.Count;
                var lastArg = ParentFuncNode.LastStatement.Arguments.LastOrDefault();

                if (lastArg == null)
                    throw new ArgumentException("Argument is missing");

                ParentFuncNode.LastStatement.Arguments.RemoveAt(argCount - 1);
                return lastArg;
            }
        }

        class FunctionContextSwitch : ContextSwitch
        {
            private AstBuilder astBuilder;

            private List<Node> Arguments = new List<Node>();

            private string primeVariable;
            private readonly List<StrInt> secondaryVariable = new List<StrInt>();

            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.DefOp:
                        if (GlobalFunctionId == value)
                        {   
                            astBuilder.globalFunctionNode = new FunctionNode(value);
                            astBuilder.currentFunctionNode = astBuilder.globalFunctionNode;
                        }
                        else
                        {
                            var userFuncNode = new UserFunctionNode(value);
                            astBuilder.globalFunctionNode.UserFunctions.Add(userFuncNode);
                            astBuilder.currentFunctionNode = userFuncNode;
                        }
                        break;
                    case ParserParts.DefAlias:
                        if(!value.StartsWith(":"))
                            throw new ArgumentException("Alias does not start with :");
                        ((UserFunctionNode)astBuilder.currentFunctionNode).Alias = value.Substring(1, value.Length-1);
                        break;
                    case ParserParts.SetRelFormalArg:
                        //((UserFunctionNode)astBuilder.currentFunctionNode).AddArguement(); = value;
                        break;
                    case ParserParts.SetRelArg:
                        primeVariable = value;
                        break;
                    case ParserParts.RelAttStr:
                        var strInt = new StrInt(value, null);
                        secondaryVariable.Add(strInt);
                        break;
                    case ParserParts.RelAttIndex:
                        var strInt2 = new StrInt(null, int.Parse(value)-1);
                        secondaryVariable.Add(strInt2);
                        break;
                    case ParserParts.SetRelFormalArgEnd:
                        if (secondaryVariable.Count == 0)
                            ((UserFunctionNode)astBuilder.currentFunctionNode).AddArguement(new SetNode(primeVariable));
                        else
                        {
                            var relation = new RelationNode(primeVariable); //todo numeric index bzw empty args
                            relation.Attributes = secondaryVariable.Select(t => t.Item1).ToList();
                            ((UserFunctionNode)astBuilder.currentFunctionNode).AddArguement(relation);
                        }

                        primeVariable = null;
                        secondaryVariable.Clear();
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                
   
            }

            public override void TearDownContext() {}

            public FunctionContextSwitch(AstBuilder astBuilder)
            {
                this.astBuilder = astBuilder;
            }
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

        class AttributePredicateSwitch : SetRelContextSwitch
        {
            private ParserParts Compare { get; set; }
            private AttributeAccessNode AttributeAccessNodeStart { get; set; }
            private Node AttributeAccessNodeEnd { get; set; }

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

                    case ParserParts.SetRelArg: 
                        PrimeVariable = value;
                        break;
                    case ParserParts.RelAttStr:
                        SecondaryVariable = GetIndexOfAttribute(value);
                        break;
                    case ParserParts.RelAttIndex:
                        SecondaryVariable =  FromAttIndexToNetIndex(value);
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
                var lastArguement = PopLastArgument(false);
                if (!(lastArguement is AttributeAccessNode))
                    throw new ArgumentException();

                AttributeAccessNodeStart = lastArguement as AttributeAccessNode;
            }
        }

        abstract class SetRelContextSwitch : ContextSwitch
        {
            protected string PrimeVariable;
            protected int? SecondaryVariable;


            public override void TearDownContext()
            {
                PrimeVariable = null;
                SecondaryVariable = null;
            }

            protected int GetIndexOfAttribute(string value)
            {
                var statement = ParentFuncNode.LastStatement;
                var arguments = statement.Arguments.ToArray();
                var revArguments = arguments.Reverse().ToList();

                var relName = (RelNameNode) revArguments.First(a => a is RelNameNode);

                var allRelReturns = new List<RelationNode>();
                var funcStat = ParentFuncNode.FirstStatement;
                while (funcStat != null)
                {
                    var relReturn = funcStat.ReturnRelationNode;
                    if (relReturn != null)
                        allRelReturns.Add(relReturn);
                    funcStat = funcStat.Next;
                }
            
                var relSymbol = allRelReturns.First(r => String.Compare(r.RelationName, relName.Value, StringComparison.InvariantCultureIgnoreCase) == 0 );
                var index = relSymbol.Attributes.FindIndex(a => String.Compare(a, value, StringComparison.InvariantCultureIgnoreCase) == 0);
                return index;
            }

            protected AttributeAccessNode DoExAtt(string value)
            {
                AttributeAccessNode attributeAccess = null;
                if (PrimeVariable != null && !SecondaryVariable.HasValue)
                    attributeAccess = new AttributeAccessNode(new SetNode(PrimeVariable), new ExAttNode(value));
                else if (SecondaryVariable.HasValue)
                {
                    attributeAccess =  new AttributeAccessNode(new RelAttNode(SecondaryVariable.Value, PrimeVariable), new ExAttNode(value));
                }


                PrimeVariable = null;
                SecondaryVariable= null;

                return attributeAccess;
            }


            protected static int FromAttIndexToNetIndex(string value)
            {
                return int.Parse(value)-1;
            }
        }

        class ParseVariableContextSwitch : ContextSwitch
        {
            public const string EmptyRelAtts = "[]";

            private string primeVariable;
            private readonly List<string> secondaryVariable = new List<string>();

            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.SetRelVar: 
                        primeVariable = value;
                        break;
                    case ParserParts.RelAttStr: 
                        secondaryVariable.Add(value);
                        break;
                    case ParserParts.EmptyRelAtt:
                        secondaryVariable.Add(EmptyRelAtts);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public override void TearDownContext()
            {
                if (secondaryVariable.Count == 0)
                    ParentFuncNode.LastStatement.SetSetReturn(new SetNode(primeVariable));
                else
                {
                    var relation = new RelationNode(primeVariable);
                    if (secondaryVariable.Count == 1 && secondaryVariable[0] == EmptyRelAtts)
                        relation.IsEmptyArgs = true;
                    else
                        relation.Attributes = secondaryVariable.ToList();
                    ParentFuncNode.LastStatement.SetRelationReturn(relation);
                }

                primeVariable = null;
                secondaryVariable.Clear();
            }

        }

        class ParseArgumentContextSwitch : SetRelContextSwitch
        {
            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.SetRelArg: 
                        PrimeVariable = value;
                        break;
                    case ParserParts.RelAttStr:
                        PromoteSetArgument();
                        SecondaryVariable = GetIndexOfAttribute(value);
                        break;
                    case ParserParts.RelAttIndex:
                        PromoteSetArgument();
                        SecondaryVariable = FromAttIndexToNetIndex(value);
                        break;

                    case ParserParts.ExAtt:
                        AddArgument(DoExAtt(value));
                        break;

                    //Type Predicate
                    case ParserParts.ExType:
                        TypePredNode typePredNode = null;
                        if (PrimeVariable != null && !SecondaryVariable.HasValue)
                            typePredNode = new TypePredNode(new SetNode(PrimeVariable), value);
                        else if (SecondaryVariable.HasValue)
                        {
                            var relAttNode  = new RelAttNode(SecondaryVariable.Value, PrimeVariable);
                            typePredNode = new TypePredNode(relAttNode, value);
                        }                     
                        AddArgument(typePredNode);
                        PrimeVariable = null;
                        SecondaryVariable = null;
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

            private void PromoteSetArgument()
            {
                var predecessorArg = PopLastArgument(true);
                if (predecessorArg == null)
                    return;

                if (!String.IsNullOrEmpty(PrimeVariable) && PrimeVariable != predecessorArg.Value)
                    throw new QueryException("a relational Attribute can only be used after a relational Argument with the same Name.");

                if (!(predecessorArg is SetNode))
                    throw new QueryException("a relational Attribute can only be used after a relational Argument.");

                var relNameNode = new RelNameNode(predecessorArg.Value);
                AddArgument(relNameNode);
            }


            public override void TearDownContext()
            {
                //set
                if (PrimeVariable != null && !SecondaryVariable.HasValue)
                {
                    AddArgument(new SetNode(PrimeVariable));
                }
                //rel
                else if (SecondaryVariable.HasValue)
                {
                    var relAttNode = new RelAttNode(SecondaryVariable.Value, PrimeVariable);
                    AddArgument(relAttNode);
                }
                base.TearDownContext();
            }

        }

        class CountPredicateContextSwitch : ContextSwitch
        {
            private ParserParts compare;
            private RelAttNode relAttNode;
            private CNumberNode cNumberNode;

            public override void InitContext()
            {
                var predecessor = PopLastArgument(false);

                if(!(predecessor is RelAttNode))
                    throw  new QueryException("predecessor arguement must be a RelAtt.");

                relAttNode = predecessor as RelAttNode;
            }

            public override void AddNode(string value, ParserParts parserPart)
            {
                switch (parserPart)
                {
                    case ParserParts.EqualsPred:
                        compare = parserPart;
                        break;
                    case ParserParts.MorePred:
                        compare = parserPart;
                        break;
                    case ParserParts.MoreEqualPred:
                        compare = parserPart;
                        break;
                    case ParserParts.LessPred:
                        compare = parserPart;
                        break;
                    case ParserParts.LessEqualPred:
                        compare = parserPart;
                        break;
                    case ParserParts.Number:
                        cNumberNode = new CNumberNode(value);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public override void TearDownContext()
            {   
                if(relAttNode == null)
                    return;

                AddArgument(new PredicateNode(relAttNode, compare, cNumberNode));
                relAttNode = null;
            }
        }
    }
}