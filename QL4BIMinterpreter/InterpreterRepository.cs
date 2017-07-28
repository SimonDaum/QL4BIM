using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{   
    public class InterpreterRepository : IInterpreterRepository
    {
        public IList<SymbolTable> SymbolTables => symbolTables;

        public void AddSymbolTable(SymbolTable symbolTable)
        {
            symbolTables.Add(symbolTable);
        }

        public IEnumerable<Symbol> AllSymbols
        {
            get { return symbolTables.SelectMany(t => t.Symbols.Select(p => p.Value)); }
        }

        public string Query { get; set; }

        public Dictionary<int, QLEntity> GlobalEntityDictionary { get; set; } = new Dictionary<int, QLEntity>();

        private readonly Dictionary<string, IOperatorValidator> operatorValidators = new Dictionary<string, IOperatorValidator>();
        private readonly IList<SymbolTable> symbolTables;


        public IOperatorValidator GetOperatorValidator(string name)
        {
            if(!operatorValidators.ContainsKey(name))
                throw new QueryException("Operator " +  name + " is not implemented");

            return operatorValidators[name];
        }

        public InterpreterRepository(IArgumentFilterValidator argumentFilterValidator, IImportModelValidator getModelValidator,
                                     ITypeFilterValidator typeFilterValidator, IDereferencerValidator dereferencerValidator,
                                     IProjectorValidator projectorValidator, IPropertyFilterValidator propertyFilterValidator,
                                     ISpatialTopoValidator spatialTopoValidator, IDeaccociaterValidator deaccociaterValidator,
                                     ITaskTimerValidator taskTimerValidator, IMaximumValidator maximumValidator)
        {
            AddValidator(argumentFilterValidator);
            AddValidator(getModelValidator);
            AddValidator(typeFilterValidator);
            AddValidator(dereferencerValidator);
            AddValidator(projectorValidator);
            AddValidator(propertyFilterValidator);
            AddValidator(deaccociaterValidator);
            AddValidator(taskTimerValidator);
            AddValidator(maximumValidator);

            foreach (var topoOperation in spatialTopoValidator.TopoOperators)
                operatorValidators.Add(topoOperation, spatialTopoValidator);

            //todo reset
            symbolTables = new List<SymbolTable>();

            GlobalSymbolTable = new SymbolTable() {Name = "Global"};
        }

        public SymbolTable GlobalSymbolTable { get;  set; }

        public void AddValidator(IOperatorValidator operatorValidator)
        {
            operatorValidators.Add(operatorValidator.Name, operatorValidator);
        }

    }

    class QueryException : Exception
    {
        public QueryException(string message) : base(message)
        {

        }
    }

    

}


