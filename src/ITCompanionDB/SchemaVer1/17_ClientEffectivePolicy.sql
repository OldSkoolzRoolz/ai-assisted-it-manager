CREATE TABLE [dbo].[ClientEffectivePolicy](
    [ClientId] NVARCHAR(64) NOT NULL,
    [PolicyDefId] NVARCHAR(64) NOT NULL,
    [AppliedValue] NVARCHAR(1024) NULL,
    [AppliedEnabled] BIT NOT NULL,
    [SourcePolicyGroupId] NVARCHAR(64) NULL,
    [Hash] NVARCHAR(128) NOT NULL,
    [AppliedUtc] DATETIME2 NOT NULL,
    CONSTRAINT PK_ClientEffectivePolicy PRIMARY KEY (ClientId, PolicyDefId),
    CONSTRAINT FK_ClientEffectivePolicy_Client FOREIGN KEY (ClientId) REFERENCES [dbo].[Client](ClientId) ON DELETE CASCADE,
    CONSTRAINT FK_ClientEffectivePolicy_Def FOREIGN KEY (PolicyDefId) REFERENCES [dbo].[PolicyDefinition](PolicyDefId) ON DELETE CASCADE,
    CONSTRAINT FK_ClientEffectivePolicy_SourceGroup FOREIGN KEY (SourcePolicyGroupId) REFERENCES [dbo].[PolicyGroup](PolicyGroupId)
);
GO
CREATE INDEX IX_ClientEffectivePolicy_Hash ON [dbo].[ClientEffectivePolicy]([Hash]);