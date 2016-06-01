INSERT INTO [lookup].[Secret]
           ([SecretName], [Blob], [Thumbprint])
	OUTPUT [INSERTED].[SecretName]
    VALUES
           (@SecretName, @Blob, @Thumbprint);