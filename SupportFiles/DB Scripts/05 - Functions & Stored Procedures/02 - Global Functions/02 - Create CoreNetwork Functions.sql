USE [EKM_App]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/* 'GFN' stands for Global Function */
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetNodeTypeID]') 
    AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetNodeTypeID]
GO

CREATE FUNCTION [dbo].[CN_FN_GetNodeTypeID](
	@ApplicationID	uniqueidentifier,
	@AdditionalID	nvarchar(50)
)
RETURNS uniqueidentifier
WITH ENCRYPTION
AS
BEGIN
	RETURN (
		SELECT TOP(1) NodeTypeID 
		FROM [dbo].[CN_NodeTypes]
		WHERE ApplicationID = @ApplicationID AND AdditionalID = @AdditionalID AND
			ISNULL(@AdditionalID, N'') <> N''
	)
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetDepartmentNodeTypeIDs]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetDepartmentNodeTypeIDs]
GO

CREATE FUNCTION [dbo].[CN_FN_GetDepartmentNodeTypeIDs](
	@ApplicationID	uniqueidentifier
)
RETURNS @OutputTable TABLE (
	NodeTypeID uniqueidentifier primary key clustered,
	ParentID uniqueidentifier,
	[Level] int,
	Name nvarchar(2000)
)
WITH ENCRYPTION
AS
BEGIN
	;WITH hierarchy (ID, ParentID, [Level], Name)
	AS
	(
		SELECT NodeTypeID AS ID, ParentID AS ParentID, 0 AS [Level], Name AS Name
		FROM [dbo].[CN_NodeTypes]
		WHERE ApplicationID = @ApplicationID AND AdditionalID = N'6'
		
		UNION ALL
		
		SELECT NT.NodeTypeID AS ID, NT.ParentID AS ParentID, 
			[Level] + 1, NT.Name AS Name
		FROM [dbo].[CN_NodeTypes] AS NT
			INNER JOIN hierarchy AS HR
			ON NT.ParentID = HR.ID
		WHERE NT.NodeTypeID <> HR.ID AND NT.Deleted = 0
	)
	INSERT INTO @OutputTable (NodeTypeID, ParentID, [Level], Name)
	SELECT ID, ParentID, [Level], Name
	FROM hierarchy
	
	RETURN
END

GO


/*
IF  EXISTS (SELECT * FROM sys.objects 
            WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetExpertiseNodeTypeID]') 
            AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetExpertiseNodeTypeID]
GO

CREATE FUNCTION [dbo].[CN_FN_GetExpertiseNodeTypeID](
	@ApplicationID	uniqueidentifier
)
RETURNS uniqueidentifier
WITH ENCRYPTION
AS
BEGIN
	RETURN (
		SELECT NodeTypeID 
		FROM [dbo].[CN_NodeTypes]
		WHERE ApplicationID = @ApplicationID AND AdditionalID = N'7'
	)
END

GO
*/


IF  EXISTS (SELECT * FROM sys.objects 
	WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetRelationTypeID]') 
    AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetRelationTypeID]
GO

CREATE FUNCTION [dbo].[CN_FN_GetRelationTypeID](
	@ApplicationID	uniqueidentifier,
	@AdditionalID	nvarchar(50)
)
RETURNS uniqueidentifier
WITH ENCRYPTION
AS
BEGIN
	RETURN (
		SELECT PropertyID 
		FROM [dbo].[CN_Properties]
		WHERE ApplicationID = @ApplicationID AND AdditionalID = @AdditionalID
	)
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetParentRelationTypeID]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetParentRelationTypeID]
GO

CREATE FUNCTION [dbo].[CN_FN_GetParentRelationTypeID](
	@ApplicationID	uniqueidentifier
)
RETURNS uniqueidentifier
WITH ENCRYPTION
AS
BEGIN
	RETURN (
		SELECT PropertyID 
		FROM [dbo].[CN_Properties]
		WHERE ApplicationID = @ApplicationID AND AdditionalID = N'1'
	)
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetChildRelationTypeID]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetChildRelationTypeID]
GO

