using System;
using System.Collections.Generic;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IOperatorValidator
    {
        string Name { get; }
        void Validate(SymbolTable symbolTable, StatementNode statement, List<List<string>> allAttributes);
    }

    public interface IArgumentFilterValidator : IOperatorValidator
    {
    }

    public interface IImportModelValidator : IOperatorValidator
    {
    }

    public interface ITypeFilterValidator : IOperatorValidator
    {
    }

    public interface IProjectorValidator : IOperatorValidator
    {
    }

    public interface IDereferencerValidator : IOperatorValidator
    {
    }

    public interface IDeaccociaterValidator : IOperatorValidator
    {
    }

    public interface IPropertyFilterValidator : IOperatorValidator
    {
    }
    public interface ISpatialTopoValidator : IOperatorValidator
    {
        string[] TopoOperators { get; }
    }
    public interface ITaskTimerValidator : IOperatorValidator
    {
    }

    public interface IMaximumValidator : IOperatorValidator
    {
    }
}