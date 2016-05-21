USE [master];
GO
--DROP DATABASE [YoctoScheduler];
--GO

SELECT * FROM [YoctoScheduler].[live].[Servers];
SELECT * FROM [YoctoScheduler].[live].[Tasks];
SELECT * FROM [YoctoScheduler].[live].[ExecutionQueue];
SELECT * FROM [YoctoScheduler].[live].[ExecutionStatus];
SELECT * FROM [YoctoScheduler].[dead].[ExecutionStatus];
SELECT * FROM [YoctoScheduler].[live].[Schedules];

