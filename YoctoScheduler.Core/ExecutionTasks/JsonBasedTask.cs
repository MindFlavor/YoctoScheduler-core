using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTasks
{
    public abstract class JsonBasedTask<T> : GenericTask
    {
        public T Configuration { get; set; }

        public override string SerializeConfiguration()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(Configuration);
        }

        public override void ParseConfiguration(string Payload)
        {
            Configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Payload);
        }
    }
}
