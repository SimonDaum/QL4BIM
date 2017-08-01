using System;

namespace QL4BIMinterpreter.QL4BIM
{
    public class PartParsedEventArgs : EventArgs
    {
        public string CurrentToken { get; set; }
        public ParserContext Context { get; set; }
        public ParserParts ParsePart { get; set; }

        public PartParsedEventArgs(ParserParts parsePart, ParserContext context, string currentToken = "")
        {
            CurrentToken = currentToken;
            Context = context;
            ParsePart = parsePart;
        }

        public override string ToString()
        {
            var outString = "ParsePart: " + ParsePart;
            if (CurrentToken != String.Empty)
                outString += " " + CurrentToken;

            return outString;
        }
    }

    public enum ParserParts {
        NoChange,
        SetRelVar,
        EmptyRelAtt, RelAtt, 
        SetRelArg,
        ExType, ExAtt, NumericOrSetRelAtt, SetRelAttPredEnd,
        Constant, String, Number, Float, Bool,
        EqualsPred, InPred, MorePred, MoreEqualPred, LessPred, LessEqualPred,
        SetRelFormalArg, DefOp, DefAlias
    }

    public enum ParserContext
    {   
        NoChange,
        GlobalBlock, FuncDefBlock,
        Statement,  Variable, Operator,
        Argument, SimpleSetRelParameter,
        TypePrdicate, AttPredicate, CountPredicate 
    }

    public delegate void PartParsedEventHandler(object sender, PartParsedEventArgs e);
}
