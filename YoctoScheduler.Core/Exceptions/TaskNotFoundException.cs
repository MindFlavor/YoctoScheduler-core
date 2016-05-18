using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Exceptions
{
    public class TaskNotFoundException : ElementNotFoundException
    {
        public int TaskID { get; set; }

        public TaskNotFoundException(int TaskID)
        {
            this.TaskID = TaskID;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[TaskID={1:N0}]", this.GetType().FullName, TaskID);
        }
    }
}
