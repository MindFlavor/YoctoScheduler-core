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
        static YoctoScheduler.Server.Server Server;
        static void Main(string[] args)
        {

            Server = YoctoScheduler.Server.Server.CreateServer("Server di prova " + DateTime.Now.ToString());

            //MasterModel mm = new MasterModel();
            //YoctoScheduler.Core.Task task = new YoctoScheduler.Core.Task();
            //mm.Tasks.Add(task);
            //mm.SaveChanges();

            //Server server = mm.Servers.Where(x => x.Guid == srv.Guid).First();

            //ExecutionStatus es = new ExecutionStatus() { Server = server, Task = task, LastUpdate = DateTime.Now };
            //mm.ExecutionStatus.Add(es);
            //mm.SaveChanges();

            //Console.WriteLine(es.Server.Description);

            bool fDone = false;
            while (!fDone)
            {
                Console.Write("> ");
                var cmd = Console.ReadLine();
                var tokens = cmd.Split(' ');
                try
                {
                    switch (tokens[0])
                    {
                        case "new_task":
                            CreateTask();
                            break;
                        case "new_execution":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("Syntax error, must specify a valid task id");
                                continue;
                            }
                            CreateExecution(int.Parse(tokens[1]));
                            break;
                        case "quit":
                            fDone = true;
                            break;
                        case "help":
                            Console.WriteLine("Available commands:");
                            Console.WriteLine("\tnew_task");
                            Console.WriteLine("\tnew_execution <task_id>");
                            Console.WriteLine("\tquit");
                            Console.WriteLine("\thelp");
                            break;
                        case "":
                            break;
                        default:
                            Console.WriteLine("Syntax error, unknown command \"{0:S}\". Type help for help, quit to quit.", tokens[0]);
                            break;

                    }
                } catch(Exception e)
                {
                    Console.WriteLine("Exception: " + e.ToString());
                }
            }
        }

        static void CreateTask()
        {
            using (MasterModel mm = new MasterModel())
            {
                var task = new YoctoScheduler.Core.Task();
                mm.Tasks.Add(task);
                mm.SaveChanges();
                Console.WriteLine("Created task id {0:S}", task.TaskID.ToString());
            }
        }

        static void CreateExecution(int TaskId)
        {
            var status = YoctoScheduler.Server.ExecutionStatus.CreateExecutionStatus(TaskId, Server.Id);

            Console.WriteLine("Created execution status", status);
        }
    }
}
