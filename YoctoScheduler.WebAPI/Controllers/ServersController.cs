using System;
using System.Collections.Generic;
using YoctoScheduler.Core.Database;
using YoctoScheduler.Core;
using System.Data.SqlClient;

namespace YoctoScheduler.WebAPI.Controllers
{
    public class ServersController : System.Web.Http.ApiController
    { 
        public IEnumerable<Server> Get()
        {
            using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var r = Server.GetAll<Server>(conn, trans);
                    return r;
                }
            }
        }
    }
}
