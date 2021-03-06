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
using QL4BIMinterpreter.QL4BIM;
using QL4BIMspatial;

namespace QL4BIMinterpreter
{
    class SpatialQueryInterpreter : ISpatialQueryInterpreter
    {
        private readonly IOverlapOperator overlapOperator;
        private readonly ITouchOperator touchOperator;
        private readonly IDirectionalOperators directionalOperators;
        private readonly IDistanceOperator distanceOperator;
        private readonly ISpatialRepository spatialRepository;

        public SpatialQueryInterpreter(IOverlapOperator overlapOperator, ITouchOperator touchOperator, 
                                       IDirectionalOperators directionalOperators, IDistanceOperator distanceOperator,
                                        ISpatialRepository spatialRepository)
        {
            this.overlapOperator = overlapOperator;
            this.touchOperator = touchOperator;
            this.directionalOperators = directionalOperators;
            this.distanceOperator = distanceOperator;
            this.spatialRepository = spatialRepository;
        }

        public void Execute(string operatorName, RelationSymbol returnSymbol, SetSymbol parameterSymbol1, SetSymbol parameterSymbol2)
        {
            Console.WriteLine(operatorName + "'ing...");
            switch (operatorName)
            {
                case "Overlaps":
                    Overlap(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "OL":
                    Overlap(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "Touches":
                    Touch(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "TO":
                    Touch(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "AboveRelaxed":
                    AboveReleaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "AR":
                    AboveReleaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "AboveStrict":
                    AboveStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "AS":
                    AboveStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "BelowRelaxed":
                    BelowReleaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "BR":
                    BelowReleaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "BelowStrict":
                    BelowStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "BS":
                    BelowStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "WestRelaxed":
                    WestRelaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "WestStrict":
                    WestStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "WS":
                    WestStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "EastRelaxed":
                    EastRelaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "ER":
                    EastRelaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "EastStrict":
                    EastStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "ES":
                    EastStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "NorthRelaxed":
                    NorthRelaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "NR":
                    NorthRelaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "NorthStrict":
                    NorthStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "NS":
                    NorthStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "SouthRelaxed":
                    SouthRelaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "SR":
                    SouthRelaxed(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "SouthStrict":
                    SouthStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
                case "SS":
                    SouthStrict(returnSymbol, parameterSymbol1, parameterSymbol2);
                    break;
            }
        }

        public void Overlap(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, overlapOperator.Overlap);
        }

        public void Touch(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {   
            var sw = new Stopwatch();
            sw.Start("Touch");
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, touchOperator.Touch);
            sw.Stop();
        }

        public void AboveReleaxed(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.AboveOfRelaxed);
        }

        public void AboveStrict(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.AboveOfStrict);
        }

        public void  BelowReleaxed(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.BelowOfRelaxed);
        }

        public void BelowStrict(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.BelowOfStrict);
        }

        public void WestRelaxed(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.WestOfRelaxed);
        }

        public void WestStrict(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.WestOfStrict);
        }

        public void EastRelaxed(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.EastOfRelaxed);
        }

        public void EastStrict(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.EastOfStrict);
        }


        public void NorthRelaxed(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.NorthOfRelaxed);
        }

        public void NorthStrict(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.NorthOfStrict);
        }

        public void SouthRelaxed(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.SouthOfRelaxed);
        }

        public void SouthStrict(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.SouthOfStrict);
        }



        public void Distance(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB)
        {
            CartesianProduct(returnSymbol, parameterSymbolA, parameterSymbolB, directionalOperators.AboveOfRelaxed);
        }




        private void CartesianProduct(RelationSymbol returnSymbol, SetSymbol parameterSymbolA, SetSymbol parameterSymbolB,
            Func<TriangleMesh, TriangleMesh, bool> operation)
        {
            foreach (var ifcEntityA in parameterSymbolA.Entites)
            {
                var meshA = GetMesh(ifcEntityA);
                if (meshA == null)
                    continue;

                foreach (var ifcEntityB in parameterSymbolB.Entites)
                {
                    var meshB = GetMesh(ifcEntityB);
                    if (meshB == null)
                        continue;

                    if (operation(meshA, meshB))
                        returnSymbol.AddTuple(new[] { ifcEntityA, ifcEntityB });
                }
            }
        }

        private TriangleMesh GetMesh(QLEntity qlEntityA)
        {
            var gloablIdA = qlEntityA.GetGloablId();
            if (string.IsNullOrEmpty(gloablIdA))
                return null;

            var meshA = spatialRepository.MeshByGlobalId(gloablIdA);
            return meshA;
        }


    }
}
