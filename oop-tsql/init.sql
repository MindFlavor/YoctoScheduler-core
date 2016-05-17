DROP DATABASE [YoctoScheduler.Core.MasterModel];
GO

CREATE DATABASE [YoctoScheduler.Core.MasterModel];
GO

SELECT * FROM [YoctoScheduler.Core.MasterModel].dbo.[Servers];
SELECT * FROM [YoctoScheduler.Core.MasterModel].[dbo].[Tasks];
SELECT * FROM [YoctoScheduler.Core.MasterModel].[dbo].[ExecutionStatus];


INSERT INTO [YoctoScheduler.Core.MasterModel].dbo.[Servers](Description, LastPing)
VALUES('Prova', '2010-01-01')
GO
INSERT INTO [YoctoScheduler.Core.MasterModel].dbo.[Servers](Description, LastPing)
VALUES('Prova2', '2014-01-01')
GO
INSERT INTO [YoctoScheduler.Core.MasterModel].dbo.[Servers](Description, LastPing)
VALUES('Prova3', '2014-04-01')
GO