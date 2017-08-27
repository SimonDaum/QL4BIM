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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Practices.Unity;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMprimitives;

namespace QL4BIMinterpreter
{
    public abstract class OperatorValidator: IOperatorValidator
    {

        public string Name { get;  set; }


        public List<FunctionSignatur> FunctionSignaturs = new List<FunctionSignatur>();
        protected List<List<string>> AllAttributes;


        protected bool IsSetAttribute(StatementNode statement, int index)
        {
            var attributeList = AllAttributes.FirstOrDefault(l => l.Contains(statement.Arguments[index].Value));
            return attributeList == null;
        }

        protected int ReferencedRelationAttributeCount(SetNode setNode)
        {
            var attributeList = AllAttributes.First(l => l.Contains(setNode.Value));
            return attributeList.Count;
        }

        public void Validate(SymbolTable symbolTable, StatementNode statement, List<List<string>> allAttributes)
        {
            AllAttributes = allAttributes;
            ValidateLocal(statement);
            AdditionalValidation(symbolTable, statement);
            AllAttributes = null;
        }

        protected void ValidateLocal(StatementNode statement)
        {
            var name = statement.OperatorNode.Value;
            var anySignaturOkay = false;
            foreach (var functionSignatur in FunctionSignaturs)
            {
                var argumentCountOkay = true;
                if (!IsVarArg(functionSignatur))
                    argumentCountOkay = functionSignatur.ArgumentTypes.Length == statement.Arguments.Count;
                
                if (!argumentCountOkay)
                    continue;

                bool argumentTypesOkay = true;
                if (functionSignatur.ArgumentTypes != null)
                    ArgumentTypeCheck(statement, functionSignatur, ref argumentTypesOkay);
                
                if (!argumentTypesOkay)
                    continue;

                if(functionSignatur.ReturnSymbol == SyUseVal.Rel && statement.ReturnRelationNode == null)
                    continue; 

                if (functionSignatur.ReturnSymbol == SyUseVal.Set && statement.ReturnSetNode == null)
                    continue;

                //var predicateOkay = true;
                //todo type check predicate
                //if (predicateCount > 0)
                //    predicateOkay = PredicateCheck(statement, functionSignatur);

                //if (!predicateOkay)
                //    continue;

                anySignaturOkay = true;
            }

            if (!anySignaturOkay)
            {
                var message = $"{Name}: no function overload suitable." +  Environment.NewLine;
                message += string.Join(Environment.NewLine, FunctionSignaturs);
                throw new QueryException(message);
            }
                
        }

        private static bool IsVarArg(FunctionSignatur functionSignatur)
        {
            return IsVarArg(functionSignatur.ArgumentTypes.Last());
        }

        private static bool IsVarArg(SyUseVal syUseVal)
        {
            return syUseVal  == SyUseVal.SetVa ||
                   syUseVal  == SyUseVal.RelVa ||
                   syUseVal  == SyUseVal.RelAttVa ||
                   syUseVal  == SyUseVal.StringVa ||
                   syUseVal  == SyUseVal.NumberVa ||
                   syUseVal  == SyUseVal.FloatVa ||
                   syUseVal  == SyUseVal.ExAttVa ||
                   syUseVal  == SyUseVal.ExTypeVa;
        }

        private static bool PredicateCheck(StatementNode statement, FunctionSignatur functionSignatur)
        {
            //todo check first and second types in predicate
            return true;
        }

        private void ArgumentTypeCheck(StatementNode statement, FunctionSignatur functionSignatur, ref bool argumentTypesOkay)
        {
            var varArg = IsVarArg(functionSignatur);
            var count = varArg ? functionSignatur.ArgumentTypes.Length : functionSignatur.ArgumentTypes.Length - 1;
            for (int i = 0; i < count; i++)
            {
                var arguement = statement.Arguments[i];
                argumentTypesOkay = TypeUsageEnumMatches(arguement, functionSignatur.ArgumentTypes[i]);

                if (!argumentTypesOkay)
                    break;
            }

            if(!varArg)
                return;

            var varArgSy = MatchSyUseVarArg(functionSignatur.ArgumentTypes.Last());

            for (int i = count-1; i < statement.Arguments.Count; i++)
            {
                var arguement = statement.Arguments[i];
                argumentTypesOkay = TypeUsageEnumMatches(arguement, varArgSy);

                if (!argumentTypesOkay)
                    break;
            }
        }

