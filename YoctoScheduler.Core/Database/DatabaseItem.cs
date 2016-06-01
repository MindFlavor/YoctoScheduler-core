using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{

    [System.Runtime.Serialization.DataContract]
    public abstract class DatabaseItem<K>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DatabaseItem<K>));

        public const string LOG_TIME_FORMAT = "yyyyMMdd HH:mm:ss";

        [System.Runtime.Serialization.DataMember]
        public K ID { get; set; }

        public DatabaseItem()
        {  }

        protected static SqlConnection OpenConnection(string ConnectionString)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();

            return conn;
        }

        public abstract void PersistChanges(SqlConnection conn, SqlTransaction trans);

        public abstract void PopolateParameters(SqlCommand cmd);

        public abstract void ParseFromDataReader(SqlDataReader r);

        public abstract bool HasValidID();

        public static void Insert<T>(SqlConnection conn, SqlTransaction trans, T t)
            where T : DatabaseItem<K>
        {
            #region Database entry
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get(typeof(T).Name + ".New"), conn, trans))
            {
                t.PopolateParameters(cmd);
                t.ID = (K)cmd.ExecuteScalar();                
            }
            #endregion
            log.DebugFormat("Created {0:S} {1:S}", t.GetType().Name, t.ToString());
        }

        public static void Delete<T>(SqlConnection conn, SqlTransaction trans, T t)
            where T : DatabaseItem<K>
        {
            #region Database entry
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get(typeof(T).Name + ".Delete"), conn, trans))
            {
                t.PopolateParameters(cmd);
                if (cmd.ExecuteNonQuery() == 1)
                    log.DebugFormat("Deleted {0:S}: {1:S}", t.GetType().Name, t.ToString());
                else
                    throw new Exceptions.ConcurrencyException(string.Format("Failed to delete {0:S} with ID={1:S}. The key was not present in the database", typeof(T).Name, t.ToString()));
            }
            #endregion
        }

        public static List<T> GetAll<T>(SqlConnection conn, SqlTransaction trans)
            where T : DatabaseItem<K>
        {
            List<T> lItems = new List<T>();

            string stmt = tsql.Extractor.Get(typeof(T).Name + ".GetAll");

            using (SqlCommand cmd = new SqlCommand(stmt, conn, trans))
            {
                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T t = Activator.CreateInstance<T>();
                        t.ParseFromDataReader(reader);
                        lItems.Add(t);
                    }
                }
            }

            return lItems;
        }

        public override string ToString()
        {
            return string.Format("ID={0:N0}", ID.ToString());
        }
    }
}
