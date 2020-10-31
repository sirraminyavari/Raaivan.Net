USE [EKM_App]

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



INSERT INTO [dbo].[SH_PostTypes]
           ([PostTypeID]
           ,[Name]
           ,[PersianName])
     VALUES
           (1
           ,N'Text'
           ,N'متن')
GO


INSERT INTO [dbo].[SH_PostTypes]
           ([PostTypeID]
           ,[Name]
           ,[PersianName])
     VALUES
           (2
           ,N'Knowledge'
           ,N'دانش')
GO


INSERT INTO [dbo].[SH_PostTypes]
           ([PostTypeID]
           ,[Name]
           ,[PersianName])
     VALUES
           (3
           ,N'Node'
           ,N'گره')
GO


INSERT INTO [dbo].[SH_PostTypes]
           ([PostTypeID]
           ,[Name]
           ,[PersianName])
     VALUES
           (4
           ,N'Question'
           ,N'پرسش')
GO


INSERT INTO [dbo].[SH_PostTypes]
           ([PostTypeID]
           ,[Name]
           ,[PersianName])
     VALUES
           (5
           ,N'File'
           ,N'فایل')
GO


INSERT INTO [dbo].[SH_PostTypes]
           ([PostTypeID]
           ,[Name]
           ,[PersianName])
     VALUES
           (6
           ,N'User'
           ,N'کاربر')
GO