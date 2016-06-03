UPDATE [live].[ExecutionStatus]
   SET 
	[LastUpdate] = @LastUpdate
 WHERE
	[GUID] = @GUID;