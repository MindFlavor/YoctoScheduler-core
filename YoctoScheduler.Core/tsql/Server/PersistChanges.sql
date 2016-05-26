UPDATE [live].[Servers]
SET    
    [Status] = @status
    ,[Description] = @description
    ,[LastPing] = @lastping
	,[HostName] = @HostName
	,[IPs] = @IPs
WHERE 
    [ServerID] = @serverID;