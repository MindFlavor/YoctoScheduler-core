SELECT
		[SecretID]
		,[Blob]
		,[Thumbprint]
  FROM	
		[lookup].[Secret]
  WHERE 
		[SecretID] = @SecretID;