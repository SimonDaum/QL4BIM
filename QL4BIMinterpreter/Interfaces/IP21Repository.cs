using System.Collections.Generic;
using QL4BIMinterpreter.P21;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IP21Repository
    {

        QLProperty InverseProperty(string className, int index);

        int InversePropertyIndex(string className, string propName);

        int InversePropertyCount(string className);

        QLProperty DirectProperty(string className, int index);

        int DirectPropertyIndex(string className, string propName);

        int DirectPropertyCount(string className);

        void SetProperties(string className, List<QLProperty> props);


        QLProperty[] DirectProperties(string className);
        QLProperty[] InverseProperties(string className);

        void Reset();
    }
}