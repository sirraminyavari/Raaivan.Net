USE EKM_App
GO


DECLARE @DBName nvarchar(100) = (SELECT DB_NAME());

DECLARE @DBFolder nvarchar(1000) = (
	SELECT LEFT(N.physical_name, LEN(N.physical_name) - 
				CHARINDEX('\', REVERSE(N.physical_name) + '\')) AS Folder
	FROM sys.database_files AS N
	WHERE LOWER(N.physical_name) LIKE N'%.mdf'
);

DECLARE @GroupNames TABLE(Name varchar(50));

INSERT INTO @GroupNames (Name)
SELECT LOWER(N'r' + REPLACE(CAST(ApplicationId AS varchar(50)), N'-', N''))
FROM dbo.aspnet_Applications;


ALTER DATABASE [EKM_App]
ADD FILEGROUP [ramin]
GO

ALTER DATABASE [EKM_App]
ADD FILEGROUP [gesi]
GO

ALTER DATABASE [EKM_App]
ADD FILE (
		NAME = [PartRamin],
		[FILENAME] = N'E:\Applications\Databases\SQLDBs\EKM_App_Ramin.ndf',
		SIZE = 3072 KB,
		MAXSIZE = UNLIMITED,
		FILEGROWTH = 1024 KB
	) TO FILEGROUP [ramin]
	
ALTER DATABASE [EKM_App]
ADD FILE (
		NAME = [PartGesi],
		[FILENAME] = N'E:\Applications\Databases\SQLDBs\EKM_App_Gesi.ndf',
		SIZE = 3072 KB,
		MAXSIZE = UNLIMITED,
		FILEGROWTH = 1024 KB
	) TO FILEGROUP [gesi]
	

CREATE PARTITION FUNCTION RV_PartitionFunction (uniqueidentifier) AS RANGE RIGHT 
FOR VALUES (
	N'08c72552-4f2c-473f-b3b0-c2dacf8cd6a9', -- All distinct ApplicationID values must be listed here
	N'df87sdf9-8s9g-8524-f2c4-73c2daff4l5j'
);
GO 

CREATE PARTITION SCHEME RV_PartitionScheme AS PARTITION RV_PartitionFunction
TO (
	N'r08c725524f2c473fb3b0c2dacf8cd6a9', -- All distinct FileGroup names must be listed here
	N'rdf87sdf98s9g8524f2c473c2daff4l5j'
);
GO 


/*
SELECT name AS AvailableFilegroups
FROM sys.filegroups
WHERE type = 'FG'

	
SELECT 
name as [FileName],
physical_name as [FilePath] 
FROM sys.database_files
where type_desc = 'ROWS'
GO
*/