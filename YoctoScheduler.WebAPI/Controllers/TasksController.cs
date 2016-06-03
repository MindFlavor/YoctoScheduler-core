using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    public class TasksController : System.Web.Http.ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TasksController));

        public IEnumerable<Task> Get()
        {
            using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var r = Task.GetAll<Task>(conn, trans);
                    trans.Commit();
                    return r;
                }
            }
        }

        public IHttpActionResult Get(int id)
        {
            Task task = null;
            using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    task = Task.GetByID<Task>(conn, trans, id);
                    trans.Commit();
                }
            }

            if (task != null)
                return Ok(task);
            else
                return NotFound();
        }

        public IHttpActionResult Post(Task value)
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
            catch (System.Exception exce)
            {
                log.ErrorFormat("Unhandled exception processing Task POST: {0:S}", exce.ToString());
                return InternalServerError();
            }
        }
    }
}
