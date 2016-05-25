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

        public static Configuration Configuration { get; set; }

        public Status Status { get; set; }

        public string Description { get; set; }

        public DateTime LastPing { get; set; }

        public DateTime LastScheduleCheck { get; protected set; }

        public string ConnectionString { get; set; }

        public Server(string ConnectionString) : base()
        {
            if (Configuration == null)
                throw new Exceptions.ConfigurationNotInitializedException();

            LastScheduleCheck = DateTime.Now;
            this.ConnectionString = ConnectionString;
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

        public static Server New(SqlConnection conn, SqlTransaction trans, string connectionString, string Description)
        {
            #region Database entry

            var server = new Server(connectionString)
            {
                Description = Description,
                LastPing = DateTime.Parse("2000-01-01"),
                Status = Status.Starting
            };

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Server.New"), conn, trans))
            {
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

            #region schedule task thread
            t = new Thread(new ThreadStart(server.TasksScheduledThread));
            t.IsBackground = true;
            log.DebugFormat("{0:S} - Starting task thread thread", server.ToString());
            t.Start();
            #endregion

            #region dequeue task thread
            t = new Thread(new ThreadStart(server.DequeueTasksThread));
            t.IsBackground = true;
            log.DebugFormat("{0:S} - Starting dequeue task thread", server.ToString());
            t.Start();
            #endregion

            #region Set server as running
            log.DebugFormat("{0:S} - Setting server as running", server.ToString());
            server.Status = Status.Running;

            server.PersistChanges(conn, trans);
            log.InfoFormat("{0:S} - Server running", server.ToString());
            #endregion

            return server;
        }

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Server.PersistChanges"), conn, trans))
            {
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
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        PersistChanges(conn, trans);
                        trans.Commit();
                    }
                }

                Thread.Sleep(int.Parse(Configuration["SERVER_KEEPALIVE_SLEEP_MS"]));
            }
        }

        protected void ClearOldServersThread()
        {
            while (true)
            {
                // a server is dead if there is no update in the last xxx msseconds
                DateTime dtDead = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(int.Parse(Configuration["SERVER_MAXIMUM_UPDATE_LAG_MS"])));

                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Server.ClearOldServersThread"), conn);

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

                Thread.Sleep(int.Parse(Configuration["SERVER_POLL_DISABLE_DEAD_SERVERS_SLEEP_MS"]));
            }
        }

        protected void DeadTasksThread()
        {
            while (true)
            {
                log.DebugFormat("{0:S} - Check for dead tasks", this.ToString());

                // a task is dead if there is no update in the xxx milliseconds (1 minute default)
                DateTime dtExpired = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(int.Parse(Configuration["TASK_MAXIMUM_UPDATE_LAG_MS"])));

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        var lExpired = LiveExecutionStatus.GetAndLockAll(conn, trans, dtExpired);

                        foreach (var les in lExpired)
                        {
                            // insert into dead table 
                            var des = DeadExecutionStatus.New(conn, trans, les, Status.Dead, null);

                            log.InfoFormat("Setting LiveExecutionStatus {0:S} as dead", les.ToString());
                            // remove from live table
                            les.Delete(conn, trans);

                        }

                        trans.Commit();
                    }
                }

                Thread.Sleep(int.Parse(Configuration["SERVER_POLL_DISABLE_DEAD_TASKS_SLEEP_MS"]));
            }
        }

        protected void DequeueTasksThread()
        {
            while (true)
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        var lToStart = ExecutionQueueItem.GetAndLockFirst(conn, trans);
                        if(lToStart != null)
                        {
                            log.DebugFormat("Starting enqueued task {0:S}", lToStart.ToString());

                            // add to live table
                            var les = LiveExecutionStatus.New(conn, trans, lToStart.TaskID, this.ID, lToStart.ScheduleID);

                            // start the execution
                            // TODO this is just a mockup
                            var wd = ExecutionTask.Factory.NewTask(this, les);
                            wd.Start();
                            log.InfoFormat("Started live execution status {0:S}", les.ToString());

                            // remove from pending execution queue
                            lToStart.Delete(conn, trans);
                        }

                        trans.Commit();
                    }
                }
                Thread.Sleep(int.Parse(Configuration["SERVER_POLL_TASK_QUEUE_SLEEP_MS"]));
            }
        }

        protected void TasksScheduledThread()
        {
            // modificare il processo perchè includa la coda di esecuzione ( [live].[Schedules]). 
            // la query da utilizzare e'
            /*
             * SELECT * FROM [live].[Schedules] S WITH(XLOCK)
                LEFT OUTER JOIN [live].[ExecutionQueue]  Q ON S.[ScheduleID] = Q.[ScheduleID]
                LEFT OUTER JOIN [live].[ExecutionStatus] E ON S.[ScheduleID] = E.[ScheduleID]
                WHERE
		                Q.[ScheduleID] IS NULL 
	                AND
		                E.[ScheduleID] IS NULL  
             * 
             * questa query locka la schedules e serializza il suo accesso. mostra solo le 
             * schedulazioni che non sono ne' in coda ne' in esecuzione
             */

            while (true)
            {
                log.DebugFormat("{0:S} - Check for tasks to start - Starting", this.ToString());

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        // Get enabled schedules
                        var lSchedules = Schedule.GetAndLockEnabledNotRunning(conn, trans);

                        // look for schedules to fire
                        lSchedules.ForEach(sched =>
                        {
                            if (sched.Cron.GetNextOccurrence(LastScheduleCheck) < DateTime.Now)
                            {
                                var task = Task.RetrieveByID(conn, trans, sched.TaskID);
                                log.InfoFormat("Starting schedulation {0:S} due to cron {1:S}", task.ToString(), sched.ToString());

                                var qi = ExecutionQueueItem.New(conn, trans, task.ID, Priority.Normal, sched.ID);
                                log.InfoFormat("Execution enqueued {0:S}", qi.ToString());
                            }
                        });

                        trans.Commit();
                    }
                }

                LastScheduleCheck = DateTime.Now;

                Thread.Sleep(int.Parse(Configuration["SERVER_POLL_TASK_SCHEDULER_SLEEP_MS"]));
            }
        }
    }
}
