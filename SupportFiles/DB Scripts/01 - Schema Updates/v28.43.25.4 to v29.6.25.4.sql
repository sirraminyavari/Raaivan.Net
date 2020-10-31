USE [EKM_App]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.43.25.4' BEGIN
	UPDATE [dbo].[AppSetting]
		SET [Version] = 'v29.5.25.4' -- 13990507
END
GO

IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v29.5.25.4' BEGIN
	UPDATE [dbo].[AppSetting]
		SET [Version] = 'v29.6.25.4' -- 13990507
END
GO