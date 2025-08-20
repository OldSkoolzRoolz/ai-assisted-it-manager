CREATE TABLE [dbo].[Meta] (
    [Key]   NVARCHAR(64) NOT NULL CONSTRAINT PK_Meta PRIMARY KEY,
    [Value] NVARCHAR(256) NOT NULL
);
GO
INSERT INTO [dbo].[Meta] ([Key],[Value]) VALUES ('SchemaVersion','1');
GO
