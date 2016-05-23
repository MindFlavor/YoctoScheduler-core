SELECT 
    [ScheduleID]
    ,[Cron]
    ,[Enabled]
    ,[TaskID]
FROM[live].[Schedules]
WHERE 
	[Enabled] = 1;