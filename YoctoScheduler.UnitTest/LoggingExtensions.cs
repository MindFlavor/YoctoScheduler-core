using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoctoScheduler.Logging.Extensions;

namespace YoctoScheduler.UnitTest
{
    [TestClass]
    public class LoggingExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LoggingExtensions));

        [TestMethod]
        public void LoggingExtensions_Test1()
        {
            log.DebugFormat("Time now is {0:S}", DateTime.Now.ToString());
            log.InfoFormat("Time now is {0:S}", DateTime.Now.ToString());
            log.WarnFormat("Time now is {0:S}", DateTime.Now.ToString());
            log.ErrorFormat("Time now is {0:S}", DateTime.Now.ToString());
            log.FatalFormat("Time now is {0:S}", DateTime.Now.ToString());

            log.TraceFormat("Time now is {0:S}", DateTime.Now.ToString());
            log.VerboseFormat("Time now is {0:S}", DateTime.Now.ToString());
        }
    }
}
