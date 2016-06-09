using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;
using System.Data.SqlClient;
using YoctoScheduler.Core;
using YoctoScheduler.Core.Database;
using System.Threading;

namespace YoctoScheduler.ServiceHost
{
    public class Service : ServiceBase
    {
        static int Main(string[] args)
        {
            CommandLineOptions opts = new CommandLineOptions();
            if (!Parser.Default.ParseArguments(args, opts))
            {
                return (int)ExitCodes.ARGUMENT_ERROR;
            }

            Service service = null;

            try
            {
                ServiceConfiguration config = ServiceConfiguration.FromJson(
                    File.ReadAllText(opts.ConfigurationFileName)
                    );

                service = new Service(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read configuration file.");
                Console.WriteLine(ex.Message);
                return (int)ExitCodes.CONFIGURATION_ERROR;
            }

            if (Environment.UserInteractive)
            {
                service.OnStart(null);
                service.Terminated.WaitOne();
                service.OnStop();
            }
            else
            {
                ServiceBase.Run(service);
            }

            return (int)ExitCodes.SUCCESS;
        }

        #region Service implementation

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Service));

        public static YoctoScheduler.Core.Server srvInstance;

        private ServiceConfiguration config;

        public ManualResetEvent Terminated;

        public Service(ServiceConfiguration config)
        {
            this.Terminated = new ManualResetEvent(false);
            this.config = config;
            this.ServiceName = config.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
            #region setup logging
            log4net.Config.XmlConfigurator.Configure(new FileInfo(config.Log4NetConfigFile));

            log.InfoFormat(
                "Instance {0} v{0:S} started.",
                config.InstanceName, 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
                );
            #endregion

            log.InfoFormat("Retrieving configuration.");
            using (SqlConnection conn = new SqlConnection(config.ConnectionString))
            {
                conn.Open();

                #region setup Configuration
                Server.Configuration = new Configuration(conn);
                #endregion

                using (var trans = conn.BeginTransaction())
                {
                    srvInstance = YoctoScheduler.Core.Server.New(
                        conn, trans,
                        config.ConnectionString,
                        String.Format("Instance {0}",config.InstanceName));

                    trans.Commit();
                }
            }

            #region Startup Owin WebAPI
            // Start OWIN host 
            YoctoScheduler.WebAPI.Startup.ConnectionString = srvInstance.ConnectionString;
            var owin = Microsoft.Owin.Hosting.WebApp.Start<YoctoScheduler.WebAPI.Startup>(url: config.RestEndpoint);
            log.InfoFormat("WebAPI initilized at {0:S}", config.RestEndpoint);
            #endregion

            #region Startup Owin http
            // Start OWIN host 
            YoctoScheduler.Web.Startup.wwwRoot = config.WwwRoot;
            owin = Microsoft.Owin.Hosting.WebApp.Start<YoctoScheduler.Web.Startup>(url: config.HttpEndpoint);
            log.InfoFormat("WebAPP initilized at {0:S}", config.HttpEndpoint);
            #endregion

            if (Environment.UserInteractive)
            {
                System.Threading.Thread consoleThread = new System.Threading.Thread(
                    new System.Threading.ThreadStart(ConsoleThreadRoutine)
                    );

                consoleThread.IsBackground = true;
                consoleThread.Start();
            }
        }

        private void ConsoleThreadRoutine()
        {
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

                            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(config.ConnectionString))
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

            this.Terminated.Set();
        }

        void CreateCommand(int serverID, int command, string payload)
        {
            YoctoScheduler.Core.ServerCommand cmd = (YoctoScheduler.Core.ServerCommand)command;
            GenericCommand gc;

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(config.ConnectionString))
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

        protected override void OnStop()
        {
        }

        #endregion

        private enum ExitCodes
        {
            SUCCESS = 0,
            ARGUMENT_ERROR = 1,
            CONFIGURATION_ERROR = 2
        }

    }
}
