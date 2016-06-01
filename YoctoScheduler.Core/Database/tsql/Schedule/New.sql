INSERT INTO [live].[Schedules]
    ([TaskID]
    ,[Cron]
    ,[Enabled]
	,[LastFired])
OUTPUT [INSERTED].[ScheduleID]    
VALUES(
        @taskID,
		@cron,
		@enabled,
		@LastFired
    );