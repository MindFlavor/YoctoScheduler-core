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

        protected static List<Type> _lTasks = null;

        public static List<Type> Tasks
        {
            get
            {
                if (_lTasks != null)
                    return _lTasks;

                // use reflection to register all the types implementing ITask
                _lTasks = new List<Type>();
                var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    var interfaces = type.GetInterfaces();

                    foreach (var intf in interfaces)
                    {
                        if (intf == typeof(ITask))
                        {
                            _lTasks.Add(type);
                            break;
                        }
                    }
                }
                return _lTasks;
            }
        }

        private Factory() { }

        public static Watchdog NewTask(Server server, string taskType, string configPayload, LiveExecutionStatus les)
        {
            log.DebugFormat("Factory.NewTask({0:S}, {1:S}) called", server.ToString(), les.ToString());

            var typeToCreate = Tasks.FirstOrDefault(t => t.Name == taskType);

            if(typeToCreate == null)
                throw new Exceptions.UnsupportedTaskException(taskType);

            GenericTask task = (GenericTask) Activator.CreateInstance(typeToCreate);
            task.ParseConfiguration(configPayload);
            return new Watchdog(server, task, les);

        }
    }
}
