using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public class Schedule
    {
        protected string _cron;

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScheduleID { get; set; }

        public string Cron {
            get {
                return _cron;
            }
            set
            {
                NCrontab.CrontabSchedule.Parse(value);
                _cron = value;            
            }
        }

        public bool Enabled { get; set; }

        public int TaskID { get; set; }
        public virtual Task Task { get; set; }

        public override string ToString()
        {
            return string.Format("{0:S}[ScheduleID={1:N0}, Cron={2:S}, Enabled={3:S}, TaskID={4:N0}]", this.GetType().FullName, ScheduleID, Cron, Enabled.ToString(), TaskID);
        }
    }
}
