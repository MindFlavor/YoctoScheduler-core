using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.UnitTest.Daatabase
{
    [TestClass]
    public class Task_Test
    {
        [TestMethod]
        public void Task_Insert_PassthroughTask()
        {
            YoctoScheduler.Core.ExecutionTasks.Passthrough.PassthroughTask pt = new Core.ExecutionTasks.Passthrough.PassthroughTask();
            pt.Configuration = "Some data";

            Task t = pt.GenerateDatabaseTask();
            t.Name = "Unit test Passthrough";
            t.Description = t.Name + " description";
            t.ReenqueueOnDead = true;

            using (SqlConnection conn = new SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    Task.Insert(conn, trans, t);
                    trans.Commit();
                }
            }
        }

        [TestMethod]
        public void Task_Insert_WaitTask()
        {
            YoctoScheduler.Core.ExecutionTasks.WaitTask.WaitTask wt = new Core.ExecutionTasks.WaitTask.WaitTask();
            wt.Configuration = new Core.ExecutionTasks.WaitTask.Configuration() { SleepSeconds = 200 };

            Task t = wt.GenerateDatabaseTask();
            t.Name = "Unit test Mock";
            t.Description = t.Name + " description";
            t.ReenqueueOnDead = true;

            using (SqlConnection conn = new SqlConnection(Config.CONNECTION_STRING))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    Task.Insert(conn, trans, t);
                    trans.Commit();
                }
            }
        }

    }
}
