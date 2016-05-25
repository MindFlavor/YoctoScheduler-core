INSERT INTO [live].[Tasks] (
	[ReenqueueOnDead]
	)
OUTPUT [INSERTED].[TaskID]
VALUES(
        @ReenqueueOnDead
    )