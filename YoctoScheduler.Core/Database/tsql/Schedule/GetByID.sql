SELECT 
    [ScheduleID]
    ,[Cron]
    ,[Enabled]
    ,[TaskID]
FROM[live].[Schedules]
WHERE 
	[ScheduleID] = @ScheduleID;