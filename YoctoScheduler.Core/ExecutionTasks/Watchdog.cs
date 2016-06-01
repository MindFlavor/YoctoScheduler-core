using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.Core.ExecutionTasks
{
    public class Watchdog
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Watchdog));

        public Server Server { get; protected set; }

        protected Thread tWatchdog = null;
        protected Thread tExecution = null;

        protected bool fRunning;

        public ITask Task { get; protected set; }

        public LiveExecutionStatus LiveExecutionStatus { get; protected set; }

        public Watchdog(Server Server, ITask task, LiveExecutionStatus LiveExecutionStatus)
        {
            this.Server = Server;
            this.Task = task;
            this.LiveExecutionStatus = LiveExecutionStatus;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[IsAlive()={1:S}, fRunning={2:S}, Server={3:S}, LiveExecutionStatus={4:S}]",
                this.GetType().FullName,
                IsAlive().ToString(),
                fRunning.ToString(),
                Server.ToString(),
                LiveExecutionStatus.ToString());
        }

        public void Start()
        {
            if (LiveExecutionStatus == null)
                throw new FormatException("Cannot start a non live task (ie with null as LiveExecutionStatus)");

            tWatchdog = new Thread(new ThreadStart(WatchdogThread));
            tWatchdog.IsBackground = true;
            tWatchdog.Start();
        }

        public void Abort()
        {
            if(!IsAlive())
                throw new Exception("Cannot abort a non live task");

            tExecution.Abort();
            tExecution.Join();
            tExecution = null;
        }

        protected void WatchdogThread()
        {
            log.DebugFormat("Watchdog thread started");

            tExecution = new Thread(new ThreadStart(ExecutionThread));
            tExecution.IsBackground = true;
            tExecution.Start();

            fRunning = true;

            while (IsAlive())
            {
                log.DebugFormat("Watchdog loop {0:S}", this.ToString());

                // update live status
                using (SqlConnection conn = new SqlConnection(Server.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        LiveExecutionStatus.UpdateKeepAlive(conn, trans);
                        trans.Commit();
                    }
                }

                Thread.Sleep(int.Parse(Server.Configuration["WATCHDOG_SLEEP_MS"]));
            }
        }

        protected void ExecutionThread()
        {
            try
            {
                // register this task so it can be interrupted by proper message sent to the server
                Server.RegisterTask(this);

                log.DebugFormat("Excecution thread started");
                string retVal = Task.Do();
                fRunning = false;

                #region Set as completed
                using (SqlConnection conn = new SqlConnection(Server.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        // Update local LastUpdate so we can use it as "completed" time.
                        LiveExecutionStatus.LastUpdate = DateTime.Now;

                        var d = DeadExecutionStatus.New(conn, trans, LiveExecutionStatus, TaskStatus.Completed, retVal);
                        LiveExecutionStatus.Delete(conn, trans);

                        trans.Commit();
                    }
                }
                #endregion

            }
            catch (ThreadAbortException tae)
            {
                log.InfoFormat("Excecution thread aborted: {0:S}", tae.ToString());
                fRunning = false;

                #region set the execution as aborted
                using (SqlConnection conn = new SqlConnection(Server.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        // Update local LastUpdate so we can use it as "aborted" time.
                        LiveExecutionStatus.LastUpdate = DateTime.Now;

                        var d = DeadExecutionStatus.New(conn, trans, LiveExecutionStatus, TaskStatus.Aborted, null);
                        LiveExecutionStatus.Delete(conn, trans);

                        trans.Commit();
                    }
                }
                #endregion
            }
            catch (Exception exce)
            {
                log.WarnFormat("Excecution thread exceptioned: {0:S}", exce.ToString());
                fRunning = false;

                #region Set as exceptioned
                using (SqlConnection conn = new SqlConnection(Server.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        // Update local LastUpdate so we can use it as "failed" time.
                        LiveExecutionStatus.LastUpdate = DateTime.Now;

                        var d = DeadExecutionStatus.New(conn, trans, LiveExecutionStatus, TaskStatus.ExceptionDuringExecution, exce.ToString());
                        LiveExecutionStatus.Delete(conn, trans);

                        trans.Commit();
                    }
                }
                #endregion
            }
            finally
            {
                // not really needed, added to be foolproof
                fRunning = false;

                // deregister itself from server
                Server.DeregisterTask(this);
            }
        }

        public virtual bool IsAlive()
        {
            return fRunning && (tExecution != null) && (tExecution.IsAlive);
        }       
    }
}
