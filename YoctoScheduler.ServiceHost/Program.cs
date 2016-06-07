using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using CommandLine;
using Newtonsoft.Json;
using System.IO;

namespace YoctoScheduler.ServiceHost
{
    class Program
    {
        static int Main(string[] args)
        {
            CommandLineOptions opts = new CommandLineOptions();
            if (!Parser.Default.ParseArguments(args, opts))
            {
                return (int)ExitCodes.PARAMETER_ERROR;
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
                if (Environment.UserInteractive)
                {
                    Console.WriteLine("Failed to read configuration file.");
                    Console.WriteLine(ex.Message);
                }
                return (int)ExitCodes.CONFIGURATION_ERROR;
            }

            if (Environment.UserInteractive)
            {
                Console.WriteLine("Running interactively.");
                service.StartInteractive(null);
                Console.WriteLine("Press any key to stop.");
                Console.ReadKey();
                service.StopInteractive();
            }
            else
            {
                ServiceBase.Run(service);
            }

            return (int)ExitCodes.SUCCESS;
        }

        private enum ExitCodes
        {
            SUCCESS = 0,
            PARAMETER_ERROR = 1,
            CONFIGURATION_ERROR = 2
        }
    }
}
