using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public class ExecutionQueueItem : DatabaseItemWithGUIDPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ExecutionQueueItem));

        public int TaskID { get; set; }
        public Priority Priority { get; set; }
        public int? ScheduleID { get; set; }
        public DateTime InsertDate { get; set; }

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

        public static ExecutionQueueItem New(SqlConnection conn, SqlTransaction trans, int TaskID, Priority priority, int? ScheduleID)
        {
            ExecutionQueueItem es = new ExecutionQueueItem(TaskID, priority)
            {
                ScheduleID = ScheduleID,
                InsertDate = DateTime.Now
            };

            #region Database entry
            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO [live].[ExecutionQueue]
                       (
                       [TaskID]
                       ,[Priority]
                       ,[ScheduleID]
                       ,[InsertDate])
                OUTPUT INSERTED.[GUID]
                 VALUES
                       (@TaskID
                       ,@Priority
                       ,@ScheduleID
                       ,@InsertDate)", conn, trans))
            {
                es.PopolateParameters(cmd);
                es.GUID = (Guid)cmd.ExecuteScalar();
            }
            #endregion
            log.DebugFormat("Created {0:S}", es.ToString());
            return es;
        }

        protected internal virtual void PopolateParameters(SqlCommand cmd)
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
                param.Value = GUID;
                cmd.Parameters.Add(param);
            }
        }

        public static ExecutionQueueItem GetAndLockFirst(SqlConnection conn, SqlTransaction trans)
        {
            string stmt =
                    @"SELECT TOP 1
		                    [GUID]
		                    ,[TaskID]
		                    ,[Priority]
		                    ,[ScheduleID]
		                    ,[InsertDate]
                      FROM [live].[ExecutionQueue] WITH (XLOCK)
                      ORDER BY 
		                    [Priority] DESC,
		                    [InsertDate] DESC;";

            using (SqlCommand cmd = new SqlCommand(stmt, conn, trans))
            {
                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return ParseFromDataReader(reader);
                    else
                        return null;
                }
            }
        }

        protected static ExecutionQueueItem ParseFromDataReader(SqlDataReader r)
        {
            var les = new ExecutionQueueItem(r.GetInt32(1), (Priority)r.GetInt32(2))
            {
                GUID = r.GetGuid(0),
                InsertDate = r.GetDateTime(4)
            };

            if (!r.IsDBNull(3))
            {
                les.ScheduleID = r.GetInt32(3);
            }

            return les;
        }

        public void Delete(SqlConnection conn, SqlTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(
                @"DELETE FROM [live].[ExecutionQueue]
	                    WHERE 
		                    [GUID] = @GUID;",
                conn, trans))
            {
                SqlParameter param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier);
                param.Value = GUID;
                cmd.Parameters.Add(param);

                cmd.Prepare();
                if (cmd.ExecuteNonQuery() != 1)
                {
                    throw new Exceptions.ConcurrencyException(
                        string.Format("Delete from [live].[ExecutionQueue] failed because no entry with GUID {0:S} was found", GUID.ToString(),
                        null));
                }
            }
        }

        public override void PersistChanges(SqlConnection conn)
        {
            throw new NotImplementedException();
        }
    }
}
