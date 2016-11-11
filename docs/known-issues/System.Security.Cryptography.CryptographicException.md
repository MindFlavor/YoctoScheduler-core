# CryptographicException

During decrypt you might receive this exception:

```
Exception: System.Security.Cryptography.CryptographicException: Invalid provider type specified.
   at System.Security.Cryptography.Utils.CreateProvHandle(CspParameters parameters, Boolean randomKeyContainer)
   at System.Security.Cryptography.Utils.GetKeyPairHelper(CspAlgorithmType keyType, CspParameters parameters, Boolean randomKeyContainer, Int32 dwKeySize, SafeProvHandle& safeProvHandle, SafeKeyHandle& safeKeyHandle)
   at System.Security.Cryptography.RSACryptoServiceProvider.GetKeyPair()
   at System.Security.Cryptography.RSACryptoServiceProvider..ctor(Int32 dwKeySize, CspParameters parameters, Boolean useDefaultKeySize)
   at System.Security.Cryptography.X509Certificates.X509Certificate2.get_PrivateKey()
   at YoctoScheduler.Core.Secret.get_PlainTextValue() in D:\GIT\c_sharp\YoctoScheduler\YoctoScheduler.Core\Secret.cs:line 25
   at Test.Program.Main(String[] args) in D:\GIT\c_sharp\YoctoScheduler\Test\Program.cs:line 105
```

This seems to be caused by missing info in the PS generated certificate. More info here: [http://stackoverflow.com/questions/22581811/invalid-provider-type-specified-cryptographicexception-when-trying-to-load-pri](http://stackoverflow.com/questions/22581811/invalid-provider-type-specified-cryptographicexception-when-trying-to-load-pri).

You can use OpenSSL to overcome this (see [http://stackoverflow.com/questions/10175812/how-to-create-a-self-signed-certificate-with-openssl](http://stackoverflow.com/questions/10175812/how-to-create-a-self-signed-certificate-with-openssl)):

1. Generate certificate
```
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365
```

2. Convert to ```pfx``` format (to import in Windows):
```
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem
```

3. Import in My store of Server account as usual.
