using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.Core.Commands
{
    public abstract class CommandBase<T>
    {
        public T Configuration { get; private set; }

        public GenericCommand Command { get; private set; }

        public CommandBase(GenericCommand Command)
        {
            this.Command = Command;
            Configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Command.Payload);
        }
    }
}
