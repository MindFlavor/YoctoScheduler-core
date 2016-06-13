using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    public enum LockType
    {
        Default, NoLock, XLock, TableLock, TableLockX
    };
}
