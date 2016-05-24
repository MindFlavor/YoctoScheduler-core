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
    public class Secret : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Secret));

        public byte[] EncryptedValue { get; set; }

        public string PlainTextValue
        {
            get
            {
                X509Certificate2 cert = GetCertificate();
                var alg = cert.GetKeyAlgorithm();

                cert.HasCngKey();

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

        public Secret(string Thumbprint)
        {
            this.Thumbprint = Thumbprint;
        }

        public static Secret New(SqlConnection conn, SqlTransaction trans, string thumbprint, string plainTextValue)
        {
            Secret es = new Secret(thumbprint) { PlainTextValue = plainTextValue };

            #region Database entry
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Secret.New"), conn, trans))
            {
                es.PopolateParameters(cmd);
                es.ID = (int)cmd.ExecuteScalar();
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

            if (HasValidID())
            {
                param = new SqlParameter("@SecretID", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
        }

        public override void PersistChanges(SqlConnection conn, SqlTransaction trans)
        {
            #region Database entry
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Secret.PersistChanges"), conn, trans))
            {
                PopolateParameters(cmd);
                cmd.ExecuteNonQuery();
            }
            #endregion
            log.DebugFormat("Updated Secret {0:S}", this.ToString());
        }

        public static Secret RetrieveByID(SqlConnection conn, SqlTransaction trans, int ID)
        {
            Secret secret;

            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Secret.RetrieveByID"), conn, trans))
            {
                SqlParameter param = new SqlParameter("@SecretID", System.Data.SqlDbType.Int);
                param.Value = ID;
                cmd.Parameters.Add(param);

                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    secret = ParseFromDataReader(reader);
                }

                log.DebugFormat("{0:S} - Retrieved secret ", secret.ToString());
                return secret;
            }
        }

        protected static Secret ParseFromDataReader(SqlDataReader r)
        {
            return new Secret(r.GetString(2))
            {
                ID = r.GetInt32(0),
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
    }
}
