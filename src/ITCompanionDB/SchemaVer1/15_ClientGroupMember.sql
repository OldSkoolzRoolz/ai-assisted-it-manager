CREATE TABLE [dbo].[ClientGroupMember](
    [ClientGroupId] NVARCHAR(64) NOT NULL,
    [ClientId] NVARCHAR(64) NOT NULL,
    CONSTRAINT PK_ClientGroupMember PRIMARY KEY (ClientGroupId, ClientId),
    CONSTRAINT FK_ClientGroupMember_Group FOREIGN KEY (ClientGroupId) REFERENCES [dbo].[ClientGroup](ClientGroupId) ON DELETE CASCADE,
    CONSTRAINT FK_ClientGroupMember_Client FOREIGN KEY (ClientId) REFERENCES [dbo].[Client](ClientId) ON DELETE CASCADE
);
GO
