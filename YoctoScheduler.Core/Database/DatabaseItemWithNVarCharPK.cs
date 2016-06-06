using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [System.Runtime.Serialization.DataContract]
    public abstract class DatabaseItemWithNVarCharPK : DatabaseItem<string>
    {
        public const string INVALID_ID = null;
        
        public override bool HasValidID()
        {
            return ID != INVALID_ID;
        }

        public DatabaseItemWithNVarCharPK() : base()
        {
            ID = INVALID_ID;
        }

        public override void InvalidateID()
        {
            this.ID = INVALID_ID;
        }
    }
}
