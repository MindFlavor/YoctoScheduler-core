USE [master];
GO
--DROP DATABASE [YoctoScheduler];
--GO

SELECT * FROM [YoctoScheduler].[live].[Servers];
SELECT * FROM [YoctoScheduler].[live].[Tasks];
SELECT 'queue', * FROM [YoctoScheduler].[live].[ExecutionQueue];
SELECT 'live', * FROM [YoctoScheduler].[live].[ExecutionStatus];
SELECT 'dead' ,* FROM [YoctoScheduler].[dead].[ExecutionStatus];
SELECT * FROM [YoctoScheduler].[live].[Schedules];
SELECT * FROM [YoctoScheduler].[lookup].[Secret];

--DELETE FROM [YoctoScheduler].[live].[ExecutionQueue];
--DELETE FROM [YoctoScheduler].[live].[ExecutionStatus];
--DELETE FROM [YoctoScheduler].[live].[Servers] WHERE [Status] < 0;
--DELETE FROM [YoctoScheduler].[lookup].[Secret];