using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public class Server
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Guid { get; set; }
    
        public Status Status { get; set; }

        public string Description { get; set; }

        public ICollection<ExecutionStatus> ExecutionStatuses { get; set; }

        public DateTime LastPing { get; set; }
    }
}
