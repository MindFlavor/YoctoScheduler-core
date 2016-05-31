using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    public abstract class DatabaseItemWithNVarCharPK : DatabaseItem
    {
        public const string INVALID_ID = null;

        public string ID { get; set; }

        public bool HasValidID()
        {
            return ID != INVALID_ID;
        }

        public DatabaseItemWithNVarCharPK() : base()
        {
            ID = INVALID_ID;
        }

        public override string ToString()
        {
            return string.Format("ID={0:S}", ID);
        }
    }
}
