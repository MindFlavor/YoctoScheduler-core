INSERT INTO [live].[Tasks] (
	[ReenqueueOnDead]
	,[Type]
	,[Payload]
	)
OUTPUT [INSERTED].[TaskID]
VALUES(
        @ReenqueueOnDead,
		@Type,
		@Payload
    )