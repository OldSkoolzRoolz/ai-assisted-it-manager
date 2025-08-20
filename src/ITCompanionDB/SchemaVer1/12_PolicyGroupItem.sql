CREATE TABLE [dbo].[PolicyGroupItem](
    [PolicyGroupId] NVARCHAR(64) NOT NULL,
    [PolicyDefId] NVARCHAR(64) NOT NULL,
    [Enabled] BIT NOT NULL,
    [DesiredValue] NVARCHAR(1024) NULL,
    [Enforced] BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_PolicyGroupItem PRIMARY KEY (PolicyGroupId, PolicyDefId),
    CONSTRAINT FK_PolicyGroupItem_Group FOREIGN KEY (PolicyGroupId) REFERENCES [dbo].[PolicyGroup](PolicyGroupId) ON DELETE CASCADE,
    CONSTRAINT FK_PolicyGroupItem_Def FOREIGN KEY (PolicyDefId) REFERENCES [dbo].[PolicyDefinition](PolicyDefId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_PolicyGroupItem_Group ON [dbo].[PolicyGroupItem]([PolicyGroupId]);
GO
CREATE INDEX IX_PolicyGroupItem_Policy ON [dbo].[PolicyGroupItem]([PolicyDefId]);