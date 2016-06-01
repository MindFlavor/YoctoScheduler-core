using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    public class ExecutionQueueItem : DatabaseItemWithGUIDPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ExecutionQueueItem));

        public int TaskID { get; set; }
        public Priority Priority { get; set; }
        public int? ScheduleID { get; set; }
        public DateTime InsertDate { get; set; }

        public ExecutionQueueItem() : base()
        {
        }

        public ExecutionQueueItem(int TaskID, Priority Priority) : base()
        {
            this.TaskID = TaskID;
            this.Priority = Priority;
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

        public override void PopolateParameters(SqlCommand cmd)
        {
            SqlParameter param = new SqlParameter("@ScheduleID", System.Data.SqlDbType.Int);
            if (ScheduleID.HasValue)
                param.Value = ScheduleID.Value;
            else
                param.Value = DBNull.Value;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@TaskID", System.Data.SqlDbType.Int);
            param.Value = TaskID;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@Priority", System.Data.SqlDbType.Int);
            param.Value = Priority;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@InsertDate", System.Data.SqlDbType.DateTime);
            param.Value = InsertDate;
            cmd.Parameters.Add(param);

            if (HasValidID())
            {
                param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
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
                ScheduleID = r.GetInt32(3);
            else
                ScheduleID = null;
        }

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            throw new NotImplementedException();
        }
    }
}
