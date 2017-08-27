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

using System;
using System.Globalization;
using System.Text;
using QL4BIMindexing;
using QL4BIMprimitives;

namespace QL4BIMspatial
{
    public class Settings : ISettings
    {
        private static double positiveOffset;
        private static double negativeOffsetAsRatio;

       
        private static double CalcNegativeOffset()
        {
            var result = positiveOffset * negativeOffsetAsRatio * (-1);
            return result;
        }

        public class ContainSetting
        {
            public double NegativeOffset
            {
                get { return CalcNegativeOffset(); }

            }
        }

        public class DirectionSetting
        {
            public double PositiveOffset { get; set; }
            public int RaysPerSquareMeter { get; set; }
            public bool SupportAnyDirection{ get; set; }
        }

        public class DistanceSetting
        {
            public double RoundToZero { get; set; }
            public double GlobalThreshold { get; set; }
        }

        public class OverlapSetting
        {
            public double NegativeOffset
            {
                get { return CalcNegativeOffset(); }

            }
        }

        public class TouchSetting
        {
            public double PositiveOffset
            {
                get { return positiveOffset; }
                set { positiveOffset = value; }
            }

            public double NegativeOffsetAsRatio
            {
                get { return negativeOffsetAsRatio; }
                set { negativeOffsetAsRatio = value; }
            }

            public double NegativeOffset
            {
                get { return CalcNegativeOffset(); }
            }

            public double GetNegativeOffset(double positivOffset)
            {
                return positivOffset * negativeOffsetAsRatio; 
            }
        }

        public class EqualSetting
        {
            public int SamplePerSquareMeter { get; set; }
            public double GlobalThreshold { get; set; }
        }

        public class LogSetting
        {
            public int Cycles { get; set; }
            public string PathLogFileOut { get; set; }
            public string PathQueryFileIn { get; set; }
        }

        public class RTreeSetting
        {
            private int smallM;
            private int bigM;

            public int SmallM
            {
                get { return smallM; }
                set
                {
                    smallM = value;
                    RTree<Triangle>.MinNodeEntriesGlobal = smallM;
                }
            }

            public int BigM
            {
                get { return bigM; }
                set
                {
                    bigM = value;
                    RTree<Triangle>.MaxNodeEntriesGlobal = bigM;
                }
            }
        }

        public ContainSetting Contain { get; set; }
        public DirectionSetting Direction { get; set; }
        public DistanceSetting Distance { get; set; }
        public OverlapSetting Overlap { get; set; }
        public TouchSetting Touch { get; set; }
        public EqualSetting Equal { get; set; }
        public LogSetting Log { get; set; }
        public RTreeSetting RsTreeSetting { get; set; }

        public Settings()
        {   
            Direction = new DirectionSetting();
            Distance = new DistanceSetting();
            Overlap = new OverlapSetting();
            Touch = new TouchSetting();
            Contain = new ContainSetting();
            Equal = new EqualSetting();
            Log = new LogSetting();
            RsTreeSetting = new RTreeSetting();

            RsTreeSetting.SmallM = 10;
            RsTreeSetting.BigM = 20;

            //Direction.PositiveOffset = 0.0001;
            //Direction.RaysPerSquareMeter = 300;

            //Distance.RoundToZero = 0.00001;
            //Distance.GlobalThreshold = 3;

            //Touch.PositiveOffset = 10;
            //Touch.NegativeOffsetAsRatio = 1;

            //Equal.GlobalThreshold = 0.50;
            //Equal.SamplePerSquareMeter = 10;
        }

        public override string ToString()
        {
            var culure = CultureInfo.CreateSpecificCulture("en-US");
            var sb = new StringBuilder();
            sb.AppendLine("DirectionSetting:");
            sb.AppendLine("\tPositiveOffset:\t" + Direction.PositiveOffset.ToString("F4", culure));
            sb.AppendLine("\tRaysSqrMeter:\t" + Direction.RaysPerSquareMeter.ToString(culure));
            sb.AppendLine("DistanceSetting:");
            sb.AppendLine("\tRoundToZero:\t" + Distance.RoundToZero.ToString("F4", culure));
            sb.AppendLine("\tGlThreshold:\t " + Distance.GlobalThreshold.ToString("F4", culure));
            sb.AppendLine("OverlapSetting:");
            sb.AppendLine("\tNegativeOffset:\t" + Overlap.NegativeOffset.ToString("F4", culure));
            sb.AppendLine("TouchSetting:");
            sb.AppendLine("\tPositiveOffset:\t" + Touch.PositiveOffset.ToString("F4", culure));
            sb.AppendLine("\tNegativeOffset:\t" + Touch.NegativeOffset.ToString("F4", culure));
            sb.AppendLine("ContainSetting:");
            sb.AppendLine("\tNegativeOffset:\t" + Contain.NegativeOffset.ToString("F4", culure));
            sb.AppendLine("EqualSetting:");
            sb.AppendLine("\tGlThreshold:\t" + Equal.GlobalThreshold.ToString("F4", culure));
            sb.AppendLine("\tSamplesSqrMeter:\t" + Equal.SamplePerSquareMeter.ToString(culure));
            sb.AppendLine("LogSetting:");
            sb.AppendLine("\tCycles:\t" + Log.Cycles);
            sb.AppendLine("\tPathQueryFileIn:\t" + Log.PathQueryFileIn);
            sb.AppendLine("\tPathLogFileOut:\t" + Log.PathLogFileOut);
            sb.AppendLine("RsTreeSetting:");
            sb.AppendLine("\tSmallM:\t" + RsTreeSetting.SmallM);
            sb.AppendLine("\tBigM:\t" + RsTreeSetting.BigM);
            return sb.ToString();
        }
    }
}
