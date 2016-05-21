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

        public Task() : base() { }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}]",
                this.GetType().FullName,
                base.ToString());
        }

        public static Task New(SqlConnection conn)
        {
            #region Database entry
            var task = new Task();

            SqlCommand cmd = new SqlCommand(
                @"INSERT INTO [live].[Tasks] 
                        OUTPUT [INSERTED].[TaskID]
                        DEFAULT VALUES"
                , conn);

            task.PopolateParameters(cmd);

            cmd.Prepare();
            task.ID = (int)cmd.ExecuteScalar();
            #endregion

            log.DebugFormat("{0:S} - Created task ", task.ToString());

            return task;
        }
        protected internal void PopolateParameters(SqlCommand cmd)
        {
        }

        public override void PersistChanges(SqlConnection conn)
        {
        }

        public static Task RetrieveByID(SqlConnection conn, SqlTransaction trans, int ID)
        {
            Task task;

            using (SqlCommand cmd = new SqlCommand(
                @"SELECT [TaskID] FROM [live].[Tasks] 
                        WHERE [TaskID] = @id"
                , conn, trans))
            {
                SqlParameter param = new SqlParameter("@id", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);

                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    task = ParseFromDataReader(reader);
                }

                log.DebugFormat("{0:S} - Retrieved task ", task.ToString());
                return task;
            }
        }

        protected static Task ParseFromDataReader(SqlDataReader r)
        {
            return new Task()
            {
                ID = r.GetInt32(0)
            };
        }
    }
}
