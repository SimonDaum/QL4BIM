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
            var symbolTable = ((FuncNode) statementNode.Parent).SymbolTable;

            var relsWithAttributes =  symbolTable.Symbols.Values.Where(s => s is RelationSymbol).Cast<RelationSymbol>().
                Select(r => new Tuple<string, List<string>>(r.Value, r.Attributes)).ToList();

            var allRelAttributes = relsWithAttributes.Select(r => r.Item2).ToList();
            var allAttributes = allRelAttributes.SelectMany(a => a).ToList();

            CheckSymbolExistance(statementNode, allAttributes);

            SetAttributesUsage(statementNode);

            Validate(symbolTable, statementNode, allRelAttributes);

            AddSymbolsFromStatments(statementNode);
        }

        public void Visit(FuncNode funcNode)
        {
            if (funcNode?.FirstStatement == null)
                return;

            if (funcNode.Previous == null)
            {   
                interpreterRepository.GlobalSymbolTable =  funcNode.SymbolTable ;
            }
            else
            {

                AddSymbolsFromFunc(funcNode);
            }

            var statement = funcNode.FirstStatement;
            do
            {
                statement.Accept(this);
                statement = statement.Next;
            } while (statement != null);

            Visit(funcNode.Next);

        }

        private void AddSymbolsFromFunc(FuncNode funcNode)
        {
            foreach (var funcNodeArgument in funcNode.Arguments)
            {
                var compLitNode = funcNodeArgument as CompLitNode;
                if(compLitNode != null)
                    funcNode.SymbolTable.AddRelSymbol(compLitNode);
                else
                    funcNode.SymbolTable.AddSetSymbol((LiteralNode)funcNodeArgument);
            }
        }

        private void Validate(SymbolTable symbolTable, StatementNode statementNode, List<List<string>> relsWithAttributes)
        {
            var validator = interpreterRepository.GetOperatorValidator(statementNode.OperatorNode.Value);

            validator.Validate(symbolTable, statementNode, relsWithAttributes);
        }

        private void SetAttributesUsage(StatementNode statementNode)
        {
            var literalArgs = statementNode.Arguments.Where(a => a is LiteralNode).Cast<LiteralNode>();

            var nodeForBackwardTraversal = statementNode;

            foreach (var literalArg in literalArgs)
            {
                do 
                {
                    if (nodeForBackwardTraversal.ReturnLiteralNode != null &&
                        nodeForBackwardTraversal.ReturnLiteralNode.Value == literalArg.Value)
                    {
                        literalArg.Usage = LiteralNode.SymbolUsage.Set;
                        break; 
                    }
                    else if (nodeForBackwardTraversal.ReturnCompLitNode != null &&
                             nodeForBackwardTraversal.ReturnCompLitNode.Literals.Contains(literalArg.Value))
                    {
                       literalArg.Usage = LiteralNode.SymbolUsage.RelAtt;
                       break;
                    }
                        
                    nodeForBackwardTraversal = nodeForBackwardTraversal.Previous;
                } while (nodeForBackwardTraversal != null);
            }
        }


        private void CheckSymbolExistance(StatementNode statementNode, List<string> allAttributes)
        {
            var symbolTable = ((FuncNode)statementNode.Parent).SymbolTable;

            var literalArgs = statementNode.Arguments.Where(a => a is LiteralNode).Cast<LiteralNode>();
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
            var symbolTable = ((FuncNode)statementNode.Parent).SymbolTable;

            if (statementNode.ReturnCompLitNode != null && statementNode.ReturnLiteralNode != null)
                throw new InvalidOperationException();

            if(statementNode.ReturnCompLitNode != null)
            {
                if (symbolTable.Symbols.ContainsKey(statementNode.ReturnCompLitNode.Value))
                    return;

                symbolTable.AddRelSymbol(statementNode.ReturnCompLitNode);
            }
            else if (statementNode.ReturnLiteralNode != null)
            {
                if (symbolTable.Symbols.ContainsKey(statementNode.ReturnLiteralNode.Value))
                    return;

                symbolTable.AddSetSymbol(statementNode.ReturnLiteralNode);
            }
        }
    }
}
