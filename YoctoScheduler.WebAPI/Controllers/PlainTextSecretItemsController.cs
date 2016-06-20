using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Http;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    [Attributes.PostSupported]
    [Attributes.PutSupported]
    public class PlainTextSecretItemsController : System.Web.Http.ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PlainTextSecretItemsController));


        public IHttpActionResult Post(Proxies.Secret secret)
        {
            if (Attribute.GetCustomAttributes(this.GetType(), typeof(Attributes.PostSupportedAttribute), false) == null)
                return ResponseMessage(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotImplemented));

            if (secret == null)
                return BadRequest();

            try
            {
                Secret secretExisting = null;
                Secret secNew = null;

                using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        secretExisting = Secret.GetByID<Secret>(conn, trans, secret.Name);
                        if (secretExisting != null)
                        {
                            Secret.Delete<Secret>(conn, trans, secretExisting);
                        }

                        #region New item
                        secNew = new Secret()
                        {
                            ID = secret.Name,
                            CertificateThumbprint = secret.CertificateThumbprint,
                            PlainTextValue = secret.PlainTextValue
                        };
                        Secret.Insert(conn, trans, secNew);
                        #endregion

                        trans.Commit();
                    }
                }

                if (secretExisting == null)
                    return Created("", secNew);
                else
                    return Ok();
            }
            catch (System.Exception exce)
            {
                log.ErrorFormat("Unhandled exception processing Secret POST: {0:S}", exce.ToString());
                return InternalServerError(exce);
            }
        }
    }
}
