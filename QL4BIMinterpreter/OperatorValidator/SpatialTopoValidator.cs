﻿
using System.Collections.Generic;
using QL4BIMinterpreter.QL4BIM;

namespace QL4BIMinterpreter
{
    class SpatialTopoValidator: OperatorValidator, ISpatialTopoValidator
    {
        public string[] TopoOperators { get; } = "Overlaps,Touches,Contains,Covers,CoveredBy,Disjoint,Equals".Split(',');

        public SpatialTopoValidator()
        {
            Name = "SpatialTopoValidator";

            var sig1 = new FunctionSignatur(SyUseVal.RelVa, new[] { SyUseVal.Set, SyUseVal.Set }, null, null);
            //todo tolerances
            //var sig2 = new FunctionSignatur(SyUseVal.Set, new[] { SyUseVal.Set, SyUseVal.String }, null, null);
            FunctionSignaturs.Add(sig1);
            //FunctionSignaturs.Add(sig2);
        }
    }
}