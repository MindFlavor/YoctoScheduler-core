using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTask
{
    public abstract class Task : ITask
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Task));

        public Server Server { get; protected set; }

        protected Thread tWatchdog = null;
        protected Thread tExecution = null;

        protected bool fRunning;

        public LiveExecutionStatus LiveExecutionStatus { get; protected set; }

        public Task(Server Server, LiveExecutionStatus LiveExecutionStatus)
        {
            this.Server = Server;
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
            tWatchdog = new Thread(new ThreadStart(WatchdogThread));
            tWatchdog.IsBackground = true;
            tWatchdog.Start();
        }

        public void Abort()
        {
            tExecution.Abort();
            tExecution.Join();
            tExecution = null;

            #region set the execution as aborted
            using (SqlConnection conn = new SqlConnection(Server.ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var d = DeadExecutionStatus.New(conn, trans, LiveExecutionStatus, Status.Aborted, null);
                    LiveExecutionStatus.Delete(conn, trans);

                    trans.Commit();
                }
            }
            #endregion
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
                log.DebugFormat("Excecution thread started");
                string retVal = Do();
                fRunning = false;

                #region Set as completed
                using (SqlConnection conn = new SqlConnection(Server.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        var d = DeadExecutionStatus.New(conn, trans, LiveExecutionStatus, Status.Completed, retVal);
                        LiveExecutionStatus.Delete(conn, trans);

                        trans.Commit();
                    }
                }
                #endregion

            }
            catch (Exception exce)
            {
                fRunning = false;

                #region Set as exceptioned
                using (SqlConnection conn = new SqlConnection(Server.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        var d = DeadExecutionStatus.New(conn, trans, LiveExecutionStatus, Status.Exception, exce.ToString());
                        LiveExecutionStatus.Delete(conn, trans);

                        trans.Commit();
                    }
                }
                #endregion
            }
        }

        public virtual bool IsAlive()
        {
            return fRunning && (tExecution != null) && (tExecution.IsAlive);
        }

        public abstract string Do();
    }
}
