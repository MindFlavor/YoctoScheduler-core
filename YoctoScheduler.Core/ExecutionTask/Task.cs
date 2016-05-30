﻿using System;
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

        protected string configPayload { get; set; }

        public LiveExecutionStatus LiveExecutionStatus { get; protected set; }

        public Task(Server Server, string configPayload, LiveExecutionStatus LiveExecutionStatus)
        {
            this.Server = Server;
            this.configPayload = configPayload;
            this.LiveExecutionStatus = LiveExecutionStatus;

            this.ValidateConfiguration();
        }

        public Task(Server server, LiveExecutionStatus liveExecutionStatus)
        {
            Server = server;
            LiveExecutionStatus = liveExecutionStatus;
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
                        var d = DeadExecutionStatus.New(conn, trans, LiveExecutionStatus, Status.Aborted, null);
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
                        var d = DeadExecutionStatus.New(conn, trans, LiveExecutionStatus, Status.Exception, exce.ToString());
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

        public abstract string Do();

        // This should throw an exception in case of problems
        // ie. NumberFormatException if a number cannot be parsed from a string with 
        // message like "Cannot convert the parameter to a number"
        public abstract void ValidateConfiguration();
    }
}
