using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public enum Status
    {
        Unknown = 0,
        Idle = 100,
        Starting = 200,
        Running = 300,
        CompletedSuccessfully = 1000,
        CompletedWithFailure = -2000,
        CompletedWithException = -3000,
        Dead = -100
    }
}
