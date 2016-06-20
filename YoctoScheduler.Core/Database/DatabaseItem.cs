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
        { }

        public abstract void InvalidateID();

        public virtual T Clone<T>(SqlConnection conn, SqlTransaction trans) where T : DatabaseItem<K>
        {
            T clone = (T)this.MemberwiseClone();
            clone.InvalidateID();
            Insert<T>(conn, trans, clone);

            return (T)clone;
        }

        protected static SqlConnection OpenConnection(string ConnectionString)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();

            return conn;
        }

        public virtual void PopolateParameters(SqlCommand cmd)
        {
            // reflect and get all the DatabaseField properties
            var properties = this.GetType().GetProperties().Where(prop => prop.GetCustomAttributes(typeof(DatabaseProperty), true).Count() > 0);


            foreach (var prop in properties)
            {
                var att = (DatabaseProperty)prop.GetCustomAttributes(typeof(DatabaseProperty), true).First();

                string name = "@";
                if (!string.IsNullOrEmpty(att.DatabaseName))
                    name += att.DatabaseName;
                else
                    name += prop.Name;

                SqlParameter param = new SqlParameter(name, SqlDbTypeFromType(prop.PropertyType), att.Size);


                if (prop.GetValue(this) == null)
                    param.Value = DBNull.Value;
                else
                    param.Value = prop.GetValue(this);
                cmd.Parameters.Add(param);
            }

            // now get the key, if needed
            if (HasValidID())
            {
                var dbkey = (DatabaseKey)this.GetType().GetCustomAttributes(typeof(DatabaseKey), true).FirstOrDefault();
                if (dbkey == null)
                    // TODO: Throw a better exception
                    throw new Exception(string.Format("The class must have a DatabaseKey attribute!"));

                SqlParameter param = new SqlParameter(dbkey.DatabaseName, SqlDbTypeFromType(ID.GetType()), dbkey.Size);
                param.Value = ID;
                cmd.Parameters.Add(param);
            }
        }

        private static System.Data.SqlDbType SqlDbTypeFromType(Type t)
        {
            if (t == typeof(string))
                return System.Data.SqlDbType.NVarChar;
            else if (t == typeof(byte[]))
                return System.Data.SqlDbType.VarBinary;
            else if (t == typeof(int))
                return System.Data.SqlDbType.Int;
            else if (t == typeof(bool))
                return System.Data.SqlDbType.Bit;
            else if (t == typeof(DateTime))
                return System.Data.SqlDbType.DateTime;
            else if (t == typeof(Guid))
                return System.Data.SqlDbType.UniqueIdentifier;
            else if (t == typeof(Nullable<int>))
                return System.Data.SqlDbType.Int;
            else if (t.IsEnum)
                return System.Data.SqlDbType.Int;

            else
                // TODO: Create a better exception
                throw new Exception(string.Format("Unsupported type: {0:S}", t.ToString()));
        }

        public abstract void ParseFromDataReader(SqlDataReader r);

        public abstract bool HasValidID();

        public virtual void Validate()
        {
            // nothing to do, classes should override this in order
            // to intercect invalid configurations
        }

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
            log.DebugFormat("Inserted {0:S} {1:S}", t.GetType().Name, t.ToString());
        }

        public static void Update<T>(SqlConnection conn, SqlTransaction trans, T t)
            where T : DatabaseItem<K>
        {
            #region Database entry
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get(typeof(T).Name + ".Update"), conn, trans))
            {
                t.PopolateParameters(cmd);
                if (cmd.ExecuteNonQuery() == 1)
                    log.DebugFormat("Updated {0:S}: {1:S}", t.GetType().Name, t.ToString());
                else
                    throw new Exceptions.ConcurrencyException(string.Format("Failed to update {0:S} with ID={1:S}. The key was not present in the database", typeof(T).Name, t.ToString()));
            }
            #endregion
            log.DebugFormat("Updated {0:S} {1:S}", t.GetType().Name, t.ToString());
        }


        public static bool Delete<T>(SqlConnection conn, SqlTransaction trans, T t)
            where T : DatabaseItem<K>
        {
            #region Database entry
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get(typeof(T).Name + ".DeleteByID"), conn, trans))
            {
                t.PopolateParameters(cmd);
                if (cmd.ExecuteNonQuery() == 1)
                {
                    log.DebugFormat("Deleted {0:S}: {1:S}", t.GetType().Name, t.ToString());
                    return true;
                }
                else
                {
                    log.WarnFormat("Failed to delete {0:S} with ID={1:S}. The key was not present in the database", typeof(T).Name, t.ToString());
                    return false;
                }
            }
            #endregion
        }

        public static T GetByID<T>(SqlConnection conn, SqlTransaction trans, K id)
            where T : DatabaseItem<K>
        {
            string stmt = tsql.Extractor.Get(typeof(T).Name + ".GetByID");

            T t = Activator.CreateInstance<T>();
            t.ID = id;

            using (SqlCommand cmd = new SqlCommand(stmt, conn, trans))
            {
                t.PopolateParameters(cmd);
                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    else
                    {
                        t.ParseFromDataReader(reader);
                        return t;
                    }
                }
            }
        }

        public static T GetBySecondary<T, SECONDARY>(SqlConnection conn, SqlTransaction trans, SECONDARY id, int size)
            where T : DatabaseItem<K>
        {
            string stmt = tsql.Extractor.Get(typeof(T).Name + ".GetBySecondary");

            T t = Activator.CreateInstance<T>();            

            using (SqlCommand cmd = new SqlCommand(stmt, conn, trans))
            {
                SqlParameter param = new SqlParameter("@Secondary", SqlDbTypeFromType(typeof(SECONDARY)), size);
                param.Value = id;
                cmd.Parameters.Add(param);

                cmd.Prepare();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    else
                    {
                        t.ParseFromDataReader(reader);
                        return t;
                    }
                }
            }
        }
    
        protected static List<T> GetAll<T>(SqlConnection conn, SqlTransaction trans, string customScript)
            where T : DatabaseItem<K>
        {
            List<T> lItems = new List<T>();

            string stmt = tsql.Extractor.Get(typeof(T).Name + "." + customScript);

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

        public static List<T> GetAll<T>(SqlConnection conn, SqlTransaction trans, LockType lockType)
            where T : DatabaseItem<K>
        {
            switch (lockType)
            {
                case LockType.Default: return GetAll<T>(conn, trans, "GetAll");
                case LockType.NoLock: return GetAll<T>(conn, trans, "GetAllNoLock");
                case LockType.TableLock: return GetAll<T>(conn, trans, "GetAllTableLock");
                case LockType.TableLockX: return GetAll<T>(conn, trans, "GetAllTableLockX");
                case LockType.XLock: return GetAll<T>(conn, trans, "GetAllXLock");
            }

            throw new Exception("this should not happen... did we add all LockTypes?");
        }

        public static List<T> GetAll<T>(SqlConnection conn, SqlTransaction trans)
            where T : DatabaseItem<K>
        {
            return GetAll<T>(conn, trans, LockType.Default);
        }

        public override string ToString()
        {
            return string.Format("ID={0:N0}", ID.ToString());
        }
    }
}
