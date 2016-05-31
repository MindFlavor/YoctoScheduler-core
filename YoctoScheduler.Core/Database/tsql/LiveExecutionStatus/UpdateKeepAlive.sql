UPDATE [live].[ExecutionStatus]
   SET 
	[LastUpdate] = @lastUpdate
 WHERE
	[GUID] = @GUID;