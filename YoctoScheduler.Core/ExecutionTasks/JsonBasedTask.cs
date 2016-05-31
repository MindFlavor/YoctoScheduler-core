using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTasks
{
    public abstract class JsonBasedTask<T> : ITask
    {
        public T Configuration { get; set; }

        public string SerializeConfiguration()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(Configuration);
        }

        public void ParseConfiguration(string Payload)
        {
            Configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Payload);
        }

        public abstract string Do();
    }
}
