INSERT INTO [lookup].[Secret]
           ([SecretName], [Blob], [Thumbprint])
    VALUES
           (@SecretName, @Blob, @Thumbprint);