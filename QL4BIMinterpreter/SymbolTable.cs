using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMspatial;

namespace QL4BIMinterpreter
{
    public class SymbolTable
    {
        public string Name { get;  set; }
        private readonly Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

        
        public Dictionary<string, Symbol> Symbols => symbols;

        public SymbolTable()
        {
            
        }

        public SymbolTable(SymbolTable symbolTable)
        {
            symbols = symbolTable.symbols;
        }

        public bool Contains(string symbolName)
        {
            return symbols.ContainsKey(symbolName);
        }

        public virtual void Reset()
        {
            foreach (var symbol in Symbols)
            {
                symbol.Value.Reset();
            }
            Symbols.Clear();
        }

        public void AddSetSymbol(SetNode setNode)
        {
            Symbol symbol = new SetSymbol(setNode);

            symbols.Add(setNode.Value, symbol);

        }

        public void AddRelSymbol(RelationNode relationNode)
        {
            Symbol symbol = new RelationSymbol(relationNode);

            symbols.Add(relationNode.Value, symbol);

        }

        public SetSymbol GetSetSymbol(SetNode node)
        {
            return (SetSymbol)symbols[node.Value];
        }

        public RelationSymbol GetRelationSymbol(RelationNode node)
        {
            return (RelationSymbol) symbols[node.Value];
        }

        public IEnumerable<int> GetIndices(IEnumerable<SetNode> nodes)
        {   
            foreach (var literalNode in nodes)
            {
                var statementNode = (StatementNode)literalNode.Parent;
                //dont check me
                statementNode = statementNode.Previous;
                while (statementNode != null)
                {
                    var indexOfAttribute = statementNode.ReturnRelationNode.Attributes.IndexOf(literalNode.Value);
                    if (indexOfAttribute != -1)
                        yield return indexOfAttribute;
                    statementNode = statementNode.Previous;
                }

                throw new InvalidOperationException();
            }


        }

        public RelationSymbol GetNearestRelationSymbolFromAttribute(Node node)
        {
            var statementNode = (StatementNode) node.Parent;

            //dont check me
            statementNode = statementNode.Previous;

            while (statementNode != null)
            {
                var indexOfAttribute = statementNode.ReturnRelationNode.Attributes.IndexOf(node.Value);
                if (indexOfAttribute != -1)
                {
                    var symbol = GetRelationSymbol(statementNode.ReturnRelationNode);
                    symbol.Index = indexOfAttribute;
                    return symbol;
                }


                statementNode = statementNode.Previous;
            }

            return null;
        }





    }
}
