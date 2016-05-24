CREATE DATABASE [YoctoScheduler];
GO
USE [YoctoScheduler];
GO
CREATE SCHEMA [live];
GO
CREATE SCHEMA [dead];
GO
CREATE SCHEMA [lookup];
GO
CREATE SCHEMA [configuration];
GO

CREATE TABLE [live].[Servers](
	[ServerID] INT IDENTITY(1,1) NOT NULL,
	[Status] INT NOT NULL,
	[Description] NVARCHAR(MAX) NULL,
	[LastPing] DATETIME NOT NULL,
 CONSTRAINT [PK_Servers] PRIMARY KEY CLUSTERED 
(
	[ServerID] ASC
));
GO

CREATE TABLE [live].[Tasks](
	[TaskID] INT IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED 
(
	[TaskID] ASC
));
GO

CREATE TABLE [live].[Schedules](
	[ScheduleID] INT IDENTITY(1,1) NOT NULL,
	[Cron] NVARCHAR(255) NOT NULL,
	[Enabled] BIT NOT NULL DEFAULT(0),
	[TaskID] INT NOT NULL,
 CONSTRAINT [PK_Schedules] PRIMARY KEY CLUSTERED 
(
	[ScheduleID] ASC
));
GO
ALTER TABLE [live].[Schedules]  WITH CHECK ADD  CONSTRAINT [FK_Schedules.Tasks_TaskID] FOREIGN KEY([TaskID])
REFERENCES [live].[Tasks] ([TaskID])
ON DELETE CASCADE;
GO
ALTER TABLE [live].[Schedules] CHECK CONSTRAINT [FK_Schedules.Tasks_TaskID];
GO

CREATE TABLE [live].[ExecutionStatus](
	[GUID] UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWID()),
	[ScheduleID] INT NULL,
	[TaskID] INT NOT NULL,	
	[ServerID] INT NOT NULL,
	[LastUpdate] DATETIME NOT NULL,
 CONSTRAINT [PK_ExecutionStatus] PRIMARY KEY CLUSTERED 
(
	[GUID] ASC
));
GO

ALTER TABLE [live].[ExecutionStatus]  WITH CHECK ADD  CONSTRAINT [FK_ExecutionStatus_Servers_ServerID] FOREIGN KEY([ServerID])
REFERENCES [live].[Servers] ([ServerID])
ON DELETE CASCADE;
GO

ALTER TABLE [live].[ExecutionStatus] CHECK CONSTRAINT [FK_ExecutionStatus_Servers_ServerID];
GO

ALTER TABLE [live].[ExecutionStatus]  WITH CHECK ADD  CONSTRAINT [FK_ExecutionStatus_Tasks_TaskID] FOREIGN KEY([TaskID])
REFERENCES [live].[Tasks] ([TaskID])
GO

ALTER TABLE [live].[ExecutionStatus] CHECK CONSTRAINT [FK_ExecutionStatus_Tasks_TaskID]
GO

ALTER TABLE [live].[ExecutionStatus]  WITH CHECK ADD  CONSTRAINT [FK_ExecutionStatus_Schedule_ScheduleID] FOREIGN KEY([ScheduleID])
REFERENCES [live].[Schedules] ([ScheduleID])
GO

ALTER TABLE [live].[ExecutionStatus] CHECK CONSTRAINT [FK_ExecutionStatus_Schedule_ScheduleID]
GO

CREATE UNIQUE INDEX IX_SingleScheduleExecution ON [live].[ExecutionStatus]([TaskID], [ScheduleID]) INCLUDE([ServerID], [LastUpdate]);
GO

CREATE INDEX IX_LastUpdate ON [live].[ExecutionStatus]([LastUpdate]);
GO

CREATE TABLE [dead].[ExecutionStatus](
	[GUID] UNIQUEIDENTIFIER NOT NULL,
	[ScheduleID] INT NULL,
	[TaskID] INT NOT NULL,	
	[Status] INT NOT NULL,
	[ServerID] INT NOT NULL,
	[LastUpdate] DATETIME NOT NULL,
 CONSTRAINT [PK_ExecutionStatus] PRIMARY KEY CLUSTERED 
(
	[GUID] ASC
));
GO

