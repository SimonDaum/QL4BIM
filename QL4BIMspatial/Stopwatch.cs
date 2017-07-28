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
