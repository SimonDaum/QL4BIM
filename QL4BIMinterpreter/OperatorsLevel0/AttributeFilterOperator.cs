using System;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter.OperatorsLevel0
{
    public class AttributeFilterOperator : IAttributeFilterOperator
    {

        //only symbols and simple types in operators, no nodes
        //symbolTable, parameterSym1, ..., returnSym
        public void AttributeFilterRelAtt(RelationSymbol parameterSym1, PredicateNode[] predicateNodes, RelationSymbol returnSym)
        {
            Console.WriteLine("AttributeFilter'ing...");
            //var index = parameterSym1.Index.Value; todo remove index prop
            var attributes = parameterSym1.Attributes;
           var indexAndPreps = predicateNodes.Select(p => new Tuple<int, PredicateNode>(
               RelAttIndexHelper.GetIndexFromRelAtt((p.FirstNode as AttributeAccessNode).RelAttNode, attributes),p));

            var result = parameterSym1.Tuples.Where(t =>indexAndPreps.All(
                indexAndPrep => AttributeSetTestLocal(t[indexAndPrep.Item1], indexAndPrep.Item2)));
            returnSym.SetTuples(result);
        }


        public void AttributeFilterSet(SetSymbol parameterSym1, PredicateNode predicateNode, SetSymbol returnSym)
        {
            Console.WriteLine("AttributeFilter'ing...");
            var result = parameterSym1.Entites.Where(e => AttributeSetTestLocal(e, predicateNode));
            returnSym.EntityDic = result.ToDictionary(e => e.Id);
        }

        public bool AttributeSetTestLocal(QLEntity entity, PredicateNode predicateNode) //todo all possible versions
        {
            var secondNode = predicateNode.SecondNode;
            var exAtt = (predicateNode.FirstNode as AttributeAccessNode).ExAttNode;

            //property present?
            var part = entity.GetPropertyValue(exAtt.Value);
            if (part == null)
                return false;

            //todo check if un nesting is ok (supports IFC TYPES such as IFCBOOLEAN)
            if (part.QLClass != null)
                part = part.QLClass.QLDirectList[0];

            if (secondNode is CStringNode && TestStringValue(part, secondNode))
                return true;

            if (secondNode is CNumberNode && TestIntValue(part, secondNode, predicateNode.Compare))
                return true;

            if (secondNode is CFloatNode && TestFloatValue(part, secondNode, predicateNode.Compare))
                return true;

            return false;
        }

        private bool TestStringValue(QLPart part, Node secondNode)
        {
            var stringValue = (secondNode as CStringNode).Value;

            if (part.QLString != null)
            {
                var strPropValue = part.QLString.QLStr;
                if (string.Compare(strPropValue, stringValue, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }
            //enum case
            else if (part.QLEnum != null)
            {
                var enumPropValue = part.QLEnum.QLStr;
                if (string.Compare(enumPropValue, stringValue, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }

            return false;
        }

        private bool TestIntValue(QLPart part, Node secondNode, ParserParts parserParts)
        {
            var intValue = (secondNode as CNumberNode).IntValue;

            //int schema to int query
            if (part.QLNumber != null)
            {
                var intPropValue = part.QLNumber.Value;
                if (Compare(intPropValue, intValue, parserParts))
                    return true;
            }
            //float schema to int query
            if (part.QLFloat != null)
            {
                var floatPropValue = part.QLFloat.Value;
                if (Compare(floatPropValue, intValue, parserParts))
                    return true;
            }

            return false;
        }

        private bool TestFloatValue(QLPart part, Node secondNode, ParserParts parserParts)
        {
            var floatValue = (secondNode as CFloatNode).FloatValue;

            //int schema to float query
            if (part.QLNumber != null)
            {
                var intPropValue = part.QLNumber.Value;
                if (Compare(intPropValue, floatValue, parserParts))
                    return true;
            }
            //float schema to float query
            if (part.QLFloat != null)
            {
                var floatPropValue = part.QLFloat.Value;
                if (Compare(floatPropValue, floatValue, parserParts))
                    return true;
            }

            return false;
        }

        const double Tolerance = Double.Epsilon*2;

        private bool Compare(double a, double b, ParserParts parserParts)
        {
            if (parserParts == ParserParts.EqualsPred)
                return Math.Abs(a - b) < Tolerance;

            if (parserParts == ParserParts.LessPred)
                return a > b;

            if (parserParts == ParserParts.LessEqualPred)
                return a >= b;

            if (parserParts == ParserParts.MorePred)
                return a < b;

            if (parserParts == ParserParts.MoreEqualPred)
                return a < b;

            throw  new InvalidOperationException();
        }

        private bool Compare(int a, int b, ParserParts parserParts)
        {
            if (parserParts == ParserParts.EqualsPred)
                return a == b;

            if (parserParts == ParserParts.LessPred)
                return a > b;

            if (parserParts == ParserParts.LessEqualPred)
                return a >= b;

            if (parserParts == ParserParts.MorePred)
                return a < b;

            if (parserParts == ParserParts.MoreEqualPred)
                return a < b;

            throw new InvalidOperationException();
        }
    }
}
