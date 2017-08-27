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
