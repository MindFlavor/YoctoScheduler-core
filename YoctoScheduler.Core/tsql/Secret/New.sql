INSERT INTO [lookup].[Secret]
           ([Blob], [Thumbprint])
	OUTPUT [INSERTED].[SecretID]    
    VALUES
           (@Blob, @Thumbprint);