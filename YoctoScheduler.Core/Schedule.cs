﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public class Schedule : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Schedule));

        public NCrontab.CrontabSchedule Cron { get; set; }

        public bool Enabled { get; set; }

        public int TaskID { get; set; }

        public Schedule(int TaskID) : base()
        {
            this.TaskID = TaskID;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, TaskID={2:N0}, Cron={3:S}, Enabled={4:S}]", this.GetType().FullName, base.ToString(), TaskID, Cron.ToString(), Enabled.ToString());
        }

        public static Schedule New(SqlConnection conn, int TaskID, string cronString, bool enabled)
        {
            #region Check for existing TaskID
            // ideally this should be in a transaction (repeatable read at least) but we don't care
            // since referential integrity would kick in anyway.
            if (Task.RetrieveByID(conn, TaskID) == null)
                throw new Exceptions.TaskNotFoundException(TaskID);
            #endregion

            NCrontab.CrontabSchedule cron = NCrontab.CrontabSchedule.Parse(cronString);

            #region Database entry
            var schedule = new Schedule(TaskID)
            {
                Cron = cron,
                Enabled = enabled
            };

            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO [live].[Schedules]
                       ([TaskID]
                       ,[Cron]
                       ,[Enabled])
	             OUTPUT [INSERTED].[ScheduleID]    
                 VALUES(
                         @taskID,
		                 @cron,
		                 @enabled
                       )"
                , conn))
            {
                schedule.PopolateParameters(cmd);

                cmd.Prepare();
                schedule.ID = (int)cmd.ExecuteScalar();
            }
            #endregion

            log.DebugFormat("{0:S} created", schedule.ToString());

            return schedule;
        }

        public override void PersistChanges(SqlConnection conn)
        {
            using (
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE [live].[Schedule]
                        SET    
                            [TaskID] = @taskID
                            ,[Cron] = @cron
                            ,[Enabled] = @enabled
                        WHERE 
                            [ScheduleID] = @scheduleID"
                    , conn))
            { 
                PopolateParameters(cmd);

                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }

        protected internal void PopolateParameters(SqlCommand cmd)
        {
            SqlParameter param = new SqlParameter("@taskID", System.Data.SqlDbType.Int);
            param.Value = TaskID;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@cron", System.Data.SqlDbType.NVarChar, 255);
            param.Value = Cron;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@enabled", System.Data.SqlDbType.Bit);
            param.Value = Enabled;
            cmd.Parameters.Add(param);

            if (HasValidID())
            {
                param = new SqlParameter("@scheduleID", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
        }

        protected static Schedule ParseFromDataReader(SqlConnection conn, SqlDataReader r)
        {
            return new Schedule(r.GetInt32(3))
            {
                ID = r.GetInt32(0),
                Cron = NCrontab.CrontabSchedule.Parse(r.GetString(1)),
                Enabled = r.GetBoolean(2)
            };
        }

        public static List<Schedule> GetAll(SqlConnection conn, bool includeDisabled)
        {
            List<Schedule> lItems = new List<Schedule>();

            string stmt = string.Format(@"SELECT 
                      [ScheduleID]
                      ,[Cron]
                      ,[Enabled]
                      ,[TaskID]
                  FROM[live].[Schedules] {0:S}",
              includeDisabled ? "" : "WHERE [Enabled] = 1");

            using (SqlCommand cmd = new SqlCommand(stmt, conn))
            {
                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lItems.Add(ParseFromDataReader(conn, reader));
                    }
                }
            }

            return lItems;
        }
    }
}
