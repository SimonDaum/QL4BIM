using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface ISpatialQueryInterpreter
    {
        void Execute(string operatorName, RelationSymbol returnSymbol, SetSymbol parameterSymbol1, SetSymbol parameterSymbol2);
    }
}