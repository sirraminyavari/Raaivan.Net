use [EKM_App]


INSERT INTO [dbo].[AccessRoles]
           ([ID]
           ,[Role]
           ,[Title])
     VALUES
           (NEWID()
           ,N'CopCreation'
           ,N'ایجاد انجمن های دانایی')
GO