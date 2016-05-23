using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Exceptions
{
    public class TSQLNotFoundException : Exception
    {
        public string Name { get; protected set; }
        public TSQLNotFoundException(string Name)
        {
            this.Name = Name;
        }

        public override string ToString()
        {
            return string.Format("TSQLNotFoundException: the TSQL named \"{0:S}\" was not found in the DLL resources", Name);
        }
    }
}
