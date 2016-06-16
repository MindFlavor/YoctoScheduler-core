using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Exceptions
{
    public class ServerInitializationException : Exception
    {
        public ServerInitializationException(string s, Exception innerException)
           : base(s, innerException) { }
        public ServerInitializationException(string s)
           : base(s) { }
    }
}
