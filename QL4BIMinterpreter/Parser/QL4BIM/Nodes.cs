using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace QL4BIMinterpreter.QL4BIM
{
    public abstract class Node
    {
        private static int _sid;

        protected Node()
        {
            _sid++;
            Id = _sid;
        }

        public override string ToString()
        {
            return this.GetType().Name + ": " + Id
                + (string.IsNullOrEmpty(Value) ? string.Empty : " : "
                + (Value.StartsWith("\"") ? Value.Substring(1, Value.Length - 1) : Value));
        }

        public virtual string Value { get; set; }

        public Node Parent { get; set; }

        public int Id { get; }

    }

    public sealed class FuncNode : Node
    {
        private string value;

        public FuncNode Copy()
        {
            var copy = new FuncNode();
            copy.Value = Value;
            copy.SymbolTable = new SymbolTable(SymbolTable);
            copy.Arguments = Arguments.ToList();
            copy.Next = Next;
            copy.Previous = Previous;
            copy.FirstStatement = FirstStatement;
            copy.LastStatement = LastStatement;

            return copy;
        }

        public SymbolTable SymbolTable { get; private set; }

        public IList<Node> Arguments { get; private set; } = new List<Node>();

        public void Accept(ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.Visit(this);
        }

        public FuncNode Next { get; private set; }
        public FuncNode Previous { get; private set; }


        public FuncNode(FuncNode previousNode = null)
        {
            SymbolTable = new SymbolTable();
      

            if (previousNode == null)
            {
                Value = "Global";
                SymbolTable.Name = Value;
                return;
            }

            SymbolTable.Name = Value;

            Previous = previousNode;
            previousNode.Next = this;
        }

        public override string Value
        {
            get { return value; }
            set
            {
                SymbolTable.Name = value;
                this.value = value;
            }
        }


        public StatementNode FirstStatement { get; private set; }

        public StatementNode LastStatement { get; private set; }

        public void AddStatement(StatementNode statementNode)
        {
            if (FirstStatement == null)
                FirstStatement = statementNode;

            if (LastStatement != null)
            {
                LastStatement.Next = statementNode;
                statementNode.Previous = LastStatement;
            }

            LastStatement = statementNode;
        }

        public void AddArgument(Node argument)
        {
            argument.Parent = this;
            Arguments.Add(argument);
        }
    }



    public sealed class OperatorNode : Node
    {
        public OperatorNode(string value)
        {
            Value = value;
        }
    }

    public class LiteralNode : Node
    {
        public LiteralNode(string value, bool isReturn)
        {
            this.IsReturn = isReturn;
            Value = value;
        }

        public bool IsReturn { get; }

        public override string ToString()
        {
            return "LiteralNode: " + Value;
        }

        public SymbolUsage Usage { get; set; }
        public enum SymbolUsage { Set, RelAtt }
    }

    public class CompLitNode : Node
    {
        public CompLitNode(string literal, bool isReturn)
        {
            AddLiteral(literal);
            this.IsReturn = isReturn;
        }

        public bool IsReturn { get; }

        public IList<string> Literals { get; } = new List<string>();

        public void AddLiteral(string literal)
        {
            Literals.Add(literal);
        }

        public override string Value => string.Join("-", Literals);

        public override string ToString()
        {
            return "CompLitNode: " + string.Join(" ", Literals);
        }
    }


    public class CStringNode : Node
    {
        public CStringNode(string value)
        {
            if (value.Count(c => c == '"') > 0 && !(value.StartsWith("\"") && value.EndsWith("\"")))
                throw new ArgumentException();

            if (value.StartsWith("\"") && value.EndsWith("\""))
                Value = value.Substring(1, value.Length - 2);
            else
                Value = value;
        }

        public override string ToString()
        {
            return "CStringNode: " + Value;
        }
    }

    public class ExAttNode : Node
    {
        public ExAttNode(string value)
        {
            if (!value.StartsWith("."))
                throw new ArgumentException();

            Value = value.Substring(1, value.Length - 1);

        }

        public override string ToString()
        {
            return "ExAttNode: " + Value;
        }
    }


    public class ExTypeNode : Node
    {
        public ExTypeNode(string value)
        {
            if (!value.StartsWith("#"))
                throw new ArgumentException();

            Value = value.Substring(1, value.Length - 1);

        }

        public override string ToString()
        {
            return "ExTypeNode: " + Value;
        }
    }



    public class CNumberNode : Node
    {
        public CNumberNode(string value)
        {
            Value = value;
            IntValue = int.Parse(value);
        }

        public int IntValue { get; }

        public override string ToString()
        {
            return "CNumberNode: " + Value;
        }
    }

    public class CFloatNode : Node
    {
        public CFloatNode(string value)
        {
            Value = value;
            FloatValue = double.Parse(value, CultureInfo.InvariantCulture);
        }

        public double FloatValue { get; }

        public override string ToString()
        {
            return "CNumberNode: " + Value;
        }
    }

    public class PredicateNode : Node
    {
        public string CompareToken { get; }

        public PredicateNode(ExAttNode attNode, string compareToken, CStringNode valueNode)
        {
            ValueStringNode = valueNode;
            AttNode = attNode;
            CompareToken = compareToken;
        }

        public PredicateNode(ExAttNode attNode, string compareToken, CNumberNode valueNode)
        {
            ValueNumberNode = valueNode;
            AttNode = attNode;
            CompareToken = compareToken;
        }

        public PredicateNode(ExAttNode attNode, string compareToken, CFloatNode valueNode)
        {
            ValueFloatNode = valueNode;
            AttNode = attNode;
            CompareToken = compareToken;
        }

        //todos second ExAttNode
        public ExAttNode AttNode { get; }
        public CStringNode ValueStringNode { get; }
        public CNumberNode ValueNumberNode { get; }
        public CFloatNode ValueFloatNode { get; }

        public Node FirstNode => AttNode;

        public Node SecondNode
        {
            get
            {
                if (ValueStringNode != null)
                    return ValueStringNode;

                if (ValueNumberNode != null)
                    return ValueNumberNode;

                if (ValueFloatNode != null)
                    return ValueFloatNode;

                throw new InvalidOperationException();
            }
        }


        public override string ToString()
        {
            if (ValueStringNode != null)
                return AttNode + " " + ValueStringNode;
            if (ValueNumberNode != null)
                return AttNode + " " + ValueNumberNode;
            if (ValueFloatNode != null)
                return AttNode + " " + ValueFloatNode;

            throw new InvalidOperationException();
        }

    }

    public sealed class StatementNode : Node
    {
        public static bool IsFunctioning;

        public StatementNode Next { get;  set; }
        public StatementNode Previous { get;  set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(" Operator: " + OperatorNode.Value);
            sb.Append(" ReturnLiteralNode.SymbolUsage : " + (ReturnLiteralNode != null));
            sb.Append(" Arguments.Count : " + Arguments.Count);
            sb.Append(string.Join(" ", Arguments));


            sb.Append(string.Join(" ", Predicate));

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ReturnLiteralNode != null ? ReturnLiteralNode.GetHashCode() : 0) * 397) ^
                    (OperatorNode != null ? OperatorNode.GetHashCode() : 0);
            }
        }

        private bool TypeEqual(Node node1, Node node2)
        {
            return (node1.GetType() == node2.GetType());
        }

        public StatementNode(FuncNode func)
        {
            Parent = func;
        }

        public OperatorNode OperatorNode { get; private set; }

        public void AddOperator(string operato)
        {
            OperatorNode = new OperatorNode(operato) {Parent = this};
        }

        public LiteralNode ReturnLiteralNode { get; set; }
        public CompLitNode ReturnCompLitNode { get; set; }



        public IList<Node> Arguments { get; } = new List<Node>();

        public PredicateNode Predicate { get; set; }

        public void AddArgument(Node argument)
        {

            if (IsFunctioning)
                return;


            argument.Parent = this;
            Arguments.Add(argument);
        }

        public void AddLiteralArgumentOrReturn(LiteralNode argument)
        {
            if (IsFunctioning)
                return;

            argument.Parent = this;
            if (argument.IsReturn)
            {
                ReturnLiteralNode = argument;
                return;
            }
            Arguments.Add(argument);
        }


        public void AddCompLitArgumentOrReturn(CompLitNode argument)
        {
            if (IsFunctioning)
                return;

            argument.Parent = this;
            if (argument.IsReturn)
            {
                ReturnCompLitNode = argument;
                return;
            }

            Arguments.Add(argument);
        }

        public void AddPredicate(ExAttNode exAttNode, string compareToken, CFloatNode floatNode)
        {
            if (IsFunctioning)
                return;

            var predicate = new PredicateNode(exAttNode, compareToken, floatNode);
            exAttNode.Parent = predicate;
            floatNode.Parent = predicate;
            Predicate = predicate;
            Predicate.Parent = this;
        }

        public void AddPredicate(ExAttNode exAttNode, string compareToken, CStringNode stringNode)
        {
            if (IsFunctioning)
                return;

            var predicate = new PredicateNode(exAttNode, compareToken, stringNode);
            exAttNode.Parent = predicate;
            stringNode.Parent = predicate;
            Predicate = predicate;
            Predicate.Parent = this;
        }

        public void AddPredicate(ExAttNode exAttNode, string compareToken, CNumberNode numberNode)
        {
            if (IsFunctioning)
                return;

            var predicate = new PredicateNode(exAttNode, compareToken, numberNode);
            exAttNode.Parent = predicate;
            numberNode.Parent = predicate;
            Predicate = predicate;
            Predicate.Parent = this;
        }

        public void Accept(IExecutionVisitor executionVisitor)
        {   
            executionVisitor.Visit(this);
        }

        public void Accept(ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.Visit(this);
        }

        public void Accept(IFuncVisitor funcVisitor)
        {
            funcVisitor.Visit(this);
        }
    }




}
