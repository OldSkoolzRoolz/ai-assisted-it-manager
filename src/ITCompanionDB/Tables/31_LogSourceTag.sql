CREATE TABLE [dbo].[LogSourceTag](
    [LogSourceId] INT NOT NULL,
    [Tag] NVARCHAR(64) NOT NULL,
    CONSTRAINT PK_LogSourceTag PRIMARY KEY (LogSourceId, Tag),
    CONSTRAINT FK_LogSourceTag_LogSource FOREIGN KEY (LogSourceId) REFERENCES [dbo].[LogSource](LogSourceId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_LogSourceTag_Tag ON [dbo].[LogSourceTag]([Tag]);
GO
