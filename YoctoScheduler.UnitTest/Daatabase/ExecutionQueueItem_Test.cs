using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoctoScheduler.Core.Database;
using System.Data.SqlClient;

namespace YoctoScheduler.UnitTest.Daatabase
{
    [TestClass]
    public class ExecutionQueueItem_Test
    {
        [TestMethod]
        public void ExecutionQueueItem_InsertNotNull()
        {
            ExecutionQueueItem eqi = new ExecutionQueueItem()
            {
                InsertDate = DateTime.Now,
                Priority = Core.Priority.Lowest,
                ScheduleID = 1,
                TaskID = 1
            };

            using (SqlConnection conn = new SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    ExecutionQueueItem.Insert(conn, trans, eqi);
                    trans.Commit();
                }
            }
        }

        [TestMethod]
        public void ExecutionQueueItem_InsertNull()
        {
            ExecutionQueueItem eqi = new ExecutionQueueItem()
            {
                InsertDate = DateTime.Now,
                Priority = Core.Priority.Lowest,
                TaskID = 1
            };

            using (SqlConnection conn = new SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    ExecutionQueueItem.Insert(conn, trans, eqi);
                    trans.Commit();
                }
            }
        }

    }
}
