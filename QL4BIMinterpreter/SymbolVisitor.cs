using System;
using System.Collections.Generic;
using System.Linq;
using QL4BIMinterpreter.QL4BIM;

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
            var symbolTable = ((FunctionNode) statementNode.Parent).SymbolTable;

            var relsWithAttributes =  symbolTable.Symbols.Values.Where(s => s is RelationSymbol).Cast<RelationSymbol>().
                Select(r => new Tuple<string, List<string>>(r.Value, r.Attributes)).ToList();

            var allRelAttributes = relsWithAttributes.Select(r => r.Item2).ToList();
            var allAttributes = allRelAttributes.SelectMany(a => a).ToList();

            CheckSymbolExistance(statementNode, allAttributes);

            SetAttributesUsage(statementNode);

            Validate(symbolTable, statementNode, allRelAttributes);

            AddSymbolsFromStatments(statementNode);
        }

        public void Visit(FunctionNode functionNode)
        {
            if (functionNode?.FirstStatement == null)
                return;

            if (functionNode.Previous == null)
            {   
                interpreterRepository.GlobalSymbolTable =  functionNode.SymbolTable ;
            }
            else
            {

                AddSymbolsFromFunc(functionNode);
            }

            var statement = functionNode.FirstStatement;
            do
            {
                statement.Accept(this);
                statement = statement.Next;
            } while (statement != null);

            Visit(functionNode.Next);

        }

        private void AddSymbolsFromFunc(FunctionNode functionNode)
        {
            foreach (var funcNodeArgument in functionNode.FormalArguments)
            {
                var compLitNode = funcNodeArgument as RelationNode;
                if(compLitNode != null)
                    functionNode.SymbolTable.AddRelSymbol(compLitNode);
                else
                    functionNode.SymbolTable.AddSetSymbol((SetNode)funcNodeArgument);
            }
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
                do 
                {
                    if (nodeForBackwardTraversal.ReturnSetNode != null &&
                        nodeForBackwardTraversal.ReturnSetNode.Value == literalArg.Value)
                    {
                        literalArg.Usage = SetNode.SymbolUsage.Set;
                        break; 
                    }
                    else if (nodeForBackwardTraversal.ReturnRelationNode != null &&
                             nodeForBackwardTraversal.ReturnRelationNode.Attributes.Contains(literalArg.Value))
                    {
                       literalArg.Usage = SetNode.SymbolUsage.RelAtt;
                       break;
                    }
                        
                    nodeForBackwardTraversal = nodeForBackwardTraversal.Previous;
                } while (nodeForBackwardTraversal != null);
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
