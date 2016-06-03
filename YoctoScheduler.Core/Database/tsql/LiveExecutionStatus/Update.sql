UPDATE [live].[ExecutionStatus]
   SET 
	[ScheduleID]	= @ScheduleID
	,[TaskID]		= @TaskID
	,[ServerID]		= @ServerID
	,[Inserted]		= @Inserted
	,[LastUpdate]	= @LastUpdate
 WHERE
	[GUID] = @GUID;