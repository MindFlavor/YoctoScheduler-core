using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Exceptions
{
    public class CertificateNotFoundException : Exception
    {
        public string CertificateThumbprint { get; set; }

        public CertificateNotFoundException(string CertificateThumbprint)
        {
            this.CertificateThumbprint = CertificateThumbprint;
        }

        public override string ToString()
        {
            return string.Format("CertificateNotFoundException: certificate with thumbprint {0:S} was not found in the user My store", CertificateThumbprint);
        }
    }
}
