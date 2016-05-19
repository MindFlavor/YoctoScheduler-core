using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public class Server : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Server));

        public Status Status { get; set; }

        public string Description { get; set; }

        public DateTime LastPing { get; set; }

        public DateTime LastScheduleCheck { get; protected set; }

        public Server(string connectionString) : base(connectionString)
        {
            LastScheduleCheck = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, Status={2:S}, Description=\"{3:S}\", LastPing={4:S}, LastScheduleCheck={5:S}]",
                this.GetType().FullName,
                base.ToString(),
                Status.ToString(),
                Description,
                LastPing.ToString(LOG_TIME_FORMAT),
                LastScheduleCheck.ToString(LOG_TIME_FORMAT));
        }

        public static Server New(string connectionString, string Description)
        {
            #region Database entry
            var server = new Server(connectionString)
            {
                Description = Description,
                LastPing = DateTime.Parse("2000-01-01"),
                Status = Status.Starting
            };

            using (var conn = OpenConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO [live].[Servers]
                       ([Status]
                       ,[Description]
                       ,[LastPing])
	             OUTPUT [INSERTED].[ServerID]    
                 VALUES(
                         @status,
		                 @description,
		                 @lastping
                       )"
                    , conn);

                server.PopolateParameters(cmd);

                cmd.Prepare();
                server.ID = (int)cmd.ExecuteScalar();
            }

            #endregion

            log.DebugFormat("{0:S} - Created server ", server.ToString());

            Thread t;
            #region start ping thread
            t = new Thread(new ThreadStart(server.PingThread));
            t.IsBackground = true;
            log.DebugFormat("{0:S} - Starting ping thread", server.ToString());
            t.Start();
            #endregion

            #region start clear old servers thread
            t = new Thread(new ThreadStart(server.ClearOldServersThread));
            t.IsBackground = true;
            log.DebugFormat("{0:S} - Starting clear old servers thread", server.ToString());
            t.Start();
            #endregion

            #region dead task thread
            t = new Thread(new ThreadStart(server.DeadTasksThread));
            t.IsBackground = true;
            log.DebugFormat("{0:S} - Starting dead task thread thread", server.ToString());
            t.Start();
            #endregion

            #region task thread
            t = new Thread(new ThreadStart(server.TasksThread));
            t.IsBackground = true;
            log.DebugFormat("{0:S} - Starting task thread thread", server.ToString());
            t.Start();
            #endregion

            #region Set server as running
            log.DebugFormat("{0:S} - Setting server as running", server.ToString());
            server.Status = Status.Running;
            server.PersistChanges();
            log.InfoFormat("{0:S} - Server running", server.ToString());
            #endregion

            return server;
        }

        public override void PersistChanges()
        {
            using (var conn = OpenConnection())
            {
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE [live].[Servers]
                        SET    
                            [Status] = @status
                            ,[Description] = @description
                            ,[LastPing] = @lastping
                        WHERE 
                            [ServerID] = @serverID"
                    , conn);

                PopolateParameters(cmd);

                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }

        protected internal void PopolateParameters(SqlCommand cmd)
        {
            SqlParameter param = new SqlParameter("@status", System.Data.SqlDbType.Int);
            param.Value = Status;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@description", System.Data.SqlDbType.NVarChar, -1);
            param.Value = Description;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@lastping", System.Data.SqlDbType.DateTime);
            param.Value = LastPing;
            cmd.Parameters.Add(param);

            if (HasValidID())
            {
                param = new SqlParameter("@serverID", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
        }

        protected void PingThread()
        {
            while (true)
            {
                LastPing = DateTime.Now;
                log.DebugFormat("{0:S} - Ping", this.ToString());
                PersistChanges();

                // every minute
                Thread.Sleep(1 * 60 * 1000);
            }
        }

        protected void ClearOldServersThread()
        {
            while (true)
            {
                // a server is dead if there is no update in the last 5 minutes
                DateTime dtDead = DateTime.Now.Subtract(TimeSpan.FromMinutes(5));
                using (var conn = OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand(
                        @"  UPDATE [live].[Servers]
                            SET [Status] = @statusToSet
                            WHERE 
                                [LastPing] < @dt 
                                AND [Status] > @minStatus"
                    , conn);

                    SqlParameter param = new SqlParameter("@statusToSet", System.Data.SqlDbType.Int);
                    param.Value = Status.Dead;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@dt", System.Data.SqlDbType.DateTime);
                    param.Value = dtDead;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@minStatus", System.Data.SqlDbType.Int);
                    param.Value = Status.Unknown;
                    cmd.Parameters.Add(param);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }

                // every min
                Thread.Sleep(1 * 60 * 1000);
            }
        }

        protected void DeadTasksThread()
        {
            while (true)
            {
                // a task is dead if there is no update in the last minute

                // TODO

                // every min
                Thread.Sleep(1 * 60 * 1000);
            }
        }

        protected void TasksThread()
        {
            while (true)
            {
                log.DebugFormat("{0:S} - Check for tasks to start - Starting", this.ToString());

                // Get enabled schedules
                var lSchedules = Schedule.GetAll(ConnectionString, false);

                // look for schedules to fire
                Parallel.ForEach(lSchedules, sched =>
                {
                    if(sched.Cron.GetNextOccurrence(LastScheduleCheck) < DateTime.Now)
                    {
                        var task = Task.RetrieveByID(ConnectionString, sched.TaskID);
                        log.InfoFormat("Startring schedulation {0:S} due to cron {1:S}", task.ToString(), sched.ToString());
                    }
                });

                LastScheduleCheck = DateTime.Now;

                log.DebugFormat("{0:S} - Check for tasks to start - Completed", this.ToString());
                // every 30 seconds
                Thread.Sleep(10 * 1000);
            }
        }
    }
}
