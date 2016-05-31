SELECT 
	[TaskID]
	,[ReenqueueOnDead]
	,[Type]
	,[Payload]
	FROM [live].[Tasks] 
WHERE 
	[TaskID] = @id;