UPDATE [live].[ExecutionStatus]
  SET 
	[LastUpdate] = GETDATE()
	OUTPUT [INSERTED].[LastUpdate]
  WHERE
	[GUID] = @GUID;