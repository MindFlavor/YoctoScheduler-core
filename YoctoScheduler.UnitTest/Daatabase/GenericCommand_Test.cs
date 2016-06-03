using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoctoScheduler.Core.Database;
using System.Data.SqlClient;

namespace YoctoScheduler.UnitTest.Daatabase
{
    [TestClass]
    public class GenericCommand_Test
    {
        [TestMethod]
        public void GenericCommand_Insert()
        {
            GenericCommand cmd = new YoctoScheduler.Core.Commands.RestartServer(1)
            { };

            using (SqlConnection conn = new SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    GenericCommand.Insert(conn, trans, cmd);
                    trans.Commit();
                }
            }
        }
    }
}
