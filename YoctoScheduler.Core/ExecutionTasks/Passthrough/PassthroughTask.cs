using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTasks.Passthrough
{
    public class PassthroughTask : StringBasedTask
    {
        public override string Do()
        {
            return Configuration;
        }
    }
}
