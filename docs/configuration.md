# Configuration

Configuration is done changing a the ```App.Config file``` and by editing the rows in the ```[YoctoScheduler].[configuration].[General]``` table. The install script provides default values for the ```[YoctoScheduler].[configuration].[General]``` table entries.

## App.config
Configure [Test\App.config](Test\App.config) ```connectionStrings``` node to point to the database installed before.


## [configuration].[General] table
The values are:

Item | Description | Default
-------|-------------|--------
SERVER_KEEPALIVE_SLEEP_MS | Time in milliseconds between a server update of its record in the ```[YoctoScheduler].[live].[Servers]``` table. | 1 minute
SERVER_POLL_DISABLE_DEAD_SERVERS_SLEEP_MS | Time in milliseconds between a server check for dead servers. | 1 minute
SERVER_POLL_DISABLE_DEAD_TASKS_SLEEP_MS | Time in milliseconds between a server check for dead tasks. | 10 seconds
SERVER_POLL_TASK_QUEUE_SLEEP_MS | Time in milliseconds between a server check for tasks to execute. | 1 second
SERVER_POLL_TASK_SCHEDULER_SLEEP_MS | Time in milliseconds between a server check for tasks to schedule. | 10 seconds
