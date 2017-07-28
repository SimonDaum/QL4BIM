using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    public interface IP21Reader
    {
        QLEntity[] LoadIfcFile(string ifcFilename);
        string[] GetAllSubtypNames(string typeName);
    }
}