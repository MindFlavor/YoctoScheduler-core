using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Commands.KillExecution
{
    public class KillExecution : CommandBase<Configuration>
    {
        public KillExecution(Database.GenericCommand Command)
            : base(Command) { }
    }
}
