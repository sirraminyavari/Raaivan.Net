USE [EKM_App]
GO

/****** Object:  Table [dbo].[WF_StateDataNeedInstances]    Script Date: 06/01/2013 14:40:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TABLE [dbo].[WF_TMPStateDataNeedInstances](
	[InstanceID] [uniqueidentifier] NOT NULL,
	[HistoryID] [uniqueidentifier] NOT NULL,
	[NodeID] [uniqueidentifier] NOT NULL,
	[Admin] [bit] NOT NULL,
	[Filled] [bit] NOT NULL,
	[FillingDate] [datetime] NULL,
	[AttachmentID] [uniqueidentifier] NOT NULL,
	[CreatorUserID] [uniqueidentifier] NULL,
	[CreationDate] [datetime] NULL,
	[LastModifierUserID] [uniqueidentifier] NULL,
	[LastModificationDate] [datetime] NULL,
	[Deleted] [bit] NOT NULL
 CONSTRAINT [PK_WF_TMPStateDataNeedInstances] PRIMARY KEY CLUSTERED 
(
	[InstanceID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


INSERT INTO [dbo].[WF_TMPStateDataNeedInstances](
	InstanceID, HistoryID, NodeID, [Admin], Filled, FillingDate, AttachmentID,
	CreatorUserID, CreationDate, LastModifierUserID, LastModificationDate, Deleted
)
SELECT InstanceID, HistoryID, NodeID, [Admin], Filled, FillingDate, NEWID(),
	CreatorUserID, CreationDate, LastModifierUserID, LastModificationDate, Deleted
FROM [dbo].[WF_StateDataNeedInstances]
GO


DROP TABLE [dbo].[WF_StateDataNeedInstances]
GO


CREATE TABLE [dbo].[WF_StateDataNeedInstances](
	[InstanceID] [uniqueidentifier] NOT NULL,
	[HistoryID] [uniqueidentifier] NOT NULL,
	[NodeID] [uniqueidentifier] NOT NULL,
	[Admin] [bit] NOT NULL,
	[Filled] [bit] NOT NULL,
	[FillingDate] [datetime] NULL,
	[AttachmentID] [uniqueidentifier] NOT NULL,
	[CreatorUserID] [uniqueidentifier] NULL,
	[CreationDate] [datetime] NULL,
	[LastModifierUserID] [uniqueidentifier] NULL,
	[LastModificationDate] [datetime] NULL,
	[Deleted] [bit] NOT NULL
 CONSTRAINT [PK_WF_StateDataNeedInstances] PRIMARY KEY CLUSTERED 
(
	[InstanceID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[WF_StateDataNeedInstances]  WITH CHECK ADD  CONSTRAINT [FK_WF_StateDataNeedInstances_aspnet_Users_Creator] FOREIGN KEY([CreatorUserID])
REFERENCES [dbo].[aspnet_Users] ([UserId])
GO

ALTER TABLE [dbo].[WF_StateDataNeedInstances] CHECK CONSTRAINT [FK_WF_StateDataNeedInstances_aspnet_Users_Creator]
GO

ALTER TABLE [dbo].[WF_StateDataNeedInstances]  WITH CHECK ADD  CONSTRAINT [FK_WF_StateDataNeedInstances_aspnet_Users_Modifier] FOREIGN KEY([LastModifierUserID])
REFERENCES [dbo].[aspnet_Users] ([UserId])
GO

ALTER TABLE [dbo].[WF_StateDataNeedInstances] CHECK CONSTRAINT [FK_WF_StateDataNeedInstances_aspnet_Users_Modifier]
GO

ALTER TABLE [dbo].[WF_StateDataNeedInstances]  WITH CHECK ADD  CONSTRAINT [FK_WF_StateDataNeedInstances_CN_Nodes] FOREIGN KEY([NodeID])
REFERENCES [dbo].[CN_Nodes] ([NodeID])
GO

ALTER TABLE [dbo].[WF_StateDataNeedInstances] CHECK CONSTRAINT [FK_WF_StateDataNeedInstances_CN_Nodes]
GO

ALTER TABLE [dbo].[WF_StateDataNeedInstances]  WITH CHECK ADD  CONSTRAINT [FK_WF_StateDataNeedInstances_WF_History] FOREIGN KEY([HistoryID])
REFERENCES [dbo].[WF_History] ([HistoryID])
GO

ALTER TABLE [dbo].[WF_StateDataNeedInstances] CHECK CONSTRAINT [FK_WF_StateDataNeedInstances_WF_History]
GO


INSERT INTO [dbo].[WF_StateDataNeedInstances]
SELECT * FROM [dbo].[WF_TMPStateDataNeedInstances]
GO


DROP TABLE [dbo].[WF_TMPStateDataNeedInstances]
GO