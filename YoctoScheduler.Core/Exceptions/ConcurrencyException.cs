using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Exceptions
{
    public class ConcurrencyException :Exception
    {
        public ConcurrencyException(string s, Exception innerException)
           : base(s, innerException) { }
        public ConcurrencyException(string s)
           : base(s) { }
    }
}
