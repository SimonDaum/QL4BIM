using System;

namespace QL4BIMinterpreter.QL4BIM
{
    public class PartParsedEventArgs : EventArgs
    {
        public string CurrentToken { get; set; }
        public string LookAhead { get; set; }
        public string Context { get; set; }

        public PartParsedEventArgs(string context, string currentToken = "", string lookAhead = "")
        {
            CurrentToken = currentToken;
            LookAhead = lookAhead;
            Context = context;
        }

        public override string ToString()
        {
            return "Context: " + Context;
        }
    }

    public delegate void PartParsedEventHandler(object sender, PartParsedEventArgs e);
}
