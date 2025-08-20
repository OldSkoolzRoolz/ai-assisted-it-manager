CREATE TABLE [dbo].[ClientGroup](
    [ClientGroupId] NVARCHAR(64) NOT NULL CONSTRAINT PK_ClientGroup PRIMARY KEY,
    [Name] NVARCHAR(128) NOT NULL,
    [Description] NVARCHAR(512) NULL,
    [CreatedUtc] DATETIME2 NOT NULL,
    [UpdatedUtc] DATETIME2 NOT NULL
);
GO
