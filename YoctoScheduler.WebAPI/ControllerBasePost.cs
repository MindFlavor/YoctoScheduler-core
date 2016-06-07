using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI
{
    public abstract class ControllerBasePost<T,K> : ControllerBase<T, K>
            where T : DatabaseItem<K>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ControllerBasePost<T, K>));
        public virtual IHttpActionResult Post(T value)
        {
            if (value == null)
                return BadRequest();

            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        var ret = value.Clone<T>(conn, trans);
                        trans.Commit();
                        // TODO : return a valid URI
                        return Created("", ret);
                    }
                }
            }
            catch (Core.Exceptions.TSQLNotFoundException)
            {
                return BadRequest(string.Format("POST not supported by {0:S}", typeof(T).FullName));
            }
            catch (Core.Exceptions.TaskNotFoundException tfe)
            {
                log.ErrorFormat("TaskNotFoundException processing {0:S} POST: {1:S}", typeof(T).Name, tfe.ToString());
                return BadRequest();
            }
            catch (System.Exception exce)
            {
                log.ErrorFormat("Unhandled exception processing {0:S} POST: {1:S}", typeof(T).Name, exce.ToString());
                return InternalServerError();
            }
        }
    }

}
