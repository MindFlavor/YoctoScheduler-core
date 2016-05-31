INSERT INTO [live].[Servers]
    ([Status]
    ,[Description]
    ,[LastPing]
	,[HostName]
	,[IPs])
OUTPUT [INSERTED].[ServerID]    
VALUES(
        @status,
		@description,
		@lastping,
		@HostName,
		@IPs
    )