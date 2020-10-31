USE [EKM_App]
GO



ALTER TABLE [dbo].[CN_Nodes]
ADD [ExpirationDate] DATETIME NULL
GO

ALTER TABLE [dbo].[CN_Nodes]
ADD [PublicDescription] nvarchar(max) NULL
GO




SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



UPDATE [dbo].[AppSetting]
	SET [Version] = 'v28.2.19.0' -- 13960217
GO