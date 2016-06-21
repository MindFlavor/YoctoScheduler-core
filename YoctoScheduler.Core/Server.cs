using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Xml;
using YoctoScheduler.Core.Database;
using YoctoScheduler.Logging.Extensions;

namespace YoctoScheduler.Core
{
    [System.Runtime.Serialization.DataContract]
    public class Server : DatabaseItemWithIntPK
    {
        public const string TEMPLATE_START = "%%[";
        public const string TEMPLATE_END = "]%%";

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Server));

        public static Configuration Configuration { get; set; }

        [System.Runtime.Serialization.DataMember]
        public TaskStatus Status { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Description { get; set; }

        [System.Runtime.Serialization.DataMember]
        public DateTime LastPing { get; set; }

        [System.Runtime.Serialization.DataMember]
        public DateTime LastScheduleCheck { get; protected set; }

        public string ConnectionString { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string HostName { get; set; }

        [System.Runtime.Serialization.DataMember]
        public List<string> IPs { get; set; }

        public System.Collections.Concurrent.ConcurrentDictionary<Guid, YoctoScheduler.Core.ExecutionTasks.Watchdog> _liveTasks = new System.Collections.Concurrent.ConcurrentDictionary<Guid, YoctoScheduler.Core.ExecutionTasks.Watchdog>();

        public void RegisterTask(YoctoScheduler.Core.ExecutionTasks.Watchdog task)
        {
            log.DebugFormat("Server {0:S} registering task {1:S}", this.ToString(), task.ToString());
            if (!_liveTasks.TryAdd(task.LiveExecutionStatus.ID, task))
            {
                log.WarnFormat("Registration failed for server {0:S}, task {1:S}", this.ToString(), task.ToString());
                throw new Exceptions.ConcurrencyException(string.Format("LiveExecutionStatus with GUID {0:S} already found in _liveTasks", task.LiveExecutionStatus.ID.ToString()));
            }
        }

        public void DeregisterTask(YoctoScheduler.Core.ExecutionTasks.Watchdog task)
        {
            YoctoScheduler.Core.ExecutionTasks.Watchdog t;

            log.DebugFormat("Server {0:S} deregistering task {1:S}", this.ToString(), task.ToString());
            if (!_liveTasks.TryRemove(task.LiveExecutionStatus.ID, out t))
            {
                log.WarnFormat("Deregistration failed for server {0:S}, task {1:S}", this.ToString(), task.ToString());
                throw new Exceptions.ConcurrencyException(string.Format("LiveExecutionStatus with GUID {0:S} not found in _liveTasks", task.LiveExecutionStatus.ID.ToString()));
            }
        }

        public Server() { }

        public Server(string ConnectionString) : base()
        {
            if (Configuration == null)
                throw new Exceptions.ConfigurationNotInitializedException();

            LastScheduleCheck = DateTime.Now;
            this.ConnectionString = ConnectionString;
            this.HostName = System.Net.Dns.GetHostName();

            this.IPs = new List<string>();
            foreach (var ip in System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName()))
            {
                IPs.Add(ip.ToString());
            }
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, Status={2:S}, Description=\"{3:S}\", LastPing={4:S}, LastScheduleCheck={5:S}, HostName={6:S}, IPs=[{7:S}, _liveTasks.Count={8:N0}]]",
                this.GetType().FullName,
                base.ToString(),
                Status.ToString(),
                Description,
                LastPing.ToString(LOG_TIME_FORMAT),
                LastScheduleCheck.ToString(LOG_TIME_FORMAT),
                HostName,
                string.Join(", ", IPs),
                _liveTasks.Count);
        }

        public static Server New(SqlConnection conn, SqlTransaction trans, string connectionString, string Description)
        {
            #region Database entry

            var server = new Server(connectionString)
            {
                Description = Description,
                LastPing = DateTime.Parse("2000-01-01"), // this is needed because TSQL DateTime doesn't accept DateTime.MinValue. The Server.New method will ignore it though.
                Status = TaskStatus.Starting
            };

            using (SqlCommand cmd = new SqlCommand(Database.tsql.Extractor.Get("Server.New"), conn, trans))
            {
                server.PopolateParameters(cmd);

                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        server.ID = reader.GetInt32(0);
                        server.LastPing = reader.GetDateTime(1);
                    }
                    else
                        throw new Exceptions.ServerInitializationException("Failed to register this server instance in the database");
                }
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

            #region commands thread
            t = new Thread(new ThreadStart(server.CommandsThread));
            t.IsBackground = true;
            log.DebugFormat("{0:S} - Starting server commands thread", server.ToString());
            t.Start();
            #endregion


            #region Set server as running
            log.DebugFormat("{0:S} - Setting server as running", server.ToString());
            server.Status = TaskStatus.Running;

            Server.Update(conn, trans, server);
            log.InfoFormat("{0:S} - Server running", server.ToString());
            #endregion

            return server;
        }

        public override void PopolateParameters(SqlCommand cmd)
        {
            SqlParameter param = new SqlParameter("@status", System.Data.SqlDbType.Int);
            param.Value = Status;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@description", System.Data.SqlDbType.NVarChar, -1);
            param.Value = string.IsNullOrEmpty(Description) ? (object)DBNull.Value : Description;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@HostName", System.Data.SqlDbType.NVarChar, -1);
            param.Value = string.IsNullOrEmpty(HostName) ? (object)DBNull.Value : HostName;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@IPs", System.Data.SqlDbType.Xml, -1);
            #region Prepare XML
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            var nRoot = doc.CreateElement("IPs");
            doc.AppendChild(nRoot);

            if (IPs != null)
            {
                foreach (var ip in IPs)
                {
                    var nElem = doc.CreateElement("IP");
                    nElem.InnerText = ip;
                    nRoot.AppendChild(nElem);
                }
            }
            #endregion
            param.Value = doc.InnerXml;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@lastping", System.Data.SqlDbType.DateTime);
            param.Value = LastPing == DateTime.MinValue ? DT_NEVER : LastPing;
            cmd.Parameters.Add(param);

            if (HasValidID())
            {
                param = new SqlParameter("@serverID", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
        }

        public Secret TranslateSecret(string secretName)
        {
            log.DebugFormat("Retrieving secret {0:S}.", secretName);

            Secret secret = null;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    secret = Secret.GetByID<Secret>(conn, trans, secretName);
                    trans.Commit();
                }
            }
            if (secret != null)
                log.DebugFormat("Secret {0:S} retrieved (char count {1:N0}).", secretName, secret.PlainTextValue.Length);
            else
            {
                log.WarnFormat("Secret {0:S} not found.", secretName);
                throw new Exceptions.SecretNotFoundException(secretName);
            }

            return secret;
        }

        protected string DetemplatePayload(string payload)
        {
            while (true)
            {
                int iStart = payload.IndexOf(TEMPLATE_START);
                if (iStart == -1)
                    return payload;

                int iEnd = payload.IndexOf(TEMPLATE_END, iStart);
                if (iEnd == -1)
                    return payload;

                string sCypher = payload.Substring(iStart + TEMPLATE_START.Length, iEnd - (iStart + TEMPLATE_START.Length)).Trim();
                Secret secret = TranslateSecret(sCypher);

                payload = payload.Substring(0, iStart) + secret.PlainTextValue + payload.Substring(iEnd + TEMPLATE_END.Length);
            }
        }

        protected void PingThread()
        {
            while (true)
            {
                try
                {
                    log.TraceFormat("{0:S} - Update server LKA", this.ToString());

                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction())
                        {
                            this.UpdateKeepAlive(conn, trans);
                            trans.Commit();
                        }
                    }
                    Thread.Sleep(int.Parse(Configuration["SERVER_KEEPALIVE_SLEEP_MS"]));
                }
                catch (Exception exce) // catch all to keep the thread alive
                {
                    log.ErrorFormat("Unhandled exception during PingThread: {0:S}", exce.ToString());
                }
            }
        }

        protected void ClearOldServersThread()
        {
            while (true)
            {
                try
                {
                    log.TraceFormat("{0:S} - Check for dead servers", this.ToString());

                    // a server is dead if there is no update in the last xxx msseconds
                    int iTimeoutMilliseconds = int.Parse(Configuration["SERVER_MAXIMUM_UPDATE_LAG_MS"]);

                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(Database.tsql.Extractor.Get("Server.ClearOldServersThread"), conn);

                        SqlParameter param = new SqlParameter("@statusToSet", System.Data.SqlDbType.Int);
                        param.Value = TaskStatus.Dead;
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@timeoutMilliSeconds", System.Data.SqlDbType.BigInt);
                        param.Value = iTimeoutMilliseconds;
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@minStatus", System.Data.SqlDbType.Int);
                        param.Value = TaskStatus.Unknown;
                        cmd.Parameters.Add(param);

                        cmd.Prepare();
                        int iRet = cmd.ExecuteNonQuery();
                        if(iRet != 0)
                            log.InfoFormat("Marked {0} server(s) as dead.", iRet);
                    }

                    Thread.Sleep(int.Parse(Configuration["SERVER_POLL_DISABLE_DEAD_SERVERS_SLEEP_MS"]));
                }
                catch(Exception exce) // catch all to keep the thread alive
                {
                    log.ErrorFormat("Unhandled exception during ClearOldServersThread: {0:S}", exce.ToString());
                }
            }
        }

        protected void DeadTasksThread()
        {
            while (true)
            {
                try
                {
                    log.TraceFormat("{0:S} - Check for dead tasks", this.ToString());

                    // a task is dead if there is no update in the xxx milliseconds (1 minute default)
                    int iTimeoutMilliseconds = int.Parse(Configuration["TASK_MAXIMUM_UPDATE_LAG_MS"]);

                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                        {
                            var lExpired = LiveExecutionStatus.GetAndLockExpired(conn, trans, iTimeoutMilliseconds);

                            foreach (var les in lExpired)
                            {
                                #region Mark execution as expired
                                // insert into dead table
                                DeadExecutionStatus des = new DeadExecutionStatus(les, TaskStatus.Dead, null);
                                DeadExecutionStatus.Insert(conn, trans, des);

                                log.WarnFormat("Setting LiveExecutionStatus {0:S} as dead", les.ToString());
                                // remove from live table
                                if (!LiveExecutionStatus.Delete(conn, trans, les))
                                    throw new Exceptions.DatabaseConcurrencyException<LiveExecutionStatus>("Delete", les);

                                // if required, reenqueue
                                var task = YoctoScheduler.Core.Database.Task.GetByID<Task>(conn, trans, les.TaskID);
                                if (task.ReenqueueOnDead)
                                {
                                    log.InfoFormat("Reenqueuing {0:S} as requested", task.ToString());
                                    ExecutionQueueItem eqi = new ExecutionQueueItem()
                                    {
                                        TaskID = task.ID,
                                        Priority = Priority.Normal,
                                        ScheduleID = les.ScheduleID,
                                        InsertDate = DateTime.Now
                                    };
                                    ExecutionQueueItem.Insert(conn, trans, eqi);
                                }
                                #endregion
                            }

                            trans.Commit();
                        }
                    }

                    Thread.Sleep(int.Parse(Configuration["SERVER_POLL_DISABLE_DEAD_TASKS_SLEEP_MS"]));
                }
                catch (Exception exce) // catch all to keep the thread alive
                {
                    log.ErrorFormat("Unhandled exception during DeadTasksThread: {0:S}", exce.ToString());
                }
            }
        }

        protected void DequeueTasksThread()
        {
            while (true)
            {
                try
                {
                    log.TraceFormat("{0:S} - Check for tasks to start", this.ToString());

                    List<LiveExecutionStatus> lLessStarted = new List<LiveExecutionStatus>();

                    // Here we get the first task to start, and start it.
                    // NewTask could raise an exception if the task constructon fails
                    // to parse the payload. We should avoid letting this starve the queue and put it in failed state.
                    // In order to do so, we commit the transaction before instantiating the Task's watchdog.
                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();

                        // Start getting a peek on running tasks. We do not care of phantom reads so we
                        // just settle for ReadCommitted isolation level.
                        using (SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                        {
                            // now we lock the ExecutionQueueItem exclusively.
                            List<ExecutionQueueItem> lEqis = ExecutionQueueItem.GetAll<ExecutionQueueItem>(conn, trans, LockType.TableLockX);

                            List<LiveExecutionStatus> lLess = LiveExecutionStatus.GetAll<LiveExecutionStatus>(conn, trans, LockType.Default);

                            // now for each task in the queue find if it can be started and the start it.
                            foreach (var eqi in lEqis)
                            {
                                #region Task to start detection with concurrency limits
                                var task = Database.Task.GetByID<Task>(conn, trans, eqi.TaskID);

                                var lGlobalExecutions = lLess.FindAll(x => x.TaskID == eqi.TaskID);
                                var lLocalExecutions = lGlobalExecutions.FindAll(x => x.ServerID == this.ID);

                                // we need to count the executions we have started in this transaction as they will not
                                // show in the previous count.
                                var lJustStartedExecutions = lLessStarted.FindAll(x => x.TaskID == eqi.TaskID);

                                int iGlobalExecutionsCount = lGlobalExecutions.Count;
                                int iLocalExecutionsCount = lLocalExecutions.Count;
                                var iJustStartedExecutions = lJustStartedExecutions.Count;

                                if (
                                    (task.ConcurrencyLimitGlobal == 0 || task.ConcurrencyLimitGlobal > iGlobalExecutionsCount) &&
                                    (task.ConcurrencyLimitSameInstance == 0 || task.ConcurrencyLimitSameInstance > (iLocalExecutionsCount + iJustStartedExecutions)) // do not forget just started entries!
                                )
                                {
                                    log.InfoFormat("Preparing to start task {0:S}", task.ToString());
                                    // add to live table
                                    var les = new LiveExecutionStatus(eqi.TaskID, this.ID, eqi.ScheduleID);
                                    LiveExecutionStatus.Insert(conn, trans, les);

                                    // remove from pending execution queue
                                    if (!ExecutionQueueItem.Delete(conn, trans, eqi))
                                        throw new Exceptions.DatabaseConcurrencyException<ExecutionQueueItem>("Delete", eqi);

                                    lLessStarted.Add(les);
                                }
                                else
                                {
                                    log.DebugFormat("Task not started: current situation ({0:N0} / {1:N0}), task {2:S}", iGlobalExecutionsCount, iLocalExecutionsCount + iJustStartedExecutions, task.ToString());
                                }
                                #endregion
                            }
                            trans.Commit();
                        }
                    }

                    // start the execution. As this call might fail we handle the failue in place.
                    // If failed the task is considered deququed succesfully. It's up to the
                    // workflow manager to handle this kind of failures.
                    foreach (var les in lLessStarted)
                    {
                        Task task;
                        try
                        {
                            #region Task thread execution start
                            using (SqlConnection conn = new SqlConnection(ConnectionString))
                            {
                                conn.Open();
                                task = Database.Task.GetByID<Task>(conn, null, les.TaskID);
                            }

                            // get the task for the configuration payload
                            log.DebugFormat("Starting enqueued task {0:S} for LES {1:S}", task.ToString(), les.ToString());

                            string detemplatedPayload = DetemplatePayload(task.Payload);

                            var wd = ExecutionTasks.Factory.NewTask(this, task.Type, detemplatedPayload, les);
                            wd.Start();
                            log.InfoFormat("Started task {0:S} as live execution status {1:S}", task.ToString(), les.ToString());
                            #endregion
                        }
                        catch (Exception exce)
                        {
                            log.WarnFormat("LES {0:S} failed at startup: {0:S}", les.ToString(), exce.ToString());

                            #region Set as failed during startup
                            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
                            {
                                conn.Open();
                                using (var trans = conn.BeginTransaction())
                                {
                                    DeadExecutionStatus des = new DeadExecutionStatus(les, TaskStatus.ExceptionAtStartup, exce.ToString());
                                    DeadExecutionStatus.Insert(conn, trans, des);

                                    if (!ExecutionQueueItem.Delete(conn, trans, les))
                                        throw new Exceptions.DatabaseConcurrencyException<LiveExecutionStatus>("Delete", les);

                                    trans.Commit();
                                }
                            }
                            #endregion
                        }
                    }
                }
                catch (Exception exce) // we must catch unhandled exceptions to keep this thread alive
                {
                    log.ErrorFormat("Unhandled exception during DequeueTasksThread: {0:S}", exce.ToString());
                }

                Thread.Sleep(int.Parse(Configuration["SERVER_POLL_TASK_QUEUE_SLEEP_MS"]));
            }
        }

        protected void TasksScheduledThread()
        {
            while (true)
            {
                log.TraceFormat("{0:S} - Check for tasks to schedule", this.ToString());

                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                        {
                            // Get enabled schedules
                            var lSchedules = Schedule.GetAndLockEnabledNotRunning(conn, trans);

                            #region look for schedules to fire
                            lSchedules.ForEach(sched =>
                            {
                                NCrontab.CrontabSchedule cs = NCrontab.CrontabSchedule.Parse(sched.Cron);
                                if (cs.GetNextOccurrence(LastScheduleCheck) < DateTime.Now && (sched.LastFired < cs.GetNextOccurrence(LastScheduleCheck)))
                                {
                                    var task = YoctoScheduler.Core.Database.Task.GetByID<Task>(conn, trans, sched.TaskID);
                                    log.InfoFormat("Starting schedulation {0:S} due to cron {1:S}", task.ToString(), sched.ToString());

                                // save schedule fire time
                                sched.LastFired = DateTime.Now;
                                    Schedule.Update(conn, trans, sched);

                                    ExecutionQueueItem eqi = new ExecutionQueueItem()
                                    {
                                        TaskID = task.ID,
                                        Priority = Priority.Normal,
                                        ScheduleID = sched.ID,
                                        InsertDate = DateTime.Now
                                    };
                                    ExecutionQueueItem.Insert(conn, trans, eqi);
                                    log.InfoFormat("Execution enqueued {0:S}", eqi.ToString());
                                }
                            });
                            #endregion

                            trans.Commit();
                        }
                    }

                    LastScheduleCheck = DateTime.Now;

                    Thread.Sleep(int.Parse(Configuration["SERVER_POLL_TASK_SCHEDULER_SLEEP_MS"]));
                }
                catch (Exception exce) // catch all to keep the thread alive
                {
                    log.ErrorFormat("Unhandled exception during TasksScheduledThread: {0:S}", exce.ToString());
                }
            }
        }

        protected void CommandsThread()
        {
            while (true)
            {
                try
                {
                    log.TraceFormat("{0:S} - Check for server commands", this.ToString());

                    List<GenericCommand> commands = null;

                    #region Retrieve commands
                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                        {
                            commands = GenericCommand.DequeueByServerID(conn, trans, this.ID);
                            trans.Commit();
                        }
                    }
                    #endregion

                    if (commands.Count > 0)
                    {
                        #region Process commands
                        log.InfoFormat("Retrieved {0:N0} commands", commands.Count);
                        foreach (var cmd in commands)
                        {
                            switch (cmd.Command)
                            {
                                case ServerCommand.KillTask:
                                    Commands.KillExecution.KillExecution kt = new Commands.KillExecution.KillExecution(cmd);
                                    log.InfoFormat("Kill task requested (LiveExecutionStatusGUID = {0:S}", kt.Configuration.TaskID.ToString());

                                    // get the LiveExecutionStatus and request the kill.
                                    // Do not remove it from the list here as it will be done by the task once dead.
                                    YoctoScheduler.Core.ExecutionTasks.Watchdog t;
                                    if (!_liveTasks.TryGetValue(kt.Configuration.TaskID, out t))
                                    {
                                        log.WarnFormat("LiveExecutionStatusGUID == {0:S} not found. Maybe it's already dead?", kt.Configuration.TaskID.ToString());
                                    }
                                    else
                                    {
                                        t.Abort();
                                    }
                                    break;
                                case ServerCommand.RestartServer:
                                    // TODO
                                    log.WarnFormat("Restart server requested");
                                    break;
                                default:
                                    log.WarnFormat("Unsupported command received: {0:S}", cmd.Command);
                                    break;
                            }
                        }
                        #endregion
                    }

                    Thread.Sleep(int.Parse(Configuration["SERVER_POLL_COMMANDS_SLEEP_MS"]));
                }
                catch (Exception exce) // catch all to keep the thread alive
                {
                    log.ErrorFormat("Unhandled exception during CommandsThread: {0:S}", exce.ToString());
                }
            }
        }

        #region Database

        public override void ParseFromDataReader(SqlDataReader r)
        {
            List<string> lIPs = new List<string>();
            string xmlStr = r.GetString(5);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            foreach (XmlNode node in doc.SelectNodes("IPs/IP"))
            {
                lIPs.Add(node.InnerText);
            }

            ID = r.GetInt32(0);
            Status = (TaskStatus)r.GetInt32(1);
            Description = r.GetString(2);
            LastPing = r.GetDateTime(3);
            HostName = r.GetString(4);
            IPs = lIPs;
        }

        public void UpdateKeepAlive(SqlConnection conn, SqlTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(Database.tsql.Extractor.Get("Server.UpdateKeepAlive"), conn, trans))
            {
                PopolateParameters(cmd);
                cmd.Prepare();
                this.LastPing = (DateTime)cmd.ExecuteScalar();
            }
        }
        #endregion
    }
}
