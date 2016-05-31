using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoScheduler.Core;
using System.Threading;
using NCrontab;
using YoctoScheduler.Core.Database;

namespace Test
{
    class Program
    {
        private const string LOG4NET_CONFIG = "Test.log4net.xml";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

        public static YoctoScheduler.Core.Server srvInstance;
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

            #region Startup Owin
            string baseAddress = FindAvailablePort();

            // Start OWIN host 
            YoctoScheduler.WebAPI.Startup.ConnectionString = srvInstance.ConnectionString;
            var owin = Microsoft.Owin.Hosting.WebApp.Start<YoctoScheduler.WebAPI.Startup>(url: baseAddress);
            log.InfoFormat("WebAPI initilized at {0:S}", baseAddress);
            #endregion

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
                        case "new_mock_task":
                            if (tokens.Length < 4)
                            {
                                Console.WriteLine("Syntax error, must specify if the task must be restarted in case determined dead, the task sleep in seconds and encryption thumbprint");
                                continue;
                            }
                            CreateMockTask(bool.Parse(tokens[1]), int.Parse(tokens[2]), tokens[3]);
                            break;                      
                        case "new_execution":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("Syntax error, must specify a valid task id");
                                continue;
                            }
                            CreateExecution(int.Parse(tokens[1]));
                            break;
                        case "new_command":
                            if (tokens.Length < 4)
                            {
                                Console.WriteLine("Syntax error, must specify a valid server id, a command id and a payload");
                                continue;
                            }
                            CreateCommand(int.Parse(tokens[1]), int.Parse(tokens[2]), string.Join(" ", tokens.Skip(3)));
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
                            if (tokens.Length < 3)
                            {
                                Console.WriteLine("Syntax error, must specify a secret name, a valid certificate thumbprint and something to encrypt");
                                continue;
                            }

                            string sToEnc = string.Join(" ", tokens.Skip(3));
                            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
                            {
                                conn.Open();
                                using (var trans = conn.BeginTransaction())
                                {
                                    var secret = Secret.New(conn, trans, tokens[1], tokens[2], sToEnc);
                                    trans.Commit();

                                    log.InfoFormat("Created secret {0:S}", secret.ToString());
                                }
                            }
                            break;
                        case "get_secret":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("Syntax error, must specify a valid secret name");
                                continue;
                            }

                            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
                            {
                                conn.Open();
                                using (var trans = conn.BeginTransaction())
                                {
                                    var secret = Secret.RetrieveByID(conn, trans, tokens[1]);
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
                            Console.WriteLine("\tnew_mock_task <sleep_seconds>");
                            Console.WriteLine("\tnew_execution <task_id>");
                            Console.WriteLine("\tnew_schedule <task_id> <cron expression>");
                            Console.WriteLine("\tnew_secret <thumbprint> <string to encrypt");
                            Console.WriteLine("\tget_secret <secret_id>");
                            Console.WriteLine("\tnew_command <server_id> <command> <payload>");

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

        static string FindAvailablePort()
        {
            int port = 9000; // preferred port

            var ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            bool fFound = false;
            while (!fFound)
            {
                var rPort = tcpConnInfoArray.FirstOrDefault(p => p.LocalEndPoint.Port == port);
                fFound = rPort == null;
                if (!fFound)
                    port++;
            }

            return "http://*:" + port + "/";      
        }

        static void CreateCommand(int serverID, int command, string payload)
        {
            YoctoScheduler.Core.ServerCommand cmd = (YoctoScheduler.Core.ServerCommand)command;
            GenericCommand gc;

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    gc = GenericCommand.New(conn, trans, serverID, cmd, payload);
                    trans.Commit();
                }
            }
            log.InfoFormat("Created command {0:S}", gc.ToString());
        }

        static void CreateMockTask(bool ReenqueueOnDead, int iSleepSeconds, string thumbprint)
        {
            YoctoScheduler.Core.ExecutionTasks.MockTask.MockTask mt = new YoctoScheduler.Core.ExecutionTasks.MockTask.MockTask();
            mt.Configuration = new YoctoScheduler.Core.ExecutionTasks.MockTask.Configuration() { SleepSeconds = iSleepSeconds };
            string payload = mt.SerializeConfiguration();

            Guid gSecret = Guid.NewGuid();
            string toreplace = iSleepSeconds.ToString();
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    Secret secret = Secret.New(conn, trans, gSecret.ToString(), thumbprint, toreplace);
                    payload = payload.Replace(toreplace, Server.TEMPLATE_START + gSecret.ToString() + Server.TEMPLATE_END);
                    trans.Commit();
                }
            }

            YoctoScheduler.Core.Database.Task task;
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    task = YoctoScheduler.Core.Database.Task.New(conn, trans, ReenqueueOnDead, "Mock", payload);
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
                    var task = YoctoScheduler.Core.Database.Task.RetrieveByID(conn, trans, TaskId);
                    if (task == null)
                        throw new YoctoScheduler.Core.Exceptions.TaskNotFoundException(TaskId);

                    var eqi = ExecutionQueueItem.New(conn, trans, TaskId, Priority.Normal, null);
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
