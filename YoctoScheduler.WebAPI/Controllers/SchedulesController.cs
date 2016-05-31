using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    public class SchedulesController : System.Web.Http.ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SchedulesController));
        public IEnumerable<Schedule> Get()
        {
            using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var r = Schedule.GetAll(conn, trans, true);
                    trans.Commit();
                    return r;
                }
            }
        }

        public IHttpActionResult Post(Schedule value)
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
                        var ret = value.Clone(conn, trans);
                        trans.Commit();
                        // TODO : return a valid URI
                        return Created("", ret);
                    }
                }
            }
            catch (YoctoScheduler.Core.Exceptions.TaskNotFoundException tfe)
            {
                log.ErrorFormat("Error processing Schedule POST: {0:S}", tfe.ToString());
                return BadRequest();
            }
            catch (Exception exce)
            {
                log.ErrorFormat("Error processing Schedule POST: {0:S}", exce.ToString());
                return InternalServerError();
            }
        }
    }
}
