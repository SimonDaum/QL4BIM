using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public class ImportModelValidator : OperatorValidator, IImportModelValidator
    {
        public ImportModelValidator()
        {
            Name = "ImportModel";
            var sig1 = new FunctionSignatur(SyUseVal.Set, new []{SyUseVal.String}, null, null);
            FunctionSignaturs.Add(sig1);
        }


    }


}
