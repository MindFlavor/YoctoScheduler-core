## Bad Length during secret encryption

If you receive *bad length* error during encryption it means you are using a too small certificate regarding the data to encrypt. Remember, the data is encoded using UTF8 so there is no 1-1 mapping between chars and bytes.

### Solution

Either decrease the byte count (maybe splitting the secret in two) or use a bigger RSA certificate.

Reference [http://stackoverflow.com/questions/1496793/rsa-encryption-getting-bad-length](http://stackoverflow.com/questions/1496793/rsa-encryption-getting-bad-length).
