CREATE TABLE [dbo].[AuditEvent](
    [AuditId] NVARCHAR(64) NOT NULL CONSTRAINT PK_AuditEvent PRIMARY KEY,
    [EventType] NVARCHAR(64) NOT NULL,
    [Actor] NVARCHAR(128) NULL,
    [ActorType] NVARCHAR(32) NULL,
    [ClientId] NVARCHAR(64) NULL,
    [PolicyGroupId] NVARCHAR(64) NULL,
    [PolicyDefId] NVARCHAR(64) NULL,
    [DetailsJson] NVARCHAR(MAX) NULL,
    [CorrelationId] NVARCHAR(64) NULL,
    [CreatedUtc] DATETIME2 NOT NULL,
    CONSTRAINT FK_AuditEvent_Client FOREIGN KEY (ClientId) REFERENCES [dbo].[Client](ClientId),
    CONSTRAINT FK_AuditEvent_PolicyGroup FOREIGN KEY (PolicyGroupId) REFERENCES [dbo].[PolicyGroup](PolicyGroupId),
    CONSTRAINT FK_AuditEvent_PolicyDef FOREIGN KEY (PolicyDefId) REFERENCES [dbo].[PolicyDefinition](PolicyDefId)
);
GO
