using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTasks
{
    public interface ITask
    {
        string Do();
    
        // This should throw an exception in case of problems
        // ie. NumberFormatException if a number cannot be parsed from a string with 
        // message like "Cannot convert the parameter to a number".
        // Repeated calls on this function should be allowed.
        void ParseConfiguration(string Payload);

        string SerializeConfiguration();
    }
}
