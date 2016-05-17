using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public class Task
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskID { get; set; }

        public ICollection<ExecutionStatus> ExecutionStatuses { get; set; }
    }
}
