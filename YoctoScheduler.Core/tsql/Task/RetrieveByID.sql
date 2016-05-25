SELECT 
	[TaskID]
	,[ReenqueueOnDead]
	FROM [live].[Tasks] 
WHERE 
	[TaskID] = @id;