using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    public class TasksController : ControllerBasePost<Task,int>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TasksController));        
    }
}
