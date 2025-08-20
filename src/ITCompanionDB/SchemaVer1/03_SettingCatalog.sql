CREATE TABLE [dbo].[SettingCatalog](
    [Name] NVARCHAR(128) NOT NULL CONSTRAINT PK_SettingCatalog PRIMARY KEY,
    [DefaultValue] NVARCHAR(256) NOT NULL,
    [RegistryName] NVARCHAR(128) NOT NULL,
    [Scope] NVARCHAR(64) NOT NULL,
    [Type] NVARCHAR(32) NOT NULL,
    [Description] NVARCHAR(512) NOT NULL,
    [Allowed] NVARCHAR(256) NULL,
    [HotReload] BIT NOT NULL,
    [RestartRequirement] NVARCHAR(32) NOT NULL,
    [ModelProperty] NVARCHAR(128) NOT NULL,
    [Owner] NVARCHAR(64) NOT NULL,
    [IntroducedVersion] NVARCHAR(16) NOT NULL,
    [LastChangedVersion] NVARCHAR(16) NOT NULL,
    [FutureAdmxPolicyName] NVARCHAR(128) NULL,
    [CreatedUtc] DATETIME2 NOT NULL,
    [UpdatedUtc] DATETIME2 NOT NULL
);
GO
