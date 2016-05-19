using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public class ExecutionQueueItem : DatabaseItemWithIntPK
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ExecutionQueueItem));



        public int TaskID { get; set; }
        public Priority Priority { get; set; }
        public DateTime InsertDate { get; set; }

        public ExecutionQueueItem(int TaskID) : base()
        {
            this.TaskID = TaskID;
            this.Priority = Priority.Normal;
        }

        public override string ToString()
        {
            return string.Format("{0:S}[{1:S}, TaskID={2:S}, Priority={3:S}, InsertDate={4:S}]",
                this.GetType().FullName,
                base.ToString(),
                TaskID.ToString(),
                Priority.ToString(),
                InsertDate.ToString(LOG_TIME_FORMAT));
        }

        public override void PersistChanges(SqlConnection conn)
        {
            throw new NotImplementedException();
        }
    }
}
