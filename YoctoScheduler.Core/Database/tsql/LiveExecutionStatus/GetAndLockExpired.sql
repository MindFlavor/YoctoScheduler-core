SELECT 
    [GUID]
    ,[ScheduleID]
    ,[TaskID]
    ,[ServerID]
	,[Inserted]
    ,[LastUpdate]
FROM [live].[ExecutionStatus]
WITH(XLOCK)
WHERE 
    [LastUpdate] < DATEADD(millisecond, -1 * @timeoutMilliSeconds, GETDATE());