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
    public class Secret : DatabaseItemWithNVarCharPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Secret));

        [System.Runtime.Serialization.DataMember]
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
        [System.Runtime.Serialization.DataMember]
        public string CertificateThumbprint { get; set; }

        public Secret(string name, string CertificateThumbprint)
        {
            this.ID = name;
            this.CertificateThumbprint = CertificateThumbprint;
        }

        public static Secret New(SqlConnection conn, SqlTransaction trans, string name, string certificateThumbprint, string plainTextValue)
        {
            Secret es = new Secret(name, certificateThumbprint) { PlainTextValue = plainTextValue };

            #region Database entry
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Secret.New"), conn, trans))
            {
                es.PopolateParameters(cmd);
                int iRet = cmd.ExecuteNonQuery();
                if (iRet != 1)
                {
                    throw new Exception(string.Format("Failed to create a new Secret. Modified rows are {0:N0}.", iRet));
                }
            }
            #endregion
            log.DebugFormat("Created Secret {0:S}", es.ToString());
            return es;
        }

        protected internal virtual void PopolateParameters(SqlCommand cmd)
        {
            SqlParameter param = new SqlParameter("@Blob", System.Data.SqlDbType.VarBinary, -1);
            param.Value = EncryptedValue;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@Thumbprint", System.Data.SqlDbType.Char, 40);
            param.Value = CertificateThumbprint;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@SecretName", System.Data.SqlDbType.NVarChar, 255);
            param.Value = ID;
            cmd.Parameters.Add(param);
        }

        public static List<Secret> GetAll(SqlConnection conn, SqlTransaction trans)
        {
            List<Secret> lSecrets = new List<Secret>();

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Secret.GetAll"), conn, trans))
            {
                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lSecrets.Add(ParseFromDataReader(reader));
                }

                return lSecrets;
            }
        }

        public static Secret GetByName(SqlConnection conn, SqlTransaction trans, string SecretName)
        {
            Secret secret;

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Secret.RetrieveByID"), conn, trans))
            {
                SqlParameter param = new SqlParameter("@SecretName", System.Data.SqlDbType.NVarChar, 255);
                param.Value = SecretName;
                cmd.Parameters.Add(param);

                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exceptions.SecretNotFoundException(SecretName);
                    secret = ParseFromDataReader(reader);
                }

                log.DebugFormat("{0:S} - Retrieved secret ", secret.ToString());
                return secret;
            }
        }

        protected static Secret ParseFromDataReader(SqlDataReader r)
        {
            return new Secret(r.GetString(0), r.GetString(2))
            {
                EncryptedValue = (byte[])r.GetValue(1)
            };
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

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            throw new NotImplementedException();
        }
    }
}
