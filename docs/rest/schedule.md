## Schedule

### GET

#### URI

```
/api/schdules
```

#### Example input payload

```
<none>
```

#### Example curl

```
curl cantun.mindflavor.it:9000/api/schdules
```

#### Example response payload

 ```json
 [
   {
     "Cron": "* * * * *",
     "Enabled": true,
     "TaskID": 1,
     "ID": 1
   },
   {
     "Cron": "* * * * *",
     "Enabled": true,
     "TaskID": 1,
     "ID": 3
   },
   {
     "Cron": "* * * * *",
     "Enabled": true,
     "TaskID": 1,
     "ID": 4
   }
 ]
```

### POST

#### URI

```
/api/schdules
```

#### Example input payload

```json
{
  "Cron": "* * * * *",
  "Enabled": true,
  "TaskID": 1
}
```

#### Example curl

```
curl -X POST -H "Content-Type: application/json" -v cantun.mindflavor.it:9000/api/schedules -d '{"Cron":"* * * * *","Enabled":true,"TaskID":1}'
```

#### Example response payload

 ```json
 {
   "Cron": "* * * * *",
   "Enabled": true,
   "TaskID": 1,
   "ID": 5
 }
```
