using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Exceptions
{
    public class ServerNotFoundException : ElementNotFoundException
    {
        public int ServerID { get; set; }

        public ServerNotFoundException(int ServerID)
        {
            this.ServerID = ServerID;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[ServerID={1:N0}]", this.GetType().FullName, ServerID);
        }
    }
}
