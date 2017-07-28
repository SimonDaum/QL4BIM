namespace QL4BIMinterpreter
{
    public class TaskTimerValidator : OperatorValidator, ITaskTimerValidator
    {
        public TaskTimerValidator()
        {
            Name = "TaskTimer";

            var sig1 = new FunctionSignatur(SyUseVal.Rel, new[] { SyUseVal.RelAtt }, null, null);

            FunctionSignaturs.Add(sig1);

        }
    }
}