using System.Collections.Generic;

namespace QL4BIMinterpreter.P21
{
    public class P21InheritanceNode
    {
        private readonly List<P21InheritanceNode> children;

        public bool IsAbstract { get; set; }

        public P21InheritanceNode(string name, bool isAbstract)
        {
            IsAbstract = isAbstract;
            Name = name;
            children = new List<P21InheritanceNode>();
        }

        public string Name { get; }

        public IList<P21InheritanceNode> Subtypes => children.ToArray();

        public P21InheritanceNode Parent { get; set; }

        public void AddSubtype(P21InheritanceNode child)
        {
            children.Add(child);
            child.Parent = this;
        }
    }
}
