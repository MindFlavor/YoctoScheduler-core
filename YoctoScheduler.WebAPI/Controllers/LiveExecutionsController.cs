﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    [Attributes.GetAllSupported]
    [Attributes.GetByIDSupported]
    public class LiveExecutionsController : ControllerBase<LiveExecutionStatus, Guid>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LiveExecutionsController));
    }
}
