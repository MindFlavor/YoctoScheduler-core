using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core
{
    public abstract class DatabaseItemWithGUIDPK : DatabaseItem
    {

        public Guid GUID { get; set; }

        public bool HasValidID()
        {
            return GUID != Guid.Empty;
        }

        public DatabaseItemWithGUIDPK() : base()
        {
            GUID = Guid.Empty;
        }

        public override string ToString()
        {
            return string.Format("ID={0:N0}", GUID.ToString());
        }
    }
}
