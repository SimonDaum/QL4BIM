using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IAstBuilder
    {
        void RegisterParseEvent(Parser parser);
        FunctionNode GlobalBlock { get; }
    }
}