UPDATE [live].[Schedules]
SET    
    [TaskID] = @taskID
    ,[Cron] = @cron
    ,[Enabled] = @enabled
	,[LastFired] = @LastFired
WHERE 
    [ScheduleID] = @scheduleID;