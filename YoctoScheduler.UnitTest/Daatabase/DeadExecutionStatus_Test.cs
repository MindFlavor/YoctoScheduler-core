using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoctoScheduler.Core.Database;
using System.Data.SqlClient;

namespace YoctoScheduler.UnitTest.Daatabase
{
    [TestClass]
    public class DeadExecutionStatus_Test
    {
        [TestMethod]
        public void DeadExecutionStatus_InsertNotNull()
        {
            DeadExecutionStatus des = new DeadExecutionStatus()
            {
                ID = Guid.NewGuid(),
                Inserted = DateTime.Now,
                LastUpdate = DateTime.Now,
                ScheduleID = 1,
                ServerID = 1,
                TaskID = 1,
                ReturnCode = "UnitTest",
                Status = Core.TaskStatus.Unknown
            };

            using (SqlConnection conn = new SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    DeadExecutionStatus.Insert(conn, trans, des);
                    trans.Commit();
                }
            }
        }

        [TestMethod]
        public void DeadExecutionStatus_InsertNull()
        {
            DeadExecutionStatus des = new DeadExecutionStatus()
            {
                ID = Guid.NewGuid(),
                Inserted = DateTime.Now,
                LastUpdate = DateTime.Now,
                ScheduleID = null,
                ServerID = 1,
                TaskID = 1,
                ReturnCode = "UnitTest",
                Status = Core.TaskStatus.Unknown
            };

            using (SqlConnection conn = new SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    DeadExecutionStatus.Insert(conn, trans, des);
                    trans.Commit();
                }
            }
        }
    }
}
