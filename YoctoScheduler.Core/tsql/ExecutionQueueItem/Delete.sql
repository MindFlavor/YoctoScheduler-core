DELETE FROM [live].[ExecutionQueue]
WHERE 
	[GUID] = @GUID;