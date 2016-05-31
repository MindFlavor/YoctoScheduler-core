using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    public abstract class DatabaseItem
    {
        public const string LOG_TIME_FORMAT = "yyyyMMdd HH:mm:ss";

        public DatabaseItem()
        {  }

        protected static SqlConnection OpenConnection(string ConnectionString)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();

            return conn;
        }

        public abstract void PersistChanges(SqlConnection conn, SqlTransaction trans);
    }
}