        private SyUseVal MatchSyUseVarArg(SyUseVal syUseVal)
        {
            if(syUseVal == SyUseVal.NumberVa)
                return SyUseVal.Number;

            if (syUseVal == SyUseVal.FloatVa)
                return SyUseVal.Float;

            if (syUseVal == SyUseVal.StringVa)
                return SyUseVal.String;

            if (syUseVal == SyUseVal.ExAttVa)
                return SyUseVal.ExAtt;

            if (syUseVal == SyUseVal.ExTypeVa)
                return SyUseVal.ExType;

            if (syUseVal == SyUseVal.RelVa)
                return SyUseVal.Rel;

            if (syUseVal == SyUseVal.RelAttVa)
                return SyUseVal.RelAtt;

            if (syUseVal == SyUseVal.SetVa)
                return SyUseVal.Set;

            throw new InvalidOperationException();
        }

        private bool TypeUsageEnumMatches(Node node, SyUseVal symbolUsage)
        {
            if (node is CNumberNode && symbolUsage == SyUseVal.Number)
                return true;

            if (node is CFloatNode && symbolUsage == SyUseVal.Float)
                return true;

            if (node is CStringNode && symbolUsage == SyUseVal.String)
                return true;

            if (node is ExAttNode && symbolUsage == SyUseVal.ExAtt)
                return true;

            if (node is ExTypeNode && symbolUsage == SyUseVal.ExType)
                return true;

            if (node is RelationNode && symbolUsage == SyUseVal.Rel)
                return true;

            var node1 = node as SetNode;
            if (node1 != null)
            {
                //var literalNode = node1;
                //if (literalNode.Usage == SetNode.SymbolUsage.RelAtt && symbolUsage == SyUseVal.RelAtt)
                //    return true;
                //if (literalNode.Usage == SetNode.SymbolUsage.Set && symbolUsage == SyUseVal.Set)
                //    return true;
            }


            return false;
        }

        protected void AllArgumentsFromOneRelation(StatementNode statement)
        {
            var lists =
                statement.Arguments.Select(a => { return AllAttributes.FirstOrDefault(l => l.Contains(a.Value)); }).Distinct();

            if (lists.Count() != 1)
                throw new QueryException($"{Name}: Only relational arguments (attributes) of one realtion are allowed.");
        }

        protected void AllRelationalArguments(StatementNode statement)
        {
            var attributes = AllAttributes.SelectMany(a => a);
            if (!statement.Arguments.All(a => attributes.Contains(a.Value)))
                throw new QueryException($"{Name}: Only relational arguments allowed");
        }

        protected virtual void AdditionalValidation(SymbolTable symbolTable, StatementNode statement) { }


    }

    public enum SyUseVal
    {
        Set, Rel, RelAtt, String, Number, Float, ExAtt, ExType,
        SetVa, RelVa, RelAttVa, StringVa, NumberVa, FloatVa, ExAttVa, ExTypeVa,
    }

    public class FunctionSignatur
    {
        public string[] AllowedPredTokens { get; private set; }
        public SyUseVal[] PredicateTypes { get; private set; }
        public SyUseVal ReturnSymbol { get; private set; }
        public SyUseVal[] ArgumentTypes { get; private set; }

        public FunctionSignatur(SyUseVal returnSymbol, SyUseVal[] argumentTypes,
            SyUseVal[] predicateTypes, string allowedPredTokens)
        {
            PredicateTypes = predicateTypes;
            ReturnSymbol = returnSymbol;
            ArgumentTypes = argumentTypes;

            if(predicateTypes != null)
                AllowedPredTokens = allowedPredTokens.Split(',');
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if(AllowedPredTokens != null)
                sb.AppendLine("AllowedPredTokens: " + string.Join(", ", AllowedPredTokens));
            sb.AppendLine("PredicateTypes: " + PredicateTypes);
            sb.AppendLine("ReturnSymbol: " + ReturnSymbol);
            sb.AppendLine("ArguementTypes: " + string.Join(", ", ArgumentTypes));

            return sb.ToString();
        }
    }
}
