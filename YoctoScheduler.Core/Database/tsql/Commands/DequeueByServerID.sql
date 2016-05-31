DELETE 
  FROM [commands].[Server]
OUTPUT DELETED.[ID]
      ,DELETED.[ServerID]
      ,DELETED.[Command]
      ,DELETED.[Payload]
  WHERE [ServerID] = @ServerID;

