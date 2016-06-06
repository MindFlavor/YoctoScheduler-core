using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;
using YoctoScheduler.Core.Database;

namespace YoctoScheduler.WebAPI.Controllers
{
    public class SchedulesController : ControllerBasePost<Schedule,int>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SchedulesController));
    }
}
