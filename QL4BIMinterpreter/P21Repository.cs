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
