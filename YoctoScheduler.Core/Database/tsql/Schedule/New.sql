INSERT INTO [live].[Schedules]
    ([TaskID]
    ,[Cron]
    ,[Enabled]
	,[LastFired])
OUTPUT [INSERTED].[ScheduleID]    
VALUES(
        @TaskID,
		@Cron,
		@Enabled,
		@LastFired
    );