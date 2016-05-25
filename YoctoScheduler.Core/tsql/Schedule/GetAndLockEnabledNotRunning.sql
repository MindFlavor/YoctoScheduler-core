SELECT 
    S.[ScheduleID]
    ,S.[Cron]
    ,S.[Enabled]
    ,S.[TaskID] 
FROM [live].[Schedules] S WITH(XLOCK)
LEFT OUTER JOIN [live].[ExecutionQueue]  Q WITH(XLOCK) ON S.[ScheduleID] = Q.[ScheduleID]
LEFT OUTER JOIN [live].[ExecutionStatus] E WITH(XLOCK) ON S.[ScheduleID] = E.[ScheduleID]
WHERE
		Q.[ScheduleID] IS NULL 
	AND
		E.[ScheduleID] IS NULL  
	AND
		S.Enabled = 1;