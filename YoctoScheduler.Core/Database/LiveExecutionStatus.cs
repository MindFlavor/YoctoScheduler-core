using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{

    public class LiveExecutionStatus : DatabaseItemWithGUIDPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LiveExecutionStatus));

        public int TaskID { get; set; }

        public int ServerID { get; set; }

        public int? ScheduleID { get; set; }

        public DateTime LastUpdate { get; set; }

        public DateTime Inserted { get; set; }

        public LiveExecutionStatus() : base()
        { }

        public LiveExecutionStatus(int TaskID, int ServerID, int? ScheduleID) : base()
        {
            this.TaskID = TaskID;
            this.ServerID = ServerID;
            this.ScheduleID = ScheduleID;

            LastUpdate = DateTime.Now;
            Inserted = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S0}, TaskID={2:N0}, ServerID={3:N0}, ScheduleID={4:S}, Inserted={5:S}, LastUpdate={6:S}]",
                this.GetType().FullName,
                base.ToString(),
                TaskID, ServerID,
                ScheduleID.HasValue ? ScheduleID.Value.ToString() : "<null>",
                Inserted.ToString(),
                LastUpdate.ToString());
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

            param = new SqlParameter("@ServerID", System.Data.SqlDbType.Int);
            param.Value = ServerID;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@LastUpdate", System.Data.SqlDbType.DateTime);
            param.Value = LastUpdate;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@Inserted", System.Data.SqlDbType.DateTime);
            param.Value = Inserted;
            cmd.Parameters.Add(param);

            if (HasValidID())
            {
                param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
        }

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            throw new NotImplementedException();
        }

        public override void ParseFromDataReader(SqlDataReader r)
        {
            ScheduleID = null;
            if (!r.IsDBNull(1))
                ScheduleID = r.GetInt32(1);
            ID = r.GetGuid(0);
            TaskID = r.GetInt32(2);
            ServerID = r.GetInt32(3);
            LastUpdate = r.GetDateTime(5);
            Inserted = r.GetDateTime(4);
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
                        LiveExecutionStatus les = new LiveExecutionStatus();
                        les.ParseFromDataReader(reader);
                        lItems.Add(les);
                    }
                }
            }

            return lItems;
        }
       
        public void UpdateKeepAlive(SqlConnection conn, SqlTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("LiveExecutionStatus.UpdateKeepAlive"), conn, trans))
            {
                SqlParameter param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier);
                param.Value = ID;
                cmd.Parameters.Add(param);

                param = new SqlParameter("@lastUpdate", System.Data.SqlDbType.DateTime);
                param.Value = DateTime.Now;
                cmd.Parameters.Add(param);

                cmd.Prepare();
                if (cmd.ExecuteNonQuery() != 1)
                {
                    throw new Exceptions.ConcurrencyException(
                        string.Format("Update from [live].[ExecutionStatus] failed because no entry with GUID {0:S} was found", ID.ToString(),
                        null));
                }
            }
        }
    }
}
