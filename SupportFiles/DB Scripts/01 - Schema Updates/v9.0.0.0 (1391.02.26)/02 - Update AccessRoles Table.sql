USE [EKM_App]
GO

/****** Object:  Table [dbo].[KKnowledges]    Script Date: 04/04/2012 12:34:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


INSERT INTO [dbo].[AccessRoles]
           ([ID]
           ,[Role]
           ,[Title])
     VALUES
           (NEWID()
           ,N'DefaultContentRegistration'
           ,N'ثبت فایل پیش فرض')
GO


