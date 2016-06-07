using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;


namespace YoctoScheduler.ServiceHost
{
    class CommandLineOptions
    {
        [Option('c', HelpText = "Configuration file.", Required = true)]
        public string ConfigurationFileName { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("Options:");
            help.AddOptions(this);
            return help;
        }
    }
}
