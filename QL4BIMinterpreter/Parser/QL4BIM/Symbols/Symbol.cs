using System.Collections.Generic;
using QL4BIMinterpreter.P21;

namespace QL4BIMinterpreter.QL4BIM
{
    public abstract class Symbol
    {
        protected Node node;
        public string Value { get; protected set; }

        public  string Header { get; protected set; }
        public abstract IEnumerable<QLEntity[]> Tuples{ get; }

        public abstract List<string> Attributes { get; }

        public Node Node => node;

        public abstract bool IsEmpty { get; }

        public abstract void Reset();

        protected Symbol(Node node)
        {
            this.node = node;
            Value = node.Value;
        }
    }
}