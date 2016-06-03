using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Commands
{
    public class RestartServer : Database.GenericCommand
    {
        public RestartServer(int ServerID) : base(ServerID, ServerCommand.RestartServer, null)
        {     }
    }
}
