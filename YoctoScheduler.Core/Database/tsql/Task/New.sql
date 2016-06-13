INSERT INTO [live].[Tasks] (
	[ReenqueueOnDead]
	,[Name]
	,[Description]
	,[Type]
	,[Payload]
	,[ConcurrencyLimitGlobal]
	,[ConcurrencyLimitSameInstance]
	)
OUTPUT [INSERTED].[TaskID]
VALUES(
        @ReenqueueOnDead,
		@Name,
		@Description,
		@Type,
		@Payload,
		@ConcurrencyLimitGlobal,
		@ConcurrencyLimitSameInstance
    )