UPDATE [live].[Schedule]
SET    
    [TaskID] = @taskID
    ,[Cron] = @cron
    ,[Enabled] = @enabled
WHERE 
    [ScheduleID] = @scheduleID;