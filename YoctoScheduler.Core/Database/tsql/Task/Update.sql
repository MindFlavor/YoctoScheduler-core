UPDATE [live].[Tasks] 
SET 
	[ReenqueueOnDead]					= @ReenqueueOnDead
	,[Name]								= @Name
	,[Description]						= @Description
	,[Type]								= @Type
	,[Payload]							= @Payload
	,[ConcurrencyLimitGlobal]			= @ConcurrencyLimitGlobal
	,[ConcurrencyLimitSameInstance]		= @ConcurrencyLimitSameInstance
WHERE 
	[TaskID] = @TaskID;