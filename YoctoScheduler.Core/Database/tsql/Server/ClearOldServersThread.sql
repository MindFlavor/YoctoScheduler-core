UPDATE [live].[Servers]
SET [Status] = @statusToSet
WHERE 
    [LastPing] < DATEADD(millisecond, -1 * @timeoutMilliSeconds, GETDATE())
    AND [Status] > @minStatus;