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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using QL4BIMspatial;

namespace QL4BIMinterpreter
{
    public class Logger : ILogger
    {
        private readonly string fileName;

        private List<LogEntry> logs;
        private string tag;
        private int[] countIn;

        private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public Logger(ISettings settings)
        {
            var path = settings.Log.PathLogFileOut;
            var dir = Path.GetDirectoryName(path);
            var file = Path.GetFileNameWithoutExtension(path) + "{0}";
            logs = new List<LogEntry>();

            if (!Directory.Exists(dir))
	        {
		        Console.Write(String.Format(
                    "settings.Log.PathLogFileOut directory \"{0}\" does not exist. \nCheck settings... (press any key to continue)",
                    dir));
                Console.ReadKey();
				return;
	        }
                
            var date = UseDateInFileName ? DateTime.Now.ToString("d_M_yyyy_HH_mm_ss") : string.Empty;
            fileName = Path.Combine(dir, file + date + ".csv");
        }

        public void LogStart(string tag, int countIn)
        {
            LogStart(tag, new[] {countIn});
        }

        public void LogStart(string tag, int[] countIn)
        {
            this.tag = tag;
            this.countIn = countIn;
            if(stopwatch.IsRunning)
                throw new InvalidOperationException();
            stopwatch.Start();
        }

        public void LogStop(int countOut)
        {
            stopwatch.Stop();
            int allIn = countIn.Aggregate(1, (x, y) => x * y);
            logs.Add(new LogEntry(tag + "_" +  StatementIndex, allIn, countOut, stopwatch.ElapsedMilliseconds));
            stopwatch.Reset();
        }

        public void AddEntry(string tag, int countIn, int countOut, long timing)
        {
            logs.Add(new LogEntry(tag + "_" + StatementIndex, countIn, countOut, timing));
        }

        public int StatementIndex { get; set; }


        public void Write(int index)
        {   
            var groups = logs.GroupBy(e => e.tag).ToArray();
            var sb = new StringBuilder();
            foreach (var grouping in groups)
            {
                var first = grouping.First();
                var timing = grouping.Average(g => g.timing);
                sb.AppendFormat("{0}\t{1}\t{2}\t{3}\n",  first.tag, string.Join(" ",first.countIn), first.countOut, timing.ToString("F2",CultureInfo.CurrentCulture));
            }

            logs.Clear();

            try
            {
                File.WriteAllText(string.Format(fileName, index), sb.ToString());
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

        }

        public bool UseDateInFileName { get; set; }


        class LogEntry
        {
            public  string tag;
            public  int countIn;
            public  int countOut;
            public long timing;

            public LogEntry(string tag, int countIn, int countOut, long timing)
            {
                this.tag = tag;
                this.countIn = countIn;
                this.countOut = countOut;
                this.timing = timing;
            }
        }
    }
}
