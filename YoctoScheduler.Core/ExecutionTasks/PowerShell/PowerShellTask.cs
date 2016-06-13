using System.Management.Automation;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace YoctoScheduler.Core.ExecutionTasks.PowerShell
{
    public class PowerShellTask : JsonBasedTask<Configuration>
    {
        // this ugly trick allows the serialized JSON to have
        // "Row":"content" in its rows. This play nicely with
        // SQL Server 2016 FROM OPENJSON feature
        private class Result
        {
            public string Row;
        }

        public override string Do()
        {
            var ps = System.Management.Automation.PowerShell.Create();
            ps.AddScript(Configuration.Script);

            var results = ps.Invoke();


            List<Result> lResults = new List<Result>();
            foreach (var res in results)
            {
                lResults.Add(new Result() { Row = res.ToString() });

            }

            JsonSerializer s = new JsonSerializer();

            using (System.IO.StringWriter sw = new System.IO.StringWriter())
            {
                using (JsonTextWriter wr = new JsonTextWriter(sw))
                {
                    s.Serialize(wr, lResults);
                }

                var ret = sw.ToString();
                return ret;
            }
        }
    }
}