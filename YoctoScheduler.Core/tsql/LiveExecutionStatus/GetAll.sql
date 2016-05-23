﻿SELECT 
    [GUID]
    ,[ScheduleID]
    ,[TaskID]
    ,[ServerID]
    ,[LastUpdate]
FROM [live].[ExecutionStatus]
WHERE 
    [LastUpdate] >= @lastUpdate;