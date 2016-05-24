using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Tasks
{
    public class Watchdog
    {
        public ITask Task { get; set; }
        public int ServerID { get; set; }

        public string ConnectionString { get; set; }
    }
}
