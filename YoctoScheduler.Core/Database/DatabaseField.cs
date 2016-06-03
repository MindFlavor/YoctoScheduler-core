using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Database
{
    [AttributeUsage(System.AttributeTargets.Property)]
    public class DatabaseProperty : Attribute
    {
        public string DatabaseName;
        public int Size;
    }

    [AttributeUsage(System.AttributeTargets.Class)]
    public class DatabaseKey : Attribute
    {
        public string DatabaseName;
        public int Size;
    }   
}
