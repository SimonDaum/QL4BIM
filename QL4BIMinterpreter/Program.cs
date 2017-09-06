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
using System.IO;
using Microsoft.Practices.Unity;
using QL4BIMinterpreter.OperatorsLevel0;
using QL4BIMinterpreter.OperatorsLevel1;
using QL4BIMinterpreter.P21;
using QL4BIMinterpreter.QL4BIM;
using QL4BIMspatial;

namespace QL4BIMinterpreter
{
    class Program
    {
        private static IContext _context;

        static void Main(string[] args)
        {
            SetupEngineDll();

            var env = Environment.Is64BitProcess ? "64bit" : "32bit";
             Console.WriteLine("->QL4BIM " + env + System.Reflection.Assembly.GetEntryAssembly().Location);


            var container = new UnityContainer();
            MainInterface mainInterface = new MainInterface(container);
            mainInterface.GetSettings();

            container.RegisterType<ILogger, Logger>(new ContainerControlledLifetimeManager());

            container.RegisterType<IInterpreterRepository, InterpreterRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IP21Repository, P21Repository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IP21Reader, P21Reader>(new ContainerControlledLifetimeManager());

            container.RegisterType<ISpatialQueryInterpreter, SpatialQueryInterpreter>();
            container.RegisterType<IQueryReader, QueryReader>();
            container.RegisterType<ISymbolVisitor, SymbolSymbolVisitor>();
            container.RegisterType<IExecutionVisitor, ExecutionVisitor>();
            container.RegisterType<IFuncVisitor, FuncVisitor>();

            container.RegisterType<IImportModelOperator, ImportModelOperator>();
            container.RegisterType<IExportModelOperator, ExportModelOperator>();
            container.RegisterType<ITypeFilterOperator, TypeFilterOperator>();
            container.RegisterType<IAttributeFilterOperator, AttributeFilterOperator>();
            container.RegisterType<IDereferenceOperator, DereferenceOperator>();
            container.RegisterType<IProjectorOperator, ProjectorOperator>();
            container.RegisterType<IPropertyFilterOperator, PropertyFilterOperator>();
            container.RegisterType<IDeassociaterOperator, DeassociaterOperator>();
            container.RegisterType<ITaskTimerOperator, TimeResolverOperator>();
            container.RegisterType<IMaximumOperator, MaximumOperator>();

            container.RegisterType<IOperatorValidator, OperatorValidator>();
            container.RegisterType<IArgumentFilterValidator, ArguementFilterValidator>();
            container.RegisterType<IImportModelValidator, ImportModelValidator>();
            container.RegisterType<ITypeFilterValidator, TypeFilterValidator>();
            container.RegisterType<IDereferencerValidator, DereferencerValidator>();
            container.RegisterType<IProjectorValidator, ProjectorValidator>();
            container.RegisterType<IPropertyFilterValidator, PropertyFilterValidator>();
            container.RegisterType<IDeaccociaterValidator, DeassociaterValidator>();
            container.RegisterType<ITaskTimerValidator, TaskTimerValidator>();
            container.RegisterType<IMaximumValidator, MaximumValidator>();

            container.RegisterType<IAstBuilder, AstBuilder>();

            container.RegisterType<ISpatialTopoValidator, SpatialTopoValidator>();

            container.RegisterType<IContext, Context>();

            QLEntityExtension.Repository = container.Resolve<IP21Repository>();

            _context = container.Resolve<IContext>();


            while (true)
            {
                if (!_context.Execute(Console.ReadLine()))
                    break;
            }
        }

        private static void SetupEngineDll()
        {
            try
            {
                var curDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                var engineX32 = Path.Combine(curDir, "ifcengineX32.dll");
                var engineX64 = Path.Combine(curDir, "ifcengineX64.dll");
                if (!File.Exists(engineX32))
                    File.Copy(Path.Combine(curDir, @"..\..\bin\ifcengineX32.dll"), engineX32);
                if (!File.Exists(engineX64))
                    File.Copy(Path.Combine(curDir, @"..\..\bin\ifcengineX64.dll"), engineX64);
            }
            catch (Exception e)
            {
                Console.WriteLine("IfcEngine binary error...check installation");
                Console.WriteLine(e);
                throw;
            }

        }
    }
}
