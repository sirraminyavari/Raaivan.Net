USE [EKM_App]
GO

/****** Object:  Table [dbo].[AttachmentFiles]    Script Date: 02/03/2018 08:59:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.9.5.2' BEGIN
	UPDATE [dbo].[AppSetting]
		SET [Version] = 'v28.10.5.3'
END
GO

IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.10.5.3' BEGIN
	CREATE TABLE [dbo].[DCT_Files](
		[ID] [uniqueidentifier] NOT NULL,
		[OwnerID] [uniqueidentifier] NOT NULL,
		[OwnerType] [varchar](50) NOT NULL,
		[FileNameGuid] [uniqueidentifier] NOT NULL,
		[Extension] [nvarchar](50) NULL,
		[FileName] [nvarchar](255) NOT NULL,
		[MIME] [nvarchar](255) NULL,
		[Size] [bigint] NULL,
		[CreatorUserID] [uniqueidentifier] NOT NULL,
		[CreationDate] [datetime] NOT NULL,
		[Deleted] [bit] NOT NULL,
		[ApplicationID] [uniqueidentifier] NOT NULL
	 CONSTRAINT [PK_DCT_Files] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.10.5.3' BEGIN
	ALTER TABLE [dbo].[DCT_Files]  WITH CHECK ADD  CONSTRAINT [FK_DCT_Files_aspnet_Applications] FOREIGN KEY([ApplicationID])
	REFERENCES [dbo].[aspnet_Applications] ([ApplicationId])
END
GO

IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.10.5.3' BEGIN
	ALTER TABLE [dbo].[DCT_Files] CHECK CONSTRAINT [FK_DCT_Files_aspnet_Applications]
END
GO

IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.10.5.3' BEGIN
	ALTER TABLE [dbo].[DCT_Files]  WITH CHECK ADD  CONSTRAINT [FK_DCT_Files_aspnet_Users] FOREIGN KEY([CreatorUserID])
	REFERENCES [dbo].[aspnet_Users] ([UserId])
END
GO

IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.10.5.3' BEGIN
	ALTER TABLE [dbo].[DCT_Files] CHECK CONSTRAINT [FK_DCT_Files_aspnet_Users]
END
GO

IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.10.5.3' BEGIN
	DECLARE @Now datetime = N'2000-01-01 00:00:00.000'

	INSERT INTO [dbo].[DCT_Files] (
		ApplicationID,
		ID,
		OwnerID,
		OwnerType,
		FileNameGuid,
		Extension,
		[FileName],
		MIME,
		Size,
		CreatorUserID,
		CreationDate,
		Deleted
	)
	SELECT	F.ApplicationID,
			F.ID,
			A.ObjectID,
			A.ObjectType,
			F.FileNameGuid,
			F.Extension,
			F.[FileName],
			F.MIME,
			F.Size,
			U.UserId,
			@Now,
			CASE WHEN ISNULL(F.Deleted, 0) = 0 AND ISNULL(A.Deleted, 0) = 0 THEN 0 ELSE 1 END
	FROM [dbo].[Attachments] AS A
		INNER JOIN [dbo].[AttachmentFiles] AS F
		ON F.AttachmentID = A.ID
		INNER JOIN [dbo].[aspnet_Applications] AS P
		ON P.ApplicationId = F.ApplicationID
		INNER JOIN [dbo].[aspnet_Users] AS U
		ON U.ApplicationId = P.ApplicationId AND LOWER(U.UserName) = N'admin'
END
GO

IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.10.5.3' BEGIN
	ALTER TABLE [dbo].[KW_KnowledgeTypes]
	ADD PreEvaluateByOwner bit NULL
END
GO

IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.10.5.3' BEGIN
	ALTER TABLE [dbo].[KW_KnowledgeTypes]
	ADD ForceEvaluatorsDescribe bit NULL
END
GO


IF (SELECT TOP(1) X.[Version] FROM [dbo].[AppSetting] AS X) = N'v28.10.5.3' BEGIN
	UPDATE [dbo].[AppSetting]
		SET [Version] = 'v28.10.8.0'
END
GO

