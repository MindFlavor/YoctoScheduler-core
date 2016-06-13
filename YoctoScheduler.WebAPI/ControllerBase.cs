using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public abstract class ControllerBase<T, K> : System.Web.Http.ApiController
        where T : DatabaseItem<K>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ControllerBase<T, K>));

        public virtual IHttpActionResult Get()
        {
            log.DebugFormat("Get::{0:S} requested", typeof(T).Name);

            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        var r = DatabaseItem<K>.GetAll<T>(conn, trans);
                        trans.Commit();
                        log.DebugFormat("{0:S} Get returning {1:N0} items.", typeof(T).FullName, r.Count);
                        return Ok(r);
                    }
                }
            }
            catch (Core.Exceptions.TSQLNotFoundException)
            {
                return BadRequest(string.Format("Get not supported by {0:S}", typeof(T).FullName));
            }
        }

        public virtual IHttpActionResult Get(K id)
        {
            try
            {
                T t = null;
                using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        t = DatabaseItem<K>.GetByID<T>(conn, trans, id);
                        trans.Commit();
                    }
                }

                if (t != null)
                    return Ok(t);
                else
                    return NotFound();

            }
            catch (Core.Exceptions.TSQLNotFoundException)
            {
                return BadRequest(string.Format("GetByID not supported by {0:S}", typeof(T).FullName));
            }
        }
    }
}