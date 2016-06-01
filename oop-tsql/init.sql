USE [master];
GO
--DROP DATABASE [YoctoScheduler];
--GO

SELECT * FROM [YoctoScheduler].[configuration].[General];
SELECT * FROM [YoctoScheduler].[live].[Servers];

SELECT * FROM [YoctoScheduler].[live].[Tasks];
SELECT * FROM [YoctoScheduler].[lookup].[Secret];
SELECT * FROM [YoctoScheduler].[live].[Schedules];

SELECT 'queue', * FROM [YoctoScheduler].[live].[ExecutionQueue];
SELECT 'live', * FROM [YoctoScheduler].[live].[ExecutionStatus];
SELECT 'dead' ,* FROM [YoctoScheduler].[dead].[ExecutionStatus];


SELECT * FROM [YoctoScheduler].[commands].[Server];

--DELETE FROM [YoctoScheduler].[live].[ExecutionQueue];
--DELETE FROM [YoctoScheduler].[live].[ExecutionStatus];
--DELETE FROM [YoctoScheduler].[live].[Servers];
--DELETE FROM [YoctoScheduler].[lookup].[Secret];
--DELETE FROM [YoctoScheduler].[live].[Schedules];
--DELETE FROM [YoctoScheduler].[lookup].[Secret];
--DELETE FROM [YoctoScheduler].[live].[Tasks];

/* 
	Clean dead
*/

DELETE FROM [YoctoScheduler].[live].[Servers] WHERE [Status] < 0;
DELETE FROM [YoctoScheduler].[dead].[ExecutionStatus];
GO