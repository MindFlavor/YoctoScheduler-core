using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Commands
{
    public class RestartServer : GenericCommand
    {
        public RestartServer(int ServerID, string Payload) : base(ServerID, Command.RestartServer, Payload)
        {     }
    }
}
