SELECT 
    [ScheduleID]
    ,[Cron]
    ,[Enabled]
    ,[TaskID]	
	,[LastFired]
FROM[live].[Schedules]
WHERE 
	[Enabled] = 1;