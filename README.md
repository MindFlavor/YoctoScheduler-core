## Intro
YoctoScheduler is a muli-thread, multi-process scheduling system. Each server in  a cluster should be indpendent from the others while maintaining some architectural constraints.

## Legend

Entity Name | Description
------------|---------|
Task | Atomic execution block. A task will never migrate between servers. |
Server | Schedulator and executor process. A server manipulates the companion database.
Task status | A ```task``` can either be alive or dead. If not alive there is also a description of why is not running.
Schedule | A predefined fire time for a ```Task```. It supports the ```NCronTab``` syntax to be flexible (up to the single minute).
Workflow | A collection of task to be orchestrated as a single entity.

## Requisites

### Done
* Each server must be independent and must rely on the companion database only.
* A schedule must fire at most *once*.
* If a schedule fires when there are no servers running the schedule is *lost*.
* Each task should be atomic and transactional. A task might fail at any moment and can be restarted on the same server (or another one).
  > this constraint can be relaxed at first
* Each task must update its status at least each ***to_define_task_update_frequency*** seconds. An update will update the ```LastUpdate``` field in the ```[live].[ExecutionStatus]``` table.
* A task not updating its status for ***to_define_task_timeout*** seconds is considered dead. A dead task will be removed from the ```[live].[ExecutionStatus]``` table and placed in the ```[dead].[ExecutionStatus]``` table (along with a specific status).
* A dead task will be restarted if so specified (the ***restart logic is to be defined***).
* Each task can specify concurrency limits, both globally and in the same server. For example, a task might run on an unlimited number of different servers but each server should only run one instance. The concurrency check must be performed by the server at runtime at it should be conservative: better to avoid starting a task is there's the possibility of exceeding the maximum configured parallelism even if it's not a certainty.
* Each task should read its configuration from the centralized server (to encourage task independence).
* Required tasks:
  * T-SQL task

### ToDo

#### Mandatory
* Required tasks:
  * A command line task
  * A PowerShell task

#### Nice to have
* Tasks can spawn another task(s) as result of their elaboration.
* You can create workflows that chain task based on:
  * Status (successful, failed, in exception)
  * Constant match (ie ```if return number = 1 then ... else if ...```)
  * Resources available
* The server can schedule concurrent tasks inspecting the available resources (to better scale in parallel).

## Requisites

* SQL Server 2012+ or SQL Azure database.
* A database and a ```dbo_owner``` user with relative login.
* C# 4.5.2. Visual Studio is suggested but not required.

## Installation
YoctoScheduler can run in two modes, as a command line program or as a Windows Service. The [configuration](docs/configuration.md) is the same, the only difference is in the command line switches that either start the command line execution or register the windows service.

1. Execute the [oop-tsql/00-create.sql](oop-tsql/00-create.sql) script on your chosen database instance. This will create the required database.
2. Create a login in the database instance and give it ```db_owner``` on the previously created database.
3. Compile the executable and relative libraries.
4. [Configure](docs/configuration.md).
5. Run (for testing purposes you may want to start with the command line program).
6. *optional* Clone the web frontend (in a separate project right now).

## Configuration

See the specific section: [configuration](docs/configuration.md).

## Interaction

Check out the [REST API reference](docs/rest/rest.md), the command line commands are deprecated and will be removed in the future.

## Testing

