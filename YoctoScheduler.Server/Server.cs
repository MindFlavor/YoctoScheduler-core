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
        public Guid Guid { get; set; }

        protected Server(Guid Guid)
        {
            this.Guid = Guid;
        }
        

        public static Server CreateServer(string Description)
        {
            // register itself
            Guid guid;
            using (MasterModel mm = new MasterModel())
            {
                var innerServer = new YoctoScheduler.Core.Server() { Description = Description, LastPing = DateTime.Now };
                mm.Servers.Add(innerServer);
                mm.SaveChanges();

                guid = innerServer.Guid;
            }
            Server srv = new Server(guid);

            // start ping thread
            Thread t = new Thread(new ThreadStart(srv.PingThread));
            t.IsBackground = true;
            t.Start();

            // start clear old servers thread
            t = new Thread(new ThreadStart(srv.ClearOldServersThread));
            t.IsBackground = true;
            t.Start();

            // dead task thread
            t = new Thread(new ThreadStart(srv.DeadTasksThread));
            t.IsBackground = true;
            t.Start();

            //// task thread
            t = new Thread(new ThreadStart(srv.TasksThread));
            t.IsBackground = true;
            t.Start();

            #region Set server as running
            using (MasterModel mm = new MasterModel())
            {
                mm.Servers.Where(x => x.Guid == guid).First().Status = Status.Running;
                mm.SaveChanges();
            }
            #endregion

            return srv;
        }

        protected void PingThread()
        {
            while (true)
            {
                using (MasterModel mm = new MasterModel())
                {
                    mm.Servers.Where(x => x.Guid == Guid).First().LastPing = DateTime.Now;
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
                    mm.Servers.Where(x => x.LastPing < dtDead).AsParallel().ForAll(x => x.Status = Status.Dead);
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
                    mm.ExecutionStatus.Where(x => x.LastUpdate < dtDead).AsParallel().ForAll(x => x.Status = Status.Dead);
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
                using (MasterModel mm = new MasterModel())
                {
                    // find task to run
                    //mm.Tasks.Where(task => task.ExecutionStatuses
                    mm.SaveChanges();
                }

                // every 30 seconds
                Thread.Sleep(30 * 1000);
            }
        }
    }
}
