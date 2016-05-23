INSERT INTO [live].[Servers]
    ([Status]
    ,[Description]
    ,[LastPing])
OUTPUT [INSERTED].[ServerID]    
VALUES(
        @status,
		@description,
		@lastping
    )