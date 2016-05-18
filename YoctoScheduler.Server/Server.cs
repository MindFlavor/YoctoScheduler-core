using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using YoctoScheduler.Core;

namespace YoctoScheduler.Server
{
    public class Server
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Server));

        public int Id { get; set; }
        public DateTime LastScheduleCheck { get; protected set; }

        protected Server(int Id)
        {
            this.Id = Id;
            this.LastScheduleCheck = DateTime.MinValue;
        }


        public static Server CreateServer(string Description)
        {
            // register itself
            int serverId;
            using (MasterModel mm = new MasterModel())
            {
                var innerServer = new YoctoScheduler.Core.Server() { Description = Description, LastPing = DateTime.Now };
                mm.Servers.Add(innerServer);
                mm.SaveChanges();

                serverId = innerServer.ServerID;

            }
            Server srv = new Server(serverId);
            log.DebugFormat("{0:S} - Created server ", srv.ToString());

            Thread t;
            //#region start ping thread
            //t = new Thread(new ThreadStart(srv.PingThread));
            //t.IsBackground = true;
            //log.DebugFormat("{0:S} - Starting ping thread", srv.ToString());
            //t.Start();
            //#endregion

            //#region start clear old servers thread
            //t = new Thread(new ThreadStart(srv.ClearOldServersThread));
            //t.IsBackground = true;
            //log.DebugFormat("{0:S} - Starting clear old servers thread", srv.ToString());
            //t.Start();
            //#endregion

            //#region dead task thread
            //t = new Thread(new ThreadStart(srv.DeadTasksThread));
            //t.IsBackground = true;
            //log.DebugFormat("{0:S} - Starting dead task thread thread", srv.ToString());
            //t.Start();
            //#endregion

            #region task thread
            t = new Thread(new ThreadStart(srv.TasksThread));
            t.IsBackground = true;
            log.DebugFormat("{0:S} - Starting task thread thread", srv.ToString());
            t.Start();
            #endregion

            #region Set server as running
            log.DebugFormat("{0:S} - Setting server as running", srv.ToString());
            using (MasterModel mm = new MasterModel())
            {
                mm.Servers.Where(x => x.ServerID == serverId).First().Status = Status.Running;
                mm.SaveChanges();
            }
            #endregion
            log.InfoFormat("{0:S} - Server running", srv.ToString());

            return srv;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[Id={1:N0}, LastScheduleCheck={2:S}]",
                this.GetType().FullName, Id,
                LastScheduleCheck.ToString("yyyyMMdd hh:mm::ss"));
        }

        protected void PingThread()
        {
            while (true)
            {
                using (MasterModel mm = new MasterModel())
                {
                    mm.Servers.Where(x => x.ServerID == Id).First().LastPing = DateTime.Now;
                    log.DebugFormat("{0:S} - Ping", this.ToString());
                    mm.SaveChanges();
                }

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

                using (MasterModel mm = new MasterModel())
                {
                    mm.Servers.Where(x => (x.LastPing < dtDead) && (x.Status != Status.Dead)).AsParallel().ForAll(x =>
                    {
                        log.InfoFormat("{0:S} - Setting server {1:N0} as dead (last ping {2:S} ago)", this.ToString(), x.ServerID, (DateTime.Now - x.LastPing).ToString());
                        x.Status = Status.Dead;
                    });
                    mm.SaveChanges();
                }

                // every min
                Thread.Sleep(1 * 60 * 1000);
            }
        }

        protected void DeadTasksThread()
        {
            while (true)
            {
                // a task is dead if there is no update in the last 1 minute
                DateTime dtDead = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
                using (MasterModel mm = new MasterModel())
                {
                    // set dead tasks as dead
                    mm.ExecutionStatus.Where(x => (x.LastUpdate < dtDead) && (x.Status != Status.Dead)).AsParallel().ForAll(x =>
                    {
                        log.InfoFormat("{0:S} - Setting task (TaskID={1:N0}, ServerID={2:N0}) as dead (last update {3:S} ago)",
                            this.ToString(), x.TaskID, x.ServerID, (DateTime.Now - x.LastUpdate).ToString());
                        x.Status = Status.Dead;
                    });
                    mm.SaveChanges();
                }

                // every 30 seconds
                Thread.Sleep(30 * 1000);
            }
        }

        protected void TasksThread()
        {
            while (true)
            {
                log.DebugFormat("{0:S} - Check for tasks to start - Starting", this.ToString());
                using (MasterModel mm = new MasterModel())
                {
                    // get last executions
                    // for this to be bearable we should create this index:
                    // CREATE NONCLUSTERED INDEX idx_LastUpdate ON [dbo].[ExecutionStatus](LastUpdate DESC) INCLUDE(TaskID, ServerID, Status);
                    var lastExecutions = mm.ExecutionStatus.Where(e => e.LastUpdate > this.LastScheduleCheck).GroupBy(e => e.TaskID).Select(grp => new
                    {
                        grp.Key,
                        ExecutionStatus = grp.OrderByDescending(s => s.LastUpdate).FirstOrDefault()
                    }).ToList();

                    // find next schedules
                    mm.Schedules.Where(s => (s.Enabled == true)).AsParallel().ForAll(s => 
                    {
                        var lastExecution = lastExecutions.Where(e => e.ExecutionStatus.TaskID == s.TaskID).FirstOrDefault();
                        var next = NCrontab.CrontabSchedule.Parse(s.Cron).GetNextOccurrence(LastScheduleCheck);
                        if (next < DateTime.Now)
                        {
                            #region Schedule execution
                            log.InfoFormat("Executing {0:S}!", s.Task.ToString());
                            #endregion
                        }
                    });

                    mm.SaveChanges();
                }

                LastScheduleCheck = DateTime.Now;

                log.DebugFormat("{0:S} - Check for tasks to start - Completed", this.ToString());
                // every 30 seconds
                Thread.Sleep(10 * 1000);
            }
        }
    }
}
