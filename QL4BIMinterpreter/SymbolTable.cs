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

        public void AddSetSymbol(LiteralNode literalNode)
        {
            Symbol symbol = new SetSymbol(literalNode);

            symbols.Add(literalNode.Value, symbol);

        }

        public void AddRelSymbol(CompLitNode compLitNode)
        {
            Symbol symbol = new RelationSymbol(compLitNode);

            symbols.Add(compLitNode.Value, symbol);

        }

        public SetSymbol GetSetSymbol(LiteralNode node)
        {
            return (SetSymbol)symbols[node.Value];
        }

        public RelationSymbol GetRelationSymbol(CompLitNode node)
        {
            return (RelationSymbol) symbols[node.Value];
        }

        public IEnumerable<int> GetIndices(IEnumerable<LiteralNode> nodes)
        {   
            foreach (var literalNode in nodes)
            {
                var statementNode = (StatementNode)literalNode.Parent;
                //dont check me
                statementNode = statementNode.Previous;
                while (statementNode != null)
                {
                    var indexOfAttribute = statementNode.ReturnCompLitNode.Literals.IndexOf(literalNode.Value);
                    if (indexOfAttribute != -1)
                        yield return indexOfAttribute;
                    statementNode = statementNode.Previous;
                }

                throw new InvalidOperationException();
            }


        }

        public RelationSymbol GetNearestRelationSymbolFromAttribute(LiteralNode node)
        {
            var statementNode = (StatementNode) node.Parent;

            //dont check me
            statementNode = statementNode.Previous;

            while (statementNode != null)
            {
                var indexOfAttribute = statementNode.ReturnCompLitNode.Literals.IndexOf(node.Value);
                if (indexOfAttribute != -1)
                {
                    var symbol = GetRelationSymbol(statementNode.ReturnCompLitNode);
                    symbol.Index = indexOfAttribute;
                    return symbol;
                }


                statementNode = statementNode.Previous;
            }

            return null;
        }





    }
}
