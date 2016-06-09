using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace YoctoScheduler.ServiceHost
{
    public class ServiceConfiguration
    {
        public static ServiceConfiguration FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ServiceConfiguration>(json);
        }

        private const string SERVICE_NAME_PREFIX = "ApsDashboard$";

        private string instanceName;
        public string InstanceName
        {
            get
            {
                return instanceName;
            }
            set
            {
                Regex re = new Regex("[A-Za-z][A-Za-z0-9]*");
                if (!re.IsMatch(value))
                {
                    throw new ArgumentException("InstanceName value is invalid.");
                }

                instanceName = value;
            }
        }

        public string ServiceName
        {
            get
            {
                return SERVICE_NAME_PREFIX + InstanceName;
            }
        }

        public string Log4NetConfigFile { get; set; }

        public string RestEndpoint { get; set; }

        public string HttpEndpoint { get; set; }

        public string WwwRoot { get; set; }

        public string ConnectionString { get; set; }
    }
}
