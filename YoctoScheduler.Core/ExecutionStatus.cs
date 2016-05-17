using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{

    public class ExecutionStatus
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public Task Task { get; set; }
        public int TaskID { get; set; }

        public Status Status { get; set; }

        public Server Server { get; set; }
        public int ServerID { get; set; }

        public DateTime LastUpdate { get; set; }

        public override string ToString()
        {
            return string.Format("{0:S}[ID={1:N0}, TaskID={2:N0}, Status={3:S}, ServerID={4:N0}, LastUpdate={5:S}]",
                this.GetType().FullName,
                ID, TaskID, Status, ServerID, LastUpdate);
        }
    }
}
