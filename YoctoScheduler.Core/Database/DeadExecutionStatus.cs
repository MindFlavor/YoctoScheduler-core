using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [DatabaseKey(DatabaseName = "GUID", Size = 16)]
    public class DeadExecutionStatus : LiveExecutionStatus
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DeadExecutionStatus));

        [DatabaseProperty(Size = 4)]
        public TaskStatus Status { get; set; }

        [DatabaseProperty(Size = -1)]
        public string ReturnCode { get; set; }

        public DeadExecutionStatus()
        { }

        public DeadExecutionStatus(LiveExecutionStatus des, TaskStatus Status)
            : this(des, Status, null) { }

        public DeadExecutionStatus(LiveExecutionStatus les, TaskStatus Status, string ReturnCode) : base(les.TaskID, les.ServerID, les.ScheduleID)
        {
            ID = les.ID;
            this.LastUpdate = les.LastUpdate;
            this.Inserted = les.Inserted;

            this.Status = Status;
            this.ReturnCode = ReturnCode;
            this.Inserted = Inserted;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S0}, Status={2:S0}]",
                this.GetType().FullName,
                base.ToString(),
                Status.ToString());
        }
    }
}
