INSERT INTO [live].[ExecutionQueue]
    (
    [TaskID]
    ,[Priority]
    ,[ScheduleID]
    ,[Inserted])
OUTPUT INSERTED.[GUID]
VALUES
    (@TaskID
    ,@Priority
    ,@ScheduleID
    ,@Inserted)