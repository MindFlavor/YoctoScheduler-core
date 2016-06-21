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
    public class Schedule : DatabaseItemWithGUIDPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Schedule));

        [DatabaseProperty(Size = 255)]
        [System.Runtime.Serialization.DataMember]
        public string Cron { get; set; }

        [DatabaseProperty(Size = 1)]
        [System.Runtime.Serialization.DataMember]
        public bool Enabled { get; set; }

        [DatabaseProperty(Size = 4)]
        [System.Runtime.Serialization.DataMember]
        public int TaskID { get; set; }

        [DatabaseProperty(Size = 8)]
        [System.Runtime.Serialization.DataMember]
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

        public override void ParseFromDataReader(SqlDataReader r)
        {
            ID = r.GetGuid(0);
            Cron = r.GetString(1);
            Enabled = r.GetBoolean(2);
            LastFired = r.GetDateTime(4);
            TaskID = r.GetInt32(3);
        }

        public override void Validate()
        {
            base.Validate();

            NCrontab.CrontabSchedule.Parse(Cron);
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
