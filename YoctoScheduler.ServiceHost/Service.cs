using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace YoctoScheduler.ServiceHost
{
    public class Service : ServiceBase
    {
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

        public void StartInteractive(string[] args)
        {
            OnStart(args);
        }

        public void StopInteractive()
        {
            OnStop();
        }
    }
}
