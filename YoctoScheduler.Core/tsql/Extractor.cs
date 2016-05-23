using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.tsql
{
    public class Extractor
    {
        public const string PREFIX = "YoctoScheduler.Core.tsql.";
        public const string SUFFIX = ".sql";

        private Extractor() { }

        public static string Get(string name)
        {
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(PREFIX + name + SUFFIX))
            {
                if(stream == null)
                    throw new Exceptions.TSQLNotFoundException(name);

                using (System.IO.StreamReader sr = new System.IO.StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
