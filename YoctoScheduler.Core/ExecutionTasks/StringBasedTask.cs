using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTasks
{
    public abstract class StringBasedTask : GenericTask
    {
        public string Configuration;

        public override void ParseConfiguration(string Payload)
        {
            this.Configuration = Payload;
        }

        public override string SerializeConfiguration()
        {
            return Configuration;
        }
    }
}
