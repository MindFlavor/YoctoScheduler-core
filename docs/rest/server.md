## Server

### GET

#### URI

```
/api/servers
```

#### Example input payload

```
<none>
```

#### Example curl

```
curl cantun.mindflavor.it:9000/api/servers
```

#### Example response payload

 ```json
 [
  {
    "Status": -100,
    "Description": "Server di prova 5/31/2016 5:32:30 PM",
    "LastPing": "2016-05-31T17:32:30.673",
    "LastScheduleCheck": "2016-05-31T17:37:58.7069187+02:00",
    "HostName": "cantun",
    "IPs": [
      "fe80::5584:f170:a1fb:cb81%5",
      "10.1.0.167"
    ],
    "ID": 35
  },
  {
    "Status": 300,
    "Description": "Server di prova 5/31/2016 5:33:40 PM",
    "LastPing": "2016-05-31T17:37:40.773",
    "LastScheduleCheck": "2016-05-31T17:37:58.7079325+02:00",
    "HostName": "cantun",
    "IPs": [
      "fe80::5584:f170:a1fb:cb81%5",
      "10.1.0.167"
    ],
    "ID": 36
  }
]
```
