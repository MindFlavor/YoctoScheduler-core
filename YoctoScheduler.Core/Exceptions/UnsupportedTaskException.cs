using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Exceptions
{
    public class UnsupportedTaskException :Exception
    {
        public string TaskType { get; set; }

        public UnsupportedTaskException(string TaskType)
        {
            this.TaskType = TaskType;
        }

        public override string ToString()
        {
            return string.Format("UnsupportedTaskException: task type {0:S} is not supported at this time", TaskType);
        }
    }
}