Here are some testing REST commands you can send to your YoctoScheduler instance. All the commands are server-agnostic so there is no difference based on which server instance you pick. Also note this samples use the linux curl command line tool. You can download a copy for Windows from [wingw.org](http://www.mingw.org/) but some commands migth require some tweaking as windows and linux handle special characters differently.

### Secret

You can create a secret using the [```SecretItems```](docs/rest/secret-item.md) REST API command. You have to specify the certificate thumbprint (as found in ```My``` certificate store) and the text to encrypt. The Secret name must be unique otherwise the call will fail.

```
curl -X POST -H "Content-Type: application/json" cantun.mindflavor.it:9000/api/secretitems -d '{"Name":"MyConnectionString", "CertificateThumbprint":"277103c882995da2d199050e58522d364513307e", "PlainTextValue":"data source=vSQL14A.mindflavor.it;initial catalog=YoctoScheduler;User Id=fagiolo;Password=cotto;MultipleActiveResultSets=False;App=YoctoTask"}"'
```

You can call the same REST API in a browser to get the secret list:

![](docs/imgs/00.png)

The same information can be retrieved by the YoctoScheduler web interface:

![](docs/imgs/01.png)

Notice how the value is stored in its encrypted format only.


*Note:* There is a bug in PowerShell generated certificates. See the [```System.Security.Cryptography.CryptographicException: Invalid provider type specified```](docs/System.Security.Cryptography.CryptographicException.md) known issue about how to resolve this.

### Task
You can add a new mock task calling the [Tasks REST API](docs/rest/tasks.md) interface.

In a nutshell the task is defined by:
* Name
* Type (TSQL task, PowerShell task, etc...)
* Server failure resiliency (whether it should be executed again if the hosting server dies before its completion)* Concurrency limits (both global and local, use 0 for unconstrained)
* Payload (task-dependent)

For example this is how to create a ```WaitTask``` (useful ony for debugging purposes):

```
curl -X POST -H "Content-Type: application/json" cantun.mindflavor.it:9000/api/tasks -d '{"Name":"MyWaitTask", "ConcurrencyLimitGlobal":0, "ConcurrencyLimitSameInstance":1, "Description":"This task will stall the thread for 35 seconds. This task will task will not be requeued in case the server owning it dies", "ReenqueueOnDead":false,"Type":"WaitTask","Payload":"{\"SleepSeconds\":35}"'
```

Here is how you create a ```TSQLTask```:

```
curl -X POST -H "Content-Type: application/json" cantun.mindflavor.it:9000/api/tasks -d '{"Name":"MyDBTask_WithWait", "ConcurrencyLimitGlobal":2, "ConcurrencyLimitSameInstance":2,  "Description":"SELECT @@VERSION after WAITFOR DELAY of 30 seconds", "ReenqueueOnDead":true,"Type":"TSQLTask","Payload":"{\"ConnectionString\":\"%%[MyConnectionString]%%\", \"Statement\":\"WAITFOR DELAY \u002700:00:30\u0027; SELECT @@VERSION;\",\"CommandTimeout\":600}"'
```

For reference, here is the above task's payload:

```json
{
  "Name": "MyDBTask_WithWait",
  "ConcurrencyLimitGlobal": 2,
  "ConcurrencyLimitSameInstance": 2,
  "Description": "SELECT @@VERSION after WAITFOR DELAY of 30 seconds",
  "ReenqueueOnDead": true,
  "Type": "TSQLTask",
  "Payload": "{\"ConnectionString\":\"%%[MyConnectionString]%%\", \"Statement\":\"WAITFOR DELAY '00:00:30'; SELECT @@VERSION;\",\"CommandTimeout\":600"
}
```

Notice how you can embed the ```Secret``` surrounding it with ```[%%``` and ```%%]```. This syntax is supported by JSON based tasks.

You can retrieve the task list calling the ```REST API``` or using the YoctoScheduler web app:

![](docs/imgs/02.png)

### Schedule

To add a schedule just call the [```schedules``` REST API](docs/rest/schedule.md) interface. For example this command will schedule the task with ID 1 every minute:

```
curl -X POST -H "Content-Type: application/json" cantun.mindflavor.it:9000/api/schedules -d '{"Cron":"* * * * *","Enabled":true,"TaskID":1}'
```

Schedules support the CRON syntax via [NCrontab](https://github.com/atifaziz/NCrontab). For details please refer here: [https://github.com/atifaziz/NCrontab](https://github.com/atifaziz/NCrontab).

Schedules can be retrieved via REST interface or web application:

![](docs/imgs/03.png)

## Direct Execution

You can also enqueue a task for immediate execution instead of scheduling it. Just call the [```queueitems``` REST API](docs/rest/queueitem.md) interface specifying the TaskID and its priority:

```
curl -X POST -H "Content-Type: application/json" cantun.mindflavor.it:9000/api/queueitems -d '{"TaskID":1,"Priority":1}'
```

Scheduled task will start as soon as the concurrency conditions are met. You can also query the execution queue using the same interface.

> Right now you cannot choose which server will execute the task. Is this right or we should change it?

## Execution Lists

There are two different ```REST API``` interfaces for execution inspection: [alive](docs/rest/LiveExecution.md) and [completed](docs/rest/DeadExecution.md) executions. These two interfaces are GET only, that is you cannot post to them. The web interface shows the in the same page:

![](docs/imgs/04.png)

## Known issues

* [```System.Security.Cryptography.CryptographicException: Invalid provider type specified```](docs/known-issues/System.Security.Cryptography.CryptographicException.md).

* [```Access denied``` during Owin initialization](docs/known-issues/owin-access-denied.md).

* [```Bad Length``` during secret encryption](docs/known-issues/bad-length-encryption.md).


## Debug

TODO

## License
Please see the [LICENSE](LICENSE) file for details.
