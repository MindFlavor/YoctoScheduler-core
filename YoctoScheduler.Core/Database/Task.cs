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
    [System.Runtime.Serialization.DataContract]
    public class Task : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Task));

        [System.Runtime.Serialization.DataMember]
        public bool ReenqueueOnDead { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Type { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Payload { get; set; }

        public Task(bool ReenqueueOnDead, string Type, string Payload) : base()
        {
            this.ReenqueueOnDead = ReenqueueOnDead;
            this.Type = Type;
            this.Payload = Payload;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, ReenqueueOnDead={2:S}, Type=\"{3:S}\", Payload=\"{4:S}\"]",
                this.GetType().FullName,
                base.ToString(),
                ReenqueueOnDead.ToString(),
                Type, Payload);
        }

        public virtual Task Clone(SqlConnection conn, SqlTransaction trans)
        {
            return New(conn, trans, this.ReenqueueOnDead, this.Type, this.Payload);
        }

        public static Task New(SqlConnection conn, SqlTransaction trans, bool ReenqueueOnDead, string Type, string Payload)
        {
            #region Database entry
            var task = new Task(ReenqueueOnDead, Type, Payload);

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

            param = new SqlParameter("@Type", System.Data.SqlDbType.NVarChar, 255);
            param.Value = Type;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@Payload", System.Data.SqlDbType.NVarChar, -1);
            if (string.IsNullOrEmpty(Payload))
                param.Value = DBNull.Value;
            else
                param.Value = Payload;
            cmd.Parameters.Add(param);

            if (HasValidID())
            {
                param = new SqlParameter("@TaskID", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
        }

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            throw new NotImplementedException();
        }

        public static List<Task> GetAll(SqlConnection conn, SqlTransaction trans)
        {
            List<Task> lTasks = new List<Task>();

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Task.GetAll"), conn, trans))
            {
                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lTasks.Add(ParseFromDataReader(reader));
                }

                return lTasks;
            }
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
            string payload = null;
            if (!r.IsDBNull(3))
                payload = r.GetString(3);

            return new Task(r.GetBoolean(1), r.GetString(2), payload)
            {
                ID = r.GetInt32(0)
            };
        }
    }
}
