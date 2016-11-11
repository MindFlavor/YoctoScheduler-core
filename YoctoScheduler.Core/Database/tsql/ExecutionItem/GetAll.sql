SELECT [GUID], TaskID, [Priority], ScheduleID, Inserted, NULL AS ServerID, NULL AS LastUpdate, NULL AS ReturnCode, 100 AS [Status] FROM [YoctoScheduler].[live].[ExecutionQueue] WITH(NOLOCK)
UNION ALL
SELECT [GUID], TaskID, NULL AS [Priority], ScheduleID, Inserted, ServerID, LastUpdate, NULL AS ReturnCode, 300 AS [Status] FROM [YoctoScheduler].[live].[ExecutionStatus] WITH(NOLOCK)
UNION ALL
SELECT [GUID], TaskID, NULL AS [Priority], ScheduleID, Inserted, ServerID, LastUpdate, ReturnCode, [Status] FROM [YoctoScheduler].[dead].[ExecutionStatus] WITH(NOLOCK);
