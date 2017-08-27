/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMinterpreter.

QL4BIMinterpreter is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMinterpreter is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMinterpreter. If not, see <http://www.gnu.org/licenses/>.

*/

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
        EmptyRelAtt, RelAttStr, RelAttIndex,
        SetRelArg, Operator,
        ExType, ExAtt, NumericOrSetRelAtt, SetRelAttPredEnd,
        Constant, String, Number, Float, Bool,
        EqualsPred, InPred, MorePred, MoreEqualPred, LessPred, LessEqualPred,
        SetRelFormalArg, SetRelFormalArgEnd, DefOp, DefAlias
    }

    public enum ParserContext
    {   
        GlobalBlock, UserFuncBlock,
        Statement,  Variable, Operator, 
        Argument, ArgumentEnd, SimpleSetRelParameter,
        TypePredicate, AttPredicate, CountPredicate 
    }

    public delegate void PartParsedEventHandler(object sender, PartParsedEventArgs e);
    public delegate void ContextChangedEventHandler(object sender, ContextChangedEventArgs e);
}
