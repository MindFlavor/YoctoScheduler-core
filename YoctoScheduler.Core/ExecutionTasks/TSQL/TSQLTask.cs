using System.Data.SqlClient;

namespace YoctoScheduler.Core.ExecutionTasks.TSQL
{
    public class TSQLTask : JsonBasedTask<Configuration>
    {
        public override string Do()
        {
            using (SqlConnection conn = new SqlConnection(Configuration.ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(Configuration.Statement, conn))
                {
                    cmd.CommandTimeout = Configuration.CommandTimeout;

                    object o = cmd.ExecuteScalar();
                    if (o != null)
                        return o.ToString();
                    else
                        return null;
                }
            }
        }
    }
}