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
using System.IO;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMprimitives;
using QL4BIMspatial;

namespace QL4BIMinterpreter.OperatorsLevel0
{
    public class ImportModelOperator : IImportModelOperator
    {   
        private readonly IInterpreterRepository interpreterRepository;
        private readonly IP21Reader p21Reader;
        private readonly IIfcReader ifcReader;
        private readonly ILogger logger;

        public ImportModelOperator(IInterpreterRepository interpreterRepository, IP21Reader p21Reader, IIfcReader ifcReader, ILogger logger)
        {
            this.interpreterRepository = interpreterRepository;
            this.p21Reader = p21Reader;
            this.ifcReader = ifcReader;
            this.logger = logger;
        }

        public void ImportModel(string path, SetSymbol returnSym)
        {
            if (!File.Exists(path))
                throw new QueryException("File not found: " + path);

            if (true) //geo off
            {
                Console.WriteLine("ImportModel'ing... side effects->");
                Console.WriteLine("\tGenerating geometry...");

                //ifcEngineTiming, meshingTiming, meshes.Count
                var importTiming = ifcReader.LoadIfc(path);
                
                logger.AddEntry("ImportModelGeo", 0, (int)importTiming[2], importTiming[0]);
                logger.AddEntry("ImportModelRsTree", 0, (int)importTiming[2], importTiming[1]);

                Console.WriteLine("\t" + importTiming[2] + " geometry representations created.");
            }

            Console.WriteLine("\tGenerating semantic representations...");
            var qlEntities = p21Reader.LoadIfcFile(path);

            //add to global dictionary
            foreach (var entity in qlEntities)
                interpreterRepository.GlobalEntityDictionary.Add(entity.Id, entity);

            Console.WriteLine("\t" + qlEntities.Length + " object representations created.");
            Console.WriteLine("ImportModel'ing finished.");
            returnSym.EntityDic = qlEntities.ToDictionary(e => e.Id);
        }
    }
}
