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
        public int Id { get; set; }

        protected Server(int Id)
        {
            this.Id = Id;
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
            Console.WriteLine("Created server {0:N0}", serverId);

            #region start ping thread
            Thread t = new Thread(new ThreadStart(srv.PingThread));
            t.IsBackground = true;
            t.Start();
            #endregion

            #region start clear old servers thread
            t = new Thread(new ThreadStart(srv.ClearOldServersThread));
            t.IsBackground = true;
            t.Start();
            #endregion

            #region dead task thread
            t = new Thread(new ThreadStart(srv.DeadTasksThread));
            t.IsBackground = true;
            t.Start();
            #endregion

            #region task thread
            t = new Thread(new ThreadStart(srv.TasksThread));
            t.IsBackground = true;
            t.Start();
            #endregion

            #region Set server as running
            using (MasterModel mm = new MasterModel())
            {
                mm.Servers.Where(x => x.ServerID == serverId).First().Status = Status.Running;
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
                    mm.Servers.Where(x => x.ServerID == Id).First().LastPing = DateTime.Now;
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
