/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMinterpreter.

QL4BIMinterpreter is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMinterpreter is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMinterpreter. If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMprimitives;

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

        }

        public SymbolTable GlobalSymbolTable { get;  set; }

        public void AddValidator(IOperatorValidator operatorValidator)
        {
            operatorValidators.Add(operatorValidator.Name, operatorValidator);
        }

    }



    

}


