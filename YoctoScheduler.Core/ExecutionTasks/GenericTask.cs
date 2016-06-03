using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTasks
{
    public abstract class GenericTask : ITask
    {
        public abstract string Do();

        // This should throw an exception in case of problems
        // ie. NumberFormatException if a number cannot be parsed from a string with 
        // message like "Cannot convert the parameter to a number".
        // Repeated calls on this function should be allowed.
        public abstract void ParseConfiguration(string Payload);

        public abstract string SerializeConfiguration();

        public virtual string TaskName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public virtual YoctoScheduler.Core.Database.Task GenerateDatabaseTask()
        {
            return new Database.Task()
            {
                Type = TaskName,
                Payload = SerializeConfiguration()
            };
        }
    }
}
