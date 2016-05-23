INSERT INTO [live].[ExecutionQueue]
    (
    [TaskID]
    ,[Priority]
    ,[ScheduleID]
    ,[InsertDate])
OUTPUT INSERTED.[GUID]
VALUES
    (@TaskID
    ,@Priority
    ,@ScheduleID
    ,@InsertDate)