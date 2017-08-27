/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMspatial.

QL4BIMspatial is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMspatial is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMspatial. If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Practices.Unity;

namespace QL4BIMspatial
{
    public class MainInterface
    {
        private readonly ISpatialMain spatialMain;
        private readonly Settings settings;
        private readonly IVectorDirOperator vectorDirOperator;

        public MainInterface(IUnityContainer container = null)
        {   
            if(container == null)
                container = new UnityContainer();
            RegisterTypes(container);
       
            spatialMain = container.Resolve<ISpatialMain>();
            settings = container.Resolve<Settings>();
            vectorDirOperator = container.Resolve<IVectorDirOperator>();
        }

        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<ITriangleIntersector, TriangleIntersector>();
            container.RegisterType<IPrismTriangleIntersector, PrismTriangleIntersector>();
            container.RegisterType<IIntervalIntersector, IntervalIntersector>();
            container.RegisterType<IRayTriangleIntersector, RayTriangleIntersector>();
            container.RegisterType<IRayBoxIntersector, RayBoxIntersector>();

            container.RegisterType<IPolygonMerger, PolygonMerger>();
            container.RegisterType<IPointSampler, PointSampler>();

            container.RegisterType<IOverlapOperator, OverlapOperator>();
            container.RegisterType<IDistanceOperator, DistanceOperator>();
            container.RegisterType<IDirectionalOperators, DirectionalOperators>();
            container.RegisterType<ITouchOperator, TouchOperator>();
            container.RegisterType<IInsideTester, InsideTester>();
            container.RegisterType<IContainOperator, ContainOperator>();
            container.RegisterType<IEqualOperator, EqualOperator>();

            container.RegisterType<IVectorDirOperator, VectorDirOperator>(new ContainerControlledLifetimeManager());

            container.RegisterType<ISpatialRepository, SpatialRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISettings, Settings>(new ContainerControlledLifetimeManager());

            container.RegisterType<IX3DExporter, X3DExporter>();
            container.RegisterType<IX3DImporter, X3DImporter>();

            container.RegisterType<ISpatialMain, SpatialMain>();
            container.RegisterType<IIfcReader, IfcReader>();
        }


        public Settings GetSettings()
        {
            settings.Touch.PositiveOffset = 0.05;
            settings.Touch.NegativeOffsetAsRatio = 1;
            settings.Direction.PositiveOffset = 0.1;
            settings.Direction.RaysPerSquareMeter = 10;

            settings.Direction.SupportAnyDirection = false;

            settings.Log.Cycles = 10;
            settings.Log.PathLogFileOut = @"C:\temp\perfTestResults_";
            settings.Log.PathQueryFileIn = @"C:\temp\perfTest.txt";

            settings.RsTreeSetting.SmallM = 3;
            settings.RsTreeSetting.BigM = 7;

            return settings;
        }

        public void Import(IEnumerable<IndexedFaceSet> faceSets)
        {
            spatialMain.ImportAndReset(faceSets);
        }

        public void Import(IEnumerable<TriangleMesh> meshes)
        {
            spatialMain.ImportAndReset(meshes);
        }

        public void AddDirection(string name, DenseVector vector)
        {
            vectorDirOperator.AddDirection(name, vector);
        }

        public void AddFaceSet(IndexedFaceSet faceSet)
        {   
            if(settings.Direction.SupportAnyDirection)
                vectorDirOperator.AddIndexedFaceSet(faceSet);
        }

        public void Overlap()
        {
            spatialMain.Overlap();
        }

        public void Contain()
        {
            spatialMain.Contain();
        }

        public void ArbitraryDirection()
        {
            spatialMain.ArbitraryDirection();
        }

        public void AboveStrict()
        {
            spatialMain.AboveOfStrict();
        }

        public void AboveRelaxed()
        {
            spatialMain.AboveOfRelaxed();
        }

        public void BelowRelaxed()
        {
            spatialMain.BelowOfRelaxed();
        }

        public void BelowStrict()
        {
            spatialMain.BelowOfStrict();
        }

        public void NorthRelaxed()
        {
            spatialMain.NorthOfRelaxed();
        }


        public void NorthStrict()
        {
            spatialMain.NorthOfStrict();
        }

        public void SouthRelaxed()
        {
            spatialMain.SouthOfRelaxed();
        }

        public void SouthStrict()
        {
            spatialMain.SouthOfStrict();
        }

        public void EastRelaxed()
        {
            spatialMain.EastOfRelaxed();
        }

        public void EastStrict()
        {
            spatialMain.EastOfStrict();
        }

        public void WestRelaxed()
        {
            spatialMain.WestofRelaxed();
        }

        public void WestStrict()
        {
            spatialMain.WestofStrict();
        }

        public void Distance()
        {
            spatialMain.Distance();
        }

        public void Touch()
        {
            spatialMain.Touch();
        }

        public void Equal()
        {
            spatialMain.Equal();
        }

        public void Export()
        {
            spatialMain.Export();
        }

        public void ExportTrees()
        {
            spatialMain.ExportTrees();
        }


        public int GetTriangleCount()
        {
            return spatialMain.GetTriangleCount();
        }
    }
}
