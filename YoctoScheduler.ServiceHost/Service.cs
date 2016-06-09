using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;

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
                Console.WriteLine("Running interactively.");
                service.OnStart(null);
                Console.WriteLine("Press any key to stop.");
                Console.ReadKey();
                service.OnStop();
            }
            else
            {
                ServiceBase.Run(service);
            }

            return (int)ExitCodes.SUCCESS;
        }

        #region Service implementation

        public Service(ServiceConfiguration config)
        {
            this.ServiceName = config.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
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
