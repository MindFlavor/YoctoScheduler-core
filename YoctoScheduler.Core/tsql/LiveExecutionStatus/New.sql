INSERT INTO [live].[ExecutionStatus]
    ([ScheduleID]
    ,[TaskID]
    ,[ServerID]
    ,[LastUpdate])
OUTPUT INSERTED.[GUID]
VALUES
    (@ScheduleID
    ,@TaskID
    ,@ServerID
    ,@LastUpdate);