using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Http;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    public class SecretItemsController : System.Web.Http.ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SecretItemsController));

        public IEnumerable<Secret> Get()
        {
            using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var r = Secret.GetAll(conn, trans);
                    return r;
                }
            }
        }

        public IHttpActionResult Get(string id)
        {
            Secret secret = null;
            using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    secret = Secret.GetByName(conn, trans, id);
                    trans.Commit();
                }
            }

            if (secret != null)
                return Ok(secret);
            else
                return NotFound();
        }

        public IHttpActionResult Post(Proxies.Secret secret)
        {
            if (secret == null)
                return BadRequest();

            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        var ret = YoctoScheduler.Core.Database.Secret.New(conn, trans, secret.Name, secret.CertificateThumbprint, secret.PlainTextValue);
                        trans.Commit();
                        // TODO : return a valid URI
                        return Created("", ret);
                    }
                }
            }
            catch (System.Exception exce)
            {
                log.ErrorFormat("Unhandled exception processing Secret POST: {0:S}", exce.ToString());
                return InternalServerError();
            }
        }
    }
}
