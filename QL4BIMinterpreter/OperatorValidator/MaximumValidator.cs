namespace QL4BIMinterpreter
{
    public class MaximumValidator : OperatorValidator, IMaximumValidator
    {
        public MaximumValidator()
        {
            Name = "Maximum";

            var sig1 = new FunctionSignatur(SyUseVal.Rel, new []{ SyUseVal.RelAtt, SyUseVal.ExAtt}, null, null);
            FunctionSignaturs.Add(sig1);
        }
    }
}