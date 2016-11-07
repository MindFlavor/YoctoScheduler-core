# Configuration

Configuration is done changing a the ```service configuration file``` and by editing the rows in the ```[YoctoScheduler].[configuration].[General]``` table. The install script provides default values for the ```[YoctoScheduler].[configuration].[General]``` table entries.

## Service configuration file
Configure [YoctoScheduler.ServiceHost/sample_service_config.json](../YoctoScheduler.ServiceHost/sample_service_config.json) as explained below. Then launch the executable with the ```-c``` flag passing the configuration file as parameter. 
For example:

```
YoctoScheduler.ServiceHost.exe -c C:\temp\sample_service_config.json
``` 

This command line will start the scheduler with the ```C:\temp\sample_service_config.json``` configuration file. You can start as many scheduler as you want on the same server but make sure to avoid port conflicts (for the RestEndpoint and HttpEndpoint).

### Sample configuration file

This is a sample configuration. Please refer to [YoctoScheduler.ServiceHost/sample_service_config.json](../YoctoScheduler.ServiceHost/sample_service_config.json) for an updated example:

```json
{
    "InstanceName": "test01",
    "Log4NetConfigFile": "sample_log4net_config.xml",
    "RestEndpoint": "http://*:9000/api",
    "HttpEndpoint": "http://*:9000/",
    "WwwRoot": "D:\\GIT\\www\\YoctoScheduler\\app",
    "ConnectionString": "data source=servername;initial catalog=YoctoScheduler;Trusted_Connection=True;MultipleActiveResultSets=False;App=YoctoServer"
}
```

### Parameters 

Item | Description | Example
-------|-------------|---------
InstanceName | This is an arbitrary string that is meant to identify the specific scheduler instance. | ```Instance pre-production```
Log4NetConfigFile | The location of the Log4Net configuration file. You can copy the sample one from here: [YoctoScheduler.ServiceHost/sample_log4net_config.xml](../YoctoScheduler.ServiceHost/sample_log4net_config.xml). | ```C:\\temp\\sample_log4net_config.xml```
RestEndpoint | This is the REST interface endpoint. It can share the same port as the web interface (in order to avoid CORS) but make sure to specify a suffix (I suggest you to use ```api```) as the example. | ```http://*:9000/api```
HttpEndpoint | Web interface endpoint. The scheduler serves a web site containing the administrative interface. The web app is, however, hosted in another project so you have to clone it indipendently. | ```http://*:9000/api```
WwwRoot | The directory containing the web app (the REST interface does not need a directory). You can point it to an empty folder to disable the web interface altogether. | ```C:\\temp\\web_interface```
ConnectionString | The connection string pointing to the YoctoScheduler configuration database. Make sure to include the ```initial catalog=``` section or rely on default database as the scheduler does not change database context (so you can have more configuration databases in the same instance). | ```Data source=servername;Initial catalog=YoctoScheduler;Trusted_Connection=True;App=YoctoServer```

## [configuration].[General] table

Item | Description | Default
-------|-------------|--------
SERVER_KEEPALIVE_SLEEP_MS | Time in milliseconds between a server update of its record in the ```[YoctoScheduler].[live].[Servers]``` table. | 1 minute
SERVER_POLL_DISABLE_DEAD_SERVERS_SLEEP_MS | Time in milliseconds between a server check for dead servers. | 1 minute
SERVER_POLL_DISABLE_DEAD_TASKS_SLEEP_MS | Time in milliseconds between a server check for dead tasks. | 10 seconds
SERVER_POLL_TASK_QUEUE_SLEEP_MS | Time in milliseconds between a server check for tasks to execute. | 1 second
SERVER_POLL_TASK_SCHEDULER_SLEEP_MS | Time in milliseconds between a server check for tasks to schedule. | 10 seconds
SERVER_POLL_COMMANDS_SLEEP_MS | Time in milliseconds between a server check commands. | 10 seconds
TASK_MAXIMUM_UPDATE_LAG_MS| Maximum allowed time in milliseconds between task updates before declaring it dead. | 1 minute
SERVER_MAXIMUM_UPDATE_LAG_MS | Maximum allowed time in milliseconds between server updates before declaring it dead. | 5 minutes
WATCHDOG_SLEEP_MS | Sleep time between watchdog thread ```IsAlive``` checks. | 2 minutes
