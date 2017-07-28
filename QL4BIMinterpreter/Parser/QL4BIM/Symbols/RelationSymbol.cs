using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMinterpreter.P21;

namespace QL4BIMinterpreter.QL4BIM
{
    public class RelationSymbol : Symbol
    {
        private readonly List<String> attributes;
        private readonly int attributeCount;
        private  List<QLEntity[]> tuples;



        public RelationSymbol(CompLitNode node) : base(node)
        {
            attributes = node.Literals.ToList();
            attributeCount = attributes.Count;
            tuples = new List<QLEntity[]>();
            Header = "Relation->";
        }

        public void AddTuple(QLEntity[] tuple)
        {
            if(tuple.Length != attributeCount)
                throw new ArgumentException();

            tuples.Add(tuple);
        }

        public void SetTuples(IEnumerable<QLEntity[]> entityTuples)
        {
            tuples = entityTuples.ToList();

            if(tuples.FirstOrDefault() != null && tuples[0].Length != attributeCount)
                throw new ArgumentException();
        }

        public sealed override IEnumerable<QLEntity[]> Tuples => tuples.ToArray();
        public override List<string> Attributes => attributes;

        public override bool IsEmpty => tuples.Count == 0;
        public int? Index { get; set; }

        public override void Reset()
        {
            tuples.Clear();
        }

        public int EntityCount => tuples.Count;
    }
}