using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [System.Runtime.Serialization.DataContract]
    [DatabaseKey(DatabaseName = "GUID", Size = 16)]
    public class DeadExecutionStatus : LiveExecutionStatus
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DeadExecutionStatus));

        [DatabaseProperty(Size = 4)]
        [System.Runtime.Serialization.DataMember]
        public TaskStatus Status { get; set; }

        [DatabaseProperty(Size = -1)]
        [System.Runtime.Serialization.DataMember]
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

        public override void ParseFromDataReader(SqlDataReader r)
        {
            ScheduleID = null;
            if (!r.IsDBNull(1))
                ScheduleID = r.GetInt32(1);
            ID = r.GetGuid(0);
            TaskID = r.GetInt32(2);
            Status = (TaskStatus)r.GetInt32(3);

            if (!r.IsDBNull(4))
                ReturnCode = r.GetString(4);

            ServerID = r.GetInt32(5);
            Inserted = r.GetDateTime(6);
            LastUpdate = r.GetDateTime(7);
        }
    }
}
