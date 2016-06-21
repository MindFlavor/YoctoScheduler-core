DELETE FROM 
	[dead].[ExecutionStatus]
WHERE 
	[ScheduleID] = @ScheduleID;

DELETE
FROM [live].[Schedules]
WHERE 
	[ScheduleID] = @ScheduleID;