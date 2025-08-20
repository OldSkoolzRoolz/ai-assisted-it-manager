CREATE TABLE [dbo].[AuditRetention](
    [RetentionDays] INT NOT NULL CONSTRAINT PK_AuditRetention PRIMARY KEY,
    [CreatedUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
INSERT INTO [dbo].[AuditRetention] (RetentionDays) VALUES (365);
GO
