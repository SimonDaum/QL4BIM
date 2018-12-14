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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using QL4BIMinterpreter.P21;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMprimitives;
using QL4BIMspatial;

namespace QL4BIMinterpreter
{
    class Context : IContext
    {
        private readonly ISpatialRepository spatialRepository;
        private readonly QueryAddState queryAddState;
        private readonly QueryClearState queryClearState;
        private readonly QueryShowState queryShowState;
        private readonly ExecuteAllState executeAllState;
        private readonly SymbolShowState symbolShowState;
        private readonly ResultShowState resultShowState;
        private readonly ResultWriteState resultWriteState;
        private readonly SymbolClearState symbolClearState;
        private readonly ShowSettingsState showSettingsState;
        private readonly PerformanceTestState performanceTestState;
        private readonly ShowLicenceState showLicenceState;

        private State currentState;


        private const string HelpString = "Available console operators: -Help, -H, -ShowLicence, -SL, -Quit, -Q, -AddStatement, -AS, -CancelQuery, -CQ, -ShowQuery, -SQ, -ExecuteAll, -EA, -PerformanceTest, -PT, -PrintSymbols, -PS, -Settings, -SE , -ClearState, -CS, -ShowResult, -SR, -WriteResult, -WR";

        public Context(IQueryReader queryReader, ISymbolVisitor symbolVisitor, IExecutionVisitor executionVisitor, IFuncVisitor funcVisitor,
            ISpatialRepository spatialRepository, IInterpreterRepository repository, ISettings settings, ILogger logger)
        {
            this.spatialRepository = spatialRepository;

            queryClearState = new QueryClearState() {Repository = repository, Name = "QueryClearState"};
            queryAddState = new QueryAddState() {Repository = repository, Name = "QueryAddState"};
            queryShowState = new QueryShowState(queryAddState) { Repository = repository, Name = "QueryShowState" };
            queryAddState.ShowState = queryShowState;

            symbolShowState = new SymbolShowState(repository, "SymbolShowState");
            symbolClearState = new SymbolClearState(repository, spatialRepository, symbolShowState, queryShowState, "SymbolClearState");

            resultShowState = new ResultShowState(repository, queryShowState, "ResultShowState");
            executeAllState = new ExecuteAllState(symbolClearState, repository, spatialRepository, queryReader, symbolVisitor, executionVisitor, funcVisitor,
                "ExecuteAllState", queryShowState, resultShowState);

            resultWriteState = new ResultWriteState(repository, "ResultWriteState", resultShowState);

            showSettingsState = new ShowSettingsState(settings, "ShowSettingsState");
           showLicenceState = new ShowLicenceState(queryAddState, "ShowLicenceState");

            performanceTestState = new PerformanceTestState(repository, spatialRepository, queryReader, symbolVisitor, executionVisitor,
                funcVisitor, logger, settings, "PerformanceTestState");

            currentState = showLicenceState;
            currentState = currentState.Execute("");
        }

