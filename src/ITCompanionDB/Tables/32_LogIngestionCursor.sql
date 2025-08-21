CREATE TABLE [dbo].[LogIngestionCursor](
    [LogSourceId] INT NOT NULL CONSTRAINT PK_LogIngestionCursor PRIMARY KEY,
    [LastFile] NVARCHAR(512) NULL,
    [LastPosition] BIGINT NOT NULL DEFAULT 0,
    [LastFileSize] BIGINT NOT NULL DEFAULT 0,
    [LastHash] VARBINARY(32) NULL,
    [UpdatedUtc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LogIngestionCursor_LogSource FOREIGN KEY (LogSourceId) REFERENCES [dbo].[LogSource](LogSourceId) ON DELETE CASCADE
);
GO
