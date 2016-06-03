namespace YoctoScheduler.Core.ExecutionTasks.TSQL
{
    public struct Configuration
    {
        public string ConnectionString;
        public string Statement;
        public int CommandTimeout;
    }
}