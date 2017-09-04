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
        private readonly IExportModelOperator exportModelOperator;
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
            IExportModelOperator exportModelOperator,
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
            this.exportModelOperator = exportModelOperator;
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
            if (operatorName == "ImportModel" || operatorName == "IM")
            {
                var returnSymbol = symbolTable.GetSetSymbol(statementNode.ReturnSetNode);
                importModelOperator.ImportModel(statementNode.Arguments[0].Value, returnSymbol); 
                return;
            }

            if (operatorName == "ExportModel" || operatorName == "EM")
            {
                var returnSymbol = symbolTable.GetSetSymbol(statementNode.ReturnSetNode);
                var relSymbol = symbolTable.GetSetSymbol((SetNode)statementNode.Arguments[0]);
                exportModelOperator.ExportModel(relSymbol, statementNode.Arguments[1].Value, returnSymbol);
                return;
            }

            //overloaded
            //sig1: Set    <- Set, String
            //sig2: Relation   <- RelAtt, String VarArg
            //sig3: Relation   <- Relation, String Var arg
            if (operatorName == "TypeFilter" || operatorName == "TF")
            {
                var isRelationOverload = statementNode.Arguments[0] is RelNameNode;
                if (isRelationOverload)
                {
                    var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                    var relSymbol = symbolTable.GetRelationSymbol((RelNameNode) statementNode.Arguments[0]);
                    var typePreds = statementNode.Arguments.Where(a => a is TypePredNode).Cast<TypePredNode>();
                    var indicesAndTypes = typePreds.Select( tp => new Tuple<int, string>(tp.RelAttNode.AttIndex, tp.Type)).ToArray();

                    logger.LogStart(operatorName, relSymbol.EntityCount);
                    typeFilterOperator.TypeFilterRelation(relSymbol, indicesAndTypes, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                }
                else
                {   
                    var returnSymbol = symbolTable.GetSetSymbol(statementNode.ReturnSetNode);
                    var typePred = (TypePredNode)statementNode.Arguments[0];
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
            if (operatorName == "AttributeFilter" || operatorName == "AF")
            {
                var relName = statementNode.Arguments[0] as RelNameNode;
                if (relName != null)
                {
                    var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                    var parameterRelationSymbol = symbolTable.GetRelationSymbol(relName);

                    var preds = statementNode.Arguments.Where(a => a is PredicateNode).Cast<PredicateNode>().ToArray();

                    logger.LogStart(operatorName, parameterRelationSymbol.EntityCount);
                    attributeFilterOperator.AttributeFilterRelAtt(parameterRelationSymbol, preds, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                    return;
                }
                //set
                else
                {
                    var set = statementNode.Arguments[0] as SetNode;
                    var returnSymbol = symbolTable.GetSetSymbol(statementNode.ReturnSetNode);       
                    var parameterSetSymbol = symbolTable.GetSetSymbol(set);
                    var predNode = statementNode.Arguments[1] as PredicateNode;

                    logger.LogStart(operatorName, parameterSetSymbol.EntityCount);
                    attributeFilterOperator.AttributeFilterSet(parameterSetSymbol, predNode, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                }

                return;
            }

            //overloaded
            //sig1: Relation [2]    <- Set, String
            //sig2: Relation [2]    <- Set, String, String
            //sig3: Relation [+1]   <- Relation, String 
            //sig3: Relation [+1]   <- Relation, String, String 
            if (operatorName == "Dereferencer" || operatorName == "DR")
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
            if (operatorName == "Deassociater" || operatorName == "DA")
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


            if (operatorName == "Projector" || operatorName == "PR")
            {
                var arguementCount = statementNode.Arguments.Count;
                if (arguementCount == 2)
                {
                    var returnSymbolSet = symbolTable.GetSetSymbol(statementNode.ReturnSetNode);
                    var relNameNode = (RelNameNode)statementNode.Arguments[0];
                    var relAttNode = (RelAttNode)statementNode.Arguments[1];

                    var parameterRelationSymbol1 = symbolTable.GetRelationSymbol(relNameNode);
                    parameterRelationSymbol1.Index = relAttNode.AttIndex;

                    logger.LogStart(operatorName, parameterRelationSymbol1.EntityCount);
                    projectorOperator.ProjectRelAttSet(parameterRelationSymbol1, returnSymbolSet);
                    logger.LogStop(returnSymbolSet.EntityCount);
                    return;
                }
                else
                {
                    var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);

                    var relNameNode = (RelNameNode)statementNode.Arguments[0];
                    var argumentsIn = statementNode.Arguments.Skip(1).Cast<RelAttNode>().ToList();

                    var parameterRelationSymbol = symbolTable.GetRelationSymbol(relNameNode);
                    var attributesInRel = parameterRelationSymbol.Attributes;
                    var indices = argumentsIn.Select( a => a.AttIndex).ToArray();

                    logger.LogStart(operatorName, parameterRelationSymbol.EntityCount);
                    projectorOperator.ProjectRelAttRelation(parameterRelationSymbol, indices, returnSymbol);
                    logger.LogStop(returnSymbol.EntityCount);
                    return;
                }


            }

            if (operatorName == "PropertyFilterOperator" || operatorName == "PF")
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

            if (operatorName == "TimeResolver" || operatorName == "TR")
            {
                var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                var parameterRelationSymbol = symbolTable.GetRelationSymbol((RelNameNode)statementNode.Arguments[0]);
                parameterRelationSymbol.Index = ((RelAttNode) statementNode.Arguments[1]).AttIndex;

                logger.LogStart(operatorName, parameterRelationSymbol.EntityCount);
                taskTimerOperator.TimeResolverRel(parameterRelationSymbol, returnSymbol);
                logger.LogStop(returnSymbol.EntityCount);
                return;
            }

            if (operatorName == "Maximum" || operatorName == "MA")
            {
                var returnSymbol = symbolTable.GetRelationSymbol(statementNode.ReturnRelationNode);
                var parameterRelationSymbol = symbolTable.GetRelationSymbol( (RelNameNode)statementNode.Arguments[0]);
                var attributeAccessNode = (AttributeAccessNode) statementNode.Arguments[1];

                parameterRelationSymbol.Index = attributeAccessNode.RelAttNode.AttIndex;
                var exAtt = attributeAccessNode.ExAttNode;

                logger.LogStart(operatorName, parameterRelationSymbol.EntityCount);
                maximumOperator.MaximumRelAtt(parameterRelationSymbol,exAtt.Value, returnSymbol);
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

            Console.WriteLine();
            Console.WriteLine("Query execution has finished");
            Console.WriteLine("Press enter to see content of last assigned symbol.");
        }
    }
}
