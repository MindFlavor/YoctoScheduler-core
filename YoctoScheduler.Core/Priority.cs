using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public enum Priority
    {
        Lowest = -1000,
        Low = -500,
        Normal = 0,
        High = 500,
        Highest = 100
    }
}
