CREATE TABLE [dbo].[BehaviorPolicyLayer](
    [Layer] INT NOT NULL CONSTRAINT PK_BehaviorPolicyLayer PRIMARY KEY,
    [LogRetentionDays] INT NOT NULL,
    [MaxLogFileSizeMB] INT NOT NULL,
    [MinLogLevel] NVARCHAR(32) NOT NULL,
    [UiLanguage] NVARCHAR(16) NOT NULL,
    [EnableTelemetry] BIT NOT NULL,
    [PolicyVersion] NVARCHAR(32) NOT NULL,
    [EffectiveUtc] DATETIME2 NOT NULL,
    [AllowedGroupsCsv] NVARCHAR(512) NOT NULL DEFAULT 'Administrators',
    [UpdatedUtc] DATETIME2 NOT NULL
);
GO
