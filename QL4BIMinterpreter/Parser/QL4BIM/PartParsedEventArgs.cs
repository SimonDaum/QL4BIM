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

    public enum ParserParts { GlobalBlock, FuncDefBlock, Statement, Variable,
                              EmptyRelAtt, RelAtt, Expression, Operator, Arguement,
                              SetRelArg, 
                              ExType, ExAtt, NumericOrSetRelAtt, SetRelAttPredEnd, AttPredicate, CountPredicate,
                              Constant, String, Number, Float, Bool,
                              EqualsPred, InPred, MorePred, MoreEqualPred, LessPred, LessEqualPred,
                              SetRelFormalArg
    }

    public delegate void PartParsedEventHandler(object sender, PartParsedEventArgs e);
}
