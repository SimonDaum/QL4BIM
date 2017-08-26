using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMprimitives;
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
            if (symbols.ContainsKey(setNode.Value))
                throw new QueryException($"Symbol {setNode.Value} already present.");

            Symbol symbol = new SetSymbol(setNode);

            if(!symbols.ContainsKey(setNode.Value))
                symbols.Add(setNode.Value, symbol);

        }

        public void AddRelSymbol(RelationNode relationNode)
        {
            if(symbols.ContainsKey(relationNode.RelationName))
                throw new QueryException($"Symbol {relationNode.RelationName} already present.");

            Symbol symbol = new RelationSymbol(relationNode);

            if (!symbols.ContainsKey(relationNode.RelationName))
                symbols.Add(relationNode.RelationName, symbol);

        }

        public SetSymbol GetSetSymbol(SetNode node)
        {   
            return (SetSymbol)symbols[node.Value];
        }

        public RelationSymbol GetRelationSymbol(RelNameNode node)
        {
            return (RelationSymbol) symbols[node.Value];
        }

        public RelationSymbol GetRelationSymbol(RelationNode node)
        {
            return (RelationSymbol)symbols[node.RelationName];
        }

    }
}
