using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoctoScheduler.Core.Database;
using System.Data.SqlClient;

namespace YoctoScheduler.UnitTest.Daatabase
{
    [TestClass]
    public class LiveExecutionStatus_Test
    {
        [TestMethod]
        public void LiveExecutionStatus_Insert_NotNull()
        {
            LiveExecutionStatus les = new LiveExecutionStatus()
            {
                Inserted = DateTime.Now,
                LastUpdate = DateTime.Now,
                ScheduleID = 1,
                ServerID = 1,
                TaskID = 1
            };

            using (SqlConnection conn = new SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    LiveExecutionStatus.Insert(conn,trans,les);
                    trans.Commit();
                }
            }

        }

        [TestMethod]
        public void LiveExecutionStatus_Insert_Null()
        {
            LiveExecutionStatus les = new LiveExecutionStatus()
            {
                Inserted = DateTime.Now,
                LastUpdate = DateTime.Now,
                ScheduleID = null,
                ServerID = 1,
                TaskID = 1
            };

            using (SqlConnection conn = new SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    LiveExecutionStatus.Insert(conn, trans, les);
                    trans.Commit();
                }
            }

        }
    }
}
