using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Exceptions
{
    public class SecretNotFoundException : Exception
    {
        public string SecretName { get; protected set; }
        public SecretNotFoundException(string SecretName)
        {
            this.SecretName = SecretName;
        }

        public override string ToString()
        {
            return string.Format("SecretNotFoundException: Secret \"{0:S}\" was not found on the database", SecretName);
        }
    }
}
