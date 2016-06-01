using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [System.Runtime.Serialization.DataContract]
    public class Schedule : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Schedule));

        private static DateTime DT_NEVER = DateTime.Parse("1900-01-01");

        [System.Runtime.Serialization.DataMember]
        public string Cron { get; set; }

        [System.Runtime.Serialization.DataMember]
        public bool Enabled { get; set; }

        [System.Runtime.Serialization.DataMember]
        public int TaskID { get; set; }

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

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            using (
                SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Schedule.PersistChanges"), conn, trans))
            {
                PopolateParameters(cmd);

                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }

        public override void PopolateParameters(SqlCommand cmd)
        {
            SqlParameter param = new SqlParameter("@taskID", System.Data.SqlDbType.Int);
            param.Value = TaskID;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@cron", System.Data.SqlDbType.NVarChar, 255);
            param.Value = Cron.ToString();
            cmd.Parameters.Add(param);

            param = new SqlParameter("@enabled", System.Data.SqlDbType.Bit);
            param.Value = Enabled;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@LastFired", System.Data.SqlDbType.DateTime);
            param.Value = LastFired;
            cmd.Parameters.Add(param);

            if (HasValidID())
            {
                param = new SqlParameter("@scheduleID", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
        }

        public static Schedule GetByID(SqlConnection conn, SqlTransaction trans, int id)
        {
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Schedule.GetByID"), conn, trans))
            {
                SqlParameter param = new SqlParameter("@ScheduleID", System.Data.SqlDbType.Int);
                param.Value = id;
                cmd.Parameters.Add(param);

                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Schedule s = new Schedule();
                        s.ParseFromDataReader(reader);
                        return s;

                    }
                }
            }

            return null;
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
