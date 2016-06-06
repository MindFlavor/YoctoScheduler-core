using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [System.Runtime.Serialization.DataContract]
    public abstract class DatabaseItemWithGUIDPK : DatabaseItem<Guid>
    {
        public DatabaseItemWithGUIDPK()
        {
            this.ID = Guid.Empty;
        }

        public override bool HasValidID()
        {
            return ID != Guid.Empty;
        }

        public override void InvalidateID()
        {
            this.ID = Guid.Empty;
        }
    }

}
