using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    public class DeadExecutionsController : ControllerBase<DeadExecutionStatus, Guid>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DeadExecutionsController));
    }
}