CREATE FUNCTION [dbo].[CN_FN_GetChildRelationTypeID](
	@ApplicationID	uniqueidentifier
)
RETURNS uniqueidentifier
WITH ENCRYPTION
AS
BEGIN
	RETURN (
		SELECT PropertyID 
		FROM [dbo].[CN_Properties]
		WHERE ApplicationID = @ApplicationID AND AdditionalID = N'2'
	)
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetRelatedRelationTypeID]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetRelatedRelationTypeID]
GO

CREATE FUNCTION [dbo].[CN_FN_GetRelatedRelationTypeID](
	@ApplicationID	uniqueidentifier
)
RETURNS uniqueidentifier
WITH ENCRYPTION
AS
BEGIN
	RETURN (
		SELECT PropertyID 
		FROM [dbo].[CN_Properties]
		WHERE ApplicationID = @ApplicationID AND AdditionalID = N'3'
	)
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetChildNodesHierarchy]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetChildNodesHierarchy]
GO

CREATE FUNCTION [dbo].[CN_FN_GetChildNodesHierarchy](
	@ApplicationID	uniqueidentifier,
	@NodeIDs		GuidTableType readonly
)	
RETURNS @OutputTable TABLE (
	NodeID uniqueidentifier primary key clustered,
	ParentID uniqueidentifier,
	[Level] int,
	Name nvarchar(2000)
)
WITH ENCRYPTION
AS
BEGIN
	INSERT INTO @OutputTable(NodeID, ParentID, [Level], Name)
	SELECT NodeID AS ID, ParentNodeID AS ParentID, 0 AS [Level], Name AS Name
	FROM @NodeIDs AS N
		INNER JOIN [dbo].[CN_Nodes] AS ND
		ON ND.ApplicationID = @ApplicationID AND ND.NodeID = N.Value
	
	INSERT INTO @OutputTable(NodeID, ParentID, [Level], Name)
	SELECT ND.NodeID AS ID, ND.ParentNodeID AS ParentID, 
		[Level] + 1, ND.Name AS Name
	FROM @OutputTable AS HR
		INNER JOIN [dbo].[CN_Nodes] AS ND
		ON ND.ApplicationID = @ApplicationID AND ND.ParentNodeID = HR.NodeID
	WHERE ND.NodeID <> HR.NodeID AND ND.Deleted = 0
	
	RETURN
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetChildNodesDeepHierarchy]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetChildNodesDeepHierarchy]
GO

CREATE FUNCTION [dbo].[CN_FN_GetChildNodesDeepHierarchy](
	@ApplicationID	uniqueidentifier,
	@NodeIDs		GuidTableType readonly
)	
RETURNS @OutputTable TABLE (
	NodeID uniqueidentifier primary key clustered,
	ParentID uniqueidentifier,
	[Level] int,
	Name nvarchar(2000)
)
WITH ENCRYPTION
AS
BEGIN
	;WITH hierarchy (ID, ParentID, [Level], Name)
	AS
	(
		SELECT NodeID AS ID, ParentNodeID AS ParentID, 0 AS [Level], Name AS Name
		FROM @NodeIDs AS N
			INNER JOIN [dbo].[CN_Nodes] AS ND
			ON ND.ApplicationID = @ApplicationID AND ND.NodeID = N.Value
		
		UNION ALL
		
		SELECT Node.NodeID AS ID, Node.ParentNodeID AS ParentID, [Level] + 1, Node.Name
		FROM [dbo].[CN_Nodes] AS Node
			INNER JOIN hierarchy AS HR
			ON HR.ID = Node.ParentNodeID
		WHERE ApplicationID = @ApplicationID AND 
			Node.NodeID <> HR.ID AND Node.Deleted = 0
	)
	
	INSERT INTO @OutputTable (NodeID, ParentID, [Level], Name)
	SELECT ID, ParentID, [Level], Name
	FROM hierarchy
	ORDER BY hierarchy.[Level] ASC
	
	RETURN
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetChildNodeTypesHierarchy]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetChildNodeTypesHierarchy]
GO

