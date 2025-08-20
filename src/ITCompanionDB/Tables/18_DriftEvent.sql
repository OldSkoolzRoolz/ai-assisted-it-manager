CREATE TABLE [dbo].[DriftEvent](
    [DriftEventId] NVARCHAR(64) NOT NULL CONSTRAINT PK_DriftEvent PRIMARY KEY,
    [ClientId] NVARCHAR(64) NOT NULL,
    [PolicyDefId] NVARCHAR(64) NOT NULL,
    [ExpectedHash] NVARCHAR(128) NOT NULL,
    [ObservedHash] NVARCHAR(128) NOT NULL,
    [ExpectedValue] NVARCHAR(1024) NULL,
    [ObservedValue] NVARCHAR(1024) NULL,
    [ExpectedEnabled] BIT NULL,
    [ObservedEnabled] BIT NULL,
    [DetectedUtc] DATETIME2 NOT NULL,
    [Severity] NVARCHAR(16) NOT NULL,
    [Status] NVARCHAR(16) NOT NULL DEFAULT 'New',
    CONSTRAINT FK_DriftEvent_Client FOREIGN KEY (ClientId) REFERENCES [dbo].[Client](ClientId) ON DELETE CASCADE,
    CONSTRAINT FK_DriftEvent_Def FOREIGN KEY (PolicyDefId) REFERENCES [dbo].[PolicyDefinition](PolicyDefId) ON DELETE CASCADE
);
GO
