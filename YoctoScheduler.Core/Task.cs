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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Task));

        public Task(string connectionString) : base(connectionString) { }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}]",
                this.GetType().FullName,
                base.ToString());
        }

        public static Task New(string connectionString)
        {
            #region Database entry
            var task = new Task(connectionString);

            using (var conn = OpenConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO [live].[Tasks] 
                        OUTPUT [INSERTED].[TaskID]
                        DEFAULT VALUES"
                    , conn);

                task.PopolateParameters(cmd);

                cmd.Prepare();
                task.ID = (int)cmd.ExecuteScalar();
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

        public static Task RetrieveByID(string connectionString, int ID)
        {
            Task task;
            using (var conn = OpenConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT [TaskID] FROM [live].[Tasks] 
                        WHERE [TaskID] = @id"
                    , conn);

                SqlParameter param = new SqlParameter("@id", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);

                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    task = ParseFromDataReader(connectionString, reader);
                }

                log.DebugFormat("{0:S} - Retrieved task ", task.ToString());
                return task;
            }
        }

        protected static Task ParseFromDataReader(string connectionString, SqlDataReader r)
        {
            return new Task(connectionString)
            {                               
                ID = r.GetInt32(0)
            };
        }
    }
}
