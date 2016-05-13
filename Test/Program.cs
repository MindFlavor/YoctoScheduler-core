using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoScheduler.Core;
using System.Threading;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            var srv = YoctoScheduler.Server.Server.CreateServer("Server di prova " + DateTime.Now.ToString());

            MasterModel mm = new MasterModel();
            YoctoScheduler.Core.Task task = new YoctoScheduler.Core.Task();
            mm.Tasks.Add(task);
            mm.SaveChanges();

            Server server = mm.Servers.Where(x => x.Guid == srv.Guid).First();

            ExecutionStatus es = new ExecutionStatus() { Server = server, Task = task, LastUpdate = DateTime.Now };
            mm.ExecutionStatus.Add(es);
            mm.SaveChanges();

            Console.WriteLine(es.Server.Description);

            Console.ReadLine();
        }
    }
}