CREATE FUNCTION [dbo].[CN_FN_GetChildNodeTypesHierarchy](
	@ApplicationID	uniqueidentifier,
	@NodeTypeIDs	GuidTableType readonly
)	
RETURNS @OutputTable TABLE (
	NodeTypeID uniqueidentifier primary key clustered,
	ParentID uniqueidentifier,
	[Level] int,
	Name nvarchar(2000)
)
WITH ENCRYPTION
AS
BEGIN
	INSERT INTO @OutputTable(NodeTypeID, ParentID, [Level], Name)
	SELECT NodeTypeID AS ID, ParentID AS ParentID, 0 AS [Level], Name AS Name
	FROM @NodeTypeIDs AS N
		INNER JOIN [dbo].[CN_NodeTypes] AS NT
		ON NT.ApplicationID = @ApplicationID AND NT.NodeTypeID = N.Value
	
	INSERT INTO @OutputTable(NodeTypeID, ParentID, [Level], Name)
	SELECT NT.NodeTypeID AS ID, NT.ParentID AS ParentID, 
		[Level] + 1, NT.Name AS Name
	FROM @OutputTable AS HR
		INNER JOIN [dbo].[CN_NodeTypes] AS NT
		ON NT.ApplicationID = @ApplicationID AND NT.ParentID = HR.NodeTypeID
	WHERE NT.NodeTypeID <> HR.NodeTypeID AND NT.Deleted = 0
	
	RETURN
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetChildNodeTypesDeepHierarchy]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetChildNodeTypesDeepHierarchy]
GO

CREATE FUNCTION [dbo].[CN_FN_GetChildNodeTypesDeepHierarchy](
	@ApplicationID	uniqueidentifier,
	@NodeTypeIDs	GuidTableType readonly
)	
RETURNS @OutputTable TABLE (
	NodeTypeID uniqueidentifier primary key clustered,
	ParentID uniqueidentifier,
	[Level] int,
	Name nvarchar(2000)
)
WITH ENCRYPTION
AS
BEGIN
	;WITH hierarchy (ID, ParentID, [Level], Name)
	AS
	(
		SELECT NodeTypeID AS ID, ParentID AS ParentID, 0 AS [Level], Name AS Name
		FROM @NodeTypeIDs AS N
			INNER JOIN [dbo].[CN_NodeTypes] AS ND
			ON ND.ApplicationID = @ApplicationID AND ND.NodeTypeID = N.Value
		
		UNION ALL
		
		SELECT NT.NodeTypeID AS ID, NT.ParentID AS ParentID, [Level] + 1, NT.Name
		FROM [dbo].[CN_NodeTypes] AS NT
			INNER JOIN hierarchy AS HR
			ON HR.ID = NT.ParentID
		WHERE ApplicationID = @ApplicationID AND NT.NodeTypeID <> HR.ID AND NT.Deleted = 0
	)
	
	INSERT INTO @OutputTable (NodeTypeID, ParentID, [Level], Name)
	SELECT *
	FROM (
			SELECT	ID, 
					CAST(MAX(CAST(ParentID AS varchar(50))) AS uniqueidentifier) AS ParentID, 
					MIN([Level]) AS [Level], 
					MAX(Name) AS Name
			FROM hierarchy
			GROUP BY ID
		) AS X
	ORDER BY X.[Level] ASC
	
	RETURN
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetNodesHierarchy]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetNodesHierarchy]
GO

