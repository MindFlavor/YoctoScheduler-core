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
    [DatabaseKey(DatabaseName = "TaskID", Size = 4)]
    public class Task : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Task));

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 255)]
        public string Name { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 255)]
        public string Type { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 1)]
        public bool ReenqueueOnDead { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = -1)]
        public string Payload { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = -1)]
        public string Description { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 4)]
        public int ConcurrencyLimitGlobal { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 4)]
        public int ConcurrencyLimitSameInstance { get; set; }

        public Task() : base()
        { }

        public Task(bool ReenqueueOnDead, string Name, string Description, string Type, string Payload, int ConcurrencyLimitGlobal, int ConcurrencyLimitSameInstance): base()
        {
            this.ReenqueueOnDead = ReenqueueOnDead;
            this.Type = Type;
            this.Payload = Payload;
            this.Name = Name;
            this.Description = Description;
            this.ConcurrencyLimitGlobal = ConcurrencyLimitGlobal;
            this.ConcurrencyLimitSameInstance = ConcurrencyLimitSameInstance;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, Name={2:S}, Description=\"{3:S}\", ConcurrencyLimitGlobal={4:N0}, ConcurrencyLimitSameInstance={5:N0}, ReenqueueOnDead={6:S}, Type=\"{7:S}\", Payload=\"{8:S}\"]",
                this.GetType().FullName,
                base.ToString(),
                Name,
                Description,
                ConcurrencyLimitGlobal, 
                ConcurrencyLimitSameInstance,
                ReenqueueOnDead.ToString(),
                Type, Payload);
        }

        public static Task New(SqlConnection conn, SqlTransaction trans, string Name, string Description, bool ReenqueueOnDead, string Type, string Payload, int ConcurrencyLimitGlobal, int ConcurrencyLimitSameInstance)
        {
            #region Database entry
            var task = new Task(ReenqueueOnDead, Name, Description, Type, Payload, ConcurrencyLimitGlobal, ConcurrencyLimitSameInstance);

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

        public override void ParseFromDataReader(SqlDataReader r)
        {
            Payload = null;
            if (!r.IsDBNull(5))
                Payload = r.GetString(5);

            Description = null;
            if (!r.IsDBNull(3))
                Description = r.GetString(3);

            ID = r.GetInt32(0);

            ReenqueueOnDead = r.GetBoolean(1);
            Name = r.GetString(2);
            Type = r.GetString(4);

            ConcurrencyLimitGlobal = r.GetInt32(6);
            ConcurrencyLimitSameInstance = r.GetInt32(7);
        }
    }
}
