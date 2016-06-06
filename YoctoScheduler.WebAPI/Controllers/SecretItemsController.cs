using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Http;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    public class SecretItemsController : ControllerBase<Secret,string>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SecretItemsController));

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
                        Secret sec = new Secret()
                        {
                            ID = secret.Name,
                            CertificateThumbprint = secret.CertificateThumbprint,
                            PlainTextValue = secret.PlainTextValue
                        };

                        YoctoScheduler.Core.Database.Secret.Insert(conn, trans, sec);
                        trans.Commit();
                        // TODO : return a valid URI
                        return Created("", sec);
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
