﻿SELECT [ServerID]
      ,[Status]
      ,[Description]
      ,[LastPing]
      ,[HostName]
      ,[IPs]
  FROM [live].[Servers]
  WHERE 
	[ServerID] = @ServerID;

