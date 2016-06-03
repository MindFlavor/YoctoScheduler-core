using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [System.Runtime.Serialization.DataContract]
    [DatabaseKey(DatabaseName = "ScheduleID", Size = 4)]
    public class Schedule : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Schedule));

        private static DateTime DT_NEVER = DateTime.Parse("1900-01-01");

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 255)]
        public string Cron { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 1)]
        public bool Enabled { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 4)]
        public int TaskID { get; set; }

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(Size = 8)]
        public DateTime LastFired { get; set; }

        public Schedule()
        {
            LastFired = DT_NEVER;
        }

        public Schedule(int TaskID) : base()
        {
            this.TaskID = TaskID;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, TaskID={2:N0}, Cron={3:S}, Enabled={4:S}, LastFired={5:S}]", this.GetType().FullName, base.ToString(), TaskID, Cron.ToString(), Enabled.ToString(), LastFired.ToString());
        }

        public Schedule Clone(SqlConnection conn, SqlTransaction trans)
        {
            Schedule s = new Schedule()
            {
                TaskID = this.TaskID,
                Cron = this.Cron,
                Enabled = this.Enabled,
                LastFired = this.LastFired == DateTime.MinValue ? DT_NEVER : this.LastFired
            };

            Schedule.Insert(conn, trans, s);
            return s;
        }

        public override void ParseFromDataReader(SqlDataReader r)
        {
            ID = r.GetInt32(0);
            Cron = r.GetString(1);
            Enabled = r.GetBoolean(2);
            LastFired = r.GetDateTime(4);
            TaskID = r.GetInt32(3);
        }

        public static List<Schedule> GetAndLockEnabledNotRunning(SqlConnection conn, SqlTransaction trans)
        {
            List<Schedule> lItems = new List<Schedule>();

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Schedule.GetAndLockEnabledNotRunning"), conn, trans))
            {
                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Schedule s = new Schedule();
                        s.ParseFromDataReader(reader);
                        lItems.Add(s);
                    }
                }
            }

            return lItems;
        }
    }
}
