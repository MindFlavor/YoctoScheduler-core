using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.WebAPI.Proxies
{
    [System.Runtime.Serialization.DataContract]
    public class Secret
    {
        [System.Runtime.Serialization.DataMember]
        public string ID { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string CertificateThumbprint { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string PlainTextValue { get; set; }
    }
}
