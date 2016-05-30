using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public class Secret : DatabaseItemWithNVarCharPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Secret));

        public byte[] EncryptedValue { get; set; }

        public string PlainTextValue
        {
            get
            {
                X509Certificate2 cert = GetCertificate();
                var alg = cert.GetKeyAlgorithm();

                RSACryptoServiceProvider rsa = cert.PrivateKey as RSACryptoServiceProvider;
                var bBuf = rsa.Decrypt(EncryptedValue, true);
                return System.Text.Encoding.Unicode.GetString(bBuf);
            }
            set
            {
                var cert = GetCertificate();

                RSACryptoServiceProvider rsa = cert.PublicKey.Key as RSACryptoServiceProvider;
                var bBuf = rsa.Encrypt(System.Text.Encoding.Unicode.GetBytes(value), true);
                EncryptedValue = bBuf;
            }
        }
        public string Thumbprint { get; set; }

        public Secret(string name, string Thumbprint)
        {
            this.ID = name;
            this.Thumbprint = Thumbprint;
        }

        public static Secret New(SqlConnection conn, SqlTransaction trans, string name, string thumbprint, string plainTextValue)
        {
            Secret es = new Secret(name, thumbprint) { PlainTextValue = plainTextValue };

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
            param.Value = Thumbprint;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@SecretName", System.Data.SqlDbType.NVarChar, 255);
            param.Value = ID;
            cmd.Parameters.Add(param);
        }

        public static Secret RetrieveByID(SqlConnection conn, SqlTransaction trans, string SecretName)
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
                if (cert.Thumbprint.Equals(Thumbprint, StringComparison.InvariantCultureIgnoreCase))
                    return cert;

            return null;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, Thumbprint={2:S}, EncryptedValue={3:S}]",
                this.GetType().FullName,
                base.ToString(), Thumbprint, EncryptedValue);
        }

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            throw new NotImplementedException();
        }
    }
}
