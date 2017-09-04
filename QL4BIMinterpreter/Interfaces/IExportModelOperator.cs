using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IExportModelOperator
    {
        void ExportModel(SetSymbol setSymbol, string path, SetSymbol returnSym);
    }
}