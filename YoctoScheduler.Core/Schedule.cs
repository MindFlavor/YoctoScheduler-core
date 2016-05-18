using System;
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

        protected string _cron;

        public string Cron
        {
            get
            {
                return _cron;
            }
            set
            {
                NCrontab.CrontabSchedule.Parse(value);
                _cron = value;
            }
        }

        public bool Enabled { get; set; }

        public int TaskID { get; set; }

        public Schedule(string connectionString, int TaskID)
            : base(connectionString)
        {
            this.TaskID = TaskID;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, TaskID={2:N0}, Cron={3:S}, Enabled={4:S}]", this.GetType().FullName, base.ToString(), TaskID, Cron, Enabled.ToString());
        }

        public static Schedule New(string connectionString, int TaskID, string cron, bool enabled)
        {
            #region Check for existing TaskID
            // ideally this should be in a transaction (repeatable read at least) but we don't care
            // since referential integrity would kick in anyway.
            if (Task.RetrieveByID(connectionString, TaskID) == null)
                throw new Exceptions.TaskNotFoundException(TaskID);
            #endregion

            #region Database entry
            var schedule = new Schedule(connectionString, TaskID)
            {
                Cron = cron,
                Enabled = enabled
            };

            using (var conn = OpenConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
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
                    , conn);

                schedule.PopolateParameters(cmd);

                cmd.Prepare();
                schedule.ID = (int)cmd.ExecuteScalar();
            }
            #endregion

            log.DebugFormat("{0:S} created", schedule.ToString());

            return schedule;
        }

        public override void PersistChanges()
        {
            using (var conn = OpenConnection())
            {
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE [live].[Schedule]
                        SET    
                            [TaskID] = @taskID
                            ,[Cron] = @cron
                            ,[Enabled] = @enabled
                        WHERE 
                            [ScheduleID] = @scheduleID"
                    , conn);

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



    }
}
