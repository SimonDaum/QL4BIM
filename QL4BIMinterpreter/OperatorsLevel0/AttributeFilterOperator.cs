using System;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter.OperatorsLevel0
{
    public class AttributeFilterOperator : IAttributeFilterOperator
    {

        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym
        public void AttributeFilterRelAtt(RelationSymbol parameterSym1, PredicateData data, RelationSymbol returnSym)
        {
            Console.WriteLine("AttributeFilter'ing...");
            var index = parameterSym1.Index.Value;
            var result = parameterSym1.Tuples.Where(t => AttributeSetTestLocal(t[index], data));
            returnSym.SetTuples(result);
        }


        public void AttributeFilterSet(SetSymbol parameterSym1, PredicateData data, SetSymbol returnSym)
        {
            Console.WriteLine("AttributeFilter'ing...");
            var result = parameterSym1.Entites.Where(e => AttributeSetTestLocal(e, data));
            returnSym.EntityDic = result.ToDictionary(e => e.Id);
        }

        public bool AttributeSetTestLocal(QLEntity entity, PredicateData data)
        {
            //property present?
            var part = entity.GetPropertyValue(data.PropName);
            if (part == null)
                return false;

            //todo check if un nesting is ok (supports IFC TYPES such as IFCBOOLEAN)
            if (part.QLClass != null)
                part = part.QLClass.QLDirectList[0];

            if (data.StringValue != null && TestStringValue(part, data))
                return true;

            if (data.IntValue.HasValue && TestIntValue(part, data))
                return true;

            if (data.DoubleValue.HasValue && TestFloatValue(part, data))
                return true;

            return false;
        }

        private bool TestStringValue(QLPart part, PredicateData data)
        {   
            if (part.QLString != null)
            {
                var strPropValue = part.QLString.QLStr;
                if (string.Compare(strPropValue, data.StringValue, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }
            //enum case
            else if (part.QLEnum != null)
            {
                var enumPropValue = part.QLEnum.QLStr;
                if (string.Compare(enumPropValue, data.StringValue, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }

            return false;
        }

        private bool TestIntValue(QLPart part, PredicateData data)
        {
            //int schema to int query
            if (part.QLNumber != null)
            {
                var intPropValue = part.QLNumber.Value;
                if (Compare(intPropValue, data.IntValue.Value, data.Compare))
                    return true;
            }
            //float schema to int query
            if (part.QLFloat != null)
            {
                var floatPropValue = part.QLFloat.Value;
                if (Compare(floatPropValue, data.IntValue.Value, data.Compare))
                    return true;
            }

            return false;
        }

        private bool TestFloatValue(QLPart part, PredicateData data)
        {
            //int schema to float query
            if (part.QLNumber != null)
            {
                var intPropValue = part.QLNumber.Value;
                if (Compare(intPropValue, data.DoubleValue.Value, data.Compare))
                    return true;
            }
            //float schema to float query
            if (part.QLFloat != null)
            {
                var floatPropValue = part.QLFloat.Value;
                if (Compare(floatPropValue, data.DoubleValue.Value, data.Compare))
                    return true;
            }

            return false;
        }

        const double Tolerance = Double.Epsilon*2;

        private bool Compare(double a, double b, string compare)
        {
            if (compare == "=")
                return Math.Abs(a - b) < Tolerance;

            if (compare == "!=")
                return Math.Abs(a - b) > Tolerance;

            if (compare == ">")
                return a > b;

            if (compare == "<")
                return a < b;

            throw  new InvalidOperationException();
        }

        public struct PredicateData
        {
            public string PropName;
            public string Compare;
            public string StringValue;
            public int? IntValue;
            public double? DoubleValue;

            public PredicateData(PredicateNode node)
            {
                PropName = node.AttNode.Value;
                Compare = node.CompareToken;
                StringValue = node.ValueStringNode?.Value;
                IntValue = node.ValueNumberNode?.IntValue;
                DoubleValue = node.ValueFloatNode?.FloatValue;
            }
        }
    }
}
