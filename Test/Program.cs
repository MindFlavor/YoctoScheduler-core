using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoScheduler.Core;
using System.Threading;
using NCrontab;

namespace Test
{
    class Program
    {
        private const string LOG4NET_CONFIG = "Test.log4net.xml";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

        static YoctoScheduler.Core.Server srvInstance;
        static void Main(string[] args)
        {
            #region setup logging        
            log4net.Config.XmlConfigurator.Configure(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(LOG4NET_CONFIG));
            log.InfoFormat("Test program v{0:S} started.", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            #endregion



            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
            {
                conn.Open();

                #region setup Configuration
                Server.Configuration = new Configuration(conn);
                #endregion

                using (var trans = conn.BeginTransaction())
                {
                    srvInstance = YoctoScheduler.Core.Server.New(
                        conn, trans,
                        System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString,
                        "Server di prova " + DateTime.Now.ToString());

                    trans.Commit();
                }
            }

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
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("Syntax error, must specify if the task must be restarted in case determined dead");
                                continue;
                            }
                            CreateTask(bool.Parse(tokens[1]));
                            break;
                        case "new_execution":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("Syntax error, must specify a valid task id");
                                continue;
                            }
                            CreateExecution(int.Parse(tokens[1]));
                            break;
                        case "new_schedule":
                            if (tokens.Length < 4)
                            {
                                Console.WriteLine("Syntax error, must specify a valid task id, a bool for enabled and a valid crontab");
                                continue;
                            }
                            CreateSchedule(int.Parse(tokens[1]), bool.Parse(tokens[2]), string.Join(" ", tokens.Skip(3)));

                            break;
                        case "new_secret":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("Syntax error, must specify a valid certificate thumbprint and something to encrypt");
                                continue;
                            }

                            string sToEnc = string.Join(" ", tokens.Skip(2));
                            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
                            {
                                conn.Open();
                                using (var trans = conn.BeginTransaction())
                                {
                                    var secret = YoctoScheduler.Core.Secret.New(conn, trans, tokens[1], sToEnc);
                                    trans.Commit();

                                    log.InfoFormat("Created secret {0:S}", secret.ToString());
                                }
                            }
                            break;
                        case "get_secret":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("Syntax error, must specify a valid secret ID");
                                continue;
                            }

                            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
                            {
                                conn.Open();
                                using (var trans = conn.BeginTransaction())
                                {
                                    var secret = YoctoScheduler.Core.Secret.RetrieveByID(conn, trans, int.Parse(tokens[1]));
                                    trans.Commit();

                                    log.InfoFormat("Retrieved secret {0:S}. Plain text is = \"{1:S}\".", secret.ToString(), secret.PlainTextValue);
                                }
                            }
                            break;

                        case "quit":
                            fDone = true;
                            break;
                        case "help":
                            Console.WriteLine("Available commands:");
                            Console.WriteLine("\tnew_task");
                            Console.WriteLine("\tnew_execution <task_id>");
                            Console.WriteLine("\tnew_schedule <task_id> <cron expression>");
                            Console.WriteLine("\tnew_secret <thumbprint> <string to encrypt");
                            Console.WriteLine("\tget_secret <secret_id>");
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

        static void CreateTask(bool ReenqueueOnDead)
        {
            YoctoScheduler.Core.Task task;
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    task = YoctoScheduler.Core.Task.New(conn, trans, ReenqueueOnDead);
                    trans.Commit();
                }
            }
            log.InfoFormat("Created task {0:S}", task.ToString());
        }

        static void CreateExecution(int TaskId)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var task = YoctoScheduler.Core.Task.RetrieveByID(conn, trans, TaskId);
                    if (task == null)
                        throw new YoctoScheduler.Core.Exceptions.TaskNotFoundException(TaskId);

                    var eqi = YoctoScheduler.Core.ExecutionQueueItem.New(conn, trans, TaskId, Priority.Normal, null);
                    trans.Commit();
                    log.InfoFormat("Task enqueued for execution: {0:S}", eqi.ToString());
                }
            }
        }

        static void CreateSchedule(int taskId, bool enabled, string cron)
        {
            Schedule sched;
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
            {
                conn.Open();
                sched = Schedule.New(conn, null, taskId, cron, enabled);
            }
            log.InfoFormat("Created schedule {0:S}", sched.ToString());
        }
    }
}
