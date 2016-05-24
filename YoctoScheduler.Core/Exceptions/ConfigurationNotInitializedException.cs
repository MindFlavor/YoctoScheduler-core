using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Exceptions
{
    public class ConfigurationNotInitializedException : Exception
    {
        public override string ToString()
        {
            return "ConfigurationNotInitializedException: Server.Configuration static field must be configured prior using the server";
        }
    }
}
