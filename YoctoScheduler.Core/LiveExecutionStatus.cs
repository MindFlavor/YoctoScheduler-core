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

        public LiveExecutionStatus(int TaskID, int ServerID, int? ScheduleID) : base()
        {
            this.TaskID = TaskID;
            this.ServerID = ServerID;
            this.ScheduleID = ScheduleID;

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
            LiveExecutionStatus es = new LiveExecutionStatus(TaskID, ServerID, ScheduleID);

            #region Database entry
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("LiveExecutionStatus.New"), conn, trans))
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
                param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier);
                param.Value = GUID;
                cmd.Parameters.Add(param);
            }
        }

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            throw new NotImplementedException();
        }

        protected static LiveExecutionStatus ParseFromDataReader(SqlDataReader r)
        {
            int? ScheduleID = null;
            if (!r.IsDBNull(1))
                ScheduleID = r.GetInt32(1);

            var les = new LiveExecutionStatus(r.GetInt32(2), r.GetInt32(3), ScheduleID)
            {
                GUID = r.GetGuid(0),
                LastUpdate = r.GetDateTime(4)
            };

            return les;
        }

        public static List<LiveExecutionStatus> GetAll(SqlConnection conn, SqlTransaction trans)
        {
            return GetAll(conn, trans, DateTime.Parse("1990-01-01"));
        }

        public static List<LiveExecutionStatus> GetAndLockAll(SqlConnection conn, SqlTransaction trans, DateTime minLastUpdate)
        {
            List<LiveExecutionStatus> lItems = new List<LiveExecutionStatus>();

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("LiveExecutionStatus.GetAndLockAll"), conn, trans))
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

        public static List<LiveExecutionStatus> GetAll(SqlConnection conn, SqlTransaction trans, DateTime minLastUpdate)
        {
            List<LiveExecutionStatus> lItems = new List<LiveExecutionStatus>();

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("LiveExecutionStatus.GetAll"), conn, trans))
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

        public void Delete(SqlConnection conn, SqlTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("LiveExecutionStatus.Delete"), conn, trans))
            {
                SqlParameter param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier);
                param.Value = GUID;
                cmd.Parameters.Add(param);

                cmd.Prepare();
                if (cmd.ExecuteNonQuery() != 1)
                {
                    throw new Exceptions.ConcurrencyException(
                        string.Format("Delete from [live].[ExecutionStatus] failed because no entry with GUID {0:S} was found", GUID.ToString(),
                        null));
                }
            }
        }
    }
}
