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

    public class ExecutionStatus : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ExecutionStatus));

        public int TaskID { get; set; }

        public Status Status { get; set; }

        public int ServerID { get; set; }

        public int? ScheduleID { get; set; }

        public DateTime LastUpdate { get; set; }

        public ExecutionStatus(int TaskID, int ServerID) : base()
        {
            this.TaskID = TaskID;
            this.ServerID = ServerID;

            Status = Status.Unknown;
            LastUpdate = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[ID={1:N0}, TaskID={2:N0}, Status={3:S}, ServerID={4:N0}, ScheduleID={5:S}, LastUpdate={6:S}]",
                this.GetType().FullName,
                ID, TaskID, Status.ToString(), ServerID,
                ScheduleID.HasValue ? ScheduleID.Value.ToString() : "<null>",
                LastUpdate.ToString());
        }

        public static ExecutionStatus New(SqlConnection conn, int TaskID, int ServerID, int? ScheduleID)
        {
            ExecutionStatus es = new ExecutionStatus(TaskID, ServerID)
            {
                ScheduleID = ScheduleID,
                Status = Status.Starting
            };

            #region Database entry
            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO [live].[ExecutionStatus]
                       ([ScheduleID]
                       ,[TaskID]
                       ,[Status]
                       ,[ServerID]
                       ,[LastUpdate])
                OUTPUT INSERTED.[ID]
                 VALUES
                       (@ScheduleID
                       ,@TaskID
                       ,@Status
                       ,@ServerID
                       ,@LastUpdate)", conn))
            {
                es.PopolateParameters(cmd);
                es.ID = (int)cmd.ExecuteScalar();
            }
            #endregion
            log.DebugFormat("Created excecution {0:S}", es.ToString());
            return es;
        }

        protected internal void PopolateParameters(SqlCommand cmd)
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

            param = new SqlParameter("@Status", System.Data.SqlDbType.Int);
            param.Value = Status;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@ServerID", System.Data.SqlDbType.Int);
            param.Value = ServerID;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@LastUpdate", System.Data.SqlDbType.DateTime);
            param.Value = LastUpdate;
            cmd.Parameters.Add(param);

            if (HasValidID())
            {
                param = new SqlParameter("@ID", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
        }

        public override void PersistChanges(SqlConnection conn)
        {
            throw new NotImplementedException();
        }
    }
}
