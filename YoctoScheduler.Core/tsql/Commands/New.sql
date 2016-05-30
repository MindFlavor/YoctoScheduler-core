INSERT INTO [commands].[Server]
           ([ServerID]
           ,[Command]
           ,[Payload])
	OUTPUT INSERTED.[ID]
     VALUES
           (@ServerID
           ,@Command
           ,@Payload
		   );
