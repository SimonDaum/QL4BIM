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
using System.Collections.Generic;
using Microsoft.Practices.Unity.Utility;
using QL4BIMinterpreter.P21;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public class P21Repository : IP21Repository
    {
        //todo reset
        private Dictionary<string, Pair<List<QLProperty>, List<QLProperty>>> classNameToProps = new Dictionary<string, Pair<List<QLProperty>, List<QLProperty>>>();


        public QLProperty[] DirectProperties(string className)
        {
            return classNameToProps[className].First.ToArray();
        }

        public QLProperty[] InverseProperties(string className)
        {
            return classNameToProps[className].Second.ToArray();
        }

        public void Reset()
        {
            classNameToProps.Clear();
        }

        public QLProperty DirectProperty(string className, int index)
        {
            return classNameToProps[className].First[index];
        }

        public QLProperty InverseProperty(string className, int index)
        {
            return classNameToProps[className].Second[index];
        }

        public int DirectPropertyIndex(string className, string propName)
        {
            return classNameToProps[className].First.FindIndex(p => string.Compare(p.PropName, propName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public int InversePropertyIndex(string className, string propName)
        {
            return classNameToProps[className].Second.FindIndex(p => string.Compare(p.PropName, propName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public int InversePropertyCount(string className)
        {
            return classNameToProps[className].Second.Count;
        }

        public int DirectPropertyCount(string className)
        {
            return classNameToProps[className].First.Count;
        }


        public void SetProperties(string className, List<QLProperty> props)
        {
            if (classNameToProps.ContainsKey(className))
                return;

            classNameToProps.Add(className, new Pair<List<QLProperty>, List<QLProperty>>(new List<QLProperty>(), new List<QLProperty>()));
            var propsPair = classNameToProps[className];

            var directProperties = propsPair.First;
            var inverseProperties = propsPair.Second;

            foreach (var prop in props)
            {
                if (prop.Inverse)
                    inverseProperties.Add(prop);
                else
                    directProperties.Add(prop);
            }
        }
    }
}
