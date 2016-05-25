INSERT INTO [live].[ExecutionStatus]
    ([ScheduleID]
    ,[TaskID]
    ,[ServerID]
	,[Inserted]
    ,[LastUpdate])
OUTPUT INSERTED.[GUID]
VALUES
    (@ScheduleID
    ,@TaskID
    ,@ServerID
	,@Inserted
    ,@LastUpdate);