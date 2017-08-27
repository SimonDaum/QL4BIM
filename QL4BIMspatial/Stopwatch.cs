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
using System.Diagnostics;

namespace QL4BIMspatial
{
    public class Stopwatch 
    {
        private string discription;
        private System.Diagnostics.Stopwatch stopwatch;

        public void Start(string discription)
        {
            this.discription = discription;
            var message = discription + " started";
            Console.WriteLine(message);
            Console.Out.Flush();
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
        }

        public long Stop()
        {
            stopwatch.Stop();
            var ts = stopwatch.Elapsed;
            var timeString = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds );
            var message = String.Format(discription + " finished in {0}", timeString);
            Console.WriteLine(message);
            Console.Out.Flush();
            return stopwatch.ElapsedTicks;
        }

    }
}
