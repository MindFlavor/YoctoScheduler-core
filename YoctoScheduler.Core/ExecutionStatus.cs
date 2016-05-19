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

        public static ExecutionStatus New(string connectionString, int TaskID, int ServerID, int? ScheduleID)
        {
            #region Data load

            #endregion

            #region Database entry
            
            #endregion

            return null;
        }

        public override void PersistChanges(SqlConnection conn)
        {
            throw new NotImplementedException();
        }
    }
}
