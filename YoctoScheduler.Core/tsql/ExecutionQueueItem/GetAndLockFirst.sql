SELECT TOP 1
	[GUID]
	,[TaskID]
	,[Priority]
	,[ScheduleID]
	,[InsertDate]
FROM [live].[ExecutionQueue] WITH (XLOCK)
ORDER BY 
	[Priority] DESC,
	[InsertDate] DESC;