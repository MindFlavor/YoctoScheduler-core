using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    public class ExecutionItem : DatabaseItemWithGUIDPK
    {
        [DatabaseProperty(Size = 4)]
        [System.Runtime.Serialization.DataMember]
        public int TaskID { get; set; }

        [DatabaseProperty(Size = 4)]
        [System.Runtime.Serialization.DataMember]
        public Priority? Priority { get; set; }

        [DatabaseProperty(Size = 4)]
        [System.Runtime.Serialization.DataMember]
        public Guid? ScheduleID { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 4)]
        public int? ServerID { get; set; }

        [DatabaseProperty(Size = 8)]
        [System.Runtime.Serialization.DataMember]
        public DateTime Inserted { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 8)]
        public DateTime LastUpdate { get; set; }

        [DatabaseProperty(Size = 4)]
        [System.Runtime.Serialization.DataMember]
        public TaskStatus Status { get; set; }

        [DatabaseProperty(Size = -1)]
        [System.Runtime.Serialization.DataMember]
        public string ReturnCode { get; set; }

        public ExecutionItem() { }

        public ExecutionItem(Guid ID,  int TaskID, Priority? Priority, Guid? ScheduleID, int? ServerID, DateTime Inserted, DateTime LastUpdate, TaskStatus Status, string ReturnCode) 
        {
            this.ID = ID;
            this.TaskID = TaskID;
            this.Priority = Priority;
            this.ScheduleID = ScheduleID;
            this.ServerID = ServerID;
            this.Inserted = Inserted;
            this.LastUpdate = LastUpdate;            
            this.Status = Status;
            this.ReturnCode = ReturnCode;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S0}, Status={2:S0}]",
                this.GetType().FullName,
                Status.ToString());
        }

        public override void ParseFromDataReader(System.Data.SqlClient.SqlDataReader r)
        {
            ID = r.GetGuid(0);
            TaskID = r.GetInt32(1);
            if (!r.IsDBNull(2))
                Priority = (Priority)r.GetInt32(2);
            else
                Priority = null;

            if (!r.IsDBNull(3))
                ScheduleID = r.GetGuid(3);
            else
                ScheduleID = null;

            if (!r.IsDBNull(4))
                Inserted = r.GetDateTime(4);
            else
                Inserted = DT_NEVER;

            if (!r.IsDBNull(5))
                ServerID = r.GetInt32(5);
            else
                ServerID = null;               

            if (!r.IsDBNull(6))
                LastUpdate = r.GetDateTime(6);
            else
                LastUpdate = DT_NEVER;

            if (!r.IsDBNull(7))
                ReturnCode = r.GetString(7);
            else
                ReturnCode = null;

            Status = (TaskStatus)r.GetInt32(8);
        }
    }
}
