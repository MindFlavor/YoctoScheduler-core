## Server

### GET

#### URI

```
/api/executions
```

#### Example input payload

```
<none>
```

#### Example curl

```
curl cantun.mindflavor.it:9000/api/executions
```

#### Example response payload

 ```json
[
    {
        "TaskID": 5,
        "Priority": null,
        "ScheduleID": null,
        "ServerID": 2,
        "Inserted": "2016-11-07T11:56:32.86",
        "LastUpdate": "2016-11-07T11:56:32.91",
        "Status": 1000,
        "ReturnCode": "FRCOGNOZBOOK\\SQL16",
        "ID": "bc4961d7-d8a4-e611-b731-e4a7a0ca65f6"
    },
    {
        "TaskID": 6,
        "Priority": null,
        "ScheduleID": null,
        "ServerID": 2,
        "Inserted": "2016-11-07T11:56:36.96",
        "LastUpdate": "2016-11-07T11:56:37.033",
        "Status": -3000,
        "ReturnCode": "System.Data.SqlClient.SqlException (0x80131904): Must declare the scalar variable \"@@VERSIONa\".\r\n   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\r\n   at System.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\r\n   at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)\r\n   at System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)\r\n   at System.Data.SqlClient.SqlDataReader.TryConsumeMetaData()\r\n   at System.Data.SqlClient.SqlDataReader.get_MetaData()\r\n   at System.Data.SqlClient.SqlCommand.FinishExecuteReader(SqlDataReader ds, RunBehavior runBehavior, String resetOptionsString, Boolean isInternal, Boolean forDescribeParameterEncryption)\r\n   at System.Data.SqlClient.SqlCommand.RunExecuteReaderTds(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, Boolean async, Int32 timeout, Task& task, Boolean asyncWrite, Boolean inRetry, SqlDataReader ds, Boolean describeParameterEncryptionRequest)\r\n   at System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method, TaskCompletionSource`1 completion, Int32 timeout, Task& task, Boolean& usedCache, Boolean asyncWrite, Boolean inRetry)\r\n   at System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method)\r\n   at System.Data.SqlClient.SqlCommand.ExecuteScalar()\r\n   at YoctoScheduler.Core.ExecutionTasks.TSQL.TSQLTask.Do() in C:\\src\\c_sharp\\YoctoScheduler\\YoctoScheduler.Core\\ExecutionTasks\\TSQL\\TSQLTask.cs:line 17\r\n   at YoctoScheduler.Core.ExecutionTasks.Watchdog.ExecutionThread() in C:\\src\\c_sharp\\YoctoScheduler\\YoctoScheduler.Core\\ExecutionTasks\\Watchdog.cs:line 101\r\nClientConnectionId:428a9abd-ab7d-40a2-9554-22b6e2a90a08\r\nError Number:137,State:2,Class:15",
        "ID": "be4961d7-d8a4-e611-b731-e4a7a0ca65f6"
    }
]
```
