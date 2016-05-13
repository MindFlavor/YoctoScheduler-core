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

                    Thread.Sleep(5000);
                }
            }
        }
    }
}
