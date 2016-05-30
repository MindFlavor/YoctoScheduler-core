using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Commands
{
    public class KillExecutionTask : GenericCommand
    {
        public Guid LiveExecutionStatusGUID { get; set; }

        public KillExecutionTask(int ServerID, string Payload) : base(ServerID, Command.KillTask, Payload)
        {
            try
            {
                this.LiveExecutionStatusGUID = Guid.Parse(Payload);
            }
            catch (Exception e)
            {
                throw new FormatException("Cannot parse KillTask from Payload. It must be a GUID", e);
            }
        }
    }
}
