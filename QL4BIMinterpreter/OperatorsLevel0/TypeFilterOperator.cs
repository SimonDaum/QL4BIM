using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter.OperatorsLevel0
{
    public class TypeFilterOperator : ITypeFilterOperator
    {
        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym
        
        private readonly IP21Reader p21Reader;

        public TypeFilterOperator(IP21Reader p21Reader)
        {
            this.p21Reader = p21Reader;
        }

        public void TypeFilterSet(SetSymbol parameterSym1, string typeName, SetSymbol returnSym)
        {
            Console.WriteLine("TypeFilter'ing...");
            var allTypes = p21Reader.GetAllSubtypNames(typeName);
            var typedEntites = parameterSym1.EntityDic.Values.Where(e => allTypes.Any(s =>
                    string.Compare(s, e.ClassName, StringComparison.InvariantCultureIgnoreCase) == 0));
            returnSym.EntityDic = typedEntites.ToDictionary(e => e.Id);
        }

        public void TypeFilterRelation(RelationSymbol parameterSym1, string[] typeNames, RelationSymbol returnSym)
        {
            Console.WriteLine("TypeFilter'ing...");

            var tuples = parameterSym1.Tuples;
            var tuplesOut = GetTuples(tuples, typeNames);

            returnSym.SetTuples(tuplesOut);
        }

        public IEnumerable<QLEntity[]> GetTuples(IEnumerable<QLEntity[]> tuples, string[] typeNames)
        {
            var typeCount = typeNames.Length;

            for (int i = 0; i < typeCount; i++)
            {   
                if(typeNames[i] == string.Empty)
                    continue;

                var allTypes = p21Reader.GetAllSubtypNames(typeNames[i]);
                tuples = tuples.Where(e => allTypes.Any(s => string.Compare(s, e[i].ClassName,
                                                                 StringComparison.InvariantCultureIgnoreCase) == 0)).ToList();
            }
            return tuples;
        }
    }
}
