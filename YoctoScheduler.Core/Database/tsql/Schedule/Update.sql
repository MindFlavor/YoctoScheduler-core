UPDATE [live].[Schedules]
SET    
    [TaskID] = @TaskID
    ,[Cron] = @Cron
    ,[Enabled] = @Enabled
	,[LastFired] = @LastFired
WHERE 
    [ScheduleID] = @ScheduleID;