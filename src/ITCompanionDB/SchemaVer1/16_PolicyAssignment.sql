CREATE TABLE [dbo].[PolicyAssignment](
    [PolicyGroupId] NVARCHAR(64) NOT NULL,
    [ClientGroupId] NVARCHAR(64) NOT NULL,
    [AssignedUtc] DATETIME2 NOT NULL,
    [AssignedBy] NVARCHAR(128) NULL,
    CONSTRAINT PK_PolicyAssignment PRIMARY KEY (PolicyGroupId, ClientGroupId),
    CONSTRAINT FK_PolicyAssignment_Group FOREIGN KEY (PolicyGroupId) REFERENCES [dbo].[PolicyGroup](PolicyGroupId) ON DELETE CASCADE,
    CONSTRAINT FK_PolicyAssignment_ClientGroup FOREIGN KEY (ClientGroupId) REFERENCES [dbo].[ClientGroup](ClientGroupId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_PolicyAssignment_Group ON [dbo].[PolicyAssignment]([ClientGroupId]);