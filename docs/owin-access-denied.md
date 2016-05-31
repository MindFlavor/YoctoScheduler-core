In case Owin complains about Access denied during the endpoint creation you have to setup the URLACLs.
You can do that using ```netsh``` with this commands:

```
http add urlacl http://*:9000/ user=EVERYONE
http add urlacl http://*:9001/ user=EVERYONE
http add urlacl http://*:9002/ user=EVERYONE
http add urlacl http://*:9003/ user=EVERYONE
```

Of course this is very weak, you should consider hardening the ACLs to the specific YoctoScheduler account only.

More details:
* [http://stackoverflow.com/questions/16642651/self-hosted-owin-and-urlacl](http://stackoverflow.com/questions/16642651/self-hosted-owin-and-urlacl)
* [http://stackoverflow.com/questions/24976425/running-self-hosted-owin-web-api-under-non-admin-account](http://stackoverflow.com/questions/24976425/running-self-hosted-owin-web-api-under-non-admin-account)
