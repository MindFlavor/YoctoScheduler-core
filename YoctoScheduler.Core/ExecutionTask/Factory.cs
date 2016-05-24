using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Tasks
{
    public class Factory
    {
        private Factory() { }

        public ITask NewTask(Type t, string json)
        {
            // reflection create object from json 
            // Product deserializedProduct = JsonConvert.DeserializeObject(json, t);
            // return deserializedProduct
            throw new NotImplementedException();
        }
    }
}
