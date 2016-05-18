using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public abstract class DatabaseItemWithIntPK : DatabaseItem
    {
        public const int INVALID_ID = -1;
    }
}
