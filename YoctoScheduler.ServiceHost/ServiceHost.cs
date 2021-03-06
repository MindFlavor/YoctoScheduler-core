﻿using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using CommandLine;
using YoctoScheduler.Core;
using YoctoScheduler.Core.Database;
using System.Security;

namespace YoctoScheduler.ServiceHost
{
    public class ServiceHost : ServiceBase
    {
        private static EventLog ApplicationLog;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ServiceHost));

        static int Main(string[] args)
        {
            ApplicationLog = new EventLog();
            ApplicationLog.Source = "YoctoScheduler.ServiceHost";

            try
            {
                ApplicationLog.WriteEntry(String.Format(
                    "Pid {0} Invoked with arguments {1}.",
                    Process.GetCurrentProcess().Id,
                    String.Join(" ", args))
                    );
            }
            catch (SecurityException)
            {
                Console.WriteLine("First execution must be performed by a user with administrative privileges.");
                return (int)ExitCodes.APPLICATION_LOG_SECURITY_ERROR;
            }

            CommandLineOptions opts = new CommandLineOptions();
            if (!Parser.Default.ParseArguments(args, opts))
            {
                return (int)ExitCodes.ARGUMENT_ERROR;
            }

            ServiceHost service = null;

            try
            {
                string configFile = opts.ConfigurationFileName;

                if (!File.Exists(configFile))
                {
                    configFile = Path.Combine(
                        GetExecutableDirectory(),
                        Path.GetFileName(configFile)
                        );
                }

                ApplicationLog.WriteEntry(String.Format(
                    "Pid {0} loading configuration file {1}.",
                    Process.GetCurrentProcess().Id,
                    configFile
                    ));

                ServiceConfiguration config = ServiceConfiguration.FromJson(
                    File.ReadAllText(configFile)
                    );

                #region setup logging
                string log4netConfigFile = config.Log4NetConfigFile;

                if (!File.Exists(log4netConfigFile))
                {
                    log4netConfigFile = Path.Combine(
                        GetExecutableDirectory(),
                        Path.GetFileName(log4netConfigFile)
                        );
                }

                log4net.Config.XmlConfigurator.Configure(new FileInfo(log4netConfigFile));
                #endregion

                service = new ServiceHost(config);
            }
            catch (Exception ex)
            {
                ApplicationLog.WriteEntry(String.Format(
                        "Pid {0} failed to read configuration file. Error: {1}",
                        Process.GetCurrentProcess().Id,
                        ex.Message), 
                    EventLogEntryType.Error
                    );
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


        public static Server srvInstance;

        private ServiceConfiguration config;
        public ManualResetEvent Terminated;

        public ServiceHost(ServiceConfiguration config)
        {
            this.Terminated = new ManualResetEvent(false);
            this.config = config;
            this.ServiceName = config.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
            log.InfoFormat(
                "Instance {0} v{0:S} started.",
                config.InstanceName, 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
                );

            log.InfoFormat("Retrieving configuration.");
            using (SqlConnection conn = new SqlConnection(config.ConnectionString))
            {
                conn.Open();

                #region setup Configuration
                Server.Configuration = new Configuration(conn);
                #endregion

                using (var trans = conn.BeginTransaction())
                {
                    srvInstance = Server.New(
                        conn, trans,
                        config.ConnectionString,
                        String.Format("Instance {0}",config.InstanceName));

                    trans.Commit();
                }
            }

            #region Startup Owin WebAPI
            // Start OWIN host 
            YoctoScheduler.WebAPI.Startup.ConnectionString = config.ConnectionString;
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
                Thread consoleThread = new Thread(
                    new ThreadStart(ConsoleThreadRoutine)
                    );

                consoleThread.IsBackground = true;
                consoleThread.Start();
            }
        }

        private void ConsoleThreadRoutine()
        {
            Console.WriteLine("Program running, press <enter> to terminate the program.");
            Console.ReadLine();

            this.Terminated.Set();
        }

        protected override void OnStop()
        {
        }

        #endregion

        private static string GetExecutableDirectory()
        {
            return new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
        }

        private enum ExitCodes
        {
            SUCCESS = 0,
            ARGUMENT_ERROR = 1,
            CONFIGURATION_ERROR = 2,
            APPLICATION_LOG_SECURITY_ERROR = 3
        }

    }
}
