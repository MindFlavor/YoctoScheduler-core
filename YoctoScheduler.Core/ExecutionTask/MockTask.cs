using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTask
{
    public class MockTask : Task
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MockTask));

        public MockTask(Server Server, LiveExecutionStatus LiveExecutionStatus) : base(Server, LiveExecutionStatus)
        {
        }

        public override string Do()
        {
            log.DebugFormat("Called Do(). Waiting for 90 seconds");
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(90));
            log.DebugFormat("Do() returing Ok!");

            return "Ok!";
        }
    }
}
