SELECT TOP 1
	[GUID]
	,[TaskID]
	,[Priority]
	,[ScheduleID]
	,[Inserted]
FROM [live].[ExecutionQueue] WITH (TABLOCKX)
ORDER BY 
	[Priority] DESC,
	[Inserted] DESC;