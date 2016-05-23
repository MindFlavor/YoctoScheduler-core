UPDATE [live].[Servers]
SET [Status] = @statusToSet
WHERE 
    [LastPing] < @dt 
    AND [Status] > @minStatus;