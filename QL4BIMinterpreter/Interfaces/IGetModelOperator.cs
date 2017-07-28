using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IImportModelOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym

        void ImportModel(string path, SetSymbol returnSym);
    }
}