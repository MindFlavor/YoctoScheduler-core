SELECT 
	[TaskID]
	,[ReenqueueOnDead]	
	,[Name]
	,[Description]
	,[Type]
	,[Payload]
	FROM [live].[Tasks] 
WHERE 
	[TaskID] = @id;