SELECT 
    [GUID]
    ,[ScheduleID]
    ,[TaskID]
    ,[ServerID]
    ,[LastUpdate]
FROM [live].[ExecutionStatus]
WITH(XLOCK)
WHERE 
    [LastUpdate] < @lastUpdate;