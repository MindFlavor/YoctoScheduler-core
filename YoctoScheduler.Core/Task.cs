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

        public bool ReenqueueOnDead { get; set; }

        public Task(bool ReenqueueOnDead) : base()
        {
            this.ReenqueueOnDead = ReenqueueOnDead;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, ReenqueueOnDead={2:S}]",
                this.GetType().FullName,
                base.ToString(),
                ReenqueueOnDead.ToString());
        }

        public static Task New(SqlConnection conn,SqlTransaction trans,  bool ReenqueueOnDead )
        {
            #region Database entry
            var task = new Task(ReenqueueOnDead);

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Task.New"), conn, trans))
            {
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
            SqlParameter param = new SqlParameter("@ReenqueueOnDead", System.Data.SqlDbType.Bit);
            param.Value = ReenqueueOnDead;
            cmd.Parameters.Add(param);
        }

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            throw new NotImplementedException();
        }

        public static Task RetrieveByID(SqlConnection conn, SqlTransaction trans, int ID)
        {
            Task task;

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Task.RetrieveByID"), conn, trans))
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
            return new Task(r.GetBoolean(1))
            {
                ID = r.GetInt32(0)
            };
        }
    }
}
