using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL4BIMprimitives
{
    public class QueryException : Exception
    {
        public QueryException(string message) : base(message)
        {

        }
    }
}
