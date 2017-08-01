using System;

namespace QL4BIMinterpreter.QL4BIM
{
    public class PartParsedEventArgs : EventArgs
    {
        public string CurrentToken { get; set; }
        public string LookAhead { get; set; }
        public ParserParts Context { get; set; }

        public PartParsedEventArgs(ParserParts context, string currentToken = "", string lookAhead = "")
        {
            CurrentToken = currentToken;
            LookAhead = lookAhead;
            Context = context;
        }

        public override string ToString()
        {
            var outString = "Context: " + Context;
            if (CurrentToken != String.Empty)
                outString += " " + CurrentToken;

            return outString;
        }
    }

    public enum ParserParts { GlobalBlock, FuncDefBlock, Statement,
                              VariableBegin, VariableEnd,
                              VariableEmptyRelAtt, VariableRelAtt, Expression, Operator, Arguement,
                              ArguementSetRelBegin, ArgumentRelSetEnd,
                              ExType, ExAtt, NumericOrSetRelAtt, SetRelAttPredEnd,
                              ArguementRelAtt, 
                              TypePrdicate, AttPredicate, CountPredicate,
                              Constant, String, Number, Float, Bool,
                              EqualsPred, InPred, MorePred, MoreEqualPred, LessPred, LessEqualPred,
                              SetRelFormalArg, DefOp, DefAlias
    }

    public delegate void PartParsedEventHandler(object sender, PartParsedEventArgs e);
}
