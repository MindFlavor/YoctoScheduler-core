using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    public class DeadExecutionStatus : LiveExecutionStatus
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DeadExecutionStatus));

        public TaskStatus Status { get; set; }

        public string ReturnCode { get; set; }

        public DeadExecutionStatus(LiveExecutionStatus des, TaskStatus Status)
            : this(des, Status, null) { }

        public DeadExecutionStatus(LiveExecutionStatus des, TaskStatus Status, string ReturnCode) : base(des.TaskID, des.ServerID, des.ScheduleID)
        {
            GUID = des.GUID;
            this.LastUpdate = des.LastUpdate;

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

        public static DeadExecutionStatus New(SqlConnection conn, SqlTransaction trans, LiveExecutionStatus les, TaskStatus status, string ReturnCode)
        {
            DeadExecutionStatus des = new DeadExecutionStatus(les, status, ReturnCode);

            #region Database entry
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("DeadExecutionStatus.New"), conn, trans))
            {
                des.PopolateParameters(cmd);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
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


            param = new SqlParameter("@ReturnCode", System.Data.SqlDbType.NVarChar, -1);
            if (string.IsNullOrEmpty(ReturnCode))
                param.Value = DBNull.Value;
            else
                param.Value = ReturnCode;
            cmd.Parameters.Add(param);
        }
    }
}
