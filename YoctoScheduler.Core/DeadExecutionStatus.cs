using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public class DeadExecutionStatus : LiveExecutionStatus
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DeadExecutionStatus));

        public Status Status { get; set; }

        public DeadExecutionStatus(LiveExecutionStatus des, Status Status) : base(des.TaskID, des.ServerID)
        {
            GUID = des.GUID;
            this.ScheduleID = des.ScheduleID;
            this.LastUpdate = des.LastUpdate;

            this.Status = Status;            
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S0}, Status={2:S0}]",
                this.GetType().FullName,
                base.ToString(),
                Status.ToString());
        }

        public static DeadExecutionStatus New(SqlConnection conn, SqlTransaction trans, LiveExecutionStatus les, Status status)
        {
            DeadExecutionStatus des = new DeadExecutionStatus(les, status);

            #region Database entry
            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO [dead].[ExecutionStatus]
                       ([GUID]
                       ,[ScheduleID]
                       ,[TaskID]
                       ,[ServerID]
                       ,[LastUpdate]
                       ,[Status])
                 VALUES
                       (
                        @GUID
                        @ScheduleID
                       ,@TaskID
                       ,@ServerID
                       ,@LastUpdate
                       ,@Status)", conn, trans))
            {
                des.PopolateParameters(cmd);
            }
            #endregion
            log.DebugFormat("Created dead excecution {0:S}", des.ToString());
            return des;
        }

        protected internal override void PopolateParameters(SqlCommand cmd)
        {
            base.PopolateParameters(cmd);

            SqlParameter param = new SqlParameter("@Status", System.Data.SqlDbType.Int);
            param.Value = Status;
            cmd.Parameters.Add(param);
        }
    }
}
