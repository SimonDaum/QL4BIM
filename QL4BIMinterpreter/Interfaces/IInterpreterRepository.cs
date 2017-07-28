using System;
using System.Collections.Generic;
using QL4BIMinterpreter.P21;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IInterpreterRepository
    {

        IList<SymbolTable> SymbolTables { get; }
        void AddSymbolTable(SymbolTable symbolTable);

        IEnumerable<Symbol> AllSymbols { get; }

        string Query { get; set; }
        Dictionary<int, QLEntity> GlobalEntityDictionary { get; set; }
        SymbolTable GlobalSymbolTable { get; set; }
        IOperatorValidator GetOperatorValidator(string name);
        void AddValidator(IOperatorValidator operatorValidator);
    }
}