ALTER TABLE [dead].[ExecutionStatus]  WITH CHECK ADD  CONSTRAINT [FK_dExecutionStatus_Servers_ServerID] FOREIGN KEY([ServerID])
REFERENCES [live].[Servers] ([ServerID])
ON DELETE CASCADE;
GO

ALTER TABLE [dead].[ExecutionStatus] CHECK CONSTRAINT [FK_dExecutionStatus_Servers_ServerID];
GO

ALTER TABLE [dead].[ExecutionStatus]  WITH CHECK ADD  CONSTRAINT [FK_dExecutionStatus_Tasks_TaskID] FOREIGN KEY([TaskID])
REFERENCES [live].[Tasks] ([TaskID])
GO

ALTER TABLE [dead].[ExecutionStatus] CHECK CONSTRAINT [FK_dExecutionStatus_Tasks_TaskID]
GO

ALTER TABLE [dead].[ExecutionStatus]  WITH CHECK ADD  CONSTRAINT [FK_dExecutionStatus_Schedule_ScheduleID] FOREIGN KEY([ScheduleID])
REFERENCES [live].[Schedules] ([ScheduleID])
GO

ALTER TABLE [dead].[ExecutionStatus] CHECK CONSTRAINT [FK_dExecutionStatus_Schedule_ScheduleID]
GO



CREATE TABLE [live].[ExecutionQueue] (
	[GUID] UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWID()),
	[TaskID] INT NOT NULL,
	[Priority] INT NOT NULL,
	[ScheduleID] INT NULL,
	[InsertDate] DATETIME NOT NULL DEFAULT(GETDATE()),
 CONSTRAINT [PK_ExecutionQueue] PRIMARY KEY CLUSTERED 
(
	[GUID] ASC
));
GO

ALTER TABLE [live].[ExecutionQueue] WITH CHECK ADD  CONSTRAINT [FK_ExecutionQueue_Tasks_TaskID] FOREIGN KEY([TaskID])
REFERENCES [live].[Tasks] ([TaskID])
GO

ALTER TABLE [live].[ExecutionQueue] CHECK CONSTRAINT [FK_ExecutionQueue_Tasks_TaskID]
GO

ALTER TABLE [live].[ExecutionQueue] WITH CHECK ADD  CONSTRAINT [FK_ExecutionQueue_ScheduleID] FOREIGN KEY([ScheduleID])
REFERENCES [live].[Schedules] ([ScheduleID])
GO

ALTER TABLE [live].[ExecutionQueue] CHECK CONSTRAINT [FK_ExecutionQueue_ScheduleID]
GO

--------------------------

CREATE TABLE [lookup].[Secret] (
	[SecretID]		INT IDENTITY(1,1),
	[Blob]			VARBINARY(MAX),
	[Thumbprint]	CHAR(40),
 CONSTRAINT [PK_Secret] PRIMARY KEY CLUSTERED 
(
	[SecretID]
));
GO

----------------
CREATE TABLE [configuration].[General] (
	[Item]			NVARCHAR(255),
	[Value]			NVARCHAR(MAX)
 CONSTRAINT [PK_General] PRIMARY KEY CLUSTERED 
(
	[Item]
));
GO

------------------------------------------------------
------------------- Default values -------------------
------------------------------------------------------
INSERT INTO [configuration].[General]([Item], [Value]) VALUES('SERVER_KEEPALIVE_SLEEP_MS',					1 * 60 * 1000); -- one minute
INSERT INTO [configuration].[General]([Item], [Value]) VALUES('SERVER_POLL_DISABLE_DEAD_SERVERS_SLEEP_MS',	1 * 60 * 1000); -- one minute
INSERT INTO [configuration].[General]([Item], [Value]) VALUES('SERVER_POLL_DISABLE_DEAD_TASKS_SLEEP_MS',	1 * 10 * 1000); -- 10 seconds
INSERT INTO [configuration].[General]([Item], [Value]) VALUES('SERVER_POLL_TASK_QUEUE_SLEEP_MS',			1 * 01 * 1000); -- 1 second
INSERT INTO [configuration].[General]([Item], [Value]) VALUES('SERVER_POLL_TASK_SCHEDULER_SLEEP_MS',		1 * 10 * 1000); -- 10 seconds
----------------
USE [master];
GO