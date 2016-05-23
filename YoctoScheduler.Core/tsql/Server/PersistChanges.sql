UPDATE [live].[Servers]
SET    
    [Status] = @status
    ,[Description] = @description
    ,[LastPing] = @lastping
WHERE 
    [ServerID] = @serverID;