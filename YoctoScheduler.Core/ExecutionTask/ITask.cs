using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.ExecutionTask
{
    public interface ITask
    {
        void Start();
        void Abort();
        bool IsAlive();
        string Do();
    }
}
