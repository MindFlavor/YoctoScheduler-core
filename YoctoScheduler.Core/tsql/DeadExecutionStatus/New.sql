INSERT INTO [dead].[ExecutionStatus]
    ([GUID]
    ,[ScheduleID]
    ,[TaskID]
    ,[ServerID]
    ,[LastUpdate]
    ,[Status])
VALUES
    (
    @GUID
    @ScheduleID
    ,@TaskID
    ,@ServerID
    ,@LastUpdate
    ,@Status)