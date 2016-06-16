using System;
using System.Collections.Generic;
using YoctoScheduler.Core.Database;
using YoctoScheduler.Core;
using System.Data.SqlClient;

namespace YoctoScheduler.WebAPI.Controllers
{
    [Attributes.GetAllSupported]
    [Attributes.GetByIDSupported]
    [Attributes.DeleteSupported]
    public class ServersController : ControllerBase<Server, int>
    {

    }
}
