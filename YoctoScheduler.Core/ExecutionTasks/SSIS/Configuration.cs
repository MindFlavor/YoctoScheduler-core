using System;

namespace YoctoScheduler.Core.ExecutionTasks.SSIS
{
    public class Configuration
    {
        public Configuration()
        {
            Use32Bit = false;
            SQLVersion = 120;
            Timeout = 0;
        }

        public bool Use32Bit { get; set; }

        private int sqlVersion;
        public int SQLVersion {
            get
            {
                return sqlVersion;
            }
            set
            {
                if (value == 120)
                {
                    sqlVersion = value;
                }
                else
                {
                    throw new ArgumentException("Unsupported SQL Server version.");
                }
            }
        }

        public string Arguments { get; set; }

        public int Timeout { get; set; }
    }
}
