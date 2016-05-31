using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    public class Configuration
    {
        protected Dictionary<string, string> _config = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                return _config[key];
            }
        }

        public Configuration(SqlConnection conn)
        {
            using (SqlCommand cmd = new SqlCommand(tsql.Extractor.Get("Configuration.GetAll"), conn))
            {
                cmd.Prepare();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _config[reader.GetString(0)] = reader.GetString(1);
                    }
                }
            }
        }
    }
}
