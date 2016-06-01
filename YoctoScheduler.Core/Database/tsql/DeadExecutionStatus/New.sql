INSERT INTO [dead].[ExecutionStatus]
    ([GUID]
    ,[ScheduleID]
    ,[TaskID]
    ,[ServerID]	
	,[Inserted]
    ,[LastUpdate]
    ,[Status]
	,[ReturnCode])
OUTPUT [INSERTED].[GUID]
VALUES
    (
    @GUID
    ,@ScheduleID
    ,@TaskID
    ,@ServerID
	,@Inserted
    ,@LastUpdate
    ,@Status
	,@ReturnCode);