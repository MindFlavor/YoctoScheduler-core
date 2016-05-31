DELETE FROM [live].[ExecutionStatus]
WHERE 
	[GUID] = @GUID;