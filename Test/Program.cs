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

            #region Startup Owin WebAPI
            string baseAddress = "http://*:" + args[0] + "/";

            // Start OWIN host 
            YoctoScheduler.WebAPI.Startup.ConnectionString = srvInstance.ConnectionString;
            var owin = Microsoft.Owin.Hosting.WebApp.Start<YoctoScheduler.WebAPI.Startup>(url: baseAddress);
            log.InfoFormat("WebAPI initilized at {0:S}", baseAddress);
            #endregion

            #region Startup Owin http
            baseAddress = "http://*:" + args[1] + "/";

            // Start OWIN host 
            // TODO: Ask Matteo to expose this config parameter
            YoctoScheduler.Web.Startup.wwwRoot = @"D:\GIT\www\YoctoScheduler\app";
            owin = Microsoft.Owin.Hosting.WebApp.Start<YoctoScheduler.Web.Startup>(url: baseAddress);
            log.InfoFormat("WebAPP initilized at {0:S}", baseAddress);
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
                        case "new_command":
                            if (tokens.Length < 4)
                            {
                                Console.WriteLine("Syntax error, must specify a valid server id, a command id and a payload");
                                continue;
                            }
                            CreateCommand(int.Parse(tokens[1]), int.Parse(tokens[2]), string.Join(" ", tokens.Skip(3)));
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
                                    var secret = Secret.GetByID<Secret>(conn, trans, tokens[1]);
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



        static void CreateCommand(int serverID, int command, string payload)
        {
            YoctoScheduler.Core.ServerCommand cmd = (YoctoScheduler.Core.ServerCommand)command;
            GenericCommand gc;

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    gc = new GenericCommand(serverID, cmd, payload);
                    GenericCommand.Insert(conn, trans, gc);
                    trans.Commit();
                }
            }
            log.InfoFormat("Created command {0:S}", gc.ToString());
        } 
   }
}
