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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMspatial;

namespace QL4BIMinterpreter.OperatorsLevel0
{
    public class ExportModelOperator : IExportModelOperator
    {   
        private readonly IInterpreterRepository interpreterRepository;
        private readonly IP21Reader p21Reader;
        private readonly IIfcReader ifcReader;
        private readonly ILogger logger;

        public ExportModelOperator(IInterpreterRepository interpreterRepository, IP21Reader p21Reader, IIfcReader ifcReader, ILogger logger)
        {
            this.interpreterRepository = interpreterRepository;
            this.p21Reader = p21Reader;
            this.ifcReader = ifcReader;
            this.logger = logger;
        }

        public void ExportModel(SetSymbol setSymbol, string path, SetSymbol returnSym)
        {   
            var sb = new StringBuilder();


            foreach (var entity in setSymbol.Entites)
            {
                sb.Append("#" + (entity.Id -1) + "=" + entity.ClassName + "(");
                var parts = entity.QLDirectList.List;
                var partList = new List<string>();
                FormatParts(parts, sb);
                sb.Append(");" + Environment.NewLine);
            }
           
            File.WriteAllText(path, sb.ToString());
        }

        private static void FormatParts(List<QLPart> parts, StringBuilder stringBuilder)
        {

            for (int i = 0; i< parts.Count; i++)
            {
                var c = i < parts.Count -1 ? "," : string.Empty;

                if (parts[i].QLEntityId != null)
                    stringBuilder.Append("#" + (parts[i].QLEntityId.Id -1)+ c);

                if (parts[i].QLNumber != null)
                    stringBuilder.Append(parts[i].QLNumber.Value + c);

                if (parts[i].QLString != null)
                    stringBuilder.Append("'" + parts[i].QLString.QLStr + "'" + c);

                if (parts[i].QLEnum != null)
                    stringBuilder.Append("." + parts[i].QLEnum.QLStr + "." + c);

                if (parts[i].IsNull)
                    stringBuilder.Append("$" + c);

                if (parts[i].IsEmptyList)
                    stringBuilder.Append("()" + c);

                if (parts[i].QLFloat.HasValue)
                    stringBuilder.Append(parts[i].QLFloat.Value.ToString("G0",CultureInfo.InvariantCulture) + c);

                if (parts[i].QLList != null)
                {
                    stringBuilder.Append("(");
                    FormatParts(parts[i].QLList.List, stringBuilder);
                    stringBuilder.Append(")" +  c);
                }
            }

        }
    }
}
