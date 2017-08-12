using System;

namespace QL4BIMinterpreter.QL4BIM
{
    public class ContextChangedEventArgs : EventArgs
    {
        public ParserContext Context { get; set; }

        public ContextChangedEventArgs(ParserContext context)
        {
            Context = context;
        }

        public override string ToString()
        {
            return "Current context: " + Context;
        }
    }

    public class PartParsedEventArgs : EventArgs
    {
        public string CurrentToken { get; set; }

        public ParserParts ParsePart { get; set; }

        public PartParsedEventArgs(ParserParts parsePart, string currentToken = "")
        {
            CurrentToken = currentToken;
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
        NullPart,
        SetRelVar,
        EmptyRelAtt, RelAtt, 
        SetRelArg, Operator,
        ExType, ExAtt, NumericOrSetRelAtt, SetRelAttPredEnd,
        Constant, String, Number, Float, Bool,
        EqualsPred, InPred, MorePred, MoreEqualPred, LessPred, LessEqualPred,
        SetRelFormalArg, DefOp, DefAlias
    }

    public enum ParserContext
    {   
        GlobalBlock, FuncDefBlock,
        Statement,  Variable, Operator, 
        Argument, ArgumentEnd, SimpleSetRelParameter,
        TypePrdicate, AttPredicate, CountPredicate 
    }

    public delegate void PartParsedEventHandler(object sender, PartParsedEventArgs e);
    public delegate void ContextChangedEventHandler(object sender, ContextChangedEventArgs e);
}