CREATE FUNCTION [dbo].[CN_FN_GetNodesHierarchy](
	@ApplicationID	uniqueidentifier,
	@NodeIDs		GuidTableType readonly
)	
RETURNS @OutputTable TABLE (
	NodeID uniqueidentifier,
	ParentID uniqueidentifier,
	[Level] int,
	Name nvarchar(2000)
)
WITH ENCRYPTION
AS
BEGIN
	;WITH hierarchy (ID, TypeID, ParentID, [Level], Name)
	AS
	(
		SELECT ND.NodeID AS ID, ND.NodeTypeID AS TypeID, ND.ParentNodeID AS ParentID, 0 AS [Level], ND.Name
		FROM @NodeIDs AS IDs
			INNER JOIN [dbo].[CN_Nodes] AS ND
			ON ND.ApplicationID = @ApplicationID AND ND.NodeID = IDs.Value
		
		UNION ALL
		
		SELECT Node.NodeID AS ID, Node.NodeTypeID, Node.ParentNodeID AS ParentID, [Level] + 1, Node.Name
		FROM [dbo].[CN_Nodes] AS Node
			INNER JOIN hierarchy AS HR
			ON HR.ParentID = Node.NodeID
		WHERE ApplicationID = @ApplicationID AND Node.NodeTypeID = HR.TypeID AND
			Node.NodeID <> HR.ID AND Node.Deleted = 0
	)
	
	INSERT INTO @OutputTable (NodeID, ParentID, [Level], Name)
	SELECT hierarchy.ID, hierarchy.ParentID, hierarchy.[Level], hierarchy.Name
	FROM hierarchy
	ORDER BY hierarchy.[Level] ASC
	
	RETURN
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetRelatedNodeIDs]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetRelatedNodeIDs]
GO

