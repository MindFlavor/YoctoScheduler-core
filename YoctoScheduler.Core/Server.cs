using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Xml;
using YoctoScheduler.Core.Database;

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
            if (!_liveTasks.TryAdd(task.LiveExecutionStatus.GUID, task))
            {
                log.WarnFormat("Registration failed for server {0:S}, task {1:S}", this.ToString(), task.ToString());
                throw new Exceptions.ConcurrencyException(string.Format("LiveExecutionStatus with GUID {0:S} already found in _liveTasks", task.LiveExecutionStatus.GUID.ToString()));
            }
        }

        public void DeregisterTask(YoctoScheduler.Core.ExecutionTasks.Watchdog task)
        {
            YoctoScheduler.Core.ExecutionTasks.Watchdog t;

            log.DebugFormat("Server {0:S} deregistering task {1:S}", this.ToString(), task.ToString());
            if (!_liveTasks.TryRemove(task.LiveExecutionStatus.GUID, out t))
            {
                log.WarnFormat("Deregistration failed for server {0:S}, task {1:S}", this.ToString(), task.ToString());
                throw new Exceptions.ConcurrencyException(string.Format("LiveExecutionStatus with GUID {0:S} not found in _liveTasks", task.LiveExecutionStatus.GUID.ToString()));
            }
        }

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
                LastPing = DateTime.Parse("2000-01-01"),
                Status = TaskStatus.Starting
            };

            using (SqlCommand cmd = new SqlCommand(Database.tsql.Extractor.Get("Server.New"), conn, trans))
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

            #region commands thread
            t = new Thread(new ThreadStart(server.CommandsThread));
            t.IsBackground = true;
            log.DebugFormat("{0:S} - Starting server commands thread", server.ToString());
            t.Start();
            #endregion


            #region Set server as running
            log.DebugFormat("{0:S} - Setting server as running", server.ToString());
            server.Status = TaskStatus.Running;

            server.PersistChanges(conn, trans);
            log.InfoFormat("{0:S} - Server running", server.ToString());
            #endregion

            return server;
        }

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(Database.tsql.Extractor.Get("Server.PersistChanges"), conn, trans))
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

            param = new SqlParameter("@HostName", System.Data.SqlDbType.NVarChar, -1);
            param.Value = HostName;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@IPs", System.Data.SqlDbType.Xml, -1);
            #region Prepare XML
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            var nRoot = doc.CreateElement("IPs");
            doc.AppendChild(nRoot);

            foreach (var ip in IPs)
            {
                var nElem = doc.CreateElement("IP");
                nElem.InnerText = ip;
                nRoot.AppendChild(nElem);
            }
            #endregion
            param.Value = doc.InnerXml;
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

        protected Secret TranslateSecret(string secretName)
        {
            log.DebugFormat("Retrieving secret {0:S}.", secretName);

            Secret secret = null;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    secret = Secret.RetrieveByID(conn, trans, secretName);
                    trans.Commit();
                }
            }
            if (secret != null)
                log.DebugFormat("Secret {0:S} retrieved (char count {1:N0}).", secretName, secret.PlainTextValue.Length);
            else
                log.WarnFormat("Secret {0:S} not found.", secretName, secret.PlainTextValue.Length);

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
                    SqlCommand cmd = new SqlCommand(Database.tsql.Extractor.Get("Server.ClearOldServersThread"), conn);

                    SqlParameter param = new SqlParameter("@statusToSet", System.Data.SqlDbType.Int);
                    param.Value = TaskStatus.Dead;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@dt", System.Data.SqlDbType.DateTime);
                    param.Value = dtDead;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@minStatus", System.Data.SqlDbType.Int);
                    param.Value = TaskStatus.Unknown;
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
                            var des = DeadExecutionStatus.New(conn, trans, les, TaskStatus.Dead, null);

                            log.WarnFormat("Setting LiveExecutionStatus {0:S} as dead", les.ToString());
                            // remove from live table
                            les.Delete(conn, trans);

                            // if required, reenqueue
                            var task = YoctoScheduler.Core.Database.Task.RetrieveByID(conn, trans, les.TaskID);
                            if (task.ReenqueueOnDead)
                            {
                                log.InfoFormat("Reenqueuing {0:S} as requested", task.ToString());
                                ExecutionQueueItem.New(conn, trans, task.ID, Priority.Normal, les.ScheduleID);
                            }
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
                try
                {
                    LiveExecutionStatus les = null;
                    Database.Task task = null;
                    ExecutionQueueItem eqi = null;

                    // Here we get the first task to start, and start it.
                    // NewTask could raise an exception if the task constructon fails
                    // to parse the payload. We should avoid letting this starve the queue and put it in failed state.
                    // In order to do so, we commit the transaction before instantiating the Task's watchdog.
                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                        {
                            eqi = ExecutionQueueItem.GetAndLockFirst(conn, trans);
                            if (eqi != null)
                            {
                                // add to live table
                                les = LiveExecutionStatus.New(conn, trans, eqi.TaskID, this.ID, eqi.ScheduleID);

                                // get the task for the configuration payload
                                task = Database.Task.RetrieveByID(conn, trans, eqi.TaskID);

                                // remove from pending execution queue
                                eqi.Delete(conn, trans);
                            }

                            trans.Commit();
                        }
                    }

                    // start the execution. As this call might fail we handle the failue in place.
                    // If failed the task is considered deququed succesfully. It's up to the
                    // workflow manager to handle this kind of failures.
                    try
                    {
                        if (eqi != null)
                        {
                            log.DebugFormat("Starting enqueued task {0:S}", eqi.ToString());

                            string detemplatedPayload = DetemplatePayload(task.Payload);

                            var wd = ExecutionTasks.Factory.NewTask(this, task.Type, detemplatedPayload, les);
                            wd.Start();
                            log.InfoFormat("Started task {0:S} as live execution status {1:S}", task.ToString(), les.ToString());
                        }
                    }
                    catch (Exception exce)
                    {
                        log.WarnFormat("Task {0:S} failed at startup: {0:S}", eqi.ToString(), exce.ToString());

                        #region Set as failed during startup
                        using (SqlConnection conn = new SqlConnection(this.ConnectionString))
                        {
                            conn.Open();
                            using (var trans = conn.BeginTransaction())
                            {
                                var d = DeadExecutionStatus.New(conn, trans, les, TaskStatus.ExceptionAtStartup, exce.ToString());
                                les.Delete(conn, trans);

                                trans.Commit();
                            }
                        }
                        #endregion
                    }
                }
                catch (Exception exce) // we must catch unhandled exceptions to keep the thread alive
                {
                    log.ErrorFormat("Unhandled exception during DequeueTasksThread: {0:S}", exce.ToString());
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
                            NCrontab.CrontabSchedule cs = NCrontab.CrontabSchedule.Parse(sched.Cron);
                            if (cs.GetNextOccurrence(LastScheduleCheck) < DateTime.Now)
                            {
                                var task = YoctoScheduler.Core.Database.Task.RetrieveByID(conn, trans, sched.TaskID);
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

        protected void CommandsThread()
        {
            while (true)
            {
                log.DebugFormat("{0:S} - Check for server commands", this.ToString());

                List<GenericCommand> commands = null;

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        commands = GenericCommand.DequeueByServerID(conn, trans, this.ID);
                        trans.Commit();
                    }
                }

                if (commands.Count > 0)
                {
                    // process commands
                    log.InfoFormat("Retrieved {0:N0} commands", commands.Count);
                    foreach (var cmd in commands)
                    {
                        if (cmd is Commands.KillExecutionTask)
                        {
                            Commands.KillExecutionTask kt = (Commands.KillExecutionTask)cmd;
                            log.InfoFormat("Kill task requested (LiveExecutionStatusGUID = {0:S}", kt.LiveExecutionStatusGUID.ToString());

                            // get the LiveExecutionStatus and request the kill. 
                            // Do not remove it from the list here as it will be done by the task once dead.
                            YoctoScheduler.Core.ExecutionTasks.Watchdog t;
                            if (!_liveTasks.TryGetValue(kt.LiveExecutionStatusGUID, out t))
                            {
                                log.WarnFormat("LiveExecutionStatusGUID == {0:S} not found. Maybe it's already dead?", kt.LiveExecutionStatusGUID.ToString());
                            }
                            else
                            {
                                t.Abort();
                            }
                        }
                        else if (cmd is Commands.RestartServer)
                        {
                            log.WarnFormat("Restart server requested");
                        }
                    }
                }

                Thread.Sleep(int.Parse(Configuration["SERVER_POLL_COMMANDS_SLEEP_MS"]));
            }
        }

        #region Database
        public static List<Server> GetAll(SqlConnection conn, SqlTransaction trans)
        {
            List<Server> lServers = new List<Server>();

            using (SqlCommand cmd = new SqlCommand(Database.tsql.Extractor.Get("Server.GetAll"), conn, trans))
            {
                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lServers.Add(ParseFromDataReader(reader));
                    }
                }
            }

            return lServers;
        }

        protected static Server ParseFromDataReader(SqlDataReader r)
        {
            List<string> lIPs = new List<string>();
            string xmlStr = r.GetString(5);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            foreach (XmlNode node in doc.SelectNodes("IPs/IP"))
            {
                lIPs.Add(node.InnerText);
            }

            return new Server(null)
            {
                ID = r.GetInt32(0),
                Status = (TaskStatus)r.GetInt32(1),
                Description = r.GetString(2),
                LastPing = r.GetDateTime(3),
                HostName = r.GetString(4),
                IPs = lIPs
            };
        }
        #endregion
    }
}
