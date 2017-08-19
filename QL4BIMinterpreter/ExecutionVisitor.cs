using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Practices.Unity;
using QL4BIMinterpreter.OperatorsLevel0;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{   
    public class ExecutionVisitor : IExecutionVisitor
    {

        private readonly ISpatialQueryInterpreter spatialQueryInterpreter;
        private readonly ITypeFilterOperator typeFilterOperator;
        private readonly IAttributeFilterOperator attributeFilterOperator;
        private readonly IDereferenceOperator dereferenceOperator;
        private readonly IImportModelOperator importModelOperator;
        private readonly IProjectorOperator projectorOperator;
        private readonly IPropertyFilterOperator propertyFilterOperator;
        private readonly ISpatialTopoValidator spatialTopoValidator;
        private readonly ILogger logger;
        private readonly IDeassociaterOperator deassociaterOperator;
        private readonly ITaskTimerOperator taskTimerOperator;
        private readonly IMaximumOperator maximumOperator;

        public ExecutionVisitor( ISpatialQueryInterpreter spatialQueryInterpreter,
            ITypeFilterOperator typeFilterOperator, IAttributeFilterOperator attributeFilterOperator,
            IDereferenceOperator dereferenceOperator, IImportModelOperator importModelOperator,
            IProjectorOperator projectorOperator, IPropertyFilterOperator propertyFilterOperator,
            IDeassociaterOperator deassociaterOperator, ITaskTimerOperator taskTimerOperator,
            IMaximumOperator maximumOperator,
            ISpatialTopoValidator spatialTopoValidator, ILogger logger)
        {
            this.spatialQueryInterpreter = spatialQueryInterpreter;
            this.typeFilterOperator = typeFilterOperator;
            this.attributeFilterOperator = attributeFilterOperator;
            this.dereferenceOperator = dereferenceOperator;
            this.importModelOperator = importModelOperator;
            this.projectorOperator = projectorOperator;
            this.propertyFilterOperator = propertyFilterOperator;
            this.spatialTopoValidator = spatialTopoValidator;
            this.logger = logger;
            this.deassociaterOperator = deassociaterOperator;
            this.taskTimerOperator = taskTimerOperator;
            this.maximumOperator = maximumOperator;
        }
        
        private readonly IList<FunctionNode> userFuncs = new List<FunctionNode>();


        public void Visit(StatementNode statementNode)
        {
            logger.StatementIndex++;

            var symbolTable = ((FunctionNode)statementNode.Parent).SymbolTable;

            var operatorName = statementNode.OperatorNode.Value;
            if (operatorName == "ImportModel")
            {
                var returnSymbol = symbolTable.GetSetSymbol(statementNode.ReturnSetNode);
                importModelOperator.ImportModel(statementNode.Arguments[0].Value, returnSymbol); 
                return;
            }

            //overloaded
            //sig1: Set    <- Set, String
            //sig2: Relation   <- RelAtt, String VarArg
            //sig3: Relation   <- Relation, String Var arg
            if (operatorName == "TypeFilter")
            {
                var isRelationOverload = statementNode.Arguments[0] is RelationNode;
                if (isRelationOverload)
                {
                    var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                    var parameterSymbol1 = symbolTable.GetRelationSymbol((RelationNode) statementNode.Arguments[0]);
                    var typeNames = statementNode.Arguments.Where(a => a is ExTypeNode).Select(n => n.Value).ToArray();

                    logger.LogStart(operatorName, parameterSymbol1.EntityCount);
                    typeFilterOperator.TypeFilterRelation(parameterSymbol1, typeNames, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                }
                else
                {   
                    var returnSymbol = symbolTable.GetSetSymbol(statementNode.ReturnSetNode);
                    var typePred = statementNode.Arguments[0] as TypePredNode;
                    var setIn = typePred.SetNode;
                    var parameterSymbol1 = symbolTable.GetSetSymbol(setIn);

                    logger.LogStart(operatorName, parameterSymbol1.EntityCount);
                    typeFilterOperator.TypeFilterSet(parameterSymbol1, typePred.Type, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                }
            }

            //overloaded
            //sig1: Set    <- Set, Predicate
            //sig2: Relation   <- Relation, Predicate
            if (operatorName == "AttributeFilter")
            {
                var relName = statementNode.Arguments[0] as RelNameNode;
                var predData = new AttributeFilterOperator.PredicateData(statementNode.Predicate);

                if (relName != null)
                {
                    var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                    var parameterRelationSymbol = symbolTable.GetRelationSymbol(relName);

                    logger.LogStart(operatorName, parameterRelationSymbol.EntityCount);
                    attributeFilterOperator.AttributeFilterRelAtt(parameterRelationSymbol, predData, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                    return;
                }
                //set
                else
                {
                    var set = statementNode.Arguments[0] as SetNode;
                    var returnSymbol = symbolTable.GetSetSymbol(statementNode.ReturnSetNode);       
                    var parameterSetSymbol = symbolTable.GetSetSymbol(set);

                    logger.LogStart(operatorName, parameterSetSymbol.EntityCount);
                    attributeFilterOperator.AttributeFilterSet(parameterSetSymbol, predData, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                }

                return;
            }

            //overloaded
            //sig1: Relation [2]    <- Set, String
            //sig2: Relation [2]    <- Set, String, String
            //sig3: Relation [+1]   <- Relation, String 
            //sig3: Relation [+1]   <- Relation, String, String 
            if (operatorName == "Dereferencer")
            {
                var relNameNode = statementNode.Arguments[0] as RelNameNode;

                var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                var refs = statementNode.Arguments.Skip(1).Cast<ExAttNode>().Select(n => n.Value).ToArray();
                if (relNameNode != null)
                {
                    var parameterRelationSymbol = symbolTable.GetRelationSymbol(relNameNode);

                    logger.LogStart(operatorName, parameterRelationSymbol.EntityCount);
                    dereferenceOperator.ReferenceRelAtt(parameterRelationSymbol, refs, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                }

                else
                {
                    var setNode = statementNode.Arguments[0] as SetNode;
                    var parameterSetSymbol = symbolTable.GetSetSymbol(setNode);

                    logger.LogStart(operatorName, parameterSetSymbol.EntityCount);
                    dereferenceOperator.ReferenceSet(parameterSetSymbol, refs, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                }
                    

                return;
            }

            //overloaded
            //sig1: Relation [2]    <- Set, String
            //sig2: Relation [2]    <- Set, String, String
            //sig3: Relation [+1]   <- RelAtt, String todo
            //sig3: Relation [+1]   <- RelAtt, String, String todo
            if (operatorName == "Deassociater")
            {
                var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                var relNameNode = statementNode.Arguments[0] as RelNameNode;

                if (relNameNode != null)
                {
                    var relAttNode = statementNode.Arguments[1] as AttributeAccessNode;
                    var parameterRelationSymbol = symbolTable.GetRelationSymbol(relNameNode);
                    parameterRelationSymbol.Index = 1;
                    var exAtts = relAttNode.ExAttNode;

                    logger.LogStart(operatorName, parameterRelationSymbol.EntityCount);
                    deassociaterOperator.DeassociaterRelAtt(parameterRelationSymbol, new string[] {exAtts.Value}, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                }
                else
                {
                    var attributeAccessNode = statementNode.Arguments[0] as AttributeAccessNode;
                    var setNode = attributeAccessNode.SetNode;
                    var parameterSetSymbol = symbolTable.GetSetSymbol(setNode);
                    var exAtts = attributeAccessNode.ExAttNode.Value;

                    logger.LogStart(operatorName, parameterSetSymbol.EntityCount);
                    deassociaterOperator.DeassociaterSet(parameterSetSymbol, new string[] {exAtts}, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                }
                    
                return;
            }


            if (operatorName == "Projector")
            {
                var arguementCount = statementNode.Arguments.Count;
                if (arguementCount == 2)
                {
                    var returnSymbolSet = symbolTable.GetSetSymbol(statementNode.ReturnSetNode);
                    var relNameNode = (RelNameNode)statementNode.Arguments[0];
                    var parameterRelationSymbol1 = symbolTable.GetRelationSymbol(relNameNode);

                    logger.LogStart(operatorName, parameterRelationSymbol1.EntityCount);
                    projectorOperator.ProjectRelAttSet(parameterRelationSymbol1, returnSymbolSet);
                    logger.LogStop(returnSymbolSet.EntityCount);
                    return;
                }
                else
                {
                    var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);

                    var relNameNode = (RelNameNode)statementNode.Arguments[0];
                    var arguments = statementNode.Arguments.Skip(1).Cast<RelAttNode>().ToList();
                    var argumentIndices = symbolTable.GetIndices(arguments).ToArray();
                    var parameterRelationSymbol = symbolTable.GetRelationSymbol(relNameNode);
                    logger.LogStart(operatorName, parameterRelationSymbol.EntityCount);
                    projectorOperator.ProjectRelAttRelation(parameterRelationSymbol, argumentIndices, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                    return;
                }


            }

            if (operatorName == "PropertyFilterOperator")
            {
                var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                var firstAttribute = symbolTable.GetSetSymbol(statementNode.Arguments[0] as SetNode);
                var parameter2 = statementNode.Arguments[0] as CStringNode;
                var parameter3 = statementNode.Arguments[1] as CStringNode;

                logger.LogStart(operatorName, firstAttribute.EntityCount);
                propertyFilterOperator.PropertyFilterSet(firstAttribute, parameter2.Value, parameter3.Value, returnSymbol);
                logger.LogStop(returnSymbol.EntityCount);
                return;
            }

            if (operatorName == "TaskTimer")
            {
                var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                var parameterRelationSymbol = symbolTable.GetRelationSymbol((RelNameNode)statementNode.Arguments[0] );

                logger.LogStart(operatorName, parameterRelationSymbol.EntityCount);
                taskTimerOperator.TaskTimerRelAtt(parameterRelationSymbol, returnSymbol);
                logger.LogStop(returnSymbol.EntityCount);
                return;
            }

            if (operatorName == "Maximum")
            {
                var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                var parameterRelationSymbol = symbolTable.GetRelationSymbol( (RelNameNode)statementNode.Arguments[0]);
                var testValue = statementNode.Arguments[1].Value;

                logger.LogStart(operatorName, parameterRelationSymbol.EntityCount);
                maximumOperator.MaximumRelAtt(parameterRelationSymbol,testValue, returnSymbol);
                logger.LogStop(returnSymbol.EntityCount);
                return;
            }

            if (spatialTopoValidator.TopoOperators.Contains(operatorName))
            {
                var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                var parameterSymbol1 = symbolTable.GetSetSymbol(statementNode.Arguments[0] as SetNode);
                var parameterSymbol2 = symbolTable.GetSetSymbol(statementNode.Arguments[1] as SetNode);

                logger.LogStart(operatorName, new int[] { parameterSymbol1.EntityCount, parameterSymbol2.EntityCount});
                spatialQueryInterpreter.Execute(operatorName, returnSymbol, parameterSymbol1, parameterSymbol2);
                logger.LogStop(returnSymbol.EntityCount);
                return;
            }

            // user func
            //var userFuncPresent = userFuncs.FirstOrDefault(f => f.Value == operatorName);
            //if(userFuncPresent == null)
            //    return;

            //var userFuncStacked = userFuncPresent.Copy();
            //var parameterSymbol = symbolTable.GetSetSymbol(statementNode.Arguments[0] as SetNode);

            //var formalParameterSymbol = userFuncStacked.SymbolTable.GetSetSymbol(userFuncStacked.FormalArguments[0] as SetNode);
            //formalParameterSymbol.EntityDic = parameterSymbol.Entites.ToDictionary(e => e.Id);

            //var userFuncStatement = userFuncStacked.FirstStatement;
            //while (userFuncStatement != null)
            //{
            //    Visit(userFuncStatement);
            //    userFuncStatement = userFuncStatement.Next;
            //}

            //var returnSymbolFunc = userFuncStacked.SymbolTable.GetRelationSymbol(userFuncStacked.LastStatement.ReturnRelationNode);

            //var formalreturnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);

            //formalreturnSymbol.SetTuples(returnSymbolFunc.Tuples);


        }



        public void Visit(FunctionNode functionNode)
        {
            logger.StatementIndex = 0;

            var firstStatement = functionNode.FirstStatement;
            if (firstStatement == null)
                return;

            //todo user func
            //if (functionNode.Value == "Global")
            //{
            //    var tempFunc = functionNode;
            //    while (tempFunc.Next != null)
            //    {
            //        userFuncs.Add(tempFunc.Next);
            //        tempFunc = tempFunc.Next;
            //    }

            //}

            var statement = firstStatement;
            do
            {
                statement.Accept(this);
                statement = statement.Next;
            } while (statement != null);


            Console.WriteLine("Query execution has finished.");
        }


    }
}
