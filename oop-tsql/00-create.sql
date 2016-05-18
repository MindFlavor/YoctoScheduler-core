CREATE DATABASE [YoctoScheduler];
GO
USE [YoctoScheduler];
GO
CREATE SCHEMA [live];
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
	[ID] INT IDENTITY(1,1) NOT NULL,
	[TaskID] INT NOT NULL,
	[Status] INT NOT NULL,
	[ServerID] INT NOT NULL,
	[LastUpdate] DATETIME NOT NULL,
 CONSTRAINT [PK_ExecutionStatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
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


