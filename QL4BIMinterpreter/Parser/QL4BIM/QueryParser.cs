using System;
using System.IO;

namespace QL4BIMinterpreter.QL4BIM
{

    public class QueryReader : IQueryReader
    {
        private readonly IInterpreterRepository interpreterRepository;
        private Scanner scanner;
        private Parser parser;

        public QueryReader(IInterpreterRepository interpreterRepository)
        {
            this.interpreterRepository = interpreterRepository;
        }

        public void Reset()
        {

        }

        public FuncNode Parse(string queryText)
        {
            using (var stream = queryText.ToStream())
            {
                scanner = new Scanner(stream);
                parser = new Parser(scanner);
                parser.Parse();
            }

            Console.WriteLine("Query parser: " + parser.errors.count + " errors detected");
            if(parser.errors.count > 0)
                throw new QueryException("Errors in func statement(s): " + parser.errors.count);

            return parser.GlobalFunc;
        }


        
    }

    static class StreamExtension
    {
        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }


}
