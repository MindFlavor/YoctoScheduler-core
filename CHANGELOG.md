## 20161111

## First GitHub release

* Added WebAPI project
* Added Server REST Interface
* Added Schedule REST Interface
* Removed WebApp (moved to a separate projects)

### Commit [104b7d9d26097e56f1d038ada970ba642fbc208e](104b7d9d26097e56f1d038ada970ba642fbc208e)

* Added support for task configuration via custom text payload. Json is encouraged.
* Added support for secrets embedded in task configuration payload. Decryption is transparent (task-wise) provided the correct certificate is found in ```My``` store.

## 20160630

### Commit [9d9eafa49035f6792950cb362791dee5b564e109](9d9eafa49035f6792950cb362791dee5b564e109)

* Switched from ID based to NVARCAHR based secret primary key.

### Commit [8c56698661b9123030c604b982c745278181d5b4](8c56698661b9123030c604b982c745278181d5b4)

* Added support for server commands.
* Added support for *Abort Task* server command.

## 20160525

### Commit [3e5369b5d134efc2fe93d904ccacacb53571c0fe](3e5369b5d134efc2fe93d904ccacacb53571c0fe)

* Added task and mock task.

### Commit [a0ad801417223d7f5876e2a46aca441275feb21f](a0ad801417223d7f5876e2a46aca441275feb21f)

* Added support for task autorestart in case of dead server (ie keepalive timeout).

## 20160524

### Commit [a158eb56b827b447c2c3ae996d6fa07f519e24bf](a158eb56b827b447c2c3ae996d6fa07f519e24bf)

* Completed dead task dectection and handling code.

### Commit [e45ddcf8c7c8e38ff8e84bfd1509b28b480256ef](e45ddcf8c7c8e38ff8e84bfd1509b28b480256ef)

* Added support for database based parameter configuration.

## 20160523

### Commit [7e7618fb82f071c2debfbc1fc33a9ef28957ab98](7e7618fb82f071c2debfbc1fc33a9ef28957ab98)

* Added Secret entity. Its purpose is to store sensitive information in ciphertext.
