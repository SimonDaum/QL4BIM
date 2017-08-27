using System;
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
            if (Environment.Is64BitProcess)
                Console.WriteLine("->QL4BIM X64, " + System.Reflection.Assembly.GetEntryAssembly().Location);
            else
                Console.WriteLine("->QL4BIM X86, " + System.Reflection.Assembly.GetEntryAssembly().Location);

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


    }
}
