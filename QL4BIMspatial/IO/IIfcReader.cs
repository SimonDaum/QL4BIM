using System.Collections.Generic;

namespace QL4BIMspatial
{
    public interface IIfcReader
    {
        long[] LoadIfc(string path);
        void AddImport(List<TriangleMesh> meshes);
    }
}