INSERT INTO [live].[Tasks] (
	[ReenqueueOnDead]
	,[Name]
	,[Description]
	,[Type]
	,[Payload]
	)
OUTPUT [INSERTED].[TaskID]
VALUES(
        @ReenqueueOnDead,
		@Name,
		@Description,
		@Type,
		@Payload
    )