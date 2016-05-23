INSERT INTO [live].[Schedules]
    ([TaskID]
    ,[Cron]
    ,[Enabled])
OUTPUT [INSERTED].[ScheduleID]    
VALUES(
        @taskID,
		@cron,
		@enabled
    );