CREATE FUNCTION [dbo].[CN_FN_GetRelatedNodeIDs](
	@ApplicationID		uniqueidentifier,
	@NodeIDs			GuidTableType readonly,
	@NodeTypeIDs		GuidTableType readonly,
	@RelatedNodeTypeIDs	GuidTableType readonly,
	@In					bit,
	@Out				bit,
	@InTags				bit,
	@OutTags			bit
)	
RETURNS @OutputTable TABLE (
	NodeID uniqueidentifier,
	RelatedNodeID uniqueidentifier,
	IsRelated bit,
	IsTagged bit
)
WITH ENCRYPTION
AS
BEGIN
	DECLARE @NodeIDsCount int = (SELECT COUNT(*) FROM @NodeIDs)
	DECLARE @NodeTypeIDsCount int = (SELECT COUNT(*) FROM @NodeTypeIDs)
	DECLARE @RelatedTypesCount int = (SELECT COUNT(*) FROM @RelatedNodeTypeIDs)
	
	--Source must not be null
	IF @NodeIDsCount = 0 AND @NodeTypeIDsCount = 0 SET @NodeIDsCount = 1

	INSERT INTO @OutputTable(NodeID, RelatedNodeID, IsRelated, IsTagged)
	SELECT X.NodeID, X.RelatedNodeID, X.IsRelated, X.IsTagged
	FROM (
			SELECT	IDs.NodeID, 
					IDs.RelatedNodeID, 
					CAST(MAX(IDs.IsRelated) AS bit) AS IsRelated, 
					CAST(MAX(IDs.IsTagged) AS bit) AS IsTagged
			FROM (
					SELECT R.NodeID, R.RelatedNodeID, 1 AS IsRelated, 0 AS IsTagged
					FROM [dbo].[CN_View_InRelatedNodes] AS R
					WHERE @In = 1 AND R.ApplicationID = @ApplicationID AND 
						(@NodeIDsCount = 0 OR R.NodeID IN (SELECT X.Value FROM @NodeIDs AS X)) AND (
							@RelatedTypesCount = 0 OR 
							R.RelatedNodeTypeID IN (SELECT X.Value FROM @RelatedNodeTypeIDs AS X)
						)
						
					UNION ALL 
					
					SELECT R.NodeID, R.RelatedNodeID, 1 AS IsRelated, 0 AS IsTagged
					FROM [dbo].[CN_View_OutRelatedNodes] AS R
					WHERE @Out = 1 AND R.ApplicationID = @ApplicationID AND 
						(@NodeIDsCount = 0 OR R.NodeID IN (SELECT X.Value FROM @NodeIDs AS X)) AND (
							@RelatedTypesCount = 0 OR 
							R.RelatedNodeTypeID IN (SELECT X.Value FROM @RelatedNodeTypeIDs AS X)
						)
						
					UNION ALL
						
					SELECT T.TaggedID, T.ContextID, 0 AS IsRelated, 1 AS IsTagged
					FROM [dbo].[RV_TaggedItems] AS T
						INNER JOIN [dbo].[CN_Nodes] AS ND
						ON ND.ApplicationID = @ApplicationID AND ND.NodeID = T.ContextID AND ND.Deleted = 0 AND (
							@RelatedTypesCount = 0 OR 
							ND.NodeTypeID IN (SELECT X.Value FROM @RelatedNodeTypeIDs AS X)
						)
					WHERE @InTags = 1 AND T.ApplicationID = @ApplicationID AND 
						(@NodeIDsCount = 0 OR T.TaggedID IN (SELECT X.Value FROM @NodeIDs AS X)) AND
						T.ContextType = N'Node'
						
					UNION ALL
						
					SELECT T.ContextID, T.TaggedID, 0 AS IsRelated, 1 AS IsTagged
					FROM [dbo].[RV_TaggedItems] AS T
						INNER JOIN [dbo].[CN_Nodes] AS ND
						ON ND.ApplicationID = @ApplicationID AND ND.NodeID = T.TaggedID AND ND.Deleted = 0 AND (
							@RelatedTypesCount = 0 OR 
							ND.NodeTypeID IN (SELECT X.Value FROM @RelatedNodeTypeIDs AS X)
						)
					WHERE @OutTags = 1 AND T.ApplicationID = @ApplicationID AND
						(@NodeIDsCount = 0 OR T.ContextID IN (SELECT X.Value FROM @NodeIDs AS X)) AND
						T.TaggedType LIKE N'Node%'
				) AS IDs
			GROUP BY IDs.NodeID, IDs.RelatedNodeID
		) AS X
		LEFT JOIN [dbo].[CN_Nodes] AS N
		ON @NodeTypeIDsCount > 0 AND N.ApplicationID = @ApplicationID AND N.NodeID = X.NodeID AND
			N.NodeTypeID IN (SELECT T.Value FROM @NodeTypeIDs AS T)
	WHERE X.NodeID <> X.RelatedNodeID AND (@NodeTypeIDsCount = 0 OR N.NodeID IS NOT NULL)
	
	RETURN
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetUserRelatedNodeIDs]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetUserRelatedNodeIDs]
GO

CREATE FUNCTION [dbo].[CN_FN_GetUserRelatedNodeIDs](
	@ApplicationID		uniqueidentifier,
	@UserIDs			GuidTableType readonly,
	@RelatedNodeTypeIDs	GuidTableType readonly
)	
RETURNS @OutputTable TABLE (
	UserID uniqueidentifier,
	RelatedNodeID uniqueidentifier
)
WITH ENCRYPTION
AS
BEGIN
	DECLARE @RelatedTypesCount int = (SELECT COUNT(*) FROM @RelatedNodeTypeIDs)

	INSERT INTO @OutputTable(UserID, RelatedNodeID)
	SELECT DISTINCT T.TaggedID, T.ContextID
	FROM @UserIDs AS IDs
		INNER JOIN [dbo].[RV_TaggedItems] AS T
		ON T.ApplicationID = @ApplicationID AND T.TaggedID = IDs.Value
		INNER JOIN [dbo].[CN_Nodes] AS ND
		ON ND.ApplicationID = @ApplicationID AND ND.NodeID = T.ContextID AND ND.Deleted = 0 AND (
			@RelatedTypesCount = 0 OR 
			ND.NodeTypeID IN (SELECT X.Value FROM @RelatedNodeTypeIDs AS X)
		)
	WHERE T.ContextID <> T.TaggedID
	
	RETURN
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_Explore]') 
	AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_Explore]
