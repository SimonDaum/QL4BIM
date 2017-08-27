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
using QL4BIMprimitives;

namespace QL4BIMinterpreter.QL4BIM
{

    public class QueryReader : IQueryReader
    {
        private readonly IInterpreterRepository interpreterRepository;
        private readonly IAstBuilder astBuilder;
        private Scanner scanner;
        private Parser parser;

        public QueryReader(IInterpreterRepository interpreterRepository, IAstBuilder astBuilder)
        {
            this.interpreterRepository = interpreterRepository;
            this.astBuilder = astBuilder;
        }

        public void Reset()
        {

        }

        public FunctionNode Parse(string queryText)
        {
            using (var stream = queryText.ToStream())
            {
                scanner = new Scanner(stream);
                parser = new Parser(scanner);
                astBuilder.RegisterParseEvent(parser);
                parser.Parse();
            }

            Console.WriteLine("Query parser: " + parser.errors.count + " errors detected");
            if(parser.errors.count > 0)
                throw new QueryException("Errors in func statement(s): " + parser.errors.count);

            return astBuilder.GlobalFunctionNode;
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
