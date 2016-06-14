UPDATE [live].[Servers]
  SET 
	[LastPing] = GETDATE()
	OUTPUT [INSERTED].[LastPing]
  WHERE
	[ServerID] = @ServerID;