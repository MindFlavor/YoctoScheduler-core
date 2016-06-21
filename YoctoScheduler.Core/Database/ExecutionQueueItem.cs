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
    public class ExecutionQueueItem : DatabaseItemWithGUIDPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ExecutionQueueItem));

        [DatabaseProperty(Size = 4)]
        [System.Runtime.Serialization.DataMember]
        public int TaskID { get; set; }

        [DatabaseProperty(Size = 4)]
        [System.Runtime.Serialization.DataMember]
        public Priority Priority { get; set; }

        [DatabaseProperty(Size = 4)]
        [System.Runtime.Serialization.DataMember]
        public Guid? ScheduleID { get; set; }

        [DatabaseProperty(Size = 8)]
        [System.Runtime.Serialization.DataMember]
        public DateTime InsertDate { get; set; }

        public ExecutionQueueItem() : base()
        {
        }

        public ExecutionQueueItem(int TaskID, Priority Priority) : base()
        {
            this.TaskID = TaskID;
            this.Priority = Priority;
        }

        public override T Clone<T>(SqlConnection conn, SqlTransaction trans)
        {
            // this check will take care of the SQL DateTime limits
            // (DateTime.MinValue would throw a SQLException due being too far away in the past)
            if (InsertDate == DateTime.MinValue)
                InsertDate = DateTime.Now;

            return base.Clone<T>(conn, trans);
        }


        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, TaskID={2:S}, ScheduleID={3:S}, Priority={4:S}, InsertDate={5:S}]",
                this.GetType().FullName,
                base.ToString(),
                TaskID.ToString(),
                ScheduleID.HasValue ? ScheduleID.Value.ToString() : "<null>",
                Priority.ToString(),
                InsertDate.ToString(LOG_TIME_FORMAT));
        }

        public static ExecutionQueueItem GetAndLockFirst(SqlConnection conn, SqlTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("ExecutionQueueItem.GetAndLockFirst"), conn, trans))
            {
                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ExecutionQueueItem eqi = new ExecutionQueueItem();
                        eqi.ParseFromDataReader(reader);
                        return eqi;
                    }
                    else
                        return null;
                }
            }
        }

        public override void ParseFromDataReader(SqlDataReader r)
        {
            TaskID = r.GetInt32(1);
            Priority = (Priority)r.GetInt32(2);
            ID = r.GetGuid(0);
            InsertDate = r.GetDateTime(4);
            if (!r.IsDBNull(3))
                ScheduleID = r.GetGuid(3);
            else
                ScheduleID = null;
        }
    }
}
