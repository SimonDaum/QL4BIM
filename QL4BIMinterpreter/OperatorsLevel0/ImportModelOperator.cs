using System;
using System.IO;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;
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
