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
using QL4BIMinterpreter.QL4BIM;
using QL4BIMprimitives;

namespace QL4BIMinterpreter
{   
    //checks parameters and stores identifiers in symbol table
    public class SymbolSymbolVisitor : ISymbolVisitor
    {

        private readonly IInterpreterRepository interpreterRepository;

        public SymbolSymbolVisitor(IInterpreterRepository interpreterRepository)
        {
            this.interpreterRepository = interpreterRepository;
        }

        public void Visit(StatementNode statementNode)
        {
            //todo user func
            //var symbolTable = ((FunctionNode) statementNode.Parent).SymbolTable;

            //var relsWithAttributes =  symbolTable.Symbols.Values.Where(s => s is RelationSymbol).Cast<RelationSymbol>().
            //    Select(r => new Tuple<string, List<string>>(r.Value, r.Attributes)).ToList();

            //var allRelAttributes = relsWithAttributes.Select(r => r.Item2).ToList();
            //var allAttributes = allRelAttributes.SelectMany(a => a).ToList();

            //CheckSymbolExistance(statementNode, allAttributes);

            //SetAttributesUsage(statementNode);

            //Validate(symbolTable, statementNode, allRelAttributes);

            AddSymbolsFromStatments(statementNode);
        }

        public void Visit(FunctionNode functionNode)
        {
            if (functionNode?.FirstStatement == null)
                return;

            //todo user func
            //if (functionNode.Previous == null)
            //{   
            //    interpreterRepository.GlobalSymbolTable =  functionNode.SymbolTable ;
            //}
            //else
            //{

            //    AddSymbolsFromFunc(functionNode);
            //}

            var statement = functionNode.FirstStatement;
            do
            {
                statement.Accept(this);
                statement = statement.Next;
            } while (statement != null);

            //todo user func
            //Visit(functionNode.Next);

        }

        private void AddSymbolsFromFunc(FunctionNode functionNode)
        {   
            //todo user func
            //foreach (var funcNodeArgument in functionNode.FormalArguments)
            //{
            //    var compLitNode = funcNodeArgument as RelationNode;
            //    if(compLitNode != null)
            //        functionNode.SymbolTable.AddRelSymbol(compLitNode);
            //    else
            //        functionNode.SymbolTable.AddSetSymbol((SetNode)funcNodeArgument);
            //}
        }

        private void Validate(SymbolTable symbolTable, StatementNode statementNode, List<List<string>> relsWithAttributes)
        {
            var validator = interpreterRepository.GetOperatorValidator(statementNode.OperatorNode.Value);

            validator.Validate(symbolTable, statementNode, relsWithAttributes);
        }

        private void SetAttributesUsage(StatementNode statementNode)
        {
            var literalArgs = statementNode.Arguments.Where(a => a is SetNode).Cast<SetNode>();

            var nodeForBackwardTraversal = statementNode;

            foreach (var literalArg in literalArgs)
            {
                //do 
                //{
                //    if (nodeForBackwardTraversal.ReturnSetNode != null &&
                //        nodeForBackwardTraversal.ReturnSetNode.Value == literalArg.Value)
                //    {
                //        literalArg.Usage = SetNode.SymbolUsage.Set;
                //        break; 
                //    }
                //    else if (nodeForBackwardTraversal.ReturnRelationNode != null &&
                //             nodeForBackwardTraversal.ReturnRelationNode.Attributes.Contains(literalArg.Value))
                //    {
                //       literalArg.Usage = SetNode.SymbolUsage.RelAtt;
                //       break;
                //    }
                        
                //    nodeForBackwardTraversal = nodeForBackwardTraversal.Previous;
                //} while (nodeForBackwardTraversal != null);
            }
        }


        private void CheckSymbolExistance(StatementNode statementNode, List<string> allAttributes)
        {
            var symbolTable = ((FunctionNode)statementNode.Parent).SymbolTable;

            var literalArgs = statementNode.Arguments.Where(a => a is SetNode).Cast<SetNode>();
            foreach (var literalArg in literalArgs)
            {   
                //sets
                var isSetSymbol = symbolTable.Symbols.ContainsKey(literalArg.Value);
  
                //relations
                var isRelSymbol = allAttributes.Contains(literalArg.Value);

                if (!isSetSymbol && !isRelSymbol)
                    throw new QueryException("Arguement symbol not (yet) present in scope: " + literalArg.Value);             
            }
        }


        private void AddSymbolsFromStatments(StatementNode statementNode)
        {
            var symbolTable = ((FunctionNode)statementNode.Parent).SymbolTable;

            if (statementNode.ReturnRelationNode != null && statementNode.ReturnSetNode != null)
                throw new InvalidOperationException();

            if(statementNode.ReturnRelationNode != null)
            {
                if (symbolTable.Symbols.ContainsKey(statementNode.ReturnRelationNode.Value))
                    return;

                symbolTable.AddRelSymbol(statementNode.ReturnRelationNode);
            }
            else if (statementNode.ReturnSetNode != null)
            {
                if (symbolTable.Symbols.ContainsKey(statementNode.ReturnSetNode.Value))
                    return;

                symbolTable.AddSetSymbol(statementNode.ReturnSetNode);
            }
        }
    }
}
