using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTask
{
    public class Factory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Factory));

        private Factory() { }

        public static Task NewTask(Server server, string taskType, string configPayload, LiveExecutionStatus les)
        {
            log.DebugFormat("Factory.NewTask({0:S}, {1:S}) called", server.ToString(), les.ToString());
            // reflection create object from json 
            // Product deserializedProduct = JsonConvert.DeserializeObject(json, t);
            // return deserializedProduct

            switch (taskType)
            {
                default:
                return new MockTask(server, configPayload, les);
            }
        }
    }
}
