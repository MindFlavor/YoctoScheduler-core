USE [master];
GO
--DROP DATABASE [YoctoScheduler];
--GO

SELECT * FROM [YoctoScheduler].[configuration].[General];
SELECT * FROM [YoctoScheduler].[live].[Servers] ORDER BY LastPing DESC;

SELECT * FROM [YoctoScheduler].[live].[Tasks];
SELECT * FROM [YoctoScheduler].[lookup].[Secret];
SELECT * FROM [YoctoScheduler].[live].[Schedules];

SELECT 'queue', * FROM [YoctoScheduler].[live].[ExecutionQueue] S INNER JOIN [YoctoScheduler].[live].[Tasks] T ON S.TaskID = T.TaskID;
SELECT 'live', * FROM [YoctoScheduler].[live].[ExecutionStatus] S INNER JOIN [YoctoScheduler].[live].[Tasks] T ON S.TaskID = T.TaskID;
SELECT 'dead' ,* FROM [YoctoScheduler].[dead].[ExecutionStatus] S INNER JOIN [YoctoScheduler].[live].[Tasks] T ON S.TaskID = T.TaskID;

SELECT * FROM [YoctoScheduler].[commands].[Server];

SELECT JSON_VALUE(ReturnCode, '$[0]') FROM [YoctoScheduler].[dead].[ExecutionStatus]

-- Extract result from PowerShell (first)
DECLARE @var NVARCHAR(MAX);
SELECT TOP 1 @var = ReturnCode FROM [YoctoScheduler].[dead].[ExecutionStatus] S INNER JOIN [YoctoScheduler].[live].[Tasks] T ON S.TaskID = T.TaskID
WHERE T.[Type] = 'PowerShellTask'

SELECT *
 FROM OPENJSON (@var, '$')
 WITH (
        Row NVARCHAR(MAX)
 ) AS OrdersArray

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