        private bool IsOnOf(string input,  params string[] operatorNames)
        {
            foreach (var operatorName in operatorNames)
            {
                if (input.Contains(" "))
                    input = input.Split(' ')[0];

                if (string.Compare(input, operatorName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }

            return false;
        }


        public bool Execute(string input)
        {   
            Console.Clear();

            if (IsOnOf(input, "-Quit","-Q"))
            {
                return false;
            }

            if (IsOnOf(input, "-Help", "-H"))
            {
                Console.WriteLine(HelpString);
                currentState = queryAddState;
                return true;
            }

            if (IsOnOf(input, "-AddStatement", "-AS"))
            {
                queryShowState.Execute("");
                currentState = queryAddState;
                return true;
            }

            if (IsOnOf(input, "-CancelQuery", "-CQ"))
            {
                currentState = queryClearState;
                currentState.Execute("");
                currentState = queryShowState;
                currentState.Execute("");
                currentState = queryAddState;
                return true;
            }

            if (IsOnOf(input, "-ShowQuery", "-SQ"))
            {
                currentState = queryShowState;
                currentState.Execute("");
                currentState = queryAddState;
                return true;
            }

            if (IsOnOf(input, "-ExecuteAll", "-EA"))
            {
                currentState = executeAllState;
                currentState = currentState.Execute("");
                return true;
            }

            if (IsOnOf(input, "-PerformanceTest", "-PT"))
            {
                currentState = performanceTestState;
                currentState = currentState.Execute(input);
                return true;
            }

            if (IsOnOf(input, "-PrintSymbols", "-PS"))
            {
                currentState = symbolShowState;
                currentState.Execute("");
                resultShowState.Reset();
                currentState = resultShowState;
                return true;
            }

            if (IsOnOf(input, "-Settings", "-SE"))
            {
                currentState = showSettingsState;
                currentState.Execute("");
                return true;
            }


            if (IsOnOf(input, "-ShowLicence", "-SL"))
            {
                currentState = showLicenceState;
                currentState.Execute("");
                return true;
            }

            if (IsOnOf(input, "-ClearSymbol", "-CS"))
            {
                currentState = symbolClearState;
                currentState = currentState.Execute(""); ;
                return true;
            }

            if (input.StartsWith("-ShowResult") || input.StartsWith("-SR"))
            {
                if(input.Split(' ').Length <= 1)
                {
                    Console.WriteLine("Add symbol name after -ShowResult/-SR");
                    return true;
                }
                currentState = resultShowState;
                currentState.Execute(input);
                return true;
            }

            if (IsOnOf(input, "-WriteResult", "-WR"))
            {
                currentState = resultWriteState;
                currentState.Execute(input);
                return true;
            }

            if (input.StartsWith("-"))
            {
                Console.WriteLine("Inputs starts with - but is no console operator. Showing query instead.");
                Console.WriteLine("Type -H for available operators");
                currentState = queryShowState;
                currentState.Execute("");
                currentState = queryAddState;             
                return true;
            }

            if (currentState == null)
                currentState = queryShowState;

            var newState = currentState.Execute(input);

            if (newState != null)
                currentState = newState;

            return true;
        }


    }



    internal abstract class State
    {
        public string Header => "<" + Name + ">" + " {0}" + Environment.NewLine + " {1}" + Environment.NewLine + 
                                new String('*', Console.WindowWidth-5) + Environment.NewLine
                                 + "{2}" + new String('*', Console.WindowWidth-5) + "{3}";

        public IInterpreterRepository Repository { get; set; }
        public ISpatialRepository SpatialRepository { get; set; }

        public abstract State Execute(string input);

        public string Name { get; set; }

        protected virtual void Reset()//todo check calls
        {   
            if(Repository.GlobalSymbolTable != null)
                Repository.GlobalSymbolTable.Reset();

            if (Repository.GlobalEntityDictionary != null)
                Repository.GlobalEntityDictionary.Clear();

            if (SpatialRepository != null)
                SpatialRepository.Reset();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

    }

    internal class QueryAddState : State
    {
        public QueryShowState ShowState { get; set; }


        public override State Execute(string input)
        {
            if(!string.IsNullOrEmpty(input) && !string.IsNullOrWhiteSpace(input))
                Repository.Query += input +  Environment.NewLine;

            ShowState.Execute(string.Empty);
            return null;
        }
    }

    internal class QueryClearState : State
    {
        public override State Execute(string input)
        {
            Repository.Query = string.Empty;
            return null;
        }
    }


    internal class ShowLicenceState : State
    {
        private readonly QueryAddState queryAddState;
        private const string LicenceFileName = "lic.txt";

        public ShowLicenceState(QueryAddState queryAddState, string name)
        {
            this.queryAddState = queryAddState;
            Name = name;
        }

        public override State Execute(string input)
        {
            if (!File.Exists(LicenceFileName))
            {
                Console.WriteLine("Licence file is missing...");
                return queryAddState;
            }

            var text = File.ReadAllText(LicenceFileName) + Environment.NewLine;
            Console.WriteLine(Header, "", "Content of " + LicenceFileName, text, Environment.NewLine + "Press enter to continue");
            return queryAddState;
        }
    }


    internal class QueryShowState : State
    {
        private readonly QueryAddState queryAddState;

        public QueryShowState(QueryAddState queryAddState)
        {
            this.queryAddState = queryAddState;
        }

        public override State Execute(string input)
        {
            string query = string.Empty;
            if (!string.IsNullOrEmpty(Repository.Query))
            {
                query = Repository.Query.Replace(") ", ")" + Environment.NewLine);
                query = query.Replace("func",  Environment.NewLine + "func");
            }

            Console.WriteLine(Header, "Type statement and press enter to add, -EA and enter to execute query", "Current Query", query, "");

            return queryAddState;
        }
    }

    internal class PerformanceTestState : State
    {
        private readonly IQueryReader queryReader;
        private readonly ISymbolVisitor symbolVisitor;
        private readonly IExecutionVisitor executionVisitor;
        private readonly IFuncVisitor funcVisitor;
        private readonly ILogger logger;
        private readonly ISettings settings;


        public PerformanceTestState(IInterpreterRepository repository, ISpatialRepository spatialRepository, IQueryReader queryReader, ISymbolVisitor symbolVisitor,
            IExecutionVisitor executionVisitor, IFuncVisitor funcVisitor, ILogger logger, ISettings settings, string name)
        {
            Repository = repository;
            SpatialRepository = spatialRepository;
            this.queryReader = queryReader;
            this.symbolVisitor = symbolVisitor;
            this.executionVisitor = executionVisitor;
            this.funcVisitor = funcVisitor;
            this.logger = logger;
            this.settings = settings;
            this.Name = name;
        }

        public override State Execute(string input)
        {
            var path = settings.Log.PathQueryFileIn;
            if (!File.Exists(path))
            {
                Console.WriteLine("Query file not found:" + path);
                return null;
            }
            var text = File.ReadAllText(path);
            var queries = text.Split('~');

            var j = 0;
            foreach (var query in queries)
            {
                j++;
                for (int i = 0; i < settings.Log.Cycles; i++)
                {
                    Console.WriteLine("Performance testing cycle " + i + ", query " + j);
                    var queryNode = queryReader.Parse(query);
                    //funcVisitor.Visit(queryNode); todo user func
                    symbolVisitor.Visit(queryNode);
                    executionVisitor.Visit(queryNode);
                    Reset();
                }
                logger.Write(j);
            }
            return null;
        }
    }


    internal class ExecuteAllState : State
    {
        private readonly SymbolClearState clearState;
        private readonly IQueryReader queryReader;
        private readonly ISymbolVisitor symbolVisitor;
        private readonly IExecutionVisitor executionVisitor;
        private readonly IFuncVisitor funcVisitor;
        private readonly SymbolTable globalSymbolTable;
        private readonly QueryShowState queryShowState;
        private readonly ResultShowState resultShowState;

        public ExecuteAllState(SymbolClearState clearState, IInterpreterRepository repository, ISpatialRepository spatialRepository, IQueryReader queryReader,
            ISymbolVisitor symbolVisitor, IExecutionVisitor executionVisitor, IFuncVisitor funcVisitor,
            string name, QueryShowState queryShowState, ResultShowState resultShowState)
        {
            Repository = repository;
            SpatialRepository = spatialRepository;
            this.clearState = clearState;
            this.queryReader = queryReader;
            this.symbolVisitor = symbolVisitor;
            this.executionVisitor = executionVisitor;
            this.funcVisitor = funcVisitor;
            this.queryShowState = queryShowState;
            this.resultShowState = resultShowState;

            Name = name;
        }

        public override State Execute(string input)
        {
            if (Repository.GlobalSymbolTable != null && Repository.GlobalSymbolTable.Symbols.Any())
            {
                Console.WriteLine("Press Y to delete current symbols before next query exection.");
                var key = Console.ReadKey(true);

                if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                    clearState.Execute("");
            }

            try
            {   
                var queryNode = queryReader.Parse(Repository.Query);
                //funcVisitor.Visit(queryNode); todo user func
                symbolVisitor.Visit(queryNode);
                executionVisitor.Visit(queryNode);

                Repository.GlobalSymbolTable = queryNode.SymbolTable;
            }
            catch (QueryException e)
            {   
                Console.Clear();
                Console.WriteLine(e.Message);

                return queryShowState;
            }


            Repository.Query = string.Empty;

            return resultShowState;
        }
    }

    internal class SymbolShowState : State
    {
        public SymbolShowState(IInterpreterRepository repository, string name)
        {
            Repository = repository;
            Name = name;
        }

        public override State Execute(string input)
        {
            var symbolNames = new [] {"no symbols stored..."};
            if (Repository.GlobalSymbolTable != null)
                symbolNames = Repository.GlobalSymbolTable.Symbols.Values.Select(v => v.Value).ToArray();
            
            string symbolOut = string.Empty;
            if (symbolNames.Length > 0)
                symbolOut = string.Join(Environment.NewLine, symbolNames);

            symbolOut = symbolOut + Environment.NewLine;

            Console.WriteLine(Header, "-SR <set/relation> to see content, -CS to delete all symbols", "Current Symbols", symbolOut, "");
            return null;
        }
    }


    internal class SymbolClearState : State
    {
        private readonly ISpatialRepository spatialRepository;
        private readonly SymbolShowState symbolShowState;
        private readonly QueryShowState queryShowState;

        public SymbolClearState(IInterpreterRepository repository, ISpatialRepository spatialRepository,
            SymbolShowState symbolShowState, QueryShowState queryShowState, string name)
        {
            SpatialRepository = spatialRepository;
            this.symbolShowState = symbolShowState;
            this.queryShowState = queryShowState;
            Repository = repository;
            Name = name;
        }

        public override State Execute(string input)
        {

            Process proc = Process.GetCurrentProcess();

            Console.WriteLine("Total Memory: {0}", GC.GetTotalMemory(true)/(1024d * 1024));
            Console.WriteLine("Deallocating memory...");

            if (Repository.GlobalSymbolTable != null)
            {
                var symbols = Repository.GlobalSymbolTable.Symbols.Select(p => p.Value);
                var entities = symbols.SelectMany(s => s.Tuples).SelectMany(e => e);

                foreach (var qlEntity in entities)
                    SpatialRepository.RemoveMeshByGlobalId(qlEntity.GlobalId);

                Reset();
            }
 
            Console.WriteLine("Total Memory: {0}", GC.GetTotalMemory(true)/(1024d * 1024));

            queryShowState.Execute("");

            return queryShowState;
        }


    }


    internal class ResultWriteState : State
    {
        private readonly IInterpreterRepository interpreterRepository;
        private readonly string name;
        private readonly ResultShowState resultShowState;

        public ResultWriteState(IInterpreterRepository interpreterRepository, string name, ResultShowState resultShowState)
        {
            this.interpreterRepository = interpreterRepository;
            this.name = name;
            this.resultShowState = resultShowState;
        }

        public override State Execute(string input)
        {
            var globalSymbolTable = interpreterRepository.GlobalSymbolTable;

            if (input.Length < 4 || !input.Contains(" "))
                return resultShowState;

            var splittedInput = input.Split(' ').Last();

            if (!globalSymbolTable.Symbols.ContainsKey(splittedInput))
                return resultShowState;

            var symbol = globalSymbolTable.Symbols[splittedInput];

            var sb = new StringBuilder();
            foreach (var tuple in symbol.Tuples)
                sb.AppendLine(String.Join(", ", tuple.Select(t => t.GlobalId)));
            
            Console.WriteLine("Exporting symbol values...");
            File.WriteAllText(symbol.Value +  ".txt", sb.ToString());

            return resultShowState;
        }
    }

    internal class ResultShowState : State
    {
        private readonly IInterpreterRepository interpreterRepository;
        private readonly QueryShowState queryShowState;
        private int height;
        private QLEntity[][] currentEntites;
        private int shownEntites;
        private bool allShown;
        private IEnumerable<string> currentAttributes;
        private string varName;
        private const string TupleSeperator = "|";

        public ResultShowState(IInterpreterRepository interpreterRepository, QueryShowState queryShowState, string name)
        {
            this.interpreterRepository = interpreterRepository;
            this.queryShowState = queryShowState;
            shownEntites = 0;
            Name = name;
        }

        public new void Reset()
        {
            shownEntites = 0;
            currentEntites = null;
            allShown = false;
        } 

        public override State Execute(string input)
        {
            var globalSymbolTable = interpreterRepository.GlobalSymbolTable;

            if (globalSymbolTable == null || globalSymbolTable.Symbols.Count == 0)
            {
                Reset();
                return queryShowState;
            }

            if(string.IsNullOrEmpty(input))
            {
                if (currentEntites == null)
                {
                    var lastSymbol = globalSymbolTable.Symbols.Values.Last();
                    ShowSymbol(lastSymbol);
                }

                else
                    ShowSymbol(varName,currentAttributes);
            }

            if (input.Length > 4 && input.Contains(" "))
            {
                var splittedInput = input.Split(' ').Last();

                if(globalSymbolTable.Symbols.ContainsKey(splittedInput))
                    ShowSymbol(globalSymbolTable.Symbols[splittedInput]);
            }

            if (allShown)
            {      
                Reset();
                return queryShowState;             
            }

            return null;
        }

        private void ShowSymbol(Symbol symbol)
        {
            if (!symbol.Tuples.Any())
            {
                allShown = true;
                Reset();
            }
                

            height = Console.WindowHeight - 5;
            shownEntites = 0;
            currentEntites = symbol.Tuples.ToArray();
            allShown = false;
            currentAttributes = symbol.Attributes;
            varName = symbol.Value;
            ShowSymbol(varName,currentAttributes);
        }


        private void ShowSymbol(string relName, IEnumerable<string> attributes)
        {
            var i = shownEntites;
            shownEntites += height;


            if (shownEntites > currentEntites.Length)
            {
                shownEntites = currentEntites.Length;
                allShown = true;
            }


            var sb = new StringBuilder();
            for (; i < shownEntites; i++)
                sb.AppendLine(string.Join("\t" + TupleSeperator + "\t", currentEntites[i].Select(e => e.ToString())));

            Console.WriteLine(Header,"Press enter to see next section", $"Symbol {relName} (Index {shownEntites} of {currentEntites.Length})", sb, "");
        }
    }

    internal class ShowSettingsState : State
    {
        private readonly ISettings settings;

        public ShowSettingsState(ISettings settings, string name)
        {
            this.settings = settings;
            Name = name;
        }

        public override State Execute(string input)
        {
            Console.WriteLine(settings);
            return null;
        }
    }
}
