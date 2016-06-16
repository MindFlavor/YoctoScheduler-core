INSERT INTO [live].[Servers]
    ([Status]
    ,[Description]
    ,[LastPing]
	,[HostName]
	,[IPs])
OUTPUT [INSERTED].[ServerID], [INSERTED].[LastPing]  
VALUES(
        @status,
		@description,
		GETDATE(),
		@HostName,
		@IPs
    )