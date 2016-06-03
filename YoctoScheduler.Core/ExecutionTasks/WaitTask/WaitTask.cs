using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTasks.WaitTask
{
    public class WaitTask : JsonBasedTask<Configuration>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(WaitTask));

        public override string Do()
        {
            log.InfoFormat("Called MockTask::Do(). Waiting for {0:N0} seconds", Configuration.SleepSeconds);
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(Configuration.SleepSeconds));
            log.InfoFormat("MockTask::Do() returing Ok!");

            return "Ok!";
        }
    }
}
