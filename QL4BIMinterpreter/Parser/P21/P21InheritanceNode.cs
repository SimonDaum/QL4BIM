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
