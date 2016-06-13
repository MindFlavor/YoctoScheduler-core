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
            GenericCommand gc = new GenericCommand(1, Core.ServerCommand.RestartServer, "200");

            using (SqlConnection conn = new SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    GenericCommand.Insert(conn, trans, gc);
                    trans.Commit();
                }
            }
        }
    }
}
