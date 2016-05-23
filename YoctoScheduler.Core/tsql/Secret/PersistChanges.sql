UPDATE [lookup].[Secret]
   SET 
	[Blob] = @Blob,
	[Thumbprint] = @Thumbprint
 WHERE
	[SecretID] = @SecretID;