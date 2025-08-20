CREATE TABLE [dbo].[Client](
    [ClientId] NVARCHAR(64) NOT NULL CONSTRAINT PK_Client PRIMARY KEY,
    [HostName] NVARCHAR(128) NOT NULL,
    [DomainName] NVARCHAR(128) NULL,
    [OSVersion] NVARCHAR(64) NULL,
    [LastHeartbeatUtc] DATETIME2 NULL,
    [EffectivePolicyHash] NVARCHAR(128) NULL,
    [CreatedUtc] DATETIME2 NOT NULL,
    [UpdatedUtc] DATETIME2 NOT NULL
);
GO
CREATE INDEX IX_Client_LastHeartbeat ON [dbo].[Client]([LastHeartbeatUtc]);