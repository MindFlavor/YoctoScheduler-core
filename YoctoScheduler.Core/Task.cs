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
    public class Task : DatabaseItemWithIntPK
    {
        public int TaskID { get; set; }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Task));

        public override string ToString()
        {
            return string.Format("{0:S}[TaskID={1:N0}]",
                this.GetType().FullName,
                TaskID);
        }

        public static Task New(string connectionString)
        {
            #region Database entry
            var task = new Task()
            {
                ConnectionString = connectionString
            };

            using (var conn = OpenConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO [live].[Tasks] 
                        OUTPUT [INSERTED].[TaskID]
                        DEFAULT VALUES"
                    , conn);

                task.PopolateParameters(cmd);

                cmd.Prepare();
                task.TaskID = (int)cmd.ExecuteScalar();
            }
            #endregion

            log.DebugFormat("{0:S} - Created task ", task.ToString());

            return task;
        }
        protected internal void PopolateParameters(SqlCommand cmd)
        {
        }

        public override void PersistChanges()
        {
        }

    }
}
