using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMinterpreter.P21;

namespace QL4BIMinterpreter.QL4BIM
{
    public class SetSymbol : Symbol
    {
        private Dictionary<int, QLEntity> entityDic;
        private List<QLEntity> entites;

        public QLEntity EntityAt(int index) => entites[index];

        public QLEntity this[int index] => entityDic[index];

        public QLEntity this[int index, bool fake] => entityDic.ContainsKey(index) ? entityDic[index] : null;

        public SetSymbol(LiteralNode node) : base(node)
        {
            Header = "Set->";
            entites = new List<QLEntity>();
            entityDic = new Dictionary<int, QLEntity>();
        }

        public Dictionary<int, QLEntity> EntityDic
        {
            get { return entityDic; }
            set
            {
                entityDic = value;
                entites = value.Values.ToList();
            }
        }

        public override IEnumerable<QLEntity[]> Tuples
        {
            get { return entityDic.Values.Select(e => new []{e}); }
        }

        public QLEntity[] Entites => entites.ToArray();

        public override List<string> Attributes => new List<string>() { Value };

        public override bool IsEmpty => entites.Count == 0;
        public override void Reset()
        {
            entityDic.Clear();
        }

        public int EntityCount => entityDic.Count;
    }


}