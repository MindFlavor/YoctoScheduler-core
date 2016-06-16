using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Exceptions
{
    public class DatabaseConcurrencyException<T> : ConcurrencyException
    {
        public string Action { get; set; }
        public T Entity { get; set; }

        public DatabaseConcurrencyException(string Action, T Entity, Exception innerException)
           : base(FormatString(Action,Entity), innerException) {
            this.Action = Action;
            this.Entity = Entity;
        }
        public DatabaseConcurrencyException(string Action, T Entity)
           : base(FormatString(Action, Entity)) {
            this.Action = Action;
            this.Entity = Entity;
        }

        protected static string FormatString(string Action, T Entity)
        {
            return string.Format("Failed to delete {0:S} with ID={1:S}. The key was not present in the database", typeof(T).Name, Entity.ToString());
        }
    }
}
