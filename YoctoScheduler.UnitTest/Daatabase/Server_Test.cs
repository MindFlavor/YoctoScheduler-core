using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoctoScheduler.Core;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.UnitTest.Daatabase
{
    [TestClass]
    public class Server_Test
    {
        [TestMethod]
        [ExpectedException(typeof(YoctoScheduler.Core.Exceptions.SecretNotFoundException))]
        public void Server_TranslateSecretMustThrowExceptionOnNotFound()
        {
            Server server;
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();

                #region setup Configuration
                Server.Configuration = new Configuration(conn);
                #endregion

                using (var trans = conn.BeginTransaction())
                {
                    server = YoctoScheduler.Core.Server.New(
                        conn, trans,
                        Config.CONNECTION_STRING,
                        "Test server" + DateTime.Now.ToString());

                    trans.Commit();
                }

            }

            server.TranslateSecret("NotFound!!!");
        }
    }
}
