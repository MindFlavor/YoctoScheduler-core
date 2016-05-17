using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoScheduler.Core;

namespace YoctoScheduler.Server
{
    public class ExecutionStatus
    {
        public int TaskId { get; set; }

        public int ServerId { get; set; }

        protected ExecutionStatus(int TaskId, int ServerId)
        {
            this.TaskId = TaskId;
            this.ServerId = ServerId;
        }

        public static ExecutionStatus CreateExecutionStatus(int TaskId, int ServerId)
        {
            using (MasterModel mm = new MasterModel())
            {
                var innerES = new YoctoScheduler.Core.ExecutionStatus() { TaskID = TaskId, ServerID = ServerId, LastUpdate = DateTime.Now };
                mm.ExecutionStatus.Add(innerES);
                mm.SaveChanges();
            }

            return new ExecutionStatus(TaskId, ServerId);
        }

        public override string ToString()
        {
            return string.Format("ExecutionStatus[ServerGuid={0:N0}, TaskId={1:N0}]", ServerId, TaskId);
        }
    }
}
