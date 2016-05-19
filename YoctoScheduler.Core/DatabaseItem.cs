using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public abstract class DatabaseItem
    {
        public const string LOG_TIME_FORMAT = "yyyyMMdd hh:mm:ss";

        public string ConnectionString { get; private set; }

        public DatabaseItem(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        protected static SqlConnection OpenConnection(string ConnectionString)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();

            return conn;
        }

        protected virtual SqlConnection OpenConnection()
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();

            return conn;
        }

        public abstract void PersistChanges();
    }
}