GO

CREATE FUNCTION [dbo].[CN_FN_Explore](
	@ApplicationID uniqueidentifier,
	@BaseIDs GuidTableType readonly,
	@BaseTypeIDs GuidTableType readonly,
	@RelatedID uniqueidentifier,
	@RelatedTypeIDs GuidTableType readonly,
	@RegistrationArea bit,
	@Tags bit,
	@Relations bit
)	
RETURNS @OutputTable TABLE (
	BaseID				UNIQUEIDENTIFIER NOT NULL,
    BaseTypeID			UNIQUEIDENTIFIER NULL,
    BaseName			NVARCHAR(2000) NULL,
    BaseType			NVARCHAR(2000) NULL,
    RelatedID			UNIQUEIDENTIFIER NOT NULL,
    RelatedTypeID		UNIQUEIDENTIFIER NULL,
    RelatedName			NVARCHAR(2000) NULL,
    RelatedType			NVARCHAR(2000) NULL,
    RelatedCreationDate	DATETIME NULL,
    IsTag				BIT NULL,
    IsRelation			BIT NULL,
    IsRegistrationArea	BIT NULL
)
WITH ENCRYPTION
AS
BEGIN
	-- Base === Context --> Folder
	-- Tagged === Related --> Content

	DECLARE @BaseIDsCount int = (SELECT COUNT(*) FROM @BaseIDs)
	DECLARE @BaseTypeIDsCount int = 0
	IF @BaseIDsCount = 0 SET @BaseTypeIDsCount = (SELECT COUNT(*) FROM @BaseTypeIDs)
	DECLARE @RelatedTypeIDsCount int = 0
	IF @RelatedID IS NULL SET @RelatedTypeIDsCount = (SELECT COUNT(*) FROM @RelatedTypeIDs)
	
	INSERT INTO @OutputTable (
		BaseID, BaseTypeID, BaseName, BaseType,
		RelatedID, RelatedTypeID, RelatedName, RelatedType, RelatedCreationDate,
		IsTag, IsRelation, IsRegistrationArea
	)
	SELECT	Base.NodeID, Base.NodeTypeID, Base.NodeName, Base.TypeName,
			Related.NodeID, Related.NodeTypeID, Related.NodeName, Related.TypeName, Related.CreationDate,
			DT.IsTag, DT.IsRelation, DT.IsRegistrationArea
	FROM (
			SELECT R.NodeID, R.RelatedNodeID, R.IsRelated AS IsRelation, 
				R.IsTagged AS IsTag, 0 AS IsRegistrationArea
			FROM [dbo].[CN_FN_GetRelatedNodeIDs](@ApplicationID, 
				@BaseIDs, @BaseTypeIDs, @RelatedTypeIDs, @Relations, @Relations, @Tags, @Tags) AS R
			WHERE @RelatedID IS NULL OR R.RelatedNodeID = @RelatedID
			
			UNION ALL
			
			SELECT N.NodeID AS ContextID, N.AreaID AS TaggedID, 0, 0, 1
			FROM [dbo].[CN_Nodes] AS N
				INNER JOIN [dbo].[CN_Nodes] AS Area
				ON Area.ApplicationID = @ApplicationID AND Area.NodeID = N.AreaID
			WHERE @RegistrationArea = 1 AND N.ApplicationID = @ApplicationID AND 
				(@RelatedID IS NULL OR N.NodeID = @RelatedID) AND
				(@RelatedTypeIDsCount = 0 OR N.NodeTypeID IN (SELECT X.Value FROM @RelatedTypeIDs AS X)) AND
				(@BaseIDsCount = 0 OR Area.NodeID IN (SELECT X.Value FROM @BaseIDs AS X)) AND
				(@BaseTypeIDsCount = 0 OR Area.NodeTypeID IN (SELECT X.Value FROM @BaseTypeIDs AS X))
		) AS DT
		INNER JOIN [dbo].[CN_View_Nodes_Normal] AS Base
		ON Base.ApplicationID = @ApplicationID AND Base.NodeID = DT.NodeID
		INNER JOIN [dbo].[CN_View_Nodes_Normal] AS Related
		ON Related.ApplicationID = @ApplicationID AND Related.NodeID = DT.RelatedNodeID
	WHERE ISNULL(Related.[Status], N'Accepted') = N'Accepted' AND ISNULL(Related.Searchable, 1) = 1

	RETURN
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CN_FN_GetNodeFileContents]') 
    AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CN_FN_GetNodeFileContents]
