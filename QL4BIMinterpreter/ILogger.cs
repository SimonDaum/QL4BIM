using System;

namespace QL4BIMinterpreter
{
    public interface ILogger
    {
        void LogStart(string tag, int countIn);

        void LogStart(string tag, int[] countIn);

        void LogStop(int countOut);

        void AddEntry(string tag, int countIn, int countOut, long timing);

        int StatementIndex { get; set; }

        void Write(int index);

        bool UseDateInFileName { get; set; }

    }
}