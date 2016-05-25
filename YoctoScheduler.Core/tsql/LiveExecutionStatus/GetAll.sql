SELECT 
    [GUID]
    ,[ScheduleID]
    ,[TaskID]
    ,[ServerID]
	,[Inserted]
    ,[LastUpdate]
FROM [live].[ExecutionStatus]
WHERE 
    [LastUpdate] >= @lastUpdate;