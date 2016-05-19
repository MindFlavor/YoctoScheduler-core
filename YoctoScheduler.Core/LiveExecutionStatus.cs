using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{

    public class LiveExecutionStatus : DatabaseItemWithGUIDPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LiveExecutionStatus));

        public int TaskID { get; set; }

         public int ServerID { get; set; }

        public int? ScheduleID { get; set; }

        public DateTime LastUpdate { get; set; }

        public LiveExecutionStatus(int TaskID, int ServerID) : base()
        {
            this.TaskID = TaskID;
            this.ServerID = ServerID;

            LastUpdate = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S0}, TaskID={2:N0}, ServerID={3:N0}, ScheduleID={4:S}, LastUpdate={5:S}]",
                this.GetType().FullName,
                base.ToString(), 
                TaskID, ServerID,
                ScheduleID.HasValue ? ScheduleID.Value.ToString() : "<null>",
                LastUpdate.ToString());
        }

        public static LiveExecutionStatus New(SqlConnection conn, SqlTransaction trans, int TaskID, int ServerID, int? ScheduleID)
        {
            LiveExecutionStatus es = new LiveExecutionStatus(TaskID, ServerID)
            {
                ScheduleID = ScheduleID,
            };

            #region Database entry
            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO [live].[ExecutionStatus]
                       ([ScheduleID]
                       ,[TaskID]
                       ,[ServerID]
                       ,[LastUpdate])
                OUTPUT INSERTED.[GUID]
                 VALUES
                       (@ScheduleID
                       ,@TaskID
                       ,@ServerID
                       ,@LastUpdate)", conn, trans))
            {
                es.PopolateParameters(cmd);
                es.GUID = (Guid)cmd.ExecuteScalar();
            }
            #endregion
            log.DebugFormat("Created excecution {0:S}", es.ToString());
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

            param = new SqlParameter("@ServerID", System.Data.SqlDbType.Int);
            param.Value = ServerID;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@LastUpdate", System.Data.SqlDbType.DateTime);
            param.Value = LastUpdate;
            cmd.Parameters.Add(param);

            if (HasValidID())
            {
                param = new SqlParameter("@GUID", System.Data.SqlDbType.Int);
                param.Value = GUID;
                cmd.Parameters.Add(param);
            }
        }

        public override void PersistChanges(SqlConnection conn)
        {
            throw new NotImplementedException();
        }

        protected static LiveExecutionStatus ParseFromDataReader(SqlDataReader r)
        {
           var les= new LiveExecutionStatus(r.GetInt32(2), r.GetInt32(3))
            {
                GUID = r.GetGuid(0),                
                LastUpdate = r.GetDateTime(4)
            };

            if (!r.IsDBNull(1))
            {
                les.ScheduleID = r.GetInt32(1);
            }

            return les;
        }

        public static List<LiveExecutionStatus> GetAll(SqlConnection conn, SqlTransaction trans)
        {
            return GetAll(conn, trans, DateTime.Parse("1990-01-01"));
        }

        public static List<LiveExecutionStatus> GetAll(SqlConnection conn, SqlTransaction trans, DateTime minLastUpdate)
        {
            List<LiveExecutionStatus> lItems = new List<LiveExecutionStatus>();

            string stmt =
                    @"SELECT 
                       [GUID]
                      ,[ScheduleID]
                      ,[TaskID]
                      ,[ServerID]
                      ,[LastUpdate]
                  FROM [live].[ExecutionStatus]
                  WITH(XLOCK)
                  WHERE 
                       [LastUpdate] >= @lastUpdate";

            using (SqlCommand cmd = new SqlCommand(stmt, conn, trans))
            {
                var param = new SqlParameter("@lastUpdate", System.Data.SqlDbType.DateTime);
                param.Value = minLastUpdate;
                cmd.Parameters.Add(param);

                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lItems.Add(ParseFromDataReader(reader));
                    }
                }
            }

            return lItems;
        }
    }
}
