CREATE TABLE [dbo].[PolicyDefinition](
    [PolicyDefId] NVARCHAR(64) NOT NULL CONSTRAINT PK_PolicyDefinition PRIMARY KEY,
    [Name] NVARCHAR(256) NOT NULL,
    [Category] NVARCHAR(128) NOT NULL,
    [RegistryRoot] NVARCHAR(8) NOT NULL, -- HKLM / HKCU
    [RegistryPath] NVARCHAR(512) NOT NULL,
    [ValueName] NVARCHAR(256) NOT NULL,
    [ValueType] NVARCHAR(32) NOT NULL,
    [Description] NVARCHAR(1024) NULL,
    [IntroducedVersion] NVARCHAR(16) NULL,
    [Deprecated] BIT NOT NULL DEFAULT 0,
    [CreatedUtc] DATETIME2 NOT NULL,
    [UpdatedUtc] DATETIME2 NOT NULL
);
GO
