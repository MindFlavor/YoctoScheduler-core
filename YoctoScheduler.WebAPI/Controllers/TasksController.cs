using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using YoctoScheduler.Core.Database;
using YoctoScheduler.Logging.Extensions;

namespace YoctoScheduler.WebAPI.Controllers
{
    [Attributes.GetAllSupported]
    [Attributes.GetByIDSupported]
    [Attributes.PostSupported]
    [Attributes.PutSupported]
    [Attributes.DeleteSupported]
    public class TasksController : ControllerBase<Task,int>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TasksController));

        public virtual IHttpActionResult Get(string id, string type)
        {
            log.TraceFormat("Get::{0:S}({1:S}, {2:S}) requested", this.GetType().Name, id.ToString(), type.ToString());

            return this.GetSecondary(id, 255);
        }
    }
}
