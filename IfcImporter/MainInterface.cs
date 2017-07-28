using System;
using System.Collections.Generic;
using IFCViewerX86;

namespace IfcImporter
{
    public class MainInterface
    {

        public IList<Tuple<string, int[], double[]>> OpenIfcFile(string file)
        {
            if (Environment.Is64BitProcess)
            {
                var ifcViewerWrapperX64 = new IFCViewerWrapperX64();
                return ifcViewerWrapperX64.OpenIfcFile(file);
            }

            var ifcViewerWrapperX86 = new IFCViewerWrapperX86();
            return ifcViewerWrapperX86.OpenIfcFile(file);
        }

    }
}
