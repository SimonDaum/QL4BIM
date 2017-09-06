/*
Copyright (c) 2017 RDF Ltd. and Chair of Computational Modeling and Simulation at TUM

RDF Ltd., 1320 Bankya, 
P.O. Box 32, 
Bulgaria
contact@rdf.bg 

Chair of Computational Modeling and Simulation, 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of IfcImporter.

IfcImporter is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

IfcImporter is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with IfcImporter. If not, see <http://www.gnu.org/licenses/>.
*/

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
