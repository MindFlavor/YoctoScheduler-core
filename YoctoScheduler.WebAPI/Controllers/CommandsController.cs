using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    [Attributes.GetAllSupported]
    [Attributes.GetByIDSupported]
    [Attributes.PostSupported]
    [Attributes.PutSupported]
    public class CommandsController : ControllerBase<GenericCommand, int>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CommandsController));
    }
}
