SELECT
		[SecretName]
		,[Blob]
		,[Thumbprint]
  FROM	
		[lookup].[Secret]
  WHERE 
		[SecretName] = @SecretName;