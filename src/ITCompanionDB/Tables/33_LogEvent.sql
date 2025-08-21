CREATE TABLE [dbo].[LogEvent](
    [LogEventId] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LogEvent PRIMARY KEY,
    [LogSourceId] INT NOT NULL,
    [Ts] DATETIME2 NOT NULL,
    [Level] TINYINT NOT NULL,
    [EventId] INT NULL,
    [Category] NVARCHAR(256) NULL,
    [Message] NVARCHAR(4000) NULL,
    [Session] NVARCHAR(64) NULL,
    [Host] NVARCHAR(64) NULL,
    [UserName] NVARCHAR(64) NULL,
    [AppVersion] NVARCHAR(32) NULL,
    [ModuleVersion] NVARCHAR(32) NULL,
    [RawJson] NVARCHAR(MAX) NULL,
    [CreatedUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LogEvent_LogSource FOREIGN KEY (LogSourceId) REFERENCES [dbo].[LogSource](LogSourceId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_LogEvent_Ts ON [dbo].[LogEvent]([Ts] DESC);
GO
CREATE INDEX IX_LogEvent_Level ON [dbo].[LogEvent]([Level]) WHERE [Level] >= 3;
GO
