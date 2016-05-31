using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [System.Runtime.Serialization.DataContract]
    public abstract class DatabaseItemWithIntPK : DatabaseItem
    {
        public const int INVALID_ID = -1;

        [System.Runtime.Serialization.DataMember]
        public int ID { get; set; }

        public bool HasValidID()
        {
            return ID != INVALID_ID;
        }

        public DatabaseItemWithIntPK() : base()
        {
            ID = INVALID_ID;
        }

        public override string ToString()
        {
            return string.Format("ID={0:N0}", ID);
        }
    }
}
