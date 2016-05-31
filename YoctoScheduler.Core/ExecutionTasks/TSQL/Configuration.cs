using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTasks.TSQL
{
    public struct Configuration
    {
        public string ConnectionString;
        public string Statement;
        public int ConnectionTimeout;
    }
}
