using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [System.Runtime.Serialization.DataContract]
    [DatabaseKey(DatabaseName = "SecretName", Size = 255)]
    public class Secret : DatabaseItemWithNVarCharPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Secret));

        [System.Runtime.Serialization.DataMember]
        [DatabaseProperty(DatabaseName = "Blob", Size = -1)]
        public byte[] EncryptedValue { get; set; }

        public string PlainTextValue
        {
            get
            {
                X509Certificate2 cert = GetCertificate();
                if (cert == null)
                    throw new Exceptions.CertificateNotFoundException(CertificateThumbprint);

                var alg = cert.GetKeyAlgorithm();

                RSACryptoServiceProvider rsa = cert.PrivateKey as RSACryptoServiceProvider;
                var bBuf = rsa.Decrypt(EncryptedValue, true);
                return System.Text.Encoding.UTF8.GetString(bBuf);
            }
            set
            {
                var cert = GetCertificate();
                if (cert == null)
                    throw new Exceptions.CertificateNotFoundException(CertificateThumbprint);

                RSACryptoServiceProvider rsa = cert.PublicKey.Key as RSACryptoServiceProvider;

                var bytes = System.Text.Encoding.UTF8.GetBytes(value);
                var bBuf = rsa.Encrypt(bytes, true);
                EncryptedValue = bBuf;
            }
        }

        [DatabaseProperty(DatabaseName = "Thumbprint", Size = 40)]
        [System.Runtime.Serialization.DataMember]
        public string CertificateThumbprint { get; set; }

        public Secret()
        { }

        public Secret(string ID, string CertificateThumbprint)
        {
            this.ID = ID;
            this.CertificateThumbprint = CertificateThumbprint;
        }

        public override void ParseFromDataReader(SqlDataReader r)
        {
            ID = r.GetString(0);
            CertificateThumbprint = r.GetString(2);
            EncryptedValue = (byte[])r.GetValue(1);
        }

        protected X509Certificate2 GetCertificate()
        {
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            foreach (var cert in store.Certificates)
                if (cert.Thumbprint.Equals(CertificateThumbprint, StringComparison.InvariantCultureIgnoreCase))
                    return cert;

            return null;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, Thumbprint={2:S}, EncryptedValue={3:S}]",
                this.GetType().FullName,
                base.ToString(), CertificateThumbprint, EncryptedValue);
        }
    }
}
