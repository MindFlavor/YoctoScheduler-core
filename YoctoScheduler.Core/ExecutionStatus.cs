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


        public Server Server { get; set; }
        public Guid ServerGuid { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
