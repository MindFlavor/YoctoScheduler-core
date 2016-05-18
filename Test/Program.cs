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
        private const string LOG4NET_CONFIG = "Test.log4net.xml";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

        static YoctoScheduler.Server.Server srvInstance;
        static void Main(string[] args)
        {
            #region setup logging        
            log4net.Config.XmlConfigurator.Configure(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(LOG4NET_CONFIG));
            log.InfoFormat("Test program v{0:S} started.", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            #endregion

            srvInstance = YoctoScheduler.Server.Server.CreateServer("Server di prova " + DateTime.Now.ToString());

            //MasterModel mm = new MasterModel();
            //YoctoScheduler.Core.Task task = new YoctoScheduler.Core.Task();
            //mm.Tasks.Add(task);
            //mm.SaveChanges();

            //Server server = mm.Servers.Where(x => x.Guid == srv.Guid).First();

            //ExecutionStatus es = new ExecutionStatus() { Server = server, Task = task, LastUpdate = DateTime.Now };
            //mm.ExecutionStatus.Add(es);
            //mm.SaveChanges();

            //Console.WriteLine(es.Server.Description);
            Console.WriteLine("Program running, please input a command!");

            bool fDone = false;
            while (!fDone)
            {
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
                            log.WarnFormat("Syntax error, unknown command \"{0:S}\". Type help for help, quit to quit.", tokens[0]);
                            break;

                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Exception: " + e.ToString());
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
                log.InfoFormat("Created task id {0:S}", task.TaskID.ToString());
            }
        }

        static void CreateExecution(int TaskId)
        {
            var status = YoctoScheduler.Server.ExecutionStatus.CreateExecutionStatus(TaskId, srvInstance.Id);

            log.InfoFormat("Created execution status", status.ToString());
        }
    }
}
