using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    public class QueueItemsController : ControllerBasePost<ExecutionQueueItem, Guid>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(QueueItemsController));
    }
}
