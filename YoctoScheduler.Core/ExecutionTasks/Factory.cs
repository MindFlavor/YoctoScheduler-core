using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.Core.ExecutionTasks
{
    public class Factory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Factory));

        private Factory() { }

        public static Watchdog NewTask(Server server, string taskType, string configPayload, LiveExecutionStatus les)
        {
            log.DebugFormat("Factory.NewTask({0:S}, {1:S}) called", server.ToString(), les.ToString());
            
            // reflection create object from json 
            // Product deserializedProduct = JsonConvert.DeserializeObject(json, t);
            // return deserializedProduct

            switch (taskType.ToLower())
            {
                case "mock":
                    MockTask.MockTask mt = new MockTask.MockTask();
                    mt.ParseConfiguration(configPayload);
                    return new Watchdog(server, mt, les);
                default:
                    throw new Exceptions.UnsupportedTaskException(taskType);
            }
        }
    }
}
