SELECT 
	[TaskID]
	,[ReenqueueOnDead]	
	,[Name]
	,[Description]
	,[Type]
	,[Payload]
	,[ConcurrencyLimitGlobal]
	,[ConcurrencyLimitSameInstance]
	FROM [live].[Tasks] 
WHERE 
	[TaskID] = @TaskID;