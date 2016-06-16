using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [System.Runtime.Serialization.DataContract]
    public abstract class DatabaseItemWithIntPK : DatabaseItem<int>
    {
        public const int INVALID_ID = 0;


        public override bool HasValidID()
        {
            return ID != INVALID_ID;
        }

        public DatabaseItemWithIntPK() : base()
        {
            ID = INVALID_ID;
        }

        public override void InvalidateID()
        {
            this.ID = INVALID_ID;
        }
    }
}
