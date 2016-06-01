SELECT 
    [ScheduleID]
    ,[Cron]
    ,[Enabled]
    ,[TaskID]
	,[LastFired]
FROM[live].[Schedules]
WHERE 
	[ScheduleID] = @ScheduleID;