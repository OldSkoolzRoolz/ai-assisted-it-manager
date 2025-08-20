CREATE TABLE [dbo].[PolicyGroup](
    [PolicyGroupId] NVARCHAR(64) NOT NULL CONSTRAINT PK_PolicyGroup PRIMARY KEY,
    [Name] NVARCHAR(256) NOT NULL,
    [Description] NVARCHAR(512) NULL,
    [Priority] INT NOT NULL DEFAULT 0,
    [CreatedUtc] DATETIME2 NOT NULL,
    [UpdatedUtc] DATETIME2 NOT NULL
);
GO