GO

CREATE FUNCTION [dbo].[CN_FN_GetNodeFileContents](
	@ApplicationID	uniqueidentifier,
	@NodeID			uniqueidentifier
)
RETURNS nvarchar(max)
WITH ENCRYPTION
AS
BEGIN
	DECLARE @Ret nvarchar(max) = NULL

	;WITH Partitioned AS
	(
		SELECT	OID.OwnerID,
				CAST(SUBSTRING(ISNULL(FC.Content, N''), 1, 4000) AS nvarchar(4000)) AS Content,
				ROW_NUMBER() OVER (PARTITION BY OID.OwnerID ORDER BY FC.FileID ASC, OID.ID ASC) AS Number,
				COUNT(*) OVER (PARTITION BY OID.OwnerID) AS [Count]
		FROM (
				SELECT @NodeID AS OwnerID, E.ElementID AS ID
				FROM [dbo].[FG_FormInstances] AS I
					INNER JOIN [dbo].[FG_InstanceElements] AS E
					ON E.ApplicationID = @ApplicationID AND E.InstanceID = I.InstanceID AND 
						E.[Type] = N'File' AND E.Deleted = 0
				WHERE I.ApplicationID = @ApplicationID AND 
					I.OwnerID = @NodeID AND I.Deleted = 0
				
				UNION
				
				SELECT @NodeID AS OwnerID, @NodeID
			) AS OID
			INNER JOIN [dbo].[DCT_Files] AS F
			ON F.ApplicationID = @ApplicationID AND F.Deleted = 0 AND
				(F.OwnerID = OID.OwnerID OR F.OwnerID = OID.ID) /* AND
				(
					(OID.ID = OID.OwnerID AND F.OwnerType = N'Node') OR
					(OID.ID <> OID.OwnerID AND 
						(F.OwnerType = N'WikiContent' OR F.OwnerType = N'FormElement')
					)
				)*/
			INNER JOIN [dbo].[DCT_FileContents] AS FC
			ON FC.ApplicationID = @ApplicationID AND 
				FC.FileID = F.FileNameGuid AND FC.NotExtractable = 0
	),
	Fetched AS
	(
		SELECT	OwnerID, Content AS FullContent, Content, Number, [COUNT] 
		FROM Partitioned WHERE Number = 1

		UNION ALL

		SELECT	P.OwnerID, C.FullContent + ' ' + P.Content, P.Content, P.Number, P.[Count]
		FROM Partitioned AS P
			INNER JOIN Fetched AS C 
			ON P.OwnerID = C.OwnerID AND P.Number = C.Number + 1
		WHERE P.Number <= 95
	)
	SELECT TOP(1) @Ret = FullContent
	FROM Fetched
	WHERE Fetched.Number = (CASE WHEN Fetched.[Count] > 90 THEN 90 ELSE Fetched.[Count] END)
	
	RETURN @Ret
END

GO