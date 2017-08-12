using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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

    public sealed class FunctionNode : Node
    {
        private string value;

        public FunctionNode Copy()
        {
            var copy = new FunctionNode(Value);
            copy.SymbolTable = new SymbolTable(SymbolTable);
            copy.FormalArguments = FormalArguments.ToList();
            copy.Next = Next;
            copy.Previous = Previous;
            copy.FirstStatement = FirstStatement;
            copy.LastStatement = LastStatement;

            return copy;
        }

        public SymbolTable SymbolTable { get; private set; }

        public IList<Node> FormalArguments { get; private set; } = new List<Node>();

        public void Accept(ISymbolVisitor symbolVisitor)
        {
            symbolVisitor.Visit(this);
        }

        public FunctionNode Next { get; private set; }
        public FunctionNode Previous { get; private set; }


        public FunctionNode(string value)
        {
            SymbolTable = new SymbolTable();
            SymbolTable.Name = value;
            Value = value;
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
            statementNode.Parent = this;
            LastStatement = statementNode;
        }


    }



    public sealed class OperatorNode : Node
    {
        public OperatorNode(string value)
        {
            Value = value;
        }
    }

    public sealed class TypePredNode : Node
    {
        public RelAttNode RelAttNode { get; set; }
        public SetNode SetNode { get; set; }

        public String Type { get; set; }

        public TypePredNode(RelAttNode relAttNode, string type)
        {
            RelAttNode = relAttNode;
            Type = type;
        }

        public TypePredNode(SetNode setNode, string type)
        {
            SetNode = setNode;
            Type = type;
        }

        public override string ToString()
        {
            if (SetNode != null)
                return "TypePredNode: " + SetNode.Value + " is " + Type;
            else
                return "TypePredNode: " + RelAttNode.ToShortString() + " is " + Type;
        }
    }

    public sealed class SetNode : Node
    {
        public SetNode(string value)
        {
            Value = value;
        }


        public override string ToString()
        {
            return "SetNode: " + Value;
        }

        public SymbolUsage Usage { get; set; }
        public enum SymbolUsage { Set, RelAtt }
    }

    public sealed class RelNameNode : Node
    {
        public RelNameNode(string value)
        {
            Value = value;
        }


        public override string ToString()
        {
            return "RelNameNode: " + Value;
        }
    }

    public class RelAttNode : Node
    {
        public string Attribute { get; set; }
        public string RelationName { get; set; }

        public RelAttNode(string attribute, string relationName)
        {
            Attribute = attribute;
            RelationName = relationName;
        }

        public override string ToString()
        {
            return "RelAttNode: " + ToShortString();
        }

        public string ToShortString()
        {
            return  RelationName + "[" + Attribute + "]";
        }
    }

    public class RelationNode : Node
    {
        public RelationNode(string literal)
        {
            RelationName = literal;
        }

        public IList<string> Attributes { get; set; }

        public string RelationName { get; private set; }

        public override string Value => RelationName +  "[" + string.Join("|", Attributes) + "]";

        public override string ToString()
        {
            return "RelationNode: " + Value;
        }
    }

    public sealed class CBoolNode : Node
    {
        public CBoolNode(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                Value = value.Substring(1, value.Length - 2);
            else
                Value = value;

            if ((Value != "false") && (Value != "true") && (Value != "unknown") )
                throw new ArgumentException();
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

    public sealed class ExAttNode : Node
    {
        public ExAttNode(string value)
        {

            Value = value;

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

    public class AttributeAccessNode : Node
    {
        public RelAttNode RelAttNode { get; set; }
        public SetNode SetNode { get; set; }
        public ExAttNode ExAttNode { get; set; }

        public AttributeAccessNode(RelAttNode relAttNode, ExAttNode exAttNode)
        {
            RelAttNode = relAttNode;
            ExAttNode = exAttNode;
        }

        public AttributeAccessNode(SetNode setNode, ExAttNode exAttNode)
        {
            SetNode = setNode;
            ExAttNode = exAttNode;
        }

        public override string ToString()
        {   
            if(SetNode != null)
                return "AttributeAccessNode: " + SetNode.Value +  "." + ExAttNode.Value;
            else
                return "AttributeAccessNode: " + RelAttNode.ToShortString() + "." + ExAttNode.Value;
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
            return "CFloatNode: " + Value;
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
        private int statementIndex;

        public StatementNode Next { get;  set; }
        public StatementNode Previous { get;  set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(" Operator: " + OperatorNode.Value);
            sb.Append(" ReturnSetNode.SymbolUsage : " + (ReturnSetNode != null));
            sb.Append(" FormalArguments.Count : " + Arguments.Count);
            sb.Append(string.Join(" ", Arguments));


            sb.Append(string.Join(" ", Predicate));

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ReturnSetNode != null ? ReturnSetNode.GetHashCode() : 0) * 397) ^
                    (OperatorNode != null ? OperatorNode.GetHashCode() : 0);
            }
        }

        private bool TypeEqual(Node node1, Node node2)
        {
            return (node1.GetType() == node2.GetType());
        }

        public OperatorNode OperatorNode { get; private set; }

        public void SetOperator(string @operator)
        {
            OperatorNode = new OperatorNode(@operator) {Parent = this};
        }

        public SetNode ReturnSetNode { get; set; }
        public RelationNode ReturnRelationNode { get; set; }



        public IList<Node> Arguments { get; } = new List<Node>();

        public PredicateNode Predicate { get; set; }

        public void AddArgument(Node argument)
        {
            argument.Parent = this;
            Arguments.Add(argument);
        }



        public void SetSetReturn(SetNode returnSetNode)
        {
            returnSetNode.Parent = this;
            ReturnSetNode = returnSetNode;
        }

        public void SetRelationReturn(RelationNode returnRelatioNode)
        {
            returnRelatioNode.Parent = this;
            ReturnRelationNode = returnRelatioNode;
        }

        public void AddPredicate(ExAttNode exAttNode, string compareToken, CFloatNode floatNode)
        {
            var predicate = new PredicateNode(exAttNode, compareToken, floatNode);
            exAttNode.Parent = predicate;
            floatNode.Parent = predicate;
            Predicate = predicate;
            Predicate.Parent = this;
        }

        public void AddPredicate(ExAttNode exAttNode, string compareToken, CStringNode stringNode)
        {
            var predicate = new PredicateNode(exAttNode, compareToken, stringNode);
            exAttNode.Parent = predicate;
            stringNode.Parent = predicate;
            Predicate = predicate;
            Predicate.Parent = this;
        }

        public void AddPredicate(ExAttNode exAttNode, string compareToken, CNumberNode numberNode)
        {